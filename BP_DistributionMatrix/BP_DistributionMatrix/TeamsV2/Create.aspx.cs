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
            HttpCookie userIdCookie = HelperClass.GetCookie("session_userId");
            _userId = int.Parse(userIdCookie.Value);
            TeamErrorLabel.Visible = false;


            _selectedUsers = new List<MemberData>();

            // Get selected users from Session ID if it exists
            if (Session["TeamUsers"] != null)
            {
                _selectedUsers = (List<MemberData>)Session["TeamUsers"];
                TeamsGrid.DataSource = _selectedUsers;
                TeamsGrid.DataBind();
            }

            // bind add members popup
            BindAvailableUsers();
        }



        // Bind available users to addmembers list box popup
        private void BindAvailableUsers()
        {

            // Get all available users from the database
            _availableUsers = _dal.GetAllUsers(_userId);

            // Bind the data source
            listAvailableUsers.DataSource = _availableUsers;
            listAvailableUsers.ValueField = "Id";
            listAvailableUsers.TextField = "PopupDisplay";
            listAvailableUsers.DataBind();

        }

        // Finalise team creation
        protected void CreateTeamBtn_Click(object sender, EventArgs e)
        {

            // First check if we have a team name
            var test = TeamInput.Text;
            if (string.IsNullOrWhiteSpace(test))
            {
                TeamErrorLabel.Text = "Please enter a value";
                TeamErrorLabel.Visible = true;
                return;
            }

            // Add the current user as the team leader
            List<Tuple<int, string>> TeamMembers = new List<Tuple<int, string>>
            {
                new Tuple<int, string>(_userId, "Leader")
            };

            // Add all selected users to the team members list
            if (Session["TeamUsers"] != null)
            {
                _selectedUsers = (List<MemberData>)Session["TeamUsers"];
                foreach (var member in _selectedUsers)
                {
                    TeamMembers.Add(new Tuple<int, string>(member.Id, member.Role));
                }
            }

            // We attempt to create the team, return false on fail
            bool created = _dal.CreateTeam(TeamMembers, TeamInput.Text);

            if (!created)
            {
                TeamErrorLabel.Text = "Team name already taken, please type a new one";
                TeamErrorLabel.Visible = true;
                return;
            }

            // redirect back to the teams list page
            Response.Redirect("~/TeamsV2/List.aspx");
        }

        // Add selected members from the popup listbox into the main gridview
        protected void btnAddSelectedMembers_Click(object sender, EventArgs e)
        {

            List<MemberData> teamUsersDataSource = new List<MemberData>();

            // Retrieve all selected users from the popup list box
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

            // Little detour to ensure that roles are preserved
            if (_selectedUsers != null)
            {
                var selectedUsersMap = _selectedUsers.ToDictionary(user => user.Id);

                foreach (var user in teamUsersDataSource)
                {
                    if (!selectedUsersMap.ContainsKey(user.Id))
                    {
                        _selectedUsers.Add(user);
                    }
                }

                _selectedUsers = _selectedUsers.Where(user => teamUsersDataSource.Any(tu => tu.Id == user.Id)).ToList();
                teamUsersDataSource = new List<MemberData>(_selectedUsers);
            }

            // Bind the data to the main grid view
            Session["TeamUsers"] = teamUsersDataSource;
            TeamsGrid.DataSource = teamUsersDataSource;
            TeamsGrid.DataBind();

            popupAddMembers.ShowOnPageLoad = false;
        }

        // Callback method for main grid view custom buttons
        protected void gridTeamMembers_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
        {
            // Remove a member from the team
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

            // Promote a member Member to Leader or vice versa
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

        // Unused
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

    }
}