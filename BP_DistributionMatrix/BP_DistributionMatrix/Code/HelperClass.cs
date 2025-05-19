using DevExpress.DocumentServices.ServiceModel.DataContracts;
using DevExpress.Xpo.DB.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;

/// <summary>
/// Summary description for HelperClass
/// </summary>
public static class HelperClass
{
    public class UserAction
    {
        public string Username { get; set; }
        public string DocType { get; set; }
        public string Discipline { get; set; }
        public string Action { get; set; }
        public int UserId { get; set; }
        public string DateInserted { get; set; }
        public string Folder { get; set; }
    }

    public static int CookieExpiryTime = 24;
    public static string VerifySessionID()
    {
        string state = "failure";

        Debug.WriteLine("Verifying Current Session ID...");
        HttpContext context = HttpContext.Current;
        string sessionID = "";
        if (context != null)
        {
            string currentUrl = context.Request.Url.AbsolutePath;
            if (currentUrl.EndsWith("/Account/SignIn.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return "failure"; // Exit the method if already on the Sign-In page
            }

            HttpCookie sessionCookie = context.Request.Cookies["session_id"];

            if (sessionCookie != null)
            {
                sessionID = sessionCookie.Value;
                Debug.WriteLine("Retrieved Session ID: " + sessionID);
            }
            else
            {
                context.Response.Redirect("~/Account/SignIn.aspx");
            }
        }
        else
        {
            throw new InvalidOperationException("HttpContext is not available.");

        }

        string command = String.Format("<listworkspaces>\r\n<authentication token=\'{0}\'/>\r\n</listworkspaces>", sessionID);
        Debug.WriteLine("Sending Command to Fusion Live API...");



        for(int i = 0; i < 3; i++) // Check 3 times to avoid FL fail with a valid session id
        {
            string result = ExecuteCommandResponse(command, "users");
            Debug.WriteLine("Response Received " + result);

            // string state = "failure"; //Failure by default
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

            if (state != "failure")
            {
                break;
            }
        }

        if(state == "failure") context.Response.Redirect("~/Account/SignIn.aspx");

        return state;

    }

    public static string ExecuteCommandResponse(string command, string category)
    {
        string result = "";

        try
        {

            //var commandName = wsname.Split('/')[0];

            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, chain, sslPolicyErrors) => true
            };

            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(2) })
            {
                var url = "https://us.fusion.live/pws/" + category;
                var content = new StringContent(command, System.Text.Encoding.UTF8, "application/xml");
                
                Debug.WriteLine("Sending request to: " + url);

                string pattern = @"(password\s*=\s*"")[^""]*("")";
                string replacement = "$1****$2";
                string obfuscatedCommand = Regex.Replace(command, pattern, replacement);

                Debug.WriteLine("Request data: " + obfuscatedCommand);

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                var response = client.SendAsync(request).Result;

                System.Diagnostics.Debug.WriteLine("Response received");

                response.EnsureSuccessStatusCode(); // Throws an exception if the status code is not successful
                result = response.Content.ReadAsStringAsync().Result;
                System.Diagnostics.Debug.WriteLine("Response content: " + result);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
        }

        return result;
    }

    public static void ClearAllCookies(HttpContext context)
    {
        foreach (string cookieName in context.Request.Cookies.AllKeys)
        {
            // Create a new cookie with the same name
            HttpCookie cookie = new HttpCookie(cookieName)
            {
                Expires = DateTime.Now.AddDays(-1) // Set expiration to a past date
            };

            // Add the expired cookie to the response to remove it from the client
            context.Response.Cookies.Add(cookie);
        }

    }

    public static HttpCookie GetCookie(string cookieName)
    {
        HttpContext context = HttpContext.Current;
        HttpCookie res = HttpContext.Current.Request.Cookies[cookieName];

        if(res == null || string.IsNullOrEmpty(res.Value) )
        {
            context.Response.Redirect("~/Account/SignIn.aspx");
            return null;
        }

        return res;
    }

    public static string GetSessionID()
    {
        HttpContext context = HttpContext.Current;
        string sessionID = "";

        HttpCookie sessionCookie = context.Request.Cookies["session_id"];

        if (sessionCookie != null)
        {
            HttpContext.Current.Session["IsAuthenticated"] = true;
            sessionID = sessionCookie.Value;
            Debug.WriteLine("Retrieved Session ID: " + sessionID);
        }

        return sessionID;
    }

    public static List<string[]> Get_Directories()
    {
        Debug.WriteLine("Getting Directories...");

        // Exclude These Folders
        HashSet<string> ExcludeSet = new HashSet<string>
        {
            "02 Project Mgmt - General",
            "AECOM",
            "BASF",
            "BP Documents",
            "DNV",
            "WORLEY H2T",
            "AKER",
            "ARUP",
            "BP",
            "PX Ltd",
            "AKER CC"
        };

        string sessionID = HelperClass.GetSessionID();
        string initialCommand = String.Format("<listfolders mode =\"list\">\r\n<authentication token=\'{0}\'/>\r\n<workspace id=\"16713\" />\r\n</listfolders>", sessionID);
        string response = HelperClass.ExecuteCommandResponse(initialCommand, "folders");

        List<string[]> folderData = read_folderXml(response, "null");
        var folderStack = new Stack<string[]>();
        foreach (string[] topFolder in folderData)
        {
            Debug.WriteLine("Folder: " + string.Join(", ", topFolder));

            if (ExcludeSet.Contains(topFolder[2])) continue; // Exclude unwanted folder

            folderStack.Push(topFolder);
        }

        while (folderStack.Count > 0)
        {
            var curFolder = folderStack.Pop(); // This corresponds to a "Row" inside the response

            if (curFolder[3] == "false") // If hasChildren is false
            {
                continue;
            }

            string listFolderCommand = String.Format("<listfolders mode =\"list\">\r\n<authentication token=\'{0}\'/>\r\n<workspace id=\"16713\" />\r\n<parent id=\"{1}\" />\r\n</listfolders>", sessionID, curFolder[1]);
            response = ExecuteCommandResponse(listFolderCommand, "folders");

            List<string[]> readXml = read_folderXml(response, curFolder[1]);
            foreach (string[] folders in readXml)
            {
                if (ExcludeSet.Contains(folders[2])) continue; // Skip unwanted folder

                Debug.WriteLine("Adding Folder: " + string.Join(", ", folders));
                string[] dataPoint = new string[] { folders[0], folders[1], folders[2], folders[3] };
                // TODO Don't add folder if name is in excluded
 

                folderData.Add(dataPoint);
                folderStack.Push(dataPoint);
            }
        }

        //LOGGING
        string foldersLog = "";
        foreach (var item in folderData)
        {
            foldersLog += string.Format("[ParentId: {0}, FolderId: {1}, FolderName: {2}]\n", item[0], item[1], item[2]);
        }
        // Write the string to the file
        Debug.WriteLine("Folders Log:" + foldersLog);
 

        return folderData;

    }

    public static List<string[]> read_folderXml(string xml, string parentId)
    {
        XmlDocument newDoc = new XmlDocument();
        newDoc.LoadXml(xml);
        XmlNodeList fNodes = newDoc.SelectNodes("//folders/folder");
        var res = new List<string[]>();
        foreach (XmlNode folder in fNodes)
        {
            string folderId = folder.Attributes["id"].InnerText;
            string folderName = folder.Attributes["name"].InnerText;
            if (folderName == "02 Project Mgmt - General") continue;
            string hasChildren = folder.Attributes["haschildren"].Value == "true" ? "true" : "false";

            res.Add(new string[] { parentId, folderId, folderName , hasChildren});
        }

        return res;
    }

    public static List<string[]> read_docXml(string xml)
    {
        XmlDocument newDoc = new XmlDocument();
        newDoc.LoadXml(xml);
        XmlNodeList dNodes = newDoc.SelectNodes("//documents/document");
        var res = new List<string[]>();
        foreach (XmlNode document in dNodes)
        {
            string docId = document.Attributes["id"].InnerText;
            string docName = document.Attributes["reference"].InnerText;
            string docTitle = document.Attributes["title"].InnerText;

            string[] parts = docName.Split('-');
            string docDisc = parts[1];
            string docType = parts[2];


            res.Add(new string[] {docId, docName, docTitle, docType, docDisc});
        }

        return res;
    }

    public static List<string[]> Get_Documents(string folderId)
    {
        string sessionID = HelperClass.GetSessionID();
        string listDocsCommand = String.Format("<listdocuments mode =\"list\">\r\n<authentication token=\'{0}\'/>\r\n<workspace id=\"16713\" />\r\n<parent id=\"{1}\" />\r\n</listdocuments>", sessionID, folderId);
        string response = ExecuteCommandResponse(listDocsCommand, "folders");

        List<string[]> xmlRes = read_docXml(response);

        Debug.WriteLine("Docs Retrieved!");

        return xmlRes;

    }

    public static List<string[]> Get_DocTypeDisc_Combo(string folderId)
    {
        string sessionID = HelperClass.GetSessionID();
        string listDocsCommand = String.Format("<listdocuments mode =\"list\">\r\n<authentication token=\'{0}\'/>\r\n<workspace id=\"16713\" />\r\n<parent id=\"{1}\" />\r\n</listdocuments>", sessionID, folderId);
        string response = ExecuteCommandResponse(listDocsCommand, "folders");

        List<string[]> xmlRes = read_docXml(response);

        Dictionary<Tuple<string, string>, string> ActionMap = new Dictionary<Tuple<string, string>, string>();
        List<string[]> res = new List<string[]>();

        foreach (string[] document in xmlRes)
        {
            string docType = document[3];
            string disc = document[4];
            Tuple<string, string> comboTuple = new Tuple<string, string>(docType, disc);

            if (!ActionMap.ContainsKey(comboTuple))
            {
                string action = Get_Action(docType, disc);
                ActionMap.Add(comboTuple, action);
                res.Add(new string[] { docType, disc, action });
            }

            Debug.WriteLine("Docs Retrieved!");
        }

        return res;
    }

    public static List<string[]> Get_LocalData(string folderId, string username, string userid)
    {
        List<string[]> xmlRes = Query_LocalData(userid, folderId);
        if (xmlRes == null)
        {
            string sessionID = HelperClass.GetSessionID();
            string listDocsCommand = String.Format("<listdocuments mode =\"list\">\r\n<authentication token=\'{0}\'/>\r\n<workspace id=\"16713\" />\r\n<parent id=\"{1}\" />\r\n</listdocuments>", sessionID, folderId);
            string response = ExecuteCommandResponse(listDocsCommand, "folders");

            xmlRes = read_docXml(response);
            // TODO: Save xmlRes into SQL

            Debug.WriteLine("Saving FL Fetch into local table...");
            foreach (string[] rowVals in xmlRes)
            {
                string docType = rowVals[0];
                string discipline = rowVals[1];
                string action = rowVals[2];
                string currentDate = DateTime.Now.ToString("dd/MM/yyyy");

                Save_Action(docType, discipline, action, username, userid, currentDate, folderId);
            }
            Debug.WriteLine("Data stored!");
        }

        Dictionary<Tuple<string, string>, string> ActionMap = new Dictionary<Tuple<string, string>, string>();
        List<string[]> res = new List<string[]>();

        foreach (string[] document in xmlRes)
        {
            string docType = document[3];
            string disc = document[4];
            Tuple<string, string> comboTuple = new Tuple<string, string>(docType, disc);

            if (!ActionMap.ContainsKey(comboTuple))
            {
                string action = Get_Action(docType, disc);
                ActionMap.Add(comboTuple, action);
                res.Add(new string[] { docType, disc, action });
            }

            Debug.WriteLine("Docs Retrieved!");
        }

        return res;
    }

    public static List<string[]> Query_LocalData(string userid, string folderId)
    {
        string checkQuery = @"SELECT TOP(1) 1 -- Return 1 if a match is found
                        FROM [
].[dbo].[UserActions]
                        WHERE [UserId] = @UserId
                          AND [FolderId] = @FolderId;SELECT TOP(1) 1 -- Return 1 if a match is found
                        FROM [HCCUK_Fusion_DDM].[dbo].[UserActions]
                        WHERE [UserId] = @UserId
                          AND [FolderId] = @FolderId;";

        string retrieveQuery = @"SELECT * 
                                 FROM [HCCUK_Fusion_DDM].[dbo].[UserActions] 
                                 WHERE [UserId] = @UserId 
                                   AND [FolderId] = @FolderId;";

        string connStr = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(connStr))
        {
            using (SqlCommand cmd = new SqlCommand(checkQuery, connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userid);
                cmd.Parameters.AddWithValue("@FolderId", folderId);

                connection.Open();

                object result = cmd.ExecuteScalar();
                connection.Close();

                if (result == null)
                {
                    Debug.WriteLine("No match found in SQL retrieving from Fusion Live...");
                    return null;
                }
            }

            using (SqlCommand cmd = new SqlCommand(retrieveQuery, connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userid);
                cmd.Parameters.AddWithValue("@FolderId", folderId);
                List<string[]> userActions = new List<string[]>();
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] row = new string[7];
                        row[0] = reader["Username"].ToString();
                        row[1] = reader["DocType"].ToString();
                        row[2] = reader["Discipline"].ToString();
                        row[3] = reader["Action"].ToString();
                        row[4] = reader["UserId"].ToString();
                        row[5] = reader["DateInserted"].ToString();
                        row[6] = reader["FolderId"].ToString();

                        userActions.Add(row);

                    }

                }

                connection.Close();
                Debug.WriteLine("Data retrieved from local DB!");
                return userActions;

            }



        }
    }

    public static string Get_Action(string docType, string disc)
    {

        string query = @"SELECT TOP(1) dur.[value]
                        FROM [HCCUK_Fusion_DDM].[ddm].[ddm_docType_user_rels] dur
                        JOIN [HCCUK_Fusion_DDM].[ddm].[ddm_disc_docType] dt ON dur.doc_type_disc_id = dt.id
                        WHERE dur.[user_id] = 342
                            AND dt.[doc_code] = @docType
                            AND dt.[disc_code] = @disc";

        string connStr = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(connStr))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@docType", docType);
                cmd.Parameters.AddWithValue("@disc", disc);

                connection.Open();

                object result = cmd.ExecuteScalar();
                connection.Close();
                string value = result != null ? result.ToString() : null;
                Debug.WriteLine("Action Retrieved " + value);

                return value;
            }

        }
    }

    public static void Save_Action(string docType, string discipline, string action, string username, string userid, string currentDate, string folderid)
    {

        //TODO: Save date
        string query = @"MERGE INTO UserActions AS target
                            USING (SELECT @Username AS Username,
                                          @DocType AS DocType,
                                          @Discipline AS Discipline,
                                          @Action AS Action,
                                          @UserId AS UserId,
                                          @DateInserted as DateInserted,
                                          @FolderId as FolderId) AS source
                            ON target.UserId = source.UserId
                               AND target.DocType = source.DocType
                               AND target.Discipline = source.Discipline
                               AND target.FolderId = source.FolderId
                            WHEN MATCHED THEN
                                UPDATE SET target.Action = source.Action
                            WHEN NOT MATCHED THEN
                                INSERT (Username, DocType, Discipline, Action, UserId, DateInserted, FolderId)
                                VALUES (source.Username, source.DocType, source.Discipline, source.Action, source.UserId, source.DateInserted, source.FolderId);";

        string connStr = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(connStr))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@Username", username);
                Debug.WriteLine("UserId: " + userid);
                cmd.Parameters.AddWithValue("@UserId", userid);
                cmd.Parameters.AddWithValue("@DocType", docType);
                cmd.Parameters.AddWithValue("@Discipline", discipline);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@DateInserted", currentDate);
                cmd.Parameters.AddWithValue("@FolderId", folderid);

                connection.Open();

                // Write
                try
                {
                    int affectedRows = cmd.ExecuteNonQuery();
                    Debug.WriteLine("Rows affected: " + affectedRows + ". Action '" + action + "' saved successfully.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error saving action: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }


            }

        }
    }

    // TODO: Complete get method
    public static void Get_Data()
    {
        return;
    }

}