using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using BP_DistributionMatrix.Model;
using DevExpress.Web;
using DevExpress.Web.Data;

namespace BP_DistributionMatrix {
    public partial class List : System.Web.UI.Page {

        Teams_Dal _dal;
        List<Teams_Model> _teams;
        int _userId;
        protected void Page_Load(object sender, EventArgs e)
        {
            _dal = new Teams_Dal();

            // Get UserId
            HttpCookie userIdCookie = HelperClass.GetCookie("session_userId");
            _userId = int.Parse(userIdCookie.Value);

            _teams = _dal.GetAllTeams( _userId.ToString() );
            TeamsGrid.DataSource = _teams;
            TeamsGrid.DataBind();

        }

        protected void TeamsGrid_CommandButtonInitialize(object sender, ASPxGridViewCommandButtonEventArgs e)
        {
            if (e.ButtonType == DevExpress.Web.ColumnCommandButtonType.Edit || e.ButtonType == DevExpress.Web.ColumnCommandButtonType.Delete)
            {
                string role = (string)TeamsGrid.GetRowValues(e.VisibleIndex, "Role");

                // Hide Edit/Delete for specific roles (example: "Member" cannot edit or delete)
                if (role == "Member")
                {
                    e.Visible = false;
                }
            }
        }

        protected void TeamsGrid_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
        {
            if (e.ButtonID == "Delete")
            {
                int teamId = Convert.ToInt32(TeamsGrid.GetRowValues(e.VisibleIndex, "Id"));
                _dal.DeleteTeam(teamId);

                _teams = _dal.GetAllTeams(_userId.ToString());
                TeamsGrid.DataSource = _teams;
                TeamsGrid.DataBind();
            }
        }

    }
}