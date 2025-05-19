<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" CodeBehind="View.aspx.cs" Inherits="BP_DistributionMatrix.View" Title="View" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
    <link href="<%= ResolveUrl("~/Content/Styles/TeamsV2.css") %>" rel="stylesheet" type="text/css" />
    <script src="<%= ResolveUrl("~/Scripts/TeamsV2/Create.js") %>"></script>
</asp:Content>

 <asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">
    <div class="return">
        <dx:ASPxButton ID="Return_Btn" runat="server" Text="<- Return"
            AutoPostBack="False" CssClass="menu_button_style"
            EnableClientSideAPI="True"
            Theme="Material"
            RenderMode="Danger">
            <ClientSideEvents Click="viewPage" />
        </dx:ASPxButton>
    </div>
     <div class="container">

            <div class="create">
                <div class="title">
                    <h1>View Team</h1>
                </div>

                <div class="main">

                    <div class="main_component">
                        <dx:ASPxLabel ID="TeamLabel" runat="server" Text="Team Name: "
                        Font-Size="18px" Font-Bold="true" />
                    </div>

                    <div class="main_component">
                        <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="Members"
                        Font-Size="18px" Font-Bold="true" />
                        <dx:ASPxButton CssClass="members_button" runat="server" Text="Leave Team" 
                        AutoPostBack="false" OnClick="LeaveTeam_Click" Theme="Material" RenderMode="Danger"/>
                    </div>

                    <dx:ASPxGridView ID="TeamsGrid" runat="server" AutoGenerateColumns="False" KeyFieldName="Id"
                        Width="70%" AllowSelectByRowClick="false"  EnableCallBacks="true" CssClass="smallRow">
                        <SettingsBehavior AllowSelectByRowClick="false"/>
                        <Settings VerticalScrollBarMode="Visible" />
                        <SettingsPager Mode="ShowAllRecords" />

                        <Styles>
                                <Header BackColor="#F0F0F0" Font-Bold="true" Border-BorderWidth="1px" Border-BorderColor="#000" />
                                <Cell Border-BorderWidth="1px" Border-BorderColor="#000" />
                                <Table CssClass="scrollable-grid" Border-BorderWidth="1px" Border-BorderColor="#000"  /> 
                        </Styles>
                        <Columns>

                            <dx:GridViewDataTextColumn FieldName="Name" Caption="Name" Width="40%">
                                <HeaderStyle HorizontalAlign="Center" Font-Size="16px" />
                                <CellStyle HorizontalAlign="Center" Font-Size="14px" Font-Bold="true" />
                            </dx:GridViewDataTextColumn>

                        <dx:GridViewDataColumn FieldName="Role" Caption="Role" Width="30%">
                            <HeaderStyle HorizontalAlign="Center" Font-Size="16px" />
                            <CellStyle HorizontalAlign="Center" Font-Size="14px" />
                        </dx:GridViewDataColumn>

                        </Columns>
                    </dx:ASPxGridView>
                </div>

            </div>
        </div>
</asp:Content>