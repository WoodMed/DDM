using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Companies_Model
/// </summary>
public class SaveAudit_Model
{
    public string User_ID { get; set; }
    public string Username { get; set; }
    public string Doc_ID { get; set; }
    public string Action { get; set; }
    public string Date_Modified { get; set; }
    public string Contractor { get; set; }
}