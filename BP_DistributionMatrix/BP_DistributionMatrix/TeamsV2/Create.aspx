<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" CodeBehind="Create.aspx.cs" Inherits="BP_DistributionMatrix.Create" Title="Create" %>

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
                <h1> Create a Team</h1>
            </div>

            <div class="main">

                <!-- Team name -->
                <div class="main_component">
                    <dx:ASPxLabel ID="TeamLabel" runat="server" Text="Name:"
                    Font-Size="18px" Font-Bold="true" />
                    <dx:ASPxLabel ID="TeamErrorLabel" runat="server" Text="Please enter a value"
                    Font-Size="12px" ForeColor="red" />
                    <dx:ASPxTextBox ID="TeamInput" runat="server" Width="300px" />
                </div>
                
                <!-- Add Members -->
                <div class="main_component">
                    <div class="members">
                        <dx:ASPxLabel ID="MembersLabel" runat="server" Text="Members:"
                        Font-Size="18px" Font-Bold="true" />
                        <dx:ASPxButton CssClass="members_button" runat="server" Text="+ Add Member" 
                        AutoPostBack="false" ClientSideEvents-Click="ShowPopup" />
                    </div>

                <dx:ASPxGridView ID="TeamsGrid" runat="server" AutoGenerateColumns="False" KeyFieldName="Id"
                    Width="70%" AllowSelectByRowClick="false"  EnableCallBacks="true" CssClass="smallRow"
                    OnCustomButtonCallback="gridTeamMembers_CustomButtonCallback" OnCustomButtonInitialize="gridTeamMembers_CustomButtonInitialize">
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

                        <dx:GridViewCommandColumn Width="30%">
                            <HeaderStyle HorizontalAlign="Center" Font-Size="16px" />
                            <CellStyle Border-BorderWidth="1px" Border-BorderColor="#000" />
                            <CustomButtons>
                                <dx:GridViewCommandColumnCustomButton ID="Promote" Text="Make Leader" />
                                <dx:GridViewCommandColumnCustomButton ID="Remove" Text="Remove" />
                            </CustomButtons>
                        </dx:GridViewCommandColumn>

                    </Columns>
                </dx:ASPxGridView>
                </div>


                <!-- Create Button -->
                <div class="create_button">
                    <dx:ASPxButton runat="server" Text="Create Team" Width="200px" Height="50px"
                    CssClass="create_button_style"
                    Theme="Material"
                    RenderMode="Button" 
                    AutoPostBack="true"
                    OnClick="CreateTeamBtn_Click"/>
                </div>


                <!-- ADD MEMBERS POPUP -->
                <dx:ASPxPopupControl 
                    ID="popupAddMembers" 
                    runat="server" 
                    ClientInstanceName="popupAddMembers"
                    PopupHorizontalAlign="WindowCenter"
                    PopupVerticalAlign="WindowCenter"
                    ShowCloseButton="true"
                    Modal="true"
                    AllowResize="true"
                    Width="700px"
                    HeaderText="Add Members"
                    CloseAction="CloseButton"
                    PopupAnimationType="Fade">

                    <ContentCollection>
                        <dx:PopupControlContentControl>
                            <div style="display: flex;">
                                <!-- User Selection List -->
                                <dx:ASPxListBox ID="listAvailableUsers" runat="server" Width="500px" Height="300px" SelectionMode="CheckColumn" ClientInstanceName="listAvailableUsers" >
                                    <FilteringSettings ShowSearchUI="true" />
                                    <ClientSideEvents SelectedIndexChanged="OnIndexChange" />
                                </dx:ASPxListBox>


                                <div id="selectedUsersPanel" style="width: 100%; padding-left: 10px; border-left: 1px solid #ccc;">
                                    <dx:ASPxListBox 
                                        ID="listSelectedUsers" 
                                        runat="server" 
                                        ClientInstanceName="listSelectedUsers"
                                        Width="300px" 
                                        Height="300px" 
                                        EnableCallbackMode="false">
                                        <ItemStyle CssClass="selectedUserStyle" />
                                    </dx:ASPxListBox>
                                </div>
                            </div>

                            <br />

                            <dx:ASPxButton
                                ID="btnAddSelectedMembers"
                                runat="server"
                                Text="Add Selected"
                                AutoPostBack="true"
                                OnClick="btnAddSelectedMembers_Click" />
                        </dx:PopupControlContentControl>
                    </ContentCollection>
                </dx:ASPxPopupControl>

            </div>
        </div>
    </div>
</asp:Content>