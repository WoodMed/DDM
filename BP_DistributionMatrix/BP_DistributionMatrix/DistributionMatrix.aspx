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

        <dx:ASPxCallback ID="ExportCallback" runat="server" ClientInstanceName="exportCallback"
            OnCallback="Export_Btn_Click">
            <ClientSideEvents EndCallback="function(s, e) {
                console.log('Export complete');
                exportPopup.Hide();
                document.getElementById('downloadFrame').src = '/DownloadExcel.ashx';

                console.log('Ashx opened');


            }" />
        </dx:ASPxCallback>

        <div class="menu_button">
            <dx:ASPxButton ID="Export_Btn" runat="server" Text="Export Actions"
                AutoPostBack="False" CssClass="menu_button_style"
                EnableClientSideAPI="True"
                Theme="Material"
                RenderMode="Secondary">
                <ClientSideEvents Click="function(s, e) {
                    exportChoicePopup.Show();
                }" />
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

           <!-- EXPORT COMPANY CHOICE PANEL -->
         <dx:ASPxPopupControl 
             ID="ExportChoicePanel"
             runat="server" 
             ClientInstanceName="exportChoicePopup"
             PopupHorizontalAlign="WindowCenter"
             PopupVerticalAlign="WindowCenter"
             ShowCloseButton="true"
             Modal="true"
             AllowResize="true"
             Width="700px"
             HeaderText="Export Actions"
             CloseAction="CloseButton"
             PopupAnimationType="Fade"
             EnableClientSideAPI="true"
             EnableCallbackMode="false">

             <ContentCollection>
                 <dx:PopupControlContentControl>
                     <div style="display: flex;">

                            <!-- User Selection List -->
                            <dx:ASPxListBox ID="ContractorList" runat="server" Width="500px" Height="300px" SelectionMode="CheckColumn" ClientInstanceName="contractorList" Caption="Contractors" CaptionSettings-Position="Top" CaptionSettings-Font-Bold="true" EnableCallbackMode="false">
                            </dx:ASPxListBox>

                            <dx:ASPxListBox ID="SupplierList" runat="server" Width="500px" Height="300px" SelectionMode="CheckColumn" ClientInstanceName="supplierList" style="margin-left: 20px;" Caption="Suppliers" CaptionSettings-Position="Top" CaptionSettings-Font-Bold="true">
                             </dx:ASPxListBox>

                     </div>

                     <br />

                     <div style="display: flex; justify-content: center; align-items: center;">
                            <dx:ASPxButton
                             ID="btnAddSelectedMembers"
                             runat="server"
                             Text="Export Actions"
                             AutoPostBack="false">
                            <ClientSideEvents Click="function(s, e) {
                                var contractorItems = contractorList.GetSelectedItems();
                                var supplierItems = supplierList.GetSelectedItems();

                                var contractorData = Array.prototype.map.call(contractorItems, function(item) {
                                    return item.value + '|' + item.text;
                                });

                                var supplierData = Array.prototype.map.call(supplierItems, function(item) {
                                    return item.value + '|' + item.text;
                                });

                                document.getElementById('hfContractors').value = contractorData.join(',');
                                document.getElementById('hfSuppliers').value = supplierData.join(',');

                                exportCallback.PerformCallback();
                                exportChoicePopup.Hide();
                                exportPopup.Show();
                            }" />
                            </dx:ASPxButton>
                     </div>


                 </dx:PopupControlContentControl>
             </ContentCollection>
         </dx:ASPxPopupControl>

    <!-- EXPORT LOADING PANEL -->
    <dx:ASPxPopupControl 
    ID="popupAddMembers" 
    runat="server" 
    ClientInstanceName="exportPopup"
    PopupHorizontalAlign="WindowCenter"
    PopupVerticalAlign="WindowCenter"
    ShowCloseButton="false"
    CloseAction="None"
    Modal="true"
    AllowResize="false"
    Width="350px"
    HeaderText=""
    PopupAnimationType="Fade">

        <ContentCollection>
        <dx:PopupControlContentControl runat="server">
            <div class="export-container">
                <p>Exporting Spreadsheet...<br /> (This can take a minute)</p>
                <div class="spinner"></div>
            </div>
        </dx:PopupControlContentControl>

        </ContentCollection>
    </dx:ASPxPopupControl>

    <iframe id="downloadFrame" style="display:none;"></iframe>
<asp:HiddenField ID="hfContractors" runat="server" ClientIDMode="Static" />
<asp:HiddenField ID="hfSuppliers" runat="server" ClientIDMode="Static" />

</asp:Content>

