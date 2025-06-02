using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using DevExpress.Web;
using System.Web.Security;

using System.Collections.Generic;

using System.Data;

using System.Linq;
using System.Data.SqlClient;
using System.Web.Configuration;
using DevExpress.Web.ASPxTreeList;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;
using System.Web.Services;
using DevExpress.XtraScheduler.Native;

public partial class Root : MasterPage
{
    public class Folder
    {
        public string id { get; set; }
        public string ParentId { get; set; }
        public string FolderName { get; set; }
    }

    public bool _isAuthenticated;

    public bool EnableBackButton { get; set; }

    protected void Page_Init(object sender, EventArgs e)
    {
        string state = HelperClass.VerifySessionID();
        _isAuthenticated = (state == "failure") ? false : true;
    }

    protected void Page_Load(object sender, EventArgs e)
    {

        Debug.WriteLine("??");

        if (!string.IsNullOrEmpty(Page.Header.Title))
            Page.Header.Title += "";
        Page.Header.Title = "HCCUK DDM";

        Page.Header.DataBind();
        UpdateUserMenuItemsVisible();

        if (!_isAuthenticated) return;

        HideUnusedContent();
        UpdateUserInfo();

        if (!IsPostBack)
        {
            BindTreeList();
        }

        else
        {
            ASPxTreeList1.DataSource = Session["BoundData"];
            ASPxTreeList1.DataBind();
        }

        LeftPanel.Visible = true;
        LeftPanelContent.Visible = true;
        LeftPanel.Collapsible = false;
        ASPxTreeList1.HtmlDataCellPrepared += ASPxTreeList1_HtmlDataCellPrepared;


        string currentPath = Request.Url.LocalPath;
        string queryString = Request.Url.Query;

        if (currentPath.StartsWith("/DistributionMatrix.aspx", StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrEmpty(queryString) || queryString.Contains("status=NoData")))
        {
            LeftPanel.Collapsible = false;
            ASPxTreeList1.Nodes[0].Expanded = true;
            ASPxTreeList1.Nodes[1].Expanded = true;
        }
        else if (currentPath.StartsWith("/DistributionMatrix.aspx", StringComparison.OrdinalIgnoreCase)) {
            LeftPanel.Collapsible = true;
        }
        else
        {
            LeftPanel.Collapsible = true;
        }

        Debug.WriteLine("");

        if (Session["TeamUsers"] != null &&
        !(Request.Url.AbsolutePath.Equals("/TeamsV2/Create.aspx", StringComparison.OrdinalIgnoreCase) ||
            Request.Url.AbsolutePath.StartsWith("/TeamsV2/Edit.aspx", StringComparison.OrdinalIgnoreCase)))
        {
            Session["TeamUsers"] = null;
        }
        else if (!currentPath.StartsWith("/DistributionMatrix.aspx", StringComparison.OrdinalIgnoreCase))
        {
            Session["CurrentReport"] = null;
            Session["CurrentDetails"] = null;
        }

    }

    protected void ASPxTreeList1_HtmlDataCellPrepared(object sender, TreeListHtmlDataCellEventArgs e)
    {
        if (e.Column.FieldName == "FolderName")
        {
            string folderName = e.CellValue.ToString();

            if (folderName == "Suppliers" || folderName == "Contractors")
            {
                e.Cell.Font.Bold = true; // Apply bold style
            }
        }
    }

    protected void BindTreeList()
    {
        // Retrieve folder data
        List<Companies_Model> companies = new List<Companies_Model>(); // Companies
        List<Companies_Model> suppliers = new List<Companies_Model>(); // Suppliers

        RelsDocsJoin_DAL dal = new RelsDocsJoin_DAL();
        Supplier_DAL sup_dal = new Supplier_DAL();
        companies = dal.GetCompanies();
        suppliers = sup_dal.GetSuppliers();
      

        // Contractor
        var contractorNode = new Folder
        {
            id = "0", 
            ParentId = null, 
            FolderName = "Contractors"
        };

        // Supplier
        var vendorNode = new Folder
        {
            id = "1",
            ParentId = null,
            FolderName = "Suppliers"
        };


        // Order by alphabetical
        List<Folder> folderList = companies
           .Select(f => new Folder
           {
               ParentId = "0",
               id = $"C-{f.id}",
               FolderName = f.Company
           })
           .Concat(suppliers.Select(f => new Folder
           {
               ParentId = "1",
               id = $"S-{f.id}",
               FolderName = f.Company
           }))
           .OrderBy(f => f.FolderName)
           .ToList();


        // Add the root node to the list
        folderList.Insert(0, contractorNode);
        folderList.Insert(1, vendorNode);


        ASPxTreeList1.DataSource = folderList;
        ASPxTreeList1.KeyFieldName = "id";
        ASPxTreeList1.ParentFieldName = "ParentId";
        ASPxTreeList1.DataBind();

        Session["BoundData"] = ASPxTreeList1.DataSource;
    }

    protected void Index_HtmlRowPrepared(object sender, DevExpress.Web.ASPxTreeList.TreeListHtmlRowEventArgs e)
    {
        try
        {
            object toolTip = e.GetValue("summaryHTML");
            if (toolTip != null)
            {
                e.Row.ToolTip = toolTip.ToString();
            }
        }
        catch
        {
            //do nothing
        }
    }
    protected void HideUnusedContent()
    {
        //LeftAreaMenu.Items[1].Visible = EnableBackButton;

        bool hasRightPanelContent = HasContent(RightPanelContent);
        RightAreaMenu.Items.FindByName("ToggleRightPanel").Visible = hasRightPanelContent;
        RightPanel.Visible = hasRightPanelContent;

        bool hasPageToolbar = HasContent(PageToolbar);
        PageToolbarPanel.Visible = hasPageToolbar;
    }

    protected bool HasContent(Control contentPlaceHolder)
    {
        if (contentPlaceHolder == null) return false;

        ControlCollection childControls = contentPlaceHolder.Controls;
        if (childControls.Count == 0) return false;

        return true;
    }

    protected void UpdateUserMenuItemsVisible()
    {
        RightAreaMenu.Items.FindByName("Profile").Visible = false; //we dont want this

        RightAreaMenu.Items.FindByName("SignInItem").Visible = !_isAuthenticated;
        RightAreaMenu.Items.FindByName("MyAccountItem").Visible = _isAuthenticated;
        RightAreaMenu.Items.FindByName("SignOutItem").Visible = _isAuthenticated;
    }

    protected void UpdateUserInfo()
    {
        var myAccountItem = RightAreaMenu.Items.FindByName("MyAccountItem");
        var lblUserName = (ASPxLabel)myAccountItem.FindControl("UserNameLabel");
        var lblFirstLastName = (ASPxLabel)myAccountItem.FindControl("FirstNameLastLabel");
        var lblEmail = (ASPxLabel)myAccountItem.FindControl("EmailLabel");
        var accountImage = (HtmlGenericControl)RightAreaMenu.Items[0].FindControl("AccountImage");

        lblUserName.Text = HelperClass.GetCookie("session_user").Value;
        lblEmail.Text = HelperClass.GetCookie("session_email").Value;


        if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
        {
            var avatarUrl = (HtmlImage)myAccountItem.FindControl("AvatarUrl");
            avatarUrl.Attributes["src"] = ResolveUrl("~/Content/Images/user.svg");
            accountImage.Style["background-image"] = ResolveUrl("~/Content/Images/user-in.svg");
        }
        else
        {
            var avatarUrl = (HtmlImage)myAccountItem.FindControl("AvatarUrl");
            avatarUrl.Attributes["src"] = ResolveUrl("~/Content/Images/user.svg");
            accountImage.Style["background-image"] = ResolveUrl("~/Content/Images/user-out.svg");
        }
    }

    protected void RightAreaMenu_ItemClick(object source, DevExpress.Web.MenuItemEventArgs e)
    {
        if (e.Item.Name == "SignOutItem")
        {
            HelperClass.ClearAllCookies(HttpContext.Current);
            Response.Redirect("~/Account/SignIn.aspx");
        }
    }

    // NAVBAR
    protected void ApplicationMenu_ItemDataBound(object source, MenuItemEventArgs e)
    {

        e.Item.Image.Url = string.Format("Content/Images/{0}.svg", e.Item.Text);
        e.Item.Image.UrlSelected = string.Format("Content/Images/{0}-white.svg", e.Item.Text);
        e.Item.Visible = true;

        if (!_isAuthenticated)
        {
            e.Item.Visible = false;
            var menuItem = LeftAreaMenu.Items.FindByName("Logo");
            menuItem.Visible = false;
        }

    }
}