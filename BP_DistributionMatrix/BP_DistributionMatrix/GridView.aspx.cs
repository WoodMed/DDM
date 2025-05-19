using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.UI;
using BP_DistributionMatrix.Model;
using DevExpress.Web;

namespace BP_DistributionMatrix {
    public partial class GridView : System.Web.UI.Page {

        protected void Page_Load(object sender, EventArgs e)
        {

            BindAvailableUsers();
        }
        protected void callbackPanelAddMembers_Callback(object sender, EventArgs e)
        {
            Debug.WriteLine("CallBack");
        }
        protected void btnAddSelectedMembers_Click(object sender, EventArgs e)
        {
            foreach (ListEditItem item in listAvailableUsers.Items)
            {
                Debug.WriteLine(item.Text);
                if (item.Selected)
                    Debug.WriteLine(item.Selected);
            }

            Debug.WriteLine("");

            popupAddMembers.ShowOnPageLoad = false;
        }

        private void BindAvailableUsers()
        {

            var hardcodedUsers = new List<TeamUser_Model>
            {
                new TeamUser_Model { Id = 1, Name = "Alice Johnson", Role = "Developer", Email = "alice@example.com", PopupDisplay = "Alice Johnson (alice@example.com)" },
                new TeamUser_Model { Id = 2, Name = "Bob Smith", Role = "Designer", Email = "bob@example.com", PopupDisplay = "Bob Smith (bob@example.com)"},
                new TeamUser_Model { Id = 3, Name = "Charlie Davis", Role = "Manager", Email = "charlie@example.com", PopupDisplay = "Charlie Davis (charlie@example.com)" }
            };


            listAvailableUsers.DataSource = hardcodedUsers;
            listAvailableUsers.ValueField = "Id";
            listAvailableUsers.TextField = "PopupDisplay";
            listAvailableUsers.DataBind();
        }

        protected void ShowPopup_Btn(object sender, EventArgs e)
        {
  
        }
    }
}