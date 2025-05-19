using DevExpress.Web;
using DevExpress.Web.ASPxTreeList;
using DevExpress.Spreadsheet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data;
using System;
using DevExpress.XtraRichEdit.Export.OpenDocument;
using System.Web;
using System.Runtime.Remoting.Contexts;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Reflection.Emit;
using DevExpress.Data.Filtering;
using System.IO;
using DevExpress.Web.ASPxSpreadsheet;
using System.Web.Services;
using System.Globalization;
using DevExpress.XtraSpreadsheet.Export.Xlsb;
using DevExpress.XtraExport.Implementation;
using DevExpress.Office.Utils;
using DevExpress.Web.ASPxThemes;
using DevExpress.XtraRichEdit.Model;
using System.Text.Json;
using System.Drawing;
public partial class technip : System.Web.UI.Page
{

    // Class Variables
    private Worksheet _worksheet;
    private string _username;
    private int _columnToUnlock = 5; // ctrl f worksheet.Unprotect("") on change update accordingly there
    private int _headerRow = 1;
    private int _userId;
    private List<Tuple<int, string>> _userTeams;
    private List<Tuple<int, string>> _teamMembers;
    private Dictionary<string, string> _actionMap;
    List<RelsDocsJoin_Model> _spreadsheetData;
    RelsDocsJoin_DAL _dal;
    string _companyId;
    int _CurrentTeamId;
    string _CurrentTeamName;
    string _filepath;
    List<int> _previousSaves;
    private int _teamStartIndex;

    public class SpreadsheetRow
    {
        public string docId { get; set; }
        public List<string> action { get; set; } // Adjusted to match JSON structure
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string status = Request.QueryString["status"];
        if (status != null) StatusLabel.Text = "No data for this company at the moment, please select another company";
        _teamStartIndex = 10;
        // Check if theres no folderid to hide buttons and dont open spreadsheet
        _companyId = Request.QueryString["folderId"];
        if (_companyId != null) Session["folderId"] = int.Parse(_companyId);
        if (_companyId == null)
        {
            ToggleVisible(false);
            return;
        }

        // Initialise Dal
        _dal = new RelsDocsJoin_DAL();

        // Verify FolderId
        if (!_dal.CheckCompanyId(int.Parse(_companyId))) Response.Redirect("/DistributionMatrix.aspx?status=NoData");

        // Get UserId
        HttpContext context = HttpContext.Current;
        HttpCookie userIdCookie = HelperClass.GetCookie("session_userID");
        _userId = int.Parse(userIdCookie.Value);

        // Get User Teams
        _userTeams = _dal.GetUserTeams(_userId);
        _userTeams.Insert(0, new Tuple<int, string>(-1, "NO TEAM"));
        _userTeams.Insert(1, new Tuple<int, string>(-2, "ALL USERS"));

        var queryString = Request.QueryString["TeamId"];
        _CurrentTeamId = queryString == null ? _userTeams[0].Item1 : int.Parse(queryString);
        if (_CurrentTeamId == null) _CurrentTeamId = _userTeams[0].Item1;
        _CurrentTeamName = "Error";
        foreach (var tuple in _userTeams)
        {
            if (tuple.Item1 == _CurrentTeamId)
            {
                _CurrentTeamName = tuple.Item2;
                break; // Exit the loop once the ID is found
            }
        }

        if(_CurrentTeamId != -2)
        {
            _teamMembers = _dal.GetTeamMembers(_userId, _CurrentTeamId);
        }
        else
        {
            _teamMembers = _dal.GetAllUsers(_userId);
        }

        //_teamMembers = (_CurrentTeamId == -2) ? _dal.GetAllUsers(_userId) : _dal.GetTeamMembers(_userId, _CurrentTeamId);

        // Set Buttons to visible
        ToggleVisible(true);

        if (!IsCallback)
        {

            // Retrieve Data and Populate
            Prepare_SpreadsheetV2(_companyId);
            PrepareComboBox();
            PopulateSpreadsheet();
        }

        Debug.WriteLine("");
    }

    protected void PopulateSpreadsheet()
    {
        Spreadsheet.Document.BeginUpdate();

        _spreadsheetData = _dal.GetAllRows(_userId, _companyId);
        _actionMap = _dal.GetTeamActions(_teamMembers, _companyId);
        // Populate Spreadsheet
        int RowIndex = _headerRow + 1;
        int ColumnOffset = 3;
        var TeamCount = _teamMembers.Count;
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

        // Populate the total IRA Count
        List<int> IRACount = _dal.GetTotalIRACount(_companyId);
        _worksheet.Cells["B2"].Value = IRACount[0];
        _worksheet.Cells["C2"].Value = IRACount[1];
        _worksheet.Cells["D2"].Value = IRACount[2];

        Spreadsheet.Document.EndUpdate();
    }


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
    protected void Prepare_SpreadsheetV2(string folderid)
    {
        Spreadsheet.Document.BeginUpdate();
        Spreadsheet.ConfirmOnLosingChanges = "false";
        Spreadsheet.ShowConfirmOnLosingChanges = false;

        _filepath = Server.MapPath("~/App_Data/Excel/SpreadsheetTemplate.xlsx");
        Spreadsheet.Open(_filepath);
        Spreadsheet.Document.LoadDocument(_filepath);
        _worksheet = Spreadsheet.Document.Worksheets[0];
        _worksheet.DataValidations.Clear();

        _worksheet.FreezePanes(1, 7);
        _worksheet.ActiveView.Zoom = 10;
        RelsDocsJoin_DAL dal = new RelsDocsJoin_DAL();

        _worksheet.Columns["A"].ColumnWidth = 10;
        // Folder Name
        _worksheet.Cells["E1"].Value += " " + dal.GetCompany(folderid).Company;
        _worksheet.Cells["E1"].Value += Environment.NewLine + "Team: " + _CurrentTeamName;


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
        var filterRange = _worksheet.Range.FromLTRB(0, 1, _teamStartIndex-1+_teamMembers.Count(), 10000);
        _worksheet.AutoFilter.Apply(filterRange);

        _worksheet.DataValidations.Clear();

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

    protected void ToggleVisible(bool visibility)
    {
        Save_Btn.Visible = visibility;
        Discard_Btn.Visible = visibility;
        Spreadsheet.Visible = visibility;
        Teams_Dropdown.Visible = visibility;
        StatusLabel.Visible = !visibility;
    }

    protected void Data_Callback(object sender, DevExpress.Web.CallbackEventArgsBase e)
    {
        try
        {
            _worksheet = Spreadsheet.Document.Worksheets[0];
            int lastRow = _worksheet.GetUsedRange().BottomRowIndex;
            List<UserRels_Model> ActionsToSave = new List<UserRels_Model>();
            _previousSaves = new List<int>();
            int row = 2;
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
                    company_id = int.Parse(_companyId),
                    user_id = _userId,
                    value = _worksheet.Cells[row, 8].Value.ToString(),
                };
                ActionsToSave.Add(rels);
                _previousSaves.Add(docid);

                // Get Team Actions
                for (int i = 0; i < _teamMembers.Count(); i++)
                {
                    UserRels_Model team_rels = new UserRels_Model()
                    {
                        doc_type_disc_id = int.Parse(_worksheet.Cells[row, 0].Value.ToString()),
                        company_id = int.Parse(_companyId),
                        user_id = _teamMembers[i].Item1,
                        value = _worksheet.Cells[row, _teamStartIndex+i].Value.ToString(),
                    };

                    ActionsToSave.Add(team_rels);
                }

                row++;
            }

            Debug.WriteLine("");
            _dal.SaveActions(ActionsToSave);
            Prepare_SpreadsheetV2(_companyId);
            PopulateSpreadsheet();
        }

        catch (Exception ex)
        {
            Debug.WriteLine("Error saving" + ex.Message);
        }

    }
}