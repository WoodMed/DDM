using System;

using System.Web;
using System.Web.Security;

using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Configuration;
using DevExpress.XtraRichEdit.Model;
using System.Runtime.Remoting.Contexts;

public partial class SignInModule : System.Web.UI.Page {
    protected void Page_Load(object sender, EventArgs e) {
        if(!IsPostBack) HelperClass.ClearAllCookies(HttpContext.Current);
    }

    protected void SignInButton_Click(object sender, EventArgs e)
    {
        // Get the user's email and password from the form controls
        string username = SignInUserTextBox.Text.Trim();
        string password = PasswordButtonEdit.Text.Trim();
        int intRememberMe = (int)RememberMeCheckBox.CheckState;

        string command = String.Format("<authenticate login = \"{0}\" password = \"{1}\" />", username, password);

        // Execute command
        string result = HelperClass.ExecuteCommandResponse(command, "users");
        Debug.WriteLine("Response received: " + result);

        // HANDLE RESULT //
        string state = "failure"; //Failure by default
        XmlDocument xmlResponse = new XmlDocument();
        xmlResponse.LoadXml(result);
        XmlNode statusNode = xmlResponse.SelectSingleNode("/status");
        if (statusNode != null && statusNode.Attributes["state"] != null)
        {
            state = statusNode.Attributes["state"].Value;
            Debug.WriteLine("Status State: " + state);
        }
        else
        {
            Debug.WriteLine("Status state attribute not found.");

        }


        if (state == "success")
        {

            // Get Session id from login response
            XmlNode sessionNode = xmlResponse.SelectSingleNode("//session");
            string sessionID = sessionNode.InnerText;
            HttpContext.Current.Session["session_id"] = sessionID;
            Debug.WriteLine("Session ID: " + sessionID);

            // Email retrieval commnad
            command = String.Format("<searchusers>\r\n<authentication token='{0}'/>\r\n<user loginname='{1}'/>\r\n<workspace id='16713'/>\r\n</searchusers>", sessionID, username);
            Debug.WriteLine("Sending Command to Fusion Live API...");
            string emailResult = HelperClass.ExecuteCommandResponse(command, "users");

            Debug.WriteLine("Email received: " + emailResult);
            XmlDocument emailResponse = new XmlDocument();
            emailResponse.LoadXml(emailResult);
            XmlNode emailNode = emailResponse.SelectSingleNode("//user");
            string email = "";
            string userid = "";
            if (emailNode != null && emailNode.Attributes["email"] != null)
            {
                email = emailNode.Attributes["email"].Value;
                Debug.WriteLine("Email retrieved State: " + email);
                userid = emailNode.Attributes["id"].Value;
            }

            // retrieveUserFromDB(username, email);

            // Cookie Creation

            HttpCookie sessionCookie = new HttpCookie("session_id", sessionID);
            sessionCookie.Expires = DateTime.Now.AddHours(HelperClass.CookieExpiryTime);
            Response.Cookies.Add(sessionCookie);

            HttpCookie sessionCookieUser = new HttpCookie("session_user", username);
            sessionCookieUser.Expires = DateTime.Now.AddHours(HelperClass.CookieExpiryTime);
            Response.Cookies.Add(sessionCookieUser);

            HttpCookie sessionCookieEmail = new HttpCookie("session_email", email);
            sessionCookieEmail.Expires = DateTime.Now.AddHours(HelperClass.CookieExpiryTime);
            Response.Cookies.Add(sessionCookieEmail);

            HttpCookie sessionCookieUserID = new HttpCookie("session_userID", userid);
            sessionCookieUserID.Expires = DateTime.Now.AddHours(HelperClass.CookieExpiryTime);

            Response.Cookies.Add(sessionCookieUserID);
            HttpContext.Current.Session["IsAuthenticated"] = true;
            Response.Cookies.Add(sessionCookie);

            Response.Redirect("~/DistributionMatrix.aspx");
        }
        else
        {
            // Set the error message
            ErrorMessageLabel.Text = "Provided password is incorrect.";
            ErrorMessageLabel.Visible = true;
        }

    }


    public void KeepUserLoggedIn(string username)
    {
        FormsAuthentication.SetAuthCookie(username, true);
    }


    protected void forgotPasswordButton_Click(object sender, EventArgs e)
    {
        // Redirect the user to the forgot password page
        //Server.Transfer("~/Account/forgotPassword.aspx");
        Server.Transfer("https://us.fusion.live/resources/jsps/pol/login/Logon.jsp#");
    }

    private List<string> GetUserRoles(SqlConnection connStr, string email)
    {
        List<string> roles = new List<string>();
        using (SqlCommand cmd = new SqlCommand("sp_GetUserRolesByEmail", connStr))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@Email", SqlDbType.NChar, 100).Value = email;

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    roles.Add(reader.GetString(0));
                }
            }
        }
        return roles;
    }

    public void loginDateTime(string username)
    {
        try
        {
            var connStr = WebConfigurationManager.ConnectionStrings["csMTR"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("UpdateLastLogin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            loginLabel.Text = ex.Message;
        }
    }

}
