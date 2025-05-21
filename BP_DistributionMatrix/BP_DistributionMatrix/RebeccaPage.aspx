<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" CodeBehind="RebeccaPage.aspx.cs" Inherits="BP_DistributionMatrix.RebeccaPage" Title="RebeccaPage" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
    <link href="<%= ResolveUrl("~/Content/Styles/Report.css") %>" rel="stylesheet" type="text/css" />
    <script src="<%= ResolveUrl("~/Scripts/TeamsV2/List.js") %>"></script>

</asp:Content>

 <asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">

          <div class="container">
                <div class="title">
                    <h1>Document Actions Report</h1>
                </div>

              <!-- Title Pane -->
              <div class="pane">

                    <div class="component">
                        <dx:ASPxLabel ID="DocumentLabel" runat="server" Text="Document Number: "
                        Font-Size="15px" Font-Bold="true" ForeColor="Black" />
                    </div>

                    <div class="component">
                        <dx:ASPxTextBox ID="TeamInput" runat="server" Width="100%" Height="15px"
                        Font-Size="12px" />
                    </div>

                    <dx:ASPxButton CssClass="members_button" runat="server" Text="Search"
                    AutoPostBack="false" ClientSideEvents-Click="ShowPopup" Style="height: 30px;" />

                    <div class="search">

                        <div class="listbox">

                            <dx:ASPxLabel ID="ASPxLabel3" runat="server" Text="Approver"
                            Font-Bold="true" Font-Size="16px" ForeColor="White" />
                            <dx:ASPxListBox ID="ASPxListBox3" runat="server" Width="100%" Height="100%" SelectionMode="CheckColumn" ClientInstanceName="listAvailableUsers" >
                                <ClientSideEvents SelectedIndexChanged="OnIndexChange" />
                             </dx:ASPxListBox>

                      </div>

                    </div>

              </div>

                            <!-- Main Pane with Information and Reviewers -->
              <div class="pane">

                  <div class="databox">

                    <div class="info_and_review">
                        <div class="listbox">

                            <dx:ASPxLabel ID="labelAvailableUsers" runat="server" Text="Information"
                            Font-Bold="true" Font-Size="16px" ForeColor="White" />
                            <dx:ASPxListBox ID="listAvailableUsers" runat="server" Width="100%" Height="100%" SelectionMode="CheckColumn" ClientInstanceName="listAvailableUsers" >
                                <ClientSideEvents SelectedIndexChanged="OnIndexChange" />
                                </dx:ASPxListBox>

                        </div>

                        <div class="listbox">

                        <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="Review"
                        Font-Bold="true" Font-Size="16px" ForeColor="White" />
                        <dx:ASPxListBox ID="ASPxListBox1" runat="server" Width="100%" Height="100%" SelectionMode="CheckColumn" ClientInstanceName="listAvailableUsers">
                            <ClientSideEvents SelectedIndexChanged="OnIndexChange" />
                            </dx:ASPxListBox>

                        </div>
                    </div>

                  </div>

          </div>
</asp:Content>