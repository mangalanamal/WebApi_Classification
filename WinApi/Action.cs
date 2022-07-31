﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.InformationProtection.File;
using Microsoft.InformationProtection.Exceptions;
using Microsoft.InformationProtection;
using System.Configuration;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace WinApi
{
    /// <summary>
    /// Action class implements the various MIP functionality.
    /// For this sample, only profile, engine, and handler creation are defined. 
    /// The IFileHandler may be used to label a file and read a labeled file.
    /// </summary>
    public class Action : IDisposable
    {
        // Fetch tenant name to build identity for service principal
        private static readonly string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        

        private AuthDelegateImplementation authDelegate;
        private ApplicationInfo appInfo;
        private IFileProfile profile;
        private IFileEngine engine;
        private MipContext mipContext;

        // Used to pass in options for labeling the file.
        public struct FileOptions
        {
            public string FileName;
            public string OutputName;
            public string LabelId;
            public DataState DataState;
            public AssignmentMethod AssignmentMethod;
            public ActionSource ActionSource;
            public bool IsAuditDiscoveryEnabled;
            public bool GenerateChangeAuditEvent;
        }

        /// <summary>
        /// Constructor for Action class. Pass in AppInfo to simplify passing settings to AuthDelegate.
        /// </summary>
        /// <param name="appInfo"></param>
        public Action(ApplicationInfo appInfo)
        {
            this.appInfo = appInfo;

            // Initialize AuthDelegateImplementation using AppInfo. 
            authDelegate = new AuthDelegateImplementation(this.appInfo);

            // Initialize SDK DLLs. If DLLs are missing or wrong type, this will throw an exception
            MIP.Initialize(MipComponent.File);

            // Create MipConfiguration Object
            MipConfiguration mipConfiguration = new MipConfiguration(appInfo, "mip_data", LogLevel.Trace, false);

            // Create MipContext using MipConfiguration
            mipContext = MIP.CreateMipContext(mipConfiguration);

            // We must construct a service principal identity mail address as it can't be fetched from the token.
            // Here, we set it to be ClientId@Tenant.com, but the SDK will accept any properly formatted email address.
            Identity id = new Identity(String.Format("{0}@{1}", appInfo.ApplicationId, tenant))
            {
                // Use this if you want the app to protect on behalf of a user. That user owns the protected content.
                // Email = "test@contoso.com"
            };

            // Create profile.
            profile = CreateFileProfile(appInfo, ref authDelegate);

            // Create engine providing Identity from authDelegate to assist with service discovery.
            engine = CreateFileEngine(id);
        }

        /// <summary>
        /// Null refs to engine and profile and release all MIP resources.
        /// </summary>
        public void Dispose()
        {
            profile.UnloadEngineAsync(engine.Settings.EngineId).Wait();
            engine.Dispose();
            profile.Dispose();
            mipContext.ShutDown();
            mipContext.Dispose();
        }

        /// <summary>
        /// Creates an IFileProfile and returns.
        /// IFileProfile is the root of all MIP SDK File API operations. Typically only one should be created per app.
        /// </summary>
        /// <param name="appInfo"></param>
        /// <param name="authDelegate"></param>
        /// <returns></returns>
        private IFileProfile CreateFileProfile(ApplicationInfo appInfo, ref AuthDelegateImplementation authDelegate)
        {           
            // Initialize file profile settings to create/use local state.                
            var profileSettings = new FileProfileSettings(mipContext, CacheStorageType.OnDiskEncrypted, new ConsentDelegateImplementation());

            // Use MIP.LoadFileProfileAsync() providing settings to create IFileProfile. 
            // IFileProfile is the root of all SDK operations for a given application.
            var profile = Task.Run(async () => await MIP.LoadFileProfileAsync(profileSettings)).Result;
            return profile;
        }

        /// <summary>
        /// Creates a file engine, associating the engine with the specified identity. 
        /// File engines are generally created per-user in an application. 
        /// IFileEngine implements all operations for fetching labels and sensitivity types.
        /// IFileHandlers are added to engines to perform labeling operations.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private IFileEngine CreateFileEngine(Identity identity)
        {
            
            // If the profile hasn't been created, do that first. 
            if (profile == null)
            {
                profile = CreateFileProfile(appInfo, ref authDelegate);
            }

            // Create file settings object. Passing in empty string for the first parameter, engine ID, will cause the SDK to generate a GUID.
            // Locale settings are supported and should be provided based on the machine locale, particular for client applications.
            var engineSettings = new FileEngineSettings("", authDelegate, "", "en-US")
            {
                // Provide the identity for service discovery.
                Identity = identity
            };

            // Add the IFileEngine to the profile and return.
            var engine = Task.Run(async () => await profile.AddEngineAsync(engineSettings)).Result;
            return engine;
        }
    

        /// <summary>
        /// Method creates a file handler and returns to the caller. 
        /// IFileHandler implements all labeling and protection operations in the File API.        
        /// </summary>
        /// <param name="options">Struct provided to set various options for the handler.</param>
        /// <returns></returns>
        private IFileHandler CreateFileHandler(FileOptions options)
        {            
                // Create the handler using options from FileOptions. Assumes that the engine was previously created and stored in private engine object.
                // There's probably a better way to pass/store the engine, but this is a sample ;)
                var handler = Task.Run(async () => await engine.CreateFileHandlerAsync(options.FileName, options.FileName, options.IsAuditDiscoveryEnabled)).Result;
                return handler;           
        }


        /// <summary>
        /// List all labels from the engine and return in IEnumerable<Label>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Label> ListLabels()
        {  
                // Get labels from the engine and return.
                // For a user principal, these will be user specific.
                // For a service principal, these may be service specific or global.
                return engine.SensitivityLabels;          
        }

        /// <summary>
        /// Set the label on the given file. 
        /// Options for the labeling operation are provided in the FileOptions parameter.
        /// </summary>
        /// <param name="options">Details about file input, output, label to apply, etc.</param>
        /// <returns></returns>
        public bool SetLabel(FileOptions options)
        {

            // LabelingOptions allows us to set the metadata associated with the labeling operations.
            // Review the API Spec at https://aka.ms/mipsdkdocs for details
            LabelingOptions labelingOptions = new LabelingOptions()
            {
                AssignmentMethod = options.AssignmentMethod
            };

            var handler = CreateFileHandler(options);

            // Use the SetLabel method on the handler, providing label ID and LabelingOptions
            // The handler already references a file, so those details aren't needed.
            handler.SetLabel(engine.GetLabelById(options.LabelId), labelingOptions, new ProtectionSettings());

            // The change isn't committed to the file referenced by the handler until CommitAsync() is called.
            // Pass the desired output file name in to the CommitAsync() function.
            bool result = false;

            // Check to see that modifications occurred on the handler. If not, skip commit. 
            if (handler.IsModified())
            {
                result = Task.Run(async () => await handler.CommitAsync(options.OutputName)).Result;
            }

            // If the commit was successful and GenerateChangeAuditEvents is true, call NotifyCommitSuccessful()
            if (result && options.GenerateChangeAuditEvent)
            {
                // Submits and audit event about the labeling action to Azure Information Protection Analytics 
                handler.NotifyCommitSuccessful(options.FileName);
            }

            return result;
        }

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        /// <summary>
        /// Read the label from a file provided via FileOptions.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ContentLabel GetLabel(FileOptions options)
        {

            

            var handler = CreateFileHandler(options);
                return handler.Label;         
        }        
    }
}
