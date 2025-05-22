<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" CodeBehind="Report.aspx.cs" Inherits="BP_DistributionMatrix.Report" Title="Report" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
    <link href="<%= ResolveUrl("~/Content/Styles/Report.css") %>" rel="stylesheet" type="text/css" />
    <script src="<%= ResolveUrl("~/Scripts/TeamsV2/List.js") %>"></script>

</asp:Content>

 <asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">

          <div class="container">
                <div class="title">
                    <h1>Document Actions Report</h1>
                </div>
                <dx:ASPxLabel ID="ErrorLabel" runat="server" Text="Document Number Not Found"
                Font-Size="16px" Font-Bold="true"  ForeColor="red" />

              <!-- Title Pane -->
              <div class="pane">

                  <div class="leftside">
                    <div class="docsearch">
                        <div class="component">
                            <dx:ASPxLabel ID="DocumentLabel" runat="server" Text="Document Number: "
                            Font-Size="15px" Font-Bold="true" ForeColor="Black" />
                        </div>

                        <div class="component">
                            <dx:ASPxTextBox ID="DocumentInput" runat="server" Width="100%" Height="15px"
                            Font-Size="12px" />
                        </div>

                        <dx:ASPxButton CssClass="members_button" runat="server" Text="Search"
                        AutoPostBack="false" Style="height: 30px; width: 19px;"
                        OnClick="Search_BtnClick"/>
                    </div>

                    <div class="selectedinfo">
                            <dx:ASPxLabel ID="CompanyLabel" runat="server" Text="Contractor: "
                            Font-Bold="true" Font-Size="14px" ForeColor="White" />

                            <dx:ASPxLabel ID="DiscLabel" runat="server" Text="Disicpline: "
                            Font-Bold="true" Font-Size="14px" ForeColor="White" />

                            <dx:ASPxLabel ID="DocLabel" runat="server" Text="Document Type: "
                            Font-Bold="true" Font-Size="14px" ForeColor="White" />
                    </div>

                  </div>


                    <div class="search">

                        <div class="listbox">

                            <dx:ASPxLabel ID="ASPxLabel3" runat="server" Text="Approver"
                            Font-Bold="true" Font-Size="16px" ForeColor="White" />
                            <dx:ASPxListBox ID="ApproverBox" runat="server" Width="100%" 
                                Height="100%"
                                EnableCallbacks="false" AddScroll="true" Font-Bold="true">
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
                            <dx:ASPxListBox ID="InformationBox" runat="server" Width="100%" Height="100%" 
                                EnableCallbacks="false" AddScroll="true" Font-Bold="true">
                            </dx:ASPxListBox>
                        </div>

                        <div class="listbox">
                            <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="Review"
                            Font-Bold="true" Font-Size="16px" ForeColor="White" />
                            <dx:ASPxListBox ID="ReviewBox" runat="server" Width="100%" Height="100%" 
                                EnableCallbacks="false" AddScroll="true" Font-Bold="true">
                            </dx:ASPxListBox>
                        </div>
                    </div>

                  </div>

          </div>
</asp:Content>