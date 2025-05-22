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
    public partial class Edit : System.Web.UI.Page {


        public class MemberData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }

        }

        private int _teamId;
        private Teams_Dal _dal;
        private List<MemberData> _members;
        private string _name;
        private int _userId;
        List<TeamUser_Model> _availableUsers;
        List<MemberData> _selectedUsers;

        protected void Page_Load(object sender, EventArgs e)
        {

            // Retrieve team id
            string teamIdParam = Request.QueryString["Id"]; 
            if (!string.IsNullOrEmpty(teamIdParam) && int.TryParse(teamIdParam, out int parsedTeamId))
            {
                _teamId = parsedTeamId; 
            }
            else
            {
                Response.Redirect($"/TeamsV2/List.aspx");
            }

            HttpCookie userIdCookie = HelperClass.GetCookie("session_userId");
            _userId = int.Parse(userIdCookie.Value);
            TeamErrorLabel.Visible = false;

            _dal = new Teams_Dal();
            string Role = _dal.GetRole(_userId, _teamId);
            _name = _dal.GetTeamName( _teamId );
            if (string.IsNullOrWhiteSpace(TeamInput.Text) ){
                TeamInput.Text = _name;
            }

            if (Session["TeamUsers"] == null)
            {
                List<TeamUser_Model> _membersData = _dal.GetTeamUsers(_teamId, _userId);
                _members = _membersData.Select(user => new MemberData
                {
                    Id = user.Id,
                    Name = user.Name, // Using Name from TeamUser_Model
                    Role = user.Role
                }).ToList();

                TeamsGrid.DataSource = _members;
                TeamsGrid.DataBind();
                Session["TeamUsers"] = _members;
            }
            else _members = (List<MemberData>)Session["TeamUsers"];

            _selectedUsers = new List<MemberData>();
            if (Session["TeamUsers"] != null)
            {
                _selectedUsers = (List<MemberData>)Session["TeamUsers"];
                TeamsGrid.DataSource = _selectedUsers;
                TeamsGrid.DataBind();
            }

            // bind popup
            BindAvailableUsers();

            Debug.WriteLine("");

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

        }

        protected void gridTeamMembers_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
        {
            if (e.ButtonID == "Remove")
            {
                object keyValue = TeamsGrid.GetRowValues(e.VisibleIndex, "Id");

                if (keyValue != null)
                {
                    int idToRemove = Convert.ToInt32(keyValue);


                    _members.RemoveAll(x => x.Id == idToRemove);
                    Session["TeamUsers"] = _members;

                    TeamsGrid.DataSource = _members;
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

        protected void SaveChanges_Click(object sender, EventArgs e)
        {

            var TeamText = TeamInput.Text;

            if (string.IsNullOrWhiteSpace(TeamText))
            {
                TeamErrorLabel.Visible = true;
                return;
            }

            List<Tuple<int, string>> TeamMembers = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(_userId, "Leader"),
            };

            foreach (var member in _members)
            {
                TeamMembers.Add(new Tuple<int, string>(member.Id, member.Role));
            }

            _dal.UpdateTeam(_teamId ,TeamMembers, TeamText);

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

            Session["TeamUsers"] = teamUsersDataSource;
            TeamsGrid.DataSource = teamUsersDataSource;
            TeamsGrid.DataBind();

            popupAddMembers.ShowOnPageLoad = false;
        }


        protected void LeaveTeam_Click(object sender, EventArgs e)
        {
            _dal.LeaveTeam(_userId, _teamId);
            Response.Redirect("/TeamsV2/List.aspx");
            Debug.WriteLine("");
        }

    }
}