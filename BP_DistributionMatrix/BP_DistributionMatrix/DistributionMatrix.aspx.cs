using DevExpress.Data.Filtering;
using DevExpress.Office.Utils;
using DevExpress.Spreadsheet;
using DevExpress.Web;
using DevExpress.Web.ASPxSpreadsheet;
using DevExpress.Web.ASPxThemes;
using DevExpress.Web.ASPxTreeList;
using DevExpress.XtraExport.Implementation;
using DevExpress.XtraRichEdit.Export.OpenDocument;
using DevExpress.XtraRichEdit.Model;
using DevExpress.XtraSpreadsheet.Export.Xlsb;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text.Json;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using System.Web.UI.WebControls;
using System.Xml;
using static Root;

public partial class technip : System.Web.UI.Page
{

    // Class Variables
    private Worksheet _worksheet;
    private string _username;
    private int _columnToUnlock = 5; // ctrl f worksheet.Unprotect("") on change update accordingly there
    private int _headerRow = 1;
    private int _userId;
    private string _spreadsheetType;
    private int _spreadsheetId;
    private List<Tuple<int, string>> _userTeams;
    private List<Tuple<int, string>> _teamMembers;
    private List<Tuple<int, string>> _teamEmails;
    private Dictionary<string, string> _actionMap;
    List<RelsDocsJoin_Model> _spreadsheetData;
    RelsDocsJoin_DAL _dal;
    Supplier_DAL _sup_dal;
    string _companyId;
    string _companyName;
    int _CurrentTeamId;
    string _CurrentTeamName;
    string _filepath;
    List<int> _previousSaves;
    private int _teamStartIndex;
    bool _isAdmin;

    List<Companies_Model> _selectedContractors;
    List<Companies_Model> _selectedSuppliers;
    ExcelWorksheet _ExportWorksheet;

    public class SpreadsheetRow
    {
        public string docId { get; set; }
        public List<string> action { get; set; } // Adjusted to match JSON structure
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string status = Request.QueryString["status"];
        if (status != null) StatusLabel.Text = "No data for this company at the moment, please select another company";

        _teamStartIndex = 10; // on of first team member in the spreadsheet row

        // Check if theres no folderid to hide buttons and dont open spreadsheet
        _companyId = Request.QueryString["folderId"];
        if (_companyId != null)
        {
            var split = _companyId.Split('-');
            if (split[0] == "C") _spreadsheetType = "Contractors";
            else _spreadsheetType = "Suppliers";
            _spreadsheetId = int.Parse(split[1]);
            Session["folderId"] = int.Parse(split[1]);

        }
        if (_companyId == null)
        {
            ToggleVisible(false);
            return;
        }

        // Initialise Dals
        _dal = new RelsDocsJoin_DAL();
        _sup_dal = new Supplier_DAL();

        // Verify FolderId
        if (!_dal.CheckCompanyId(_spreadsheetId)) Response.Redirect("/DistributionMatrix.aspx?status=NoData");
        _companyName = _dal.GetCompany(_spreadsheetId.ToString()).Company;

        // Get UserId
        HttpContext context = HttpContext.Current;
        HttpCookie userIdCookie = HelperClass.GetCookie("session_userID");
        _userId = int.Parse(userIdCookie.Value);

        // Verify we're admin for export function
        _isAdmin = _dal.CheckAdmin(_userId);

        // Get User Teams
        _userTeams = _dal.GetUserTeams(_userId);
        _userTeams.Insert(0, new Tuple<int, string>(-1, "NO TEAM"));
        _userTeams.Insert(1, new Tuple<int, string>(-2, "ALL USERS"));

        // Set the current team ID based on query string or default to no team
        var queryString = Request.QueryString["TeamId"];
        _CurrentTeamId = queryString == null ? _userTeams[0].Item1 : int.Parse(queryString);
        if (_CurrentTeamId == null) _CurrentTeamId = _userTeams[0].Item1;

        _CurrentTeamName = "NO TEAM";
        foreach (var tuple in _userTeams)
        {
            if (tuple.Item1 == _CurrentTeamId)
            {
                _CurrentTeamName = tuple.Item2;
                break; // Exit the loop once the ID is found
            }
        }


        // If the teamID is -2 we get all users, otherwise we get team members just for that team
        if (_CurrentTeamId == -2)
        {
            _teamMembers = _dal.GetAllUsers(_userId);
        }
        else
        {
            _teamMembers = _dal.GetTeamMembers(_userId, _CurrentTeamId);
        }

        // Set Buttons to visible
        ToggleVisible(true);

        if (!IsCallback)
        {

            // Retrieve Data and Populate
            Prepare_SpreadsheetV2(_spreadsheetId.ToString());
            PrepareComboBox();
            PopulateSpreadsheet();
        }

        // Bind popup for the export button
        BindExportList();

    }

    // Prepare spreadsheet before data population, add all team users to the spreadsheet
    protected void Prepare_SpreadsheetV2(string folderid)
    {
        Spreadsheet.Document.BeginUpdate();
        Spreadsheet.ConfirmOnLosingChanges = "false";
        Spreadsheet.ShowConfirmOnLosingChanges = false;

        // Select relevant EXCEL template to open
        if (_spreadsheetType == "Contractors")
            _filepath = Server.MapPath("~/App_Data/Excel/ContractorTemplate.xlsx");
        else if (_spreadsheetType == "Suppliers")
            _filepath = Server.MapPath("~/App_Data/Excel/SupplierTemplate.xlsx");


        // Open the spreadsheet and clear previous data
        Spreadsheet.Open(_filepath);
        Spreadsheet.Document.LoadDocument(_filepath);
        _worksheet = Spreadsheet.Document.Worksheets[0];
        _worksheet.DataValidations.Clear();

        _worksheet.FreezePanes(1, 7);
        _worksheet.ActiveView.Zoom = 10;
        RelsDocsJoin_DAL dal = new RelsDocsJoin_DAL();

        _worksheet.Columns["A"].ColumnWidth = 10;

        // Set team name
        _worksheet.Cells["E1"].Value += " " + dal.GetCompany(folderid).Company;
        _worksheet.Cells["E1"].Value += Environment.NewLine + "Team: " + _CurrentTeamName;

        // Set main user name
        _worksheet.Cells["I1"].ClearContents();
        _worksheet.Cells["I1"].Value = dal.GetUserName(_userId);


        // Prepare Team names in first row
        int startColumnIndex = _teamStartIndex;
        var sourceCell = _worksheet.Cells["I1"];
        var sub_sourceCell = _worksheet.Cells["I2"];
        for (int i = 0; i < _teamMembers.Count; i++)
        {
            var targetCell = _worksheet.Cells[0, startColumnIndex + i];
            targetCell.CopyFrom(sourceCell);
            targetCell.ClearContents();
            targetCell.Value = _teamMembers[i].Item2;

            _worksheet.Cells[1, startColumnIndex + i].CopyFrom(sub_sourceCell);
        }

        // Add filters
        var filterRange = _worksheet.Range.FromLTRB(0, 1, _teamStartIndex - 1 + _teamMembers.Count(), 10000);
        _worksheet.AutoFilter.Apply(filterRange);

        _worksheet.DataValidations.Clear();

        // Add some data validation for outside the main spreadsheet
        var EndValidationRange = _worksheet.Range.FromLTRB(_teamStartIndex + _teamMembers.Count(), 1, 10000, 10000); // Column I
        var EndValidation = _worksheet.DataValidations.Add(
                EndValidationRange,
                DataValidationType.Custom,
                DataValidationOperator.Between,
                "=FALSE()"
            );
        EndValidation.ErrorTitle = "Input Not Allowed";
        EndValidation.ErrorMessage = "You cannot enter any value in this column.";
        EndValidation.ShowErrorMessage = true;

        Spreadsheet.Document.EndUpdate();

    }

    // Populate the spreadsheet with the data from the database
    protected void PopulateSpreadsheet()
    {
        
        Spreadsheet.Document.BeginUpdate();

        // Check if we're dealing with contractors or suppliers
        if(_spreadsheetType == "Contractors")
        {
            _spreadsheetData = _dal.GetAllRows(_userId, _spreadsheetId.ToString());
            _actionMap = _dal.GetTeamActions(_teamMembers, _spreadsheetId.ToString());
        }
        else if(_spreadsheetType == "Suppliers")
        {
            _spreadsheetData = _sup_dal.GetAllRows(_userId, _spreadsheetId.ToString());
            _actionMap = _sup_dal.GetTeamActions(_teamMembers, _spreadsheetId.ToString());
        }

        // _spreadsheetData holds all the main data for first 10 columns //
        // _actionMap stores actions for all Team Users for the rest of columns //

        // Populate Spreadsheet
        int RowIndex = _headerRow + 1;
        int ColumnOffset = 3;
        var TeamCount = _teamMembers.Count;
        bool exists = _actionMap.ContainsKey("1106305");
        // NOTE: make a change in global.asax for validation if you change column positions
        foreach (RelsDocsJoin_Model row in _spreadsheetData)
        {
            var rowRange = _worksheet.Range.FromLTRB(0, RowIndex, 8 + _teamMembers.Count(), RowIndex);

     
            // If row was saved previously make it blue
            if (_previousSaves != null && _previousSaves.Contains( int.Parse(row.doc_type_disc_id) ))
            {
                rowRange.Fill.BackgroundColor = System.Drawing.Color.LightBlue;
            }
            if(int.Parse(row.A) > 1)
            {
                rowRange.Fill.BackgroundColor = Color.FromArgb(255, 255, 140, 140);
            }
            


            // Populate each column with data
            _worksheet.Cells[RowIndex, 0].Value = row.doc_type_disc_id;
            _worksheet.Cells[RowIndex, 1].Value = row.R;
            _worksheet.Cells[RowIndex, 2].Value = row.I;
            _worksheet.Cells[RowIndex, 3].Value = row.A;
            _worksheet.Cells[RowIndex, 4].Value = row.disc_code;
            _worksheet.Cells[RowIndex, 5].Value = row.disc_desc;
            _worksheet.Cells[RowIndex, 6].Value = row.doc_code;
            _worksheet.Cells[RowIndex,7].Value = row.doc_desc;
            _worksheet.Cells[RowIndex, 8].Value = row.value;
            _worksheet.Cells[RowIndex, 9].Value = row.consolidators;

            var templateRange = _worksheet.Range.FromLTRB(8, RowIndex, 8, RowIndex);
            var targetRange = _worksheet.Range.FromLTRB(9, RowIndex, 9 + TeamCount, RowIndex);

            // Copy formatting & content, but exclude values to avoid unnecessary clearing
            targetRange.CopyFrom(templateRange, PasteSpecial.Formats);

            // Fill in Team Members actions
            for (int i = 0; i < TeamCount; i++)
            {
                string concat_id = row.doc_type_disc_id + _teamMembers[i].Item1;
                var targetCell = _worksheet.Cells[RowIndex, _teamStartIndex + i];
                if (_actionMap.ContainsKey(concat_id))
                {
                    targetCell.Value = _actionMap[concat_id];
                }
            }
            

            // Move onto next row
            RowIndex++;
        }

        // Delete all unused rows formatting after last row of data
        RowIndex--;
        int totalRows = _worksheet.GetUsedRange().BottomRowIndex + 1;
        if (RowIndex < totalRows - 1)
        {
            _worksheet.Rows.Remove(RowIndex + 1, totalRows - RowIndex - 1);
        }

        // Populate the total IRA Count
        List<int> IRACount = new List<int>();
        if(_spreadsheetType == "Contractors")
        {
            IRACount = _dal.GetTotalIRACount(_spreadsheetId.ToString());
        }
        else if(_spreadsheetType == "Suppliers")
        {
            IRACount = _sup_dal.GetTotalIRACount(_spreadsheetId.ToString());
        }

        _worksheet.Cells["B2"].Value = IRACount[0];
        _worksheet.Cells["C2"].Value = IRACount[1];
        _worksheet.Cells["D2"].Value = IRACount[2];

        // END
        Spreadsheet.Document.EndUpdate();
    }


    // Combo box for team selection, populated depending on which teams the main user is a part of
    protected void PrepareComboBox()
    {
        // Disable user input entirely
        //Teams_Dropdown.ReadOnly = true;

        var teams = new List<dynamic>();

        foreach(Tuple<int, string> teamTuple in _userTeams)
        {
            teams.Add(new { ID = teamTuple.Item1, Team = teamTuple.Item2 });
        }

        // Assign the data source
        Teams_Dropdown.DataSource = teams;
        // Bind the data to the ComboBox
        Teams_Dropdown.DataBind();

    }

    // Toggle buttons as visible
    // If there's no spreadsheet selected, it will be all false, if there is then it will be true
    protected void ToggleVisible(bool visibility)
    {
        Save_Btn.Visible = visibility;
        Discard_Btn.Visible = visibility;
        Spreadsheet.Visible = visibility;
        Export_Btn.Visible = visibility;
        Teams_Dropdown.Visible = visibility;
        StatusLabel.Visible = !visibility;

        if (Export_Btn.Visible)
        {
            Export_Btn.Visible = _isAdmin;
        }
    }

    // SAVE ACTION
    protected void Data_Callback(object sender, DevExpress.Web.CallbackEventArgsBase e)
    {
        try
        {
            _worksheet = Spreadsheet.Document.Worksheets[0];
            int lastRow = _worksheet.GetUsedRange().BottomRowIndex;
            List<UserRels_Model> ActionsToSave = new List<UserRels_Model>();
            _previousSaves = new List<int>();
            int row = 2;

            // Loop through all of the rows, skip rows whos first cell was green.
            while(!string.IsNullOrEmpty(_worksheet.Cells[row, 5]?.Value?.ToString()))
            {
                // Skip Rows that weren't changed
                var cellColor = _worksheet.Cells[row, 0].Fill.BackgroundColor;
                if (cellColor != System.Drawing.Color.Green)
                {
                    row++;
                    continue;
                }

                // Get Main User actions
                var docid = int.Parse(_worksheet.Cells[row, 0].Value.ToString());
                UserRels_Model rels = new UserRels_Model()
                {
                    doc_type_disc_id = docid,
                    company_id = _spreadsheetId,
                    user_id = _userId,
                    value = _worksheet.Cells[row, 8].Value.ToString(),
                    username = _worksheet.Cells[0, 8].Value.ToString(),
                    contractor = _companyName
                };
                ActionsToSave.Add(rels);
                _previousSaves.Add(docid);

                // Get Team Actions
                for (int i = 0; i < _teamMembers.Count(); i++)
                {
                    UserRels_Model team_rels = new UserRels_Model()
                    {
                        doc_type_disc_id = int.Parse(_worksheet.Cells[row, 0].Value.ToString()),
                        company_id = _spreadsheetId,
                        user_id = _teamMembers[i].Item1,
                        value = _worksheet.Cells[row, _teamStartIndex+i].Value.ToString(),
                        username = _worksheet.Cells[0, _teamStartIndex + i].Value.ToString(),
                        contractor = _companyName
                    };

                    ActionsToSave.Add(team_rels);
                }

                // Next row
                row++;
            }

            // SAVE ACTION
            // Contractor or Supplier actiion save
            if (_spreadsheetType == "Contractors")
                _dal.SaveActions(ActionsToSave, _userId);
            else if (_spreadsheetType == "Suppliers")
                _sup_dal.SaveActions(ActionsToSave);

            // Refresh the spreadsheet
            Prepare_SpreadsheetV2(_spreadsheetId.ToString());
            PopulateSpreadsheet();
        }

        catch (Exception ex)
        {
            Debug.WriteLine("Error saving" + ex.Message);
        }

    }



    // EXPORT FUNCTIONALITY

    // Initial click, prepare some data
    protected void Export_Btn_Click(object sender, EventArgs e)
    {

        // Grab selected contractors and suppliers from the listboxes in the popup
        _selectedContractors = hfContractors.Value
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(pair =>
            {
                var parts = pair.Split('|');
                return new Companies_Model
                {
                    Id = int.Parse(parts[0]),
                    Company = parts.Length > 1 ? parts[1] : string.Empty
                };
            })
            .ToList();

        _selectedSuppliers = hfSuppliers.Value
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(pair =>
        {
            var parts = pair.Split('|');
            return new Companies_Model
            {
                Id = int.Parse(parts[0]),
                Company = parts.Length > 1 ? parts[1] : string.Empty
            };
        })
            .ToList();

        // Initialise dals
        _dal = new RelsDocsJoin_DAL();
        _sup_dal = new Supplier_DAL();

        // Get all the users from FL
        _teamMembers = _dal.GetAllUsers(_userId);
        Export_DAL ExportDAL = new Export_DAL();

        // Get all the rows from the database
        List<Export_Model> RawExportData = ExportDAL.GetAllRowsV2(_userId, _spreadsheetId.ToString());

        // Here we're simply converting actions A, I, R to IFA, IFI or IFR, since that's what we want in export
        List<Export_Model> ExportData = new List<Export_Model>();
        foreach (Export_Model model in RawExportData)
        {
            for (int i = 0; i < 3; i++)
            {
                Export_Model CurModel = new Export_Model
                {
                    User_Id = model.User_Id,
                    Doc_type_disc_id = model.Doc_type_disc_id,
                    Doc_Type = model.Doc_Type,
                    Contractor = model.Contractor,
                    UserAction = model.UserAction,
                };

                // Add IFA
                if (i == 0)
                {
                    CurModel.Action_Type = "IFA";
                }
                // Add IFI
                else if (i == 1) 
                {
                    CurModel.Action_Type = "IFI";
                }
                // Add IFR
                else if (i == 2)
                {
                    CurModel.Action_Type = "IFR";
                }

                ExportData.Add(CurModel);
            }
        }

        Debug.WriteLine("");

        // We prepare the spreadsheet here
        PrepareAllContractors_ExportSpreadsheet(ExportData);
    }

    // UNUSED
    protected void Practice_ExportSpreadsheet(List<Export_Model> ExportData)
    {
        Dictionary<string, string> ScopeMap = new Dictionary<string, string>();
        Export_DAL ExportDAL = new Export_DAL();

        foreach (var member in _teamMembers)
        {
            string username = member.Item2;
            string[] parts = username.Split(' ');

            if (parts.Length > 1)
            {
                username = $"{parts[1]}, {parts[0]}";
            }

            ScopeMap[member.Item2] = ExportDAL.GetScope(username);
        }

        // Create Hashmap for the action mapping
        Dictionary<string, string> map = new Dictionary<string, string>();

        map["AIFR"] = "R/S";
        map["RIFR"] = "R/C";
        map["IIFR"] = "R/I";
        map["AIFA"] = "R/S";
        map["RIFA"] = "R/I";
        map["IIFA"] = "R/I";
        map["AIFI"] = "M/T";
        map["RIFI"] = "M/T";
        map["IIFI"] = "M/T";

        ExcelPackage.License.SetNonCommercialPersonal("Maybe");
        string filepath = Server.MapPath("~/App_Data/Excel/PracticeMaster.xlsx");
        var file = new FileInfo(filepath);
        using (var package = new ExcelPackage(file))
        {

            // Open and clear workbook
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;
            if (rowCount > 1)
            {
                worksheet.DeleteRow(2, rowCount - 1);
            }

            int teamStartIndex = 6;
            var sourceCell = worksheet.Cells["D1"];

            for (int i = 0; i < _teamMembers.Count; i++)
            {
                var targetCell = worksheet.Cells[1, teamStartIndex + i];

                targetCell.Clear();
                targetCell.StyleID = sourceCell.StyleID;
                targetCell.Value = _teamMembers[i].Item2;
                worksheet.Column(targetCell.Start.Column).Width = 20;
            }

            int BatchStartIndex = 2;
            int TeamCount = _teamMembers.Count;

            _spreadsheetData = _dal.GetAllRows( _userId, _spreadsheetId.ToString() );
            _actionMap = _dal.GetTeamActions( _teamMembers, _spreadsheetId.ToString() );
            bool exists = _actionMap.ContainsKey("1106305");
            foreach (RelsDocsJoin_Model row in _spreadsheetData)
            {
                // Populate all scope types and action types

                int CurrentRowIndex = BatchStartIndex;
                // First we do NEP and create the format
                for (int i = 0; i < 3; i++)
                {
                    worksheet.Cells[CurrentRowIndex, 1].Value = row.disc_code;
                    worksheet.Cells[CurrentRowIndex, 2].Value = row.doc_code;
                    worksheet.Cells[CurrentRowIndex, 3].Value = "ALCATEL";
                    string ActionType = "IFA";
                    string Scope = "NEP";

                    // Add IFA
                    if (i == 0)
                    {
                        ActionType = "IFA";
                    }
                    // Add IFI
                    else if (i == 1)
                    {
                        ActionType = "IFI";
                    }
                    // Add IFR
                    else if (i == 2)
                    {
                        ActionType = "IFR";
                    }
                    worksheet.Cells[CurrentRowIndex, 4].Value = ActionType;
                    worksheet.Cells[CurrentRowIndex, 5].Value = Scope;

                    CurrentRowIndex += 1;
                }


                // Now we take the last 3 rows, copy them and only change scope to NZT
                int CopyStartIndex = BatchStartIndex;
                CurrentRowIndex = BatchStartIndex+3;
                for (int k = 0; k < 3; k++)
                {
                    string Scope = "NZT";

                    for (int col = 1; col < 6; col++)
                    {
                        worksheet.Cells[CurrentRowIndex, col].Value = worksheet.Cells[CopyStartIndex + k, col].Value;
                    }

                    worksheet.Cells[CurrentRowIndex, 5].Value = Scope;
                    CurrentRowIndex += 1;
                }

                // Repeat NZT-NEP
                CopyStartIndex = BatchStartIndex + 3;
                CurrentRowIndex = BatchStartIndex + 6;
                for (int k = 0; k < 3; k++)
                {
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        worksheet.Cells[CurrentRowIndex, col].Value = worksheet.Cells[CopyStartIndex + k, col].Value;
                    }

                    worksheet.Cells[CurrentRowIndex, 5].Value = "NZT-NEP";
                    CurrentRowIndex += 1;
                }

                // Repeat H2T
                CopyStartIndex = BatchStartIndex + 6;
                CurrentRowIndex = BatchStartIndex + 9;
                for (int k = 0; k < 3; k++)
                {
                    string Scope = "H2T";

                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        worksheet.Cells[CurrentRowIndex, col].Value = worksheet.Cells[CopyStartIndex + k, col].Value;
                    }

                    worksheet.Cells[CurrentRowIndex, 5].Value = Scope;
                    CurrentRowIndex += 1;
                }

                // Now we go back to the start of this batch and populate the actions
                // A batch is 12 rows
                for(CurrentRowIndex = BatchStartIndex; CurrentRowIndex<BatchStartIndex+12; CurrentRowIndex++)
                {
                    // On each of these rows populate all team actions
                    for (int j = 0; j < TeamCount; j++)
                    {
                        string Scope = worksheet.Cells[CurrentRowIndex, 5].Value?.ToString() ?? "NEP";
                        string ActionType = worksheet.Cells[CurrentRowIndex, 4].Value?.ToString() ?? "IFA";

                        // Check if user has assigned scope or not
                        string user = _teamMembers[j].Item2.ToString();
                        if (!ScopeMap.TryGetValue(user, out var scopes) || !scopes.Contains(Scope))
                        {
                            continue;
                        }

                        // Check in the maps for the resulting actions
                        string concat_id = row.doc_type_disc_id + _teamMembers[j].Item1;
                        var targetCell = worksheet.Cells[CurrentRowIndex, teamStartIndex + j];
                        if (_actionMap.ContainsKey(concat_id) && !string.IsNullOrEmpty(_actionMap[concat_id]) )
                        {
                            string UserAction = _actionMap[concat_id];
                            string ActionKey = UserAction + ActionType;
                            string res = map[ActionKey]; // Debugging variable here can delete this
                            targetCell.Value = map[ActionKey];
                        }

                    }
                }
                
                BatchStartIndex = CurrentRowIndex;
                         
            }


            // END
            package.Save();
        }
    } // No lnger use this one, it's for the other spreadsheet "PracticeTemplate"
    // Can still use it if needed

    // UNUSED
    protected void Prepare_ExportSpreadsheet(List<Export_Model> ExportData)
    {
        Dictionary<string, string> ScopeMap = new Dictionary<string, string>();
        Export_DAL ExportDAL = new Export_DAL();

        _teamEmails = ExportDAL.GetAllEmails(_teamMembers);
        List<string> formattedUsernames = _teamMembers
        .Select(member =>
        {
            string[] parts = member.Item2.Split(' ');
            return parts.Length > 1 ? $"{parts[1]}, {parts[0]}" : member.Item2;
        })
        .ToList();

        ScopeMap = ExportDAL.GetAllScopes(_teamEmails);

        // Create Hashmap for the action mapping
        Dictionary<string, string> map = new Dictionary<string, string>();

        map["AIFR"] = "R/S";
        map["RIFR"] = "R/C";
        map["IIFR"] = "R/I";
        map["AIFA"] = "R/S";
        map["RIFA"] = "R/I";
        map["IIFA"] = "R/I";
        map["AIFI"] = "M/T";
        map["RIFI"] = "M/T";
        map["IIFI"] = "M/T";

        ExcelPackage.License.SetNonCommercialPersonal("Maybe");
        string filepath = Server.MapPath("~/App_Data/Excel/MasterTemplate.xlsx");
        var file = new FileInfo(filepath);
        using (var package = new ExcelPackage(file))
        {

            int BatchStartIndex = 11;
            int DataOffset = 5;
            int TeamCount = _teamMembers.Count;
            int teamStartIndex = 16;

            // Open and clear workbook content
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            // Clear all previous data but keep formatting
            worksheet.Cells[$"A11:{ExcelCellAddress.GetColumnLetter(worksheet.Dimension.Columns)}{rowCount}"].Value = null;

            var sourceCell = worksheet.Cells["O8"];

            // Populate team names and keep style
            for (int i = 0; i < _teamMembers.Count; i++)
            {
                var targetCell = worksheet.Cells[8, teamStartIndex + i];
                targetCell.StyleID = sourceCell.StyleID;
                targetCell.Value = _teamMembers[i].Item2;

                worksheet.Column(targetCell.Start.Column).Width = 20;
            }


            _spreadsheetData = _dal.GetAllRows(_userId, _spreadsheetId.ToString());
            _actionMap = _dal.GetTeamActions(_teamMembers, _spreadsheetId.ToString());
            string CompanyCode = ExportDAL.GetCompanyCode(_companyId.Split('-')[1]);
            List<Companies_Model> companies = _dal.GetAllCompanies(_spreadsheetId.ToString());

            foreach (RelsDocsJoin_Model row in _spreadsheetData)
            {
                // Populate all scope types and action types

                int CurrentRowIndex = BatchStartIndex;
                // First we do NEP and create the format
                for (int i = 0; i < 3; i++)
                {
                    worksheet.Cells[CurrentRowIndex, DataOffset + 1].Value = row.disc_code;
                    worksheet.Cells[CurrentRowIndex, DataOffset +  2].Value = row.doc_code;
                    worksheet.Cells[CurrentRowIndex, DataOffset +  3].Value = CompanyCode;
                    string ActionType = "IFA";
                    string Scope = "NEP";

                    // Add IFA
                    if (i == 0)
                    {
                        ActionType = "IFA";
                    }
                    // Add IFI
                    else if (i == 1)
                    {
                        ActionType = "IFI";
                    }
                    // Add IFR
                    else if (i == 2)
                    {
                        ActionType = "IFR";
                    }
                    worksheet.Cells[CurrentRowIndex, DataOffset + 4].Value = ActionType;
                    worksheet.Cells[CurrentRowIndex, DataOffset + 5].Value = Scope;

                    CurrentRowIndex += 1;
                }


                // Now we take the last 3 rows, copy them and only change scope to NZT
                int CopyStartIndex = BatchStartIndex;
                CurrentRowIndex = BatchStartIndex + 3;
                for (int k = 0; k < 3; k++)
                {
                    string Scope = "NZT";

                    for (int col = DataOffset + 1; col < DataOffset + 6; col++)
                    {
                        worksheet.Cells[CurrentRowIndex, col].Value = worksheet.Cells[CopyStartIndex + k, col].Value;
                    }

                    worksheet.Cells[CurrentRowIndex, DataOffset + 5].Value = Scope;
                    CurrentRowIndex += 1;
                }

                // Repeat NZT-NEP
                CopyStartIndex = BatchStartIndex + 3;
                CurrentRowIndex = BatchStartIndex + 6;
                for (int k = 0; k < 3; k++)
                {
                    for (int col = DataOffset + 1; col <= DataOffset + 6; col++)
                    {
                        worksheet.Cells[CurrentRowIndex, col].Value = worksheet.Cells[CopyStartIndex + k, col].Value;
                    }

                    worksheet.Cells[CurrentRowIndex, DataOffset + 5].Value = "NZT-NEP";
                    CurrentRowIndex += 1;
                }

                // Repeat H2T
                CopyStartIndex = BatchStartIndex + 6;
                CurrentRowIndex = BatchStartIndex + 9;
                for (int k = 0; k < 3; k++)
                {
                    string Scope = "H2T";

                    for (int col = DataOffset + 1; col <= DataOffset + 6; col++)
                    {
                        worksheet.Cells[CurrentRowIndex, col].Value = worksheet.Cells[CopyStartIndex + k, col].Value;
                    }

                    worksheet.Cells[CurrentRowIndex, DataOffset + 5].Value = Scope;
                    CurrentRowIndex += 1;
                }

                // Now we go back to the start of this batch and populate the actions
                // A batch is 12 rows
                for (CurrentRowIndex = BatchStartIndex; CurrentRowIndex < BatchStartIndex + 12; CurrentRowIndex++)
                {
                    // On each of these rows populate all team actions
                    for (int j = 0; j < TeamCount; j++)
                    {
                        string Scope = worksheet.Cells[CurrentRowIndex, DataOffset + 5].Value?.ToString() ?? "NEP";
                        string ActionType = worksheet.Cells[CurrentRowIndex, DataOffset + 4].Value?.ToString() ?? "IFA";

                        // Check if user has assigned scope or not. If not skip action
                        string user = _teamMembers[j].Item2.ToString();
                        // ThIs just adds comma so it works for scopemap
                        // Also reverses names for the scopemap
                        string[] nameParts = user.Split(' ');
                        if (nameParts.Length == 2)
                        {
                            user = $"{nameParts[1]}, {nameParts[0]}";
                        }



                        if (user == "Emily, Lyle")
                        {
                            Debug.WriteLine("");
                        }

                        if (Scope == "NZT-NEP") // Reserve this for NZT-NEP special case
                        {
                            string[] DoubleScopes = Scope.Split('-');

                            if (!ScopeMap.TryGetValue(user, out var scopes) ||
                                !scopes.Contains(DoubleScopes[0]) || !scopes.Contains(DoubleScopes[1]))
                            {
                                continue;
                            }
                        }
                        else if (!ScopeMap.TryGetValue(user, out var scopes) || !scopes.Contains(Scope))
                        {
                            continue;
                            }

                        // Check in the maps for the resulting actions
                        string concat_id = row.doc_type_disc_id + _teamMembers[j].Item1;
                        var targetCell = worksheet.Cells[CurrentRowIndex, teamStartIndex + j];
                        if (_actionMap.ContainsKey(concat_id) && !string.IsNullOrEmpty(_actionMap[concat_id]))
                        {
                            string UserAction = _actionMap[concat_id];
                            string ActionKey = UserAction + ActionType;
                            string res = map[ActionKey]; // Debugging variable here can delete this
                            targetCell.Value = map[ActionKey];
                        }

                    }
                }

                BatchStartIndex = CurrentRowIndex;

            }

            
            // END
            package.Save();
        }
    }

    // Prepare spreadsheet for export
    protected void PrepareAllContractors_ExportSpreadsheet(List<Export_Model> ExportData)
    {
        Dictionary<string, string> ScopeMap = new Dictionary<string, string>();
        Export_DAL ExportDAL = new Export_DAL();

        _teamEmails = ExportDAL.GetAllEmails(_teamMembers);

        // Get all usernames and scopes of users
        List<string> formattedUsernames = _teamMembers
        .Select(member =>
        {
            string[] parts = member.Item2.Split(' ');
            return parts.Length > 1 ? $"{parts[1]}, {parts[0]}" : member.Item2;
        })
        .ToList();

        // Get emails from _teamMembers

        ScopeMap = ExportDAL.GetAllScopes(_teamEmails); // Get all scopes for all team members

        // Create Hashmap for the action mapping
        var map = new Dictionary<string, string>
        {
            ["AIFR"] = "R/S",
            ["RIFR"] = "R/C",
            ["IIFR"] = "R/I",
            ["AIFA"] = "R/S",
            ["RIFA"] = "R/I",
            ["IIFA"] = "R/I",
            ["AIFI"] = "M/T",
            ["RIFI"] = "M/T",
            ["IIFI"] = "M/T"
        };

        // An additional layer for additional codes added after the initial mapping
        var LayerMap = new Dictionary<string, string>
        {
            ["IFA"] = "IFA",
            ["IFI"] = "IFI",
            ["IFR"] = "IFR",
            ["IFH"] = "IFA",
            ["IFP"] = "IFA",
            ["IFE"] = "IFA",
            ["IFF"] = "IFA",
            ["AFD"] = "IFA",
            ["ASB"] = "IFA",
            ["IFCR"] = "IFR",
            ["IFC"] = "IFA",
            ["IFU"] = "IFA",
            ["ISUP"] = "IFI",
            ["IAV"] = "IFI"
        };


        // We open the template here and prepare it for export
        ExcelPackage.License.SetNonCommercialPersonal("Maybe");
        string filepath = Server.MapPath("~/App_Data/Excel/MasterTemplate.xlsx");
        var file = new FileInfo(filepath);
        using (var package = new ExcelPackage(file))
        {

            // We process the data in batches.
            int BatchStartIndex = 11;
            int DataOffset = 5;
            int TeamCount = _teamMembers.Count;
            int teamStartIndex = 16;

            // Open and clear workbook content
            _ExportWorksheet = package.Workbook.Worksheets[0];
            int rowCount = _ExportWorksheet.Dimension.Rows;

            var sourceCell = _ExportWorksheet.Cells["O8"];

            // Populate user names and keep style
            for (int i = 0; i < _teamMembers.Count; i++)
            {
                var targetCell = _ExportWorksheet.Cells[8, teamStartIndex + i];
                targetCell.StyleID = sourceCell.StyleID;
                targetCell.Value = _teamMembers[i].Item2;
            }



            // These 2 following for loops could be abstracted into their own methods, but it works currently so leaving it as is

            // Set lists of codes for each batch.
            // The data will be written in batches. Each document will be rewritten for each code in the batch.
            // If you need to add a new code, you can add it here
            // Make sure to adjust the maps above if you do
            List<string> CodeList = new List<string>
            {
                "IFI",
                "IFR",
                "IFH",
                "IFP",
                "IFE",
                "IFF",
                "AFD",
                "ASB",
                "IFCR",
                "IFC",
                "IFU",
                "ISUP",
                "IAV"
            };
            int BatchLength = CodeList.Count; 

            // CONTRACTORS DATA
            foreach ( Companies_Model company in _selectedContractors)
            {
                string contractor_ID = company.Id.ToString();
                string CompanyCode = ExportDAL.GetCompanyCode(contractor_ID);
                
                _spreadsheetData = _dal.GetAllRows(_userId, contractor_ID);
                _actionMap = _dal.GetTeamActions(_teamMembers, contractor_ID);

                // Iterate through each row in the spreadsheet data
                foreach (RelsDocsJoin_Model row in _spreadsheetData)
                {
                    // Populate all scope types and action types
                    int CurrentRowIndex = BatchStartIndex;
                    if(BatchStartIndex < 50)
                    {
                        Debug.WriteLine("");
                    }

                    // First we do the NEP batch which we will be copying down
                    // Remember a batch is one document, for each code. and we want a batch for each scope for the same document.
                    foreach (string ActionCode in CodeList)
                    {
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 1].Value = row.disc_code;
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 2].Value = row.doc_code;
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 3].Value = CompanyCode;
                        string Scope = "NEP";

                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 4].Value = ActionCode;
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 5].Value = Scope;

                        CurrentRowIndex += 1;
                    }

                    // Create a new batch for each scope, copying the Format batch we made just before
                    AddScope(BatchStartIndex, BatchStartIndex + BatchLength, DataOffset, "NZT", BatchLength);
                    AddScope(BatchStartIndex, BatchStartIndex + (BatchLength*2), DataOffset, "NZT-NEP", BatchLength);
                    AddScope(BatchStartIndex, BatchStartIndex + (BatchLength * 3), DataOffset, "H2T", BatchLength);

                    // Now we iterate backwards and populate all of the actions for each user
                    // We go backwards because we will be deleting rows, and we want to avoid index shifting issues
                    int rowsDeleted = 0;
                    // Start from the end of the final batch and go backwards all the way to the start of the first batch
                    for (CurrentRowIndex = BatchStartIndex + (BatchLength*4)-1; CurrentRowIndex >= BatchStartIndex; CurrentRowIndex--)
                    {
                        // On each of these rows populate all team actions
                        bool DeleteRow = true;
                        string Scope = _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 5].Value?.ToString() ?? "NEP";
                        string ActionType = _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 4].Value?.ToString();

                        // Layer map comes in to ensure we translate the added codes to something we can use
                        ActionType = LayerMap[ActionType];

                        // Populate DCC wood Column
                        if (ActionType == "IFI")
                        {
                            _ExportWorksheet.Cells[CurrentRowIndex, 15].Value = "M/T";
                        }
                        else
                        {
                            _ExportWorksheet.Cells[CurrentRowIndex, 15].Value = "R/T";
                        }

                        // We go through every user
                        for (int j = 0; j < TeamCount; j++)
                        {

                            string user = _teamEmails[j].Item2.ToString();


                            // Check if user has assigned scope or not by comparing user scopes and current row scope. If not we skip action
                            if (Scope == "NZT-NEP") // Reserve this for NZT-NEP special case
                            {
                                string[] DoubleScopes = Scope.Split('-');

                                if (!ScopeMap.TryGetValue(user, out var scopes) ||
                                    !scopes.Contains(DoubleScopes[0]) || !scopes.Contains(DoubleScopes[1]))
                                {
                                    continue;
                                }

                                /*
                                if (!ScopeMap.TryGetValue(user, out var scopes))
                                {
                                    Debug.WriteLine($"User '{user}' not found in ScopeMap.");
                                    continue;
                                }
                                else
                                {
                                    Debug.WriteLine($"User '{user}' found. Scopes: '{scopes}'");
                                    Debug.WriteLine("");
                                }

                                if (!scopes.Contains(DoubleScopes[0]))
                                {
                                    Debug.WriteLine($"Scope '{DoubleScopes[0]}' not found in scopes for user '{user}'.");
                                    continue;
                                }

                                if (!scopes.Contains(DoubleScopes[1]))
                                {
                                    Debug.WriteLine($"Scope '{DoubleScopes[1]}' not found in scopes for user '{user}'.");
                                    continue;
                                }
                                */
                            }
                            else if (!ScopeMap.TryGetValue(user, out var scopes) || !scopes.Contains(Scope))
                            {
                                continue;
                            }


                            // Check in the maps for the user's actions
                            string concat_id = row.doc_type_disc_id + _teamMembers[j].Item1;
                            var targetCell = _ExportWorksheet.Cells[CurrentRowIndex, teamStartIndex + j];
                            if (_actionMap.ContainsKey(concat_id) && !string.IsNullOrEmpty(_actionMap[concat_id]))
                            {

                                try
                                {
                                    // Action key is going to be Useraction + Action type. Use a breakpoint on the if statement here to understand the format of the map key
                                    string UserAction = _actionMap[concat_id];

                                    string ActionKey = (UserAction + ActionType).ToUpper();
                                    if (map.ContainsKey(ActionKey))
                                    {
                                        
                                        string res = map[ActionKey];
                                        // Here if its not empty, then a user has made an action in the row.
                                        // This means we no longer want to delete this row.
                                        if (!string.IsNullOrWhiteSpace(res))
                                        {
                                            targetCell.Value = res;
                                            DeleteRow = false;
                                        }
                                    }
                                }

                                catch (KeyNotFoundException ex)
                                {
                                    Debug.WriteLine($"Missing key in 'map': concat_id = {concat_id}, ActionKey = {_actionMap[concat_id] + ActionType}");
                                    throw; // Optional: rethrow if you want to halt execution
                                }

                            }


                        }

                        // DELETE EMPTY ROWS
                        // If there was even one action in a row, this will be set to false.
                        if (DeleteRow)
                        {
                            _ExportWorksheet.DeleteRow(CurrentRowIndex);
                            rowsDeleted++;
                        }

                    }

                    // We set the BatchStartIndex to be the last row of the group of batches accounting for deleted rows.
                    // From here we will move on to the next document.
                    BatchStartIndex = BatchStartIndex + ((BatchLength*4) - rowsDeleted);

                }

            }

            // SUPPLIERS DATA
            // Very similar logic to contractors with minor adjustments
            foreach(Companies_Model supplier in _selectedSuppliers)
            {
                string contractor_ID = supplier.Id.ToString();
                string CompanyCode = ExportDAL.GetCompanyCode(contractor_ID);

                _spreadsheetData = _sup_dal.GetAllRows(_userId, contractor_ID);
                _actionMap = _sup_dal.GetTeamActions(_teamMembers, contractor_ID);

                // Iterate through each row in the spreadsheet data
                foreach (RelsDocsJoin_Model row in _spreadsheetData)
                {
                    // Populate all scope types and action types

                    int CurrentRowIndex = BatchStartIndex;

                    // First we do the NEP batch which we will be copying down
                    // Remember a batch is one document, for each code. and we want a batch for each scope for the same document.
                    foreach (string ActionCode in CodeList)
                    {
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 6].Value = row.disc_code;
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 7].Value = row.doc_code;
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 3].Value = CompanyCode;
                        string Scope = "NEP";

                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 4].Value = ActionCode;
                        _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 5].Value = Scope;

                        CurrentRowIndex += 1;
                    }

                    // Create a new batch for each scope, copying the Format batch we made just before
                    AddScope(BatchStartIndex, BatchStartIndex + BatchLength, DataOffset+2, "NZT", BatchLength);
                    AddScope(BatchStartIndex, BatchStartIndex + (BatchLength * 2), DataOffset+2, "NEP-NZT", BatchLength);
                    AddScope(BatchStartIndex, BatchStartIndex + (BatchLength * 3), DataOffset+2, "H2T", BatchLength);

                    // Now we iterate backwards and populate all of the actions for each user
                    // We go backwards because we will be deleting rows, and we want to avoid index shifting issues
                    int rowsDeleted = 0;
                    // Start from the end of the final batch and go backwards all the way to the start of the first batch
                    for (CurrentRowIndex = BatchStartIndex + (BatchLength * 4) - 1; CurrentRowIndex >= BatchStartIndex; CurrentRowIndex--)
                    {
                        string Scope = _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 5].Value?.ToString() ?? "NEP";
                        string ActionType = _ExportWorksheet.Cells[CurrentRowIndex, DataOffset + 4].Value?.ToString();

                        // Layer map comes in to ensure we translate the added codes to something we can use
                        ActionType = LayerMap[ActionType];

                        // Populate DCC wood Colum
                        if (ActionType == "IFI")
                        {
                            _ExportWorksheet.Cells[CurrentRowIndex, 15].Value = "M/T";
                        }
                        else
                        {
                            _ExportWorksheet.Cells[CurrentRowIndex, 15].Value = "R/T";
                        }

                        // On each of these rows populate all team actions
                        bool DeleteRow = true;
                        for (int j = 0; j < TeamCount; j++)
                        {

                            string user = _teamMembers[j].Item2.ToString();

                            // This just adds comma so it works for scopemap
                            // Also reverses names for the scopemap
                            string[] nameParts = user.Split(' ');
                            if (nameParts.Length == 2)
                            {
                                user = $"{nameParts[1]}, {nameParts[0]}";
                            }

                            // Check if user has assigned scope or not. If not skip action
                            if (Scope == "NZT-NEP") // Reserve this for NZT-NEP special case
                            {
                                string[] DoubleScopes = Scope.Split('-');

                                if (!ScopeMap.TryGetValue(user, out var scopes) ||
                                    !scopes.Contains(DoubleScopes[0]) || !scopes.Contains(DoubleScopes[1]))
                                {
                                    continue;
                                }
                            }
                            else if (!ScopeMap.TryGetValue(user, out var scopes) || !scopes.Contains(Scope))
                            {
                                continue;
                            }


                            // Check in the maps for the resulting actions
                            string concat_id = row.doc_type_disc_id + _teamMembers[j].Item1;
                            var targetCell = _ExportWorksheet.Cells[CurrentRowIndex, teamStartIndex + j];
                            if (_actionMap.ContainsKey(concat_id) && !string.IsNullOrEmpty(_actionMap[concat_id]))
                            {

                                try
                                {
                                    string UserAction = _actionMap[concat_id];
                                    string ActionKey = (UserAction + ActionType).ToUpper();

                                    if (map.ContainsKey(ActionKey))
                                    {
                                        string res = map[ActionKey]; // Debugging variable here can delete this
                                        if (!string.IsNullOrWhiteSpace(res))
                                        {
                                            targetCell.Value = res;
                                            DeleteRow = false;
                                        }
                                    }
                                }
                                catch (KeyNotFoundException ex)
                                {
                                    Debug.WriteLine($"Missing key in 'map': concat_id = {concat_id}, ActionKey = {_actionMap[concat_id] + ActionType}");
                                    throw; // Optional: rethrow if you want to halt execution
                                }

                            }


                        }

                        // DELETE EMPTY ROWS
                        if (DeleteRow)
                        {
                            _ExportWorksheet.DeleteRow(CurrentRowIndex);
                            rowsDeleted++;
                        }
                    }

                    BatchStartIndex = BatchStartIndex + ((BatchLength*4) - rowsDeleted);

                }
            }

            int startRow = _ExportWorksheet.Dimension.Start.Row;
            int endRow = _ExportWorksheet.Dimension.End.Row;

            // Here just remove the "-" from NEP-NZT or any other with a "-"
            for (int row = startRow; row <= endRow; row++)
            {
                var cell = _ExportWorksheet.Cells[row, 10];
                if (cell.Value != null)
                {
                    cell.Value = cell.Value.ToString().Replace("-", "");
                }
            }

            // END
            string exportPath = Server.MapPath("~/App_Data/Excel/MasterExport.xlsx");
            FileInfo exportFile = new FileInfo(exportPath);
            package.SaveAs(exportFile);

        }
    }

    // Copy a batch down with a different scope
    protected void AddScope(int CopyStartIndex, int CurrentRowIndex,int DataOffset, string Scope, int BatchLength)
    {
        // Iterate through the batch length and copy the rows of the format batch
        // It's just copy's everything from the first batch, and just changes the scope
        for (int k = 0; k < BatchLength; k++)
        {

            for (int col = DataOffset + 1; col <= DataOffset + 6; col++)
            {
                _ExportWorksheet.Cells[CurrentRowIndex, col].Value = _ExportWorksheet.Cells[CopyStartIndex + k, col].Value;
            }

            _ExportWorksheet.Cells[CurrentRowIndex, 10].Value = Scope;
            CurrentRowIndex += 1;

        }
    }

    // Populates the listbox for the Export popup
    protected void BindExportList()
    {
        Export_DAL ExportDAL = new Export_DAL();
        _sup_dal = new Supplier_DAL();

        // Retrieve all companies

        List<Companies_Model> companies = ExportDAL.GetAllCompanies();
        ContractorList.DataSource = companies;
        ContractorList.ValueField = "Id";
        ContractorList.TextField = "Company";
        ContractorList.DataBind();
        foreach (ListEditItem item in ContractorList.Items)
        {
            item.Selected = true;
        }

        // Retrieve all suppliers
        List<Companies_Model> suppliers = _sup_dal.GetAllSuppliers();
        SupplierList.DataSource = suppliers;
        SupplierList.ValueField = "Id";
        SupplierList.TextField = "Company";
        SupplierList.DataBind();
        foreach (ListEditItem item in SupplierList.Items)
        {
            item.Selected = true;
        }


    }

}