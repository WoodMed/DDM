using DevExpress.Web.ASPxSpreadsheet;
using System;
using System.Drawing;

namespace BP_DistributionMatrix {
    public class Global_asax : System.Web.HttpApplication {
        void Application_Start(object sender, EventArgs e) {
            DevExpress.Web.ASPxWebControl.CallbackError += new EventHandler(Application_Error);
            DevExpress.Security.Resources.AccessSettings.DataResources.SetRules(
                DevExpress.Security.Resources.DirectoryAccessRule.Allow(Server.MapPath("~/Content")),
                DevExpress.Security.Resources.UrlAccessRule.Allow()
            );
            DevExpress.Web.ASPxWebControl.CallbackError += new EventHandler(Application_Error);
            ASPxSpreadsheet.CellValueChanged += Spreadsheet_CellValueChanged;
        }

        void Application_End(object sender, EventArgs e) {
            // Code that runs on application shutdown
        }
    
        void Application_Error(object sender, EventArgs e) {
            // Code that runs when an unhandled error occurs
        }
    
        void Session_Start(object sender, EventArgs e) {
            // Code that runs when a new session is started
        }
    
        void Session_End(object sender, EventArgs e) {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.
        }

        static void Spreadsheet_CellValueChanged(object sender, DevExpress.XtraSpreadsheet.SpreadsheetCellEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Cell Changed");
            var curCell = e.Worksheet.Cells[e.Cell.RowIndex, e.Cell.ColumnIndex];
            var ActionIndex = 8;
            if (e.Cell.ColumnIndex == 9|| e.Cell.ColumnIndex < ActionIndex || e.Cell.RowIndex < 2)
            {
                curCell.Value = e.OldValue;
            }
            else
            {
                // Validation to allow only "I", "A", or "R"
                string newValue = curCell.Value.ToString().ToUpper();
                if (newValue == "I" || newValue == "A" || newValue == "R" || newValue == "")
                {
                    e.Worksheet.Cells[e.Cell.RowIndex, e.Cell.ColumnIndex].Value = newValue;
                    e.Worksheet.Cells[e.Cell.RowIndex, e.Cell.ColumnIndex].Fill.BackgroundColor = Color.Orange; // Highlight cell
                    e.Worksheet.Cells[e.Cell.RowIndex, 0].Fill.BackgroundColor = Color.Green; // Highlight cell
                }
                else
                {
                    curCell.Value = e.OldValue;
                }

                if (newValue == "A")
                {
                    int acount = int.Parse(e.Worksheet.Cells[e.Cell.RowIndex, 3].Value.ToString());
                    if (acount >= 1)
                    {
                        e.Worksheet.Cells[e.Cell.RowIndex, e.Cell.ColumnIndex].Fill.BackgroundColor = Color.Red;
                        System.Diagnostics.Debug.WriteLine("Did it");
                    }
                }
            }
        }
    }
}