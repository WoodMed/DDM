<%@ Page Title="Team Management" Language="C#" MasterPageFile="~/Root.master" AutoEventWireup="true" Inherits="teams" Codebehind="teams.aspx.cs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
    <style>
        .split-container {
            display: flex;
            gap: 20px;
        }

        .left-pane, .right-pane, .audit {
            padding: 20px;
            margin: 20px;
            background: #fff;
            border-radius: 5px;
            /* box-shadow: 0 2px 4px rgba(0,0,0,0.1); */
            box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
        }

        .left-pane {
            width: 400px;
        }

        .right-pane {
            flex: 1;
        }

        .section-title {
            font-size: 1.2em;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .audit {
            clear: both;
        }
    </style>

    <script type="text/javascript">
        function onTeamSelectionChanged(s, e) {
            if (e.isSelected) {
                var key = s.GetRowKey(e.visibleIndex);
                // alert('Key is: ' + key);
                setHiddenFieldValue(key);
                callbackPanelTeamDetails.PerformCallback(key);
                callbackPanelAudit.PerformCallback(key);
            }
        }

        function setHiddenFieldValue(key) {
            ASPxHiddenField_teamId.Set("key", key);
            var value = ASPxHiddenField_teamId.Get("key");
            // alert('Changed Value to: ' + value);
        }

    </script>

</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="PageContent" runat="server">

    <dx:ASPxLabel ID="lblMessage" runat="server" ClientVisible="false" Style="color: red;" />

    <div class="split-container">
        <!-- Left Panel: Teams Grid -->
        <div class="left-pane">
            <div class="section-title">Teams</div>
            <dx:ASPxGridView ID="gridTeams" ClientInstanceName="gridTeams" runat="server" AutoGenerateColumns="False" KeyFieldName="Id"
                Width="100%"
                OnDataBinding="gridTeams_DataBinding"
                OnCustomButtonCallback="gridTeams_CustomButtonCallback"
                ClientSideEvents-SelectionChanged="onTeamSelectionChanged"
                >

                <SettingsBehavior AllowSelectByRowClick="True" AllowSelectSingleRowOnly="True" />
                <Columns>
                    <dx:GridViewDataTextColumn FieldName="Name" Caption="Team Name" />
                    <dx:GridViewDataTextColumn FieldName="MemberCount" Caption="Members" Width="60px" />
                    <dx:GridViewCommandColumn ShowClearFilterButton="false" Width="100px">
                        <CustomButtons>
                            <dx:GridViewCommandColumnCustomButton ID="Edit" Text="" Image-Url="~/Content/Images/edit.svg" Image-Width="16px" />
                            <dx:GridViewCommandColumnCustomButton ID="Delete" Text="" Image-Url="~/Content/Images/delete.svg" Image-Width="16px" />
                        </CustomButtons>
                    </dx:GridViewCommandColumn>
                </Columns>
            </dx:ASPxGridView>

            <br />
            <dx:ASPxButton ID="btnCreateTeam" runat="server" Text="Create New Team" Width="100%" OnClick="btnCreateTeam_Click" />
            <!-- Confirmation Popup for Delete -->
            <dx:ASPxPopupControl ID="popupConfirmDelete" runat="server" ClientInstanceName="popupConfirmDelete" Width="300px" HeaderText="Confirm Delete">
                <ContentCollection>
                    <dx:PopupControlContentControl>
                        <div style="padding: 10px;">
                            <dx:ASPxLabel ID="lblConfirmDeleteText" runat="server" Text="Are you sure you want to delete this team?" />
                            <br />
                            <br />
                            <dx:ASPxButton ID="btnConfirmDelete" runat="server" Text="Yes" AutoPostBack="true" OnClick="btnConfirmDelete_Click" />
                            <dx:ASPxButton ID="btnCancelDelete" runat="server" Text="No" AutoPostBack="false">
                                <ClientSideEvents Click="function(s,e){ popupConfirmDelete.Hide(); }" />
                            </dx:ASPxButton>
                        </div>
                    </dx:PopupControlContentControl>
                </ContentCollection>
            </dx:ASPxPopupControl>
        </div>
        <!-- Right Panel: Details + Members -->
        <div class="right-pane">
            <dx:ASPxCallbackPanel ID="callbackPanelTeamDetails" runat="server" ClientInstanceName="callbackPanelTeamDetails" OnCallback="callbackPanelTeamDetails_Callback">
                <PanelCollection>
                    <dx:PanelContent>
                        <asp:Panel ID="panelDetails" runat="server" Visible="true">
                            <div class="section-title">Team Details</div>

                            <dx:ASPxHiddenField ID="ASPxHiddenField_teamId" ClientInstanceName="ASPxHiddenField_teamId" runat="server"></dx:ASPxHiddenField>

                            <dx:ASPxLabel ID="lblTeamName" runat="server" Text="Team Name:" />
                            <dx:ASPxTextBox ID="txtTeamName" runat="server" Width="300px" />
                            <br />
                            <br />
                            <dx:ASPxLabel ID="lblCreatedBy" runat="server" />
                            <br />
                            <dx:ASPxLabel ID="lblDateCreated" runat="server" />
                            <br />
                            <br />
                            <dx:ASPxButton ID="btnSaveTeam" runat="server" Text="Save Team" OnClick="btnSaveTeam_Click" />


                            <dx:ASPxButton
                                ID="btnAddMembers"
                                runat="server"
                                Text="Add Members"
                                AutoPostBack="false">

                                <ClientSideEvents Click="function(s, e) {
                                    var strKey = 'key';
                                    var value = ASPxHiddenField_teamId.Get(strKey);

                                    if (!value) {
                                        alert('You must select a team');
                                        return;
                                    }

                                    // alert(var value);

                                    popupAddMembers.Show();

                                    // callbackPanelAddMembers.PerformCallback(value);
                                    
                                }" />
                            </dx:ASPxButton>

                            <br />
                            <br />
                            <div class="section-title">Members</div>
                            <dx:ASPxGridView ID="gridTeamMembers" runat="server" AutoGenerateColumns="False" KeyFieldName="Id"
                                OnDataBinding="gridTeamMembers_DataBinding"
                                OnCustomButtonCallback="gridTeamMembers_CustomButtonCallback"
                                ClientInstanceName="gridTeamMembers">
                                <SettingsBehavior AllowSelectByRowClick="False" />
                                <Columns>
                                    <dx:GridViewDataTextColumn FieldName="UserName" Caption="User" />
                                    <dx:GridViewDataTextColumn FieldName="Role" Caption="Role" Width="100px" />
                                    <dx:GridViewCommandColumn Width="150px">
                                        <CustomButtons>
                                            <dx:GridViewCommandColumnCustomButton ID="Promote" Text="Make Leader" />
                                            <dx:GridViewCommandColumnCustomButton ID="Remove" Text="Remove" />
                                        </CustomButtons>
                                    </dx:GridViewCommandColumn>
                                </Columns>
                            </dx:ASPxGridView>
                        </asp:Panel>
                    </dx:PanelContent>
                </PanelCollection>
            </dx:ASPxCallbackPanel>

            <!-- Add Members Popup -->
            <dx:ASPxPopupControl
                ID="popupAddMembers"
                runat="server"
                ClientInstanceName="popupAddMembers"
                Width="400px" HeaderText="Add Members">
                <ContentCollection>
                    <dx:PopupControlContentControl>
                        <dx:ASPxCallbackPanel
                            ID="callbackPanelAddMembers"
                            runat="server"
                            ClientInstanceName="callbackPanelAddMembers"
                            OnCallback="callbackPanelAddMembers_Callback">
                            <PanelCollection>
                                <dx:PanelContent>
                                    <dx:ASPxListBox ID="listAvailableUsers" runat="server" Width="100%" Height="300px" SelectionMode="CheckColumn">
                                        <FilteringSettings ShowSearchUI="true" />
                                    </dx:ASPxListBox>
                                    <br />
                                    <dx:ASPxButton
                                        ID="btnAddSelectedMembers"
                                        runat="server"
                                        Text="Add Selected"
                                        AutoPostBack="true"
                                        OnClick="btnAddSelectedMembers_Click" />
                                </dx:PanelContent>
                            </PanelCollection>
                        </dx:ASPxCallbackPanel>
                    </dx:PopupControlContentControl>
                </ContentCollection>
                <ClientSideEvents Shown="function(s, e) {
                    var teamId = ASPxHiddenField_teamId.Get('key');
                    callbackPanelAddMembers.PerformCallback(teamId);
                    }" />
            </dx:ASPxPopupControl>

        </div>
    </div>
    <!-- Audit Log Section -->
    <div class="audit">
        <dx:ASPxCallbackPanel ID="callbackPanelAudit" runat="server" ClientInstanceName="callbackPanelAudit" OnCallback="callbackPanelAudit_Callback">
            <PanelCollection>
                <dx:PanelContent>
                    <dx:ASPxPanel ID="panelAudit" runat="server" HeaderText="Audit Log" Width="100%" Visible="true">
                        <PanelCollection>
                            <dx:PanelContent>
                                <dx:ASPxGridView ID="gridAuditLog" runat="server" AutoGenerateColumns="False" Width="100%">
                                    <Columns>
                                        <dx:GridViewDataTextColumn FieldName="Timestamp" Caption="Timestamp" Width="150px" />
                                        <dx:GridViewDataTextColumn FieldName="Action" Caption="Action" Width="120px" />
                                        <dx:GridViewDataTextColumn FieldName="Details" Caption="Details" />
                                    </Columns>
                                </dx:ASPxGridView>
                            </dx:PanelContent>
                        </PanelCollection>
                    </dx:ASPxPanel>
                </dx:PanelContent>
            </PanelCollection>
        </dx:ASPxCallbackPanel>
    </div>

</asp:Content>