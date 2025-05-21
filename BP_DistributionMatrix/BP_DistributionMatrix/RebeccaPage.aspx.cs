using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using BP_DistributionMatrix.Model;
using DevExpress.Web;
using DevExpress.Web.Data;

namespace BP_DistributionMatrix {
    public partial class RebeccaPage : System.Web.UI.Page {

        Teams_Dal _dal;
        List<Teams_Model> _teams;
        int _userId;
        protected void Page_Load(object sender, EventArgs e)
        {
            _dal = new Teams_Dal();

            // Get UserId
            HttpCookie userIdCookie = HelperClass.GetCookie("session_userId");
            _userId = int.Parse(userIdCookie.Value);

        }

    }
}