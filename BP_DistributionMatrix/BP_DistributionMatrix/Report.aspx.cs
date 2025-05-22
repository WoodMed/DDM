using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using BP_DistributionMatrix.Model;
using DevExpress.Web;
using DevExpress.Web.Data;

namespace BP_DistributionMatrix {
    public partial class Report : System.Web.UI.Page {

        Reports_DAL _dal;
        List<Report_Model> _report;
        int _userId;
        bool _isPopulated;

        protected void Page_Load(object sender, EventArgs e)
        {
            ErrorLabel.Visible = false;
            CompanyLabel.Text = "Contractor: ";
            DiscLabel.Text = "Discipline: ";
            DocLabel.Text = "Type: ";
            _dal = new Reports_DAL();

            // Get UserId
            HttpCookie userIdCookie = HelperClass.GetCookie("session_userId");
            _userId = int.Parse(userIdCookie.Value);

            if(Session["CurrentReport"] == null)
            {
                return;
            }

            _report = (List<Report_Model>)Session["CurrentReport"];


            if (!IsCallback)
            {
                populateBoxes();
            }

        }

        protected void Search_BtnClick(object sender, EventArgs e)
        {
            string number = DocumentInput.Text.Trim();

            string pattern = @"^[A-Z0-9]+-[A-Z]+-[A-Z]+-\d+-[A-Z0-9]+(-\d+)*$";
            if (!Regex.IsMatch(number, pattern))
            {
                ErrorLabel.Visible = true;
                clearPage();
                return;
            }

            string[] parts = number.Split('-');

            string disc_code = parts[1];
            string doc_code = parts[2];
            string contractor_code = parts[3];

            _report = _dal.GetReportActions(disc_code, doc_code, contractor_code);
            List<string> details = _dal.GetReportDetails(disc_code, doc_code, contractor_code);

            if (_report == null || !_report.Any() || details == null || !details.Any())
            {
                ErrorLabel.Visible = true;
                clearPage();
                return;
            }

            Session["CurrentReport"] = _report;
            Session["CurrentDetails"] = details;

            populateBoxes();
        }

        protected void populateBoxes()
        {
            //if (_isPopulated) return;
            _isPopulated = true;
            List<string> approvers = new List<string>();
            List<string> information = new List<string>();
            List<string> reviewers = new List<string>();
            foreach(var user in _report)
            {
                if(user.Action == "A")
                {
                    approvers.Add(user.Username);
                }
                if (user.Action == "I")
                {
                    information.Add(user.Username);
                }
                if (user.Action == "R")
                {
                    reviewers.Add(user.Username);
                }
            }

            ApproverBox.DataSource = approvers.Select(a => new {Name = a }).ToList();
            ApproverBox.TextField = "Name";
            ApproverBox.DataBind();

            InformationBox.DataSource = information.Select(a => new {  Name = a }).ToList();
            InformationBox.TextField = "Name";
            InformationBox.DataBind();

            ReviewBox.DataSource = reviewers.Select(a => new { Name = a }).ToList();
            ReviewBox.TextField = "Name";
            ReviewBox.DataBind();

            if (Session["CurrentDetails"] != null)
            {
                List<string> details = (List<string>)Session["CurrentDetails"];

                CompanyLabel.Text = "Contractor: " + details[0];
                DiscLabel.Text = "Discipline: " + details[1];
                DocLabel.Text = "Type: " + details[2];
            }

        }

        protected void clearPage()
        {
            Session["CurrentReport"] = null;
            Session["CurrentDetails"] = null;
            ApproverBox.Items.Clear();
            InformationBox.Items.Clear();
            ReviewBox.Items.Clear();

            CompanyLabel.Text = "Contractor: ";
            DiscLabel.Text = "Discipline: ";
            DocLabel.Text = "Type: ";
        }

    }
}