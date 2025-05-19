<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" CodeBehind="List.aspx.cs" Inherits="BP_DistributionMatrix.List" Title="List" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
    <link href="<%= ResolveUrl("~/Content/Styles/TeamsV2.css") %>" rel="stylesheet" type="text/css" />
    <script src="<%= ResolveUrl("~/Scripts/TeamsV2/List.js") %>"></script>

</asp:Content>

 <asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">
    <div class="container">
        
        <div class="title">
            <h1>Your Teams</h1>
        </div>

        <dx:ASPxButton ID="Create_Btn" runat="server" Text="Create New Team"
            AutoPostBack="False" CssClass="menu_button_style"
            EnableClientSideAPI="True"
            Theme="Material"
            RenderMode="Button">
            <ClientSideEvents Click="createTeam" />
        </dx:ASPxButton>


        <div class="grid">

                <dx:ASPxGridView ID="TeamsGrid" runat="server" AutoGenerateColumns="False" KeyFieldName="Id"
                    Width="100%" AllowSelectByRowClick="true"
                    ClientSideEvents-SelectionChanged="OnSelectionChanged"
                    OnCustomButtonCallback="TeamsGrid_CustomButtonCallback"
                    OnCommandButtonInitialize="TeamsGrid_CommandButtonInitialize">

                <SettingsBehavior AllowSelectByRowClick="True"/>
                <Styles>
                        <Header BackColor="#336699" ForeColor="white" Font-Bold="true" Font-Size="14px" />
                        <Header Border-BorderWidth="1px" Border-BorderColor="#000" />
                        <Cell Border-BorderWidth="1px" Border-BorderColor="#000" />
                </Styles>
                <Columns>

                    <dx:GridViewDataTextColumn FieldName="Name" Caption="Team Name" Width="40%">
                        <HeaderStyle HorizontalAlign="Center" Font-Size="16px" />
                        <CellStyle HorizontalAlign="Center" Font-Size="14px" />
                    </dx:GridViewDataTextColumn>

                    <dx:GridViewDataTextColumn FieldName="Role" Caption="Role" Width="30%">
                        <HeaderStyle HorizontalAlign="Center" Font-Size="16px" />
                        <CellStyle HorizontalAlign="Center" Font-Size="14px" />
                    </dx:GridViewDataTextColumn>

                    <dx:GridViewDataTextColumn FieldName="MemberCount" Caption="Members" Width="15%">
                        <HeaderStyle HorizontalAlign="Center" Font-Size="16px" />
                        <CellStyle HorizontalAlign="Center" Font-Size="14px" />
                    </dx:GridViewDataTextColumn>

                    <dx:GridViewCommandColumn Caption=" " Width="15%">
                        <HeaderStyle HorizontalAlign="Center" Font-Size="16px" Border-BorderWidth="1px" Border-BorderColor="#000" />
                        <CellStyle Border-BorderWidth="1px" Border-BorderColor="#000" />
                        <CustomButtons>
                            <dx:GridViewCommandColumnCustomButton ID="Delete" Text="" Image-Url="~/Content/Images/delete.svg" Image-Width="16px" />
                        </CustomButtons>
                    </dx:GridViewCommandColumn>

                </Columns>
            </dx:ASPxGridView>

        </div>
    </div>
</asp:Content>