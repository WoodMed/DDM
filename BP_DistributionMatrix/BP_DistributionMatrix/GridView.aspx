<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" CodeBehind="GridView.aspx.cs" Inherits="BP_DistributionMatrix.GridView" Title="GridView" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">

                <dx:ASPxPopupControl 
                    ID="popupAddMembers" 
                    runat="server" 
                    ClientInstanceName="popupAddMembers"
                    PopupHorizontalAlign="WindowCenter"
                    PopupVerticalAlign="WindowCenter"
                    ShowCloseButton="true"
                    Modal="true"
                    AllowResize="true"
                    Width="400px"
                    HeaderText="Add Members"
                    CloseAction="CloseButton"
                    PopupAnimationType="Fade">
    
                    <ContentCollection>
                        <dx:PopupControlContentControl>
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
                        </dx:PopupControlContentControl>
                    </ContentCollection>
                </dx:ASPxPopupControl>

                <dx:ASPxButton 
                ID="ASPxButton1" 
                runat="server" 
                Text="Show Popup" 
                AutoPostBack="false"
                ClientSideEvents-Click="function(s, e) { popupAddMembers.Show(); }" />

</asp:Content>