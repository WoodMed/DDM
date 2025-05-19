<%@ Page Title="" Language="C#" MasterPageFile="~/Root.master" AutoEventWireup="true" Inherits="technip" Codebehind="DistributionMatrix.aspx.cs" %>

<%@ Register Assembly="DevExpress.Web.ASPxSpreadsheet.v23.1, Version=23.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxSpreadsheet" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.ASPxTreeList.v23.1, Version=23.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxTreeList" TagPrefix="dx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="Server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="<%= ResolveUrl("~/Scripts/DistributionMatrix.js") %>"></script>
    <link href="<%= ResolveUrl("~/Content/Styles/DistributionMatrix.css") %>" rel="stylesheet" type="text/css" />

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="RightPanelContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageToolbar" runat="Server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PageContent" runat="Server">

    <div class="statusLabel">
            <asp:Label ID="StatusLabel" runat="server" Text="Select a folder to display a spreadsheet" Visible="true" />
    </div>

    <div class="ddm_menu">

        <div class="menu_button">
            <dx:ASPxComboBox ID="Teams_Dropdown" runat="server" CssClass="combobox menu_dropdown"  
                ValueType="System.Int32" ValueField="ID" TextFormatString="{0}" Width="100%" DropDownStyle="DropDownList"
                NullText="Select Team">
                <ClientSideEvents SelectedIndexChanged="onTeamSelected" />
                <Columns>
                    <dx:ListBoxColumn FieldName="Team" />
                </Columns>
            </dx:ASPxComboBox>
        </div>

        <div class="menu_button">
            <dx:ASPxButton ID="Save_Btn" runat="server" Text="Save  All Changes"
                AutoPostBack="False" CssClass="menu_button_style"
                EnableClientSideAPI="True"
                Theme="Material"
                RenderMode="Button">
                <ClientSideEvents Click="testData" />
            </dx:ASPxButton>
        </div>

        <div class="menu_button">
            <dx:ASPxButton ID="Discard_Btn" runat="server" Text="Discard All Changes"
                AutoPostBack="False" CssClass="menu_button_style"
                EnableClientSideAPI="True"
                Theme="Material"
                RenderMode="Danger">
                <ClientSideEvents Click="revertSpreadsheet" />
            </dx:ASPxButton>
        </div>

    </div>

    <div>
        <dx:ASPxCallbackPanel ID="CallbackPanel" runat="server" ClientInstanceName="clientCallbackPanel" OnCallback="Data_Callback">
            <PanelCollection>
                <dx:PanelContent>
                    <dx:ASPxSpreadsheet ID="Spreadsheet" ClientInstanceName="clientSpreadSheet" runat="server"
                        WorkDirectory="~/App_Data/Excel" ActiveTabIndex="0" Width="100%" Height="100%"
                        RibbonMode="None" ShowFormulaBar="True" ShowSheetTabs="False" ShowConfirmOnLosing="false">
                        <ClientSideEvents Init="function(s, e) {
                            var ssHeight = window.innerHeight;
                            s.SetHeight(ssHeight - 175);
                        }" />
                    </dx:ASPxSpreadsheet>
                </dx:PanelContent>
            </PanelCollection>
        </dx:ASPxCallbackPanel>

    </div>

    <!-- 
                    <RibbonTabs>
                            <dx:RibbonTab Text="Commands">
                            </dx:RibbonTab>
                    </RibbonTabs>
-->
</asp:Content>

