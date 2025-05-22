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
                    OnCustomButtonInitialize="TeamsGrid_CommandButtonInitialize">

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

                    <dx:GridViewCommandColumn Caption="Action" Width="15%">
                        <HeaderStyle HorizontalAlign="Center" Font-Size="16px" Border-BorderWidth="1px" Border-BorderColor="#000" />
                        <CellStyle Border-BorderWidth="1px" Border-BorderColor="#000" />
                        <CustomButtons>
                            <dx:GridViewCommandColumnCustomButton ID="Delete" Text="" Image-Url="~/Content/Images/delete.svg" Image-Width="16px">
                            </dx:GridViewCommandColumnCustomButton>
                        </CustomButtons>
                    </dx:GridViewCommandColumn>

                </Columns>
                <ClientSideEvents CustomButtonClick="onDeleteClicked" />
            </dx:ASPxGridView>

        </div>

        <dx:ASPxPopupControl  
                ID="deleteCheckPopup"  
                runat="server"  
                ClientInstanceName="deleteCheckPopup"
                PopupHorizontalAlign="WindowCenter"
                PopupVerticalAlign="WindowCenter"
                ShowCloseButton="true"
                Modal="true"
                AllowResize="true"
                Width="350px"
                HeaderText="Confirm Deletion"
                CloseAction="CloseButton"
                PopupAnimationType="Fade">
    
            <ContentCollection>
                <dx:PopupControlContentControl runat="server">
                    <dx:ASPxLabel ID="DeleteLabel" runat="server" Text="Are you sure you want to delete this team?" Font-Size="15px"/>
            
                    <div style="text-align: center; margin-top: 20px;">
                        <dx:ASPxButton ID="DeleteButton" runat="server" Text="Delete"
                        AutoPostBack="true" OnClick="ConfirmDelete_Click"
                        ClientSideEvents-Click="onDelete" Theme="Material"
                        RenderMode="Danger" Style="margin-right: 30px;" />

                        <dx:ASPxButton ID="CancelButton" runat="server" Text="Cancel" 
                            AutoPostBack="false" ClientSideEvents-Click="onCancelDelete" Theme="Material"
                            RenderMode="Button"/>
                    </div>
                </dx:PopupControlContentControl>
            </ContentCollection>
        </dx:ASPxPopupControl>
    </div>

     <input type="hidden" id="hiddenTeamId" runat="server" />
</asp:Content>