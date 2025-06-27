<%@ WebHandler Language="C#" Class="DownloadExcel" %>

using System;
using System.IO;
using System.Web;

public class DownloadExcel : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        // Map the file path on the server
        string filePath = context.Server.MapPath("~/App_Data/Excel/MasterExport.xlsx");

        if (File.Exists(filePath))
        {
            context.Response.Clear();
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=MasterExport.xlsx");
            context.Response.TransmitFile(filePath);
            context.Response.Flush();
            // Instead of Response.End(), complete the request in a less disruptive way:
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        else
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Error: File not found.");
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}