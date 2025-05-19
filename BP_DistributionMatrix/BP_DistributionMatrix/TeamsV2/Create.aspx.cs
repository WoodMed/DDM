using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BP_DistributionMatrix.Model;
using DevExpress.Web;

namespace BP_DistributionMatrix {
    public partial class Create : System.Web.UI.Page {


        public class MemberData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }

        }

        Teams_Dal _dal;
        List<TeamUser_Model> _availableUsers;
        List<Tuple<string, string>> _teamUsers;
        int _userId;
        List<MemberData> _selectedUsers;
        protected void Page_Load(object sender, EventArgs e)
        {
            _dal = new Teams_Dal();
            // Get UserId
            if (!IsPostBack)
            {
                Debug.WriteLine("hII"); 
            }
            HttpCookie userIdCookie = HelperClass.GetCookie("session_userId");
            _userId = int.Parse(userIdCookie.Value);
            TeamErrorLabel.Visible = false;
            if (!IsPostBack)
            {
            }

            List<MemberData> _selectedUsers = new List<MemberData>();

            if (Session["TeamUsers"] != null)
            {
                _selectedUsers = (List<MemberData>)Session["TeamUsers"];
                TeamsGrid.DataSource = _selectedUsers;
                TeamsGrid.DataBind();
            }

            // bind popup
            BindAvailableUsers();


        }

        private void BindAvailableUsers()
        {

            // Get all available users from the database
            _availableUsers = _dal.GetAllUsers(_userId);

            // Bind the data source
            listAvailableUsers.DataSource = _availableUsers;
            listAvailableUsers.ValueField = "Id";
            listAvailableUsers.TextField = "PopupDisplay";
            listAvailableUsers.DataBind();

            // Pre-select users that exist in the session
            if (_selectedUsers != null)
            {
                foreach (ListEditItem item in listAvailableUsers.Items)
                {
                    int itemId = Convert.ToInt32(item.Value);
                    if (_selectedUsers.Any(u => u.Id == itemId))
                    {
                        item.Selected = true; // Mark as selected
                    }
                }
            }
        }

        protected void CreateTeamBtn_Click(object sender, EventArgs e)
        {

            var test = TeamInput.Text;
            if (string.IsNullOrWhiteSpace(test))
            {
                TeamErrorLabel.Visible = true;
                return;
            }

            List<Tuple<int, string>> TeamMembers = new List<Tuple<int, string>>
            {
                new Tuple<int, string>(_userId, "Leader")
            };

            _selectedUsers = (List<MemberData>)Session["TeamUsers"];

            foreach (var member in _selectedUsers)
            {
                TeamMembers.Add(new Tuple<int, string>(member.Id, member.Role));
            }

            _dal.CreateTeam(TeamMembers, TeamInput.Text);

            Debug.WriteLine("");
            Response.Redirect("~/TeamsV2/List.aspx");
        }

        protected void btnAddSelectedMembers_Click(object sender, EventArgs e)
        {

            List<MemberData> teamUsersDataSource = new List<MemberData>();

            foreach (ListEditItem item in listAvailableUsers.Items)
            {
                if (item.Selected)
                {
                    teamUsersDataSource.Add(new MemberData
                    {
                        Id = int.Parse(item.Value.ToString()),
                        Name = item.Text,
                        Role = "Member"
                    });
                }
            }

            Session["TeamUsers"] = teamUsersDataSource;
            TeamsGrid.DataSource = teamUsersDataSource;
            TeamsGrid.DataBind();

            popupAddMembers.ShowOnPageLoad = false;
        }


        protected void gridTeamMembers_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
        {
            if (e.ButtonID == "Remove")
            {
                List<MemberData> teamUsersDataSource = (List<MemberData>)Session["TeamUsers"];
                object keyValue = TeamsGrid.GetRowValues(e.VisibleIndex, "Id");

                if (keyValue != null)
                {
                    int idToRemove = Convert.ToInt32(keyValue);


                    teamUsersDataSource.RemoveAll(x => x.Id == idToRemove);
                    Session["TeamUsers"] = teamUsersDataSource;

                    TeamsGrid.DataSource = teamUsersDataSource;
                    TeamsGrid.DataBind();
                }
            }


            if (e.ButtonID == "Promote")
            {
                // Retrieve the session data
                List<MemberData> teamUsersDataSource = (List<MemberData>)Session["TeamUsers"];

                // Get the row ID and current role
                object keyValue = TeamsGrid.GetRowValues(e.VisibleIndex, "Id");
                object roleValue = TeamsGrid.GetRowValues(e.VisibleIndex, "Role");

                if (keyValue != null && roleValue != null)
                {
                    int idToUpdate = Convert.ToInt32(keyValue);
                    string currentRole = roleValue.ToString();

                    // Find the member in the session list
                    var member = teamUsersDataSource.FirstOrDefault(m => m.Id == idToUpdate);
                    if (member != null)
                    {
                        // Toggle role
                        member.Role = (currentRole == "Member") ? "Leader" : "Member";

                        // Update session variable
                        Session["TeamUsers"] = teamUsersDataSource;

                        // Rebind the grid to reflect changes
                        TeamsGrid.DataSource = teamUsersDataSource;
                        TeamsGrid.DataBind();
                    }
                }
            }
        }

        protected void gridTeamMembers_CustomButtonInitialize(object sender, ASPxGridViewCustomButtonEventArgs e)
        {
            if (e.ButtonID == "Promote")
            {
                // Get the role of the current row
                object roleValue = ((ASPxGridView)sender).GetRowValues(e.VisibleIndex, "Role");

                if (roleValue != null)
                {
                    string currentRole = roleValue.ToString();
                    e.Text = (currentRole == "Member") ? "Promote to Leader" : "Demote to Member";
                }
            }
        }

        protected void callbackPanel_Callback(object sender, CallbackEventArgsBase e)
        {
        }
    }
}