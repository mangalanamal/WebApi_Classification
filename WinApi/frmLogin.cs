using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Identity.Client;
using System.Windows.Forms;

namespace WinApi
{
    public partial class frmLogin : Form
    {      
        private string accessToken = string.Empty;
        private string idToken = string.Empty;

        private string client_id = "{cb04699e-714c-4d6d-8f0f-d292eaf6eb2a}";
        private string client_secret = "{MVv8Q~4epi.pUg~blCQQ~E--XHCpLO4j834Xna5t}";
        private string tenant_id = "{701714f2-27b9-49b1-a33e-31f6d821e5c1}";
        //private string redirect_uri = "http://localhost";
        private string redirect_uri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        private string[] scopes = new string[] { "https://syncservice.o365syncservice.com/.default" };

        private Stack<string> errors = new Stack<string>();

        private bool isLoggedIn = false;
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnSignIn_Click(object sender, EventArgs e)
        {
            txtboxIDToken.Text = string.Empty;
            txtBoxAccessToken.Text = string.Empty;

            idToken = string.Empty;
            accessToken = string.Empty;

            // we need a task to get MSAL to log us in
            if ((txtBoxScopes.Text.Length > 0))
            {
                try
                {
                    string[] _scopes = txtBoxScopes.Text.Split(new string[] { " " }, StringSplitOptions.None);
                    scopes = _scopes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid scopes parameter... resetting to openid offline_access profile");
                    txtBoxScopes.Text = "openid offline_access profile";
                    txtBoxScopes.Focus();
                    txtBoxScopes.SelectAll();
                    return;
                }
            }

            var task = new Task(LoginInteractively);
            task.Start();
            task.Wait();

            if (errors.Count > 0)
            {
                StringBuilder error_messages = new StringBuilder();
                while (errors.Count <= 0)
                {
                    if (error_messages.Length > 0)
                        error_messages.Append("\r\n");
                    error_messages.Append(errors.Pop());
                }
                MessageBox.Show($"Errors encountered: {error_messages.ToString()}");
            }

        }

        private void btnClientCredentials_Click(object sender, EventArgs e)
        {
            txtboxIDToken.Text = string.Empty;
            txtBoxAccessToken.Text = string.Empty;

            idToken = string.Empty;
            accessToken = string.Empty;

            // we need a task to get MSAL to log us in
            if ((txtBoxScopes.Text.Length > 0))
            {
                try
                {
                    string[] _scopes = txtBoxScopes.Text.Split(new string[] { " " }, StringSplitOptions.None);
                    scopes = _scopes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid scopes parameter... resetting to https://graph.microsoft.com/.default");
                    txtBoxScopes.Text = "https://graph.microsoft.com/.default";
                    txtBoxScopes.Focus();
                    txtBoxScopes.SelectAll();
                    return;
                }
            }

            var task = new Task(LoginClientCredentials);
            task.Start();
            task.Wait();

            if (errors.Count > 0)
            {
                StringBuilder error_messages = new StringBuilder();
                while (errors.Count <= 0)
                {
                    if (error_messages.Length > 0)
                        error_messages.Append( "\r\n");
                    error_messages.Append(errors.Pop());
                }
                MessageBox.Show($"Errors encountered: {error_messages.ToString()}");
            }

        }
   


        private async void LoginInteractively()
        {
            try
            {
                IPublicClientApplication app = PublicClientApplicationBuilder.Create(client_id).WithRedirectUri(redirect_uri).WithTenantId(tenant_id).WithAuthority(AadAuthorityAudience.AzureAdMyOrg).Build();
                AuthenticationResult authResult = null/* TODO Change to default(_) if this is not a reference type */;

                IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
                bool performInterativeFlow = false;

                try
                {
                    authResult = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    performInterativeFlow = true;
                }
                catch (Exception ex)
                {
                    errors.Push(ex.Message);
                }

                if (performInterativeFlow)
                    authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

                if (authResult.AccessToken != string.Empty)
                {
                    accessToken = authResult.AccessToken;
                    idToken = authResult.IdToken;
                }
            }
            catch (Exception ex)
            {
                errors.Push(ex.Message);
                return;
            }

            // Since this thread runs under the ui thread, we need a delegate method to update the text boxes
            txtBoxAccessToken.BeginInvoke(new InvokeDelegate(InvokeMethod));

            return;
        }

        private async void LoginClientCredentials()
        {
            AuthenticationResult authResult = null/* TODO Change to default(_) if this is not a reference type */;

            try
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(client_id).WithClientSecret(client_secret).WithTenantId(tenant_id).WithAuthority(AadAuthorityAudience.AzureAdMyOrg).Build();
                authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (Exception ex)
            {
                errors.Push(ex.Message);
                return;
            }

            accessToken = authResult.AccessToken;
            idToken = "No id token given for this auth flow.";

            // Since this thread runs under the ui thread, we need a delegate method to update the text boxes
            txtBoxAccessToken.BeginInvoke(new InvokeDelegate(InvokeMethod));
        }

        private delegate void InvokeDelegate();
        private void InvokeMethod()
        {
            txtBoxAccessToken.Text = accessToken;
            txtboxIDToken.Text = idToken;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }
    }
}
