using Microsoft.Identity.Client;
using Microsoft.InformationProtection;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinApi.DBContext;
using WinApi.Models;
using WinApi.Models.SQLiteModels;

namespace WinApi
{
    public partial class MainScreen : Form
    {
        private static readonly string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string appName = ConfigurationManager.AppSettings["app:Name"];
        private static readonly string appVersion = ConfigurationManager.AppSettings["app:Version"];
        private static readonly string graphAPIEndpoint = ConfigurationManager.AppSettings["graphAPIEndpoint"];
        DataContext db = new DataContext();
        private Form activeForm = null;        
        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read" };
        IPublicClientApplication app =  Program.PublicClientApp;
        AuthenticationResult authResult = null;
        List<OwnerDetailsModel> owner = new List<OwnerDetailsModel>();
        public MainScreen()
        {
            InitializeComponent();
        }

        private void openChildForm(Form childForm)
        {
            if (activeForm != null)
            {
                activeForm.Close();
            }

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel.Controls.Add(childForm);
            panel.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
     
        private void MainScreen_Load(object sender, EventArgs e)
        {
        
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void checkVoilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openChildForm(new Form1(owner));
        }

        private void scanLocalFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openChildForm(new ScanFiles());
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logDirectoryPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Log";
            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }
            Process.Start(logDirectoryPath);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string target = "http://www.google.com";
            try
            {
                System.Diagnostics.Process.Start(target);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

    
        private async void AuthenticateUser()
        {
            try
            {
                authResult = await app.AcquireTokenInteractive(scopes).WithAccount(null).WithPrompt(Prompt.SelectAccount).ExecuteAsync();
                if (authResult != null)
                {
                    ManupulateGraphAPIData(authResult);
                }
                else
                {
                    MessageBox.Show("Authentication failed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occured in authentication for new account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<OwnerDetailsModel> CallGraphAPI(string accessToken)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = null;
            OwnerDetailsModel obj = new OwnerDetailsModel();
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, graphAPIEndpoint);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                obj = JsonConvert.DeserializeObject<OwnerDetailsModel>(content);
                return obj;
            }
            catch (Exception ex)
            {
                return obj;
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
                if (httpClient != null)
                {
                    httpClient.Dispose();
                }
            }
        }

        private async void ManupulateGraphAPIData(AuthenticationResult authResult)
        {
            OwnerDetailsModel graphAPIResponse = await CallGraphAPI(authResult.AccessToken);
            if (graphAPIResponse != null)
            {
                owner = new List<OwnerDetailsModel>
                {
                    graphAPIResponse
                };
                MessageBox.Show("User Login Successfully..!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                owner = new List<OwnerDetailsModel>();
                MessageBox.Show("User Login Failed..!\n Please try again", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
               
        private async void Signin()
        {
            var accounts = await app.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();
            if (firstAccount != null)
            {
                //try
                //{
                //    authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();
                //    if (authResult != null)
                //    {
                //        ManupulateGraphAPIData(authResult);
                //    }
                //    else
                //    {
                //        MessageBox.Show("Authentication failed " + firstAccount.Username, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("An exception occurred in authentication for the current account " + firstAccount.Username, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //}

                await app.RemoveAsync(accounts.FirstOrDefault());
                owner = new List<OwnerDetailsModel>();
            }

            AuthenticateUser();
        }

        private async void SignOut()
        {
            var accounts = await app.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await app.RemoveAsync(accounts.FirstOrDefault());
                    owner = new List<OwnerDetailsModel>();
                    MessageBox.Show("Sign Out Successfully..!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (MsalException ex)
                {
                    MessageBox.Show($"Error signing-out user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void signInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Signin();
        }

        private void signOutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SignOut();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create ApplicationInfo, setting the clientID from Azure AD App Registration as the ApplicationId
            // If any of these values are not set API throws BadInputException.
            ApplicationInfo appInfo = new ApplicationInfo()
            {
                // ApplicationId should ideally be set to the same ClientId found in the Azure AD App Registration.
                // This ensures that the clientID in AAD matches the AppId reported in AIP Analytics.
                ApplicationId = clientId,
                ApplicationName = appName,
                ApplicationVersion = appVersion
            };
            // Initialize Action class, passing in AppInfo.
            Action action = new Action(appInfo);
            // List all labels available to the engine created in Action
            IEnumerable<Microsoft.InformationProtection.Label> labels = action.ListLabels();
            // Enumerate parent and child labels and print name/ID. 
            List<ClassificationDetails> cd = new List<ClassificationDetails>();
            List<ClassificationDetails> cdlist = db.ClassificationDetails.OrderBy(x => x.Name).ToList();
            foreach (var label in labels)
            {
                if (label.Children.Count > 0)
                {
                    foreach (Microsoft.InformationProtection.Label child in label.Children)
                    {
                        if (cdlist.Count > 0)
                        {
                            if (!cdlist.Any(x => x.Id == child.Id))
                            {
                                ClassificationDetails obj = new ClassificationDetails();
                                obj.Id = child.Id;
                                obj.Name = label.Name.ToUpper() + "-" + child.Name.ToUpper();
                                cd.Add(obj);
                            }
                        }
                        else
                        {
                            ClassificationDetails obj = new ClassificationDetails();
                            obj.Id = child.Id;
                            obj.Name = label.Name.ToUpper() + " - " + child.Name.ToUpper();
                            cd.Add(obj);
                        }
                    }
                }
            }

            if (cd.Count > 0)
            {
                using (var ctx = new DataContext())
                {
                    using (DbContextTransaction tran = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            ctx.ClassificationDetails.AddRange(cd);
                            ctx.SaveChangesAsync();
                            tran.Commit();
                            MessageBox.Show($"successfully updated {cd.Count} new lables.!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No more new lables.!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
