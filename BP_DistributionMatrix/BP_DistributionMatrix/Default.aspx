﻿<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Root.master" CodeBehind="Default.aspx.cs" Inherits="BP_DistributionMatrix.Default" Title="" %>

<asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">
    <div class="text-content" runat="server" id="TextContent"></div>
</asp:Content>

<asp:Content ContentPlaceHolderID="LeftPanelContent" runat="server">
    <h3 class="section-caption contents-caption">Contents</h3>

    <dx:ASPxTreeView runat="server" ID="TableOfContentsTreeView" ClientInstanceName="tableOfContentsTreeView"
        EnableNodeTextWrapping="true" AllowSelectNode="true" Width="100%" SyncSelectionMode="None" DataSourceID="NodesDataSource">
        <Styles>
            <Elbow CssClass="tree-view-elbow" />
            <Node CssClass="tree-view-node" HoverStyle-CssClass="hovered" />
        </Styles>
        <ClientSideEvents NodeClick="function (s, e) { HideLeftPanelIfRequired(); }" />
    </dx:ASPxTreeView>

   <asp:XmlDataSource ID="NodesDataSource" runat="server" DataFile="~/App_Data/OverviewContents.xml" XPath="//Nodes/*" />
</asp:Content>