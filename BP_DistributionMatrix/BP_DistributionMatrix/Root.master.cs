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

public partial class Root : MasterPage
{
    public class Folder
    {
        public string ParentId { get; set; }
        public string id { get; set; }
        public string FolderName { get; set; }
    }

    public bool _isAuthenticated;

    public bool EnableBackButton { get; set; }

    protected void Page_Init(object sender, EventArgs e)
    {

        if (!IsPostBack)
        {
            string state = HelperClass.VerifySessionID();
            _isAuthenticated = (state == "failure") ? false : true;
        }
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

            // Retrieve folder data
            List<Companies_Model> companies = new List<Companies_Model>();
            RelsDocsJoin_DAL dal = new RelsDocsJoin_DAL();
            companies = dal.GetCompanies();

            List<Folder> folderList = companies
               .Select(f => new Folder
               {
                   ParentId = "",
                   id = f.id.ToString(),
                   FolderName = f.Company
               })
               .OrderBy(f => f.FolderName) // Sort alphabetically by FolderName
               .ToList();

            // Bind data to the ASPxTreeList
            ASPxTreeList1.DataSource = folderList; // Your data source
            ASPxTreeList1.KeyFieldName = "id";
            ASPxTreeList1.ParentFieldName = "ParentId";

            // Bind the data
            ASPxTreeList1.DataBind();

            Session["BoundData"] = ASPxTreeList1.DataSource;
        }

        else
        {

            ASPxTreeList1.DataSource = Session["BoundData"];
            ASPxTreeList1.DataBind();

        }

        LeftPanel.Visible = true;
        LeftPanelContent.Visible = true;
        LeftPanel.Collapsible = false;


        string currentPath = Request.Url.LocalPath;
        string queryString = Request.Url.Query;

        if (currentPath.StartsWith("/DistributionMatrix.aspx", StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrEmpty(queryString) || queryString.Contains("status=NoData")))
        {
            LeftPanel.Collapsible = false;
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