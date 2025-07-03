using BP_DistributionMatrix.Model;
using DevExpress.Web;
using DevExpress.Web.Data;
using DevExpress.XtraSpreadsheet.Import.Xls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

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
                populateBoxes(false);
            }

        }

        protected void Search_BtnClick(object sender, EventArgs e)
        {
            string number = DocumentInput.Text.Trim();

            string ContractorPattern = @"(^[A-Z0-9]+-)([A-Z]+)(-)([A-Z]+)(-)([A-Z0-9]+)";
            string SupplierPattern = @"(^[A-Z0-9]+-)([A-Z0-9]+)(-)([A-Z0-9]+)(-)?([0-9]+)?";

            bool noMatch = true;
            bool isContractor = true;

            // Match for supplier
            if (Regex.IsMatch(number, SupplierPattern))
            {
                isContractor = false;
                noMatch = false;
            }

            // Match for contractor
            if (Regex.IsMatch(number, ContractorPattern))
            {
                isContractor = true;
                noMatch = false;
            }

            // No match found for either
            if (noMatch)
            {
                ErrorLabel.Visible = true;
                clearPage();
                return;
            }

            
            // Different search for contractor and supplier
            if (isContractor)
            {
                Contractor_Search(number);
            } else
            {
                Supplier_Search(number);
            }

        }

        protected void Contractor_Search(string number)
        {

            string[] parts = number.Split('-');

            string disc_code = parts[1];
            string doc_code = parts[2];
            string contractor_code = parts[3];

            /* List<Report_Models> */
            _report = _dal.GetReportActions(disc_code, doc_code, contractor_code); // This is uers and actions
            List<string> details = _dal.GetReportDetails(disc_code, doc_code, contractor_code); // this is just for the infromation pane

            if (_report == null || !_report.Any() || details == null || !details.Any())
            {
                ErrorLabel.Visible = true;
                clearPage();
                return;
            }

            Session["CurrentReport"] = _report;
            Session["CurrentDetails"] = details;

            populateBoxes(true);
        }

        protected void Supplier_Search(string number)
        {
            string[] parts = number.Split('-');

            string PO_Number = parts[1];
            string SDRML_Code = parts[2];

            /* List<Report_Models> */
            _report = _dal.GetReportActionsSuppliers(PO_Number, SDRML_Code); // This is users and actions
            List<string> details = _dal.GetReportDetailsSuppliers(PO_Number, SDRML_Code); // this is just for the information pane

            if (_report == null || !_report.Any() || details == null || !details.Any())
            {
                ErrorLabel.Visible = true;
                clearPage();
                return;
            }

            Session["CurrentReport"] = _report;
            Session["CurrentDetails"] = details;

            populateBoxes(false);
        }
        protected void populateBoxes(bool isContractor)
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

                if (isContractor)
                {
                    CompanyLabel.Text = "Contractor: " + details[0];
                    DiscLabel.Text = "Discipline: " + details[1];
                    DocLabel.Text = "Type: " + details[2];
                }
                else
                {
                    CompanyLabel.Text = "Supplier: " + details[0];
                    DiscLabel.Text = "PO Number: " + details[1];
                    DocLabel.Text = "SDRML Code: " + details[2];
                }
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