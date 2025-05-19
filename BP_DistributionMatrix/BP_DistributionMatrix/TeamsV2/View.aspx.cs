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
    public partial class View : System.Web.UI.Page {


        public class MemberData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }

        }

        private int _teamId;
        private Teams_Dal _dal;
        private List<TeamUser_Model> _members;
        private string _name;
        private int _userId;

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

            _dal = new Teams_Dal();
            string Role = _dal.GetRole(_userId, _teamId);

            if (Role.ToLower() == "leader") Response.Redirect($"/TeamsV2/Edit.aspx?Id={_teamId}");

            _members = _dal.GetTeamUsers(_teamId, _userId);
            _name = _dal.GetTeamName( _teamId );
            TeamLabel.Text += _name;

            List<MemberData> _membersData = _members.Select(user => new MemberData
            {
                Id = user.Id,
                Name = user.Name, // Using Name from TeamUser_Model
                Role = user.Role
            }).ToList();

            TeamsGrid.DataSource = _members;
            TeamsGrid.DataBind();

            Debug.WriteLine("");

        }
        protected void LeaveTeam_Click(object sender, EventArgs e)
        {
            _dal.LeaveTeam(_userId, _teamId);
            Response.Redirect("/TeamsV2/List.aspx");
            Debug.WriteLine("");
        }
    }
}