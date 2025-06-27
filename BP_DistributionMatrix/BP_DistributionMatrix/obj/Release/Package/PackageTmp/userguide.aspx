<%@ Page Title="FusionLive Document Distribution System User Guide" Language="C#" MasterPageFile="~/Root.master" AutoEventWireup="true" CodeFile="userguide.aspx.cs" Inherits="userguide" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="LeftPanelContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="RightPanelContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PageToolbar" Runat="Server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PageContent" Runat="Server">

    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            color: #333;
        }
        p, li {
            margin-bottom: 10px;
        }
        ul, ol {
            margin-left: 20px;
        }
        code {
            background-color: #f4f4f4;
            padding: 2px 5px;
            border-radius: 3px;
        }
        pre {
            background-color: #f4f4f4;
            padding: 10px;
            border-radius: 5px;
            overflow-x: auto;
        }
        a {
            color: #0066cc;
            text-decoration: none;
        }
        .note {
            background-color: #e7f3fe;
            border-left: 4px solid #2196F3;
            padding: 10px;
            margin-bottom: 15px;
        }
    </style>

    <h1>User Guide for FusionLive Document Distribution System</h1>

    <h2>Introduction</h2>
    <p>Welcome to the FusionLive Document Distribution System, hosted at aim01.woodplc.com. This system streamlines document distribution management, replacing the previous Excel-based approach on SharePoint. This guide provides step-by-step instructions for navigating the system, managing document distribution preferences, and creating and managing teams.</p>

    <h2>Accessing the System</h2>
    <ol>
        <li><strong>Navigate to the System</strong>:
            <ul>
                <li>Open your web browser and go to aim01.woodplc.com.</li>
                <li>You will see the main interface with three primary menu options: <strong>Distribution Matrix</strong>, <strong>Manage Teams</strong>, and <strong>Action Report</strong>.</li>
            </ul>
        </li>
    </ol>

    <h2>Navigating the Distribution Matrix</h2>

    <h3>Accessing the Spreadsheet Tree View</h3>
    <ol>
        <li><strong>Locate the Tree View</strong>:
            <ul>
                <li>On the left side of the screen, find the <strong>Spreadsheets</strong> tree view.</li>
                <li>If the tree view is not visible, click the <strong>Distribution Matrix</strong> menu in the header panel to expand it.</li>
            </ul>
        </li>
        <li><strong>Tree View Structure</strong>:
            <ul>
                <li>The tree is divided into two high-level categories:
                    <ul>
                        <li><strong>Contractors</strong>: Includes companies like ALCATEL, COSTAIN, GENESIS ENERGIES, MARUBENI, SAIPEM, TECHNIP, TFMC, and WOOD ONSHORE HUMBER.</li>
                        <li><strong>Suppliers</strong>: Includes companies like Costain and TechnipFMC.</li>
                    </ul>
                </li>
            </ul>
        </li>
        <li><strong>Select a Contractor or Supplier</strong>:
            <ul>
                <li>Click on a contractor or supplier name to open the corresponding spreadsheet.</li>
                <li>The spreadsheet displays a list of <strong>Document Types</strong> and <strong>Disciplines</strong> for the selected entity.</li>
            </ul>
        </li>
    </ol>

    <h3>Understanding the Spreadsheet Layout</h3>
    <p>The spreadsheet provides details about document distribution, including:</p>
    <ul>
        <li><strong>Folder (Contractor/Supplier name)</strong>: e.g., COSTAIN</li>
        <li><strong>Team</strong>: e.g., Team Name (if selected)</li>
        <li><strong>Consolidator</strong>: The designated approver (e.g., Ian Johnson)</li>
        <li><strong>Columns</strong>:
            <ul>
                <li><strong>R (Review)</strong>: Indicates number of users assigned to review the document.</li>
                <li><strong>I (Information)</strong>: Indicates number of users receiving the document for information.</li>
                <li><strong>A (Approver)</strong>: Indicates the number of users responsible for approving the document (should only be one).</li>
            </ul>
        </li>
        <li><strong>Rows</strong>: Each row represents a unique combination of <strong>Discipline Code</strong> (e.g., CG for Commissioning) and <strong>Document Type</strong> (e.g., REG for Register, REP for Report), along with their descriptions and assigned users.</li>
    </ul>

    <p><strong>Example Spreadsheet</strong>:</p>
<img src="/Content/Images/spreadsheet_snippet_01.png" alt="Example Spreadsheet Snippet" style="max-width: 100%; margin: 10px 0; border: 1px solid #ddd; border-radius: 5px;">

    <h2>Managing Document Distribution Preferences</h2>

    <h3>Selecting a Team</h3>
    <ol>
        <li><strong>Team Selection Dropdown</strong>:
            <ul>
                <li>Above the spreadsheet, locate the dropdown menu to select a team you are a member of.</li>
                <li>This filters the spreadsheet to show only the document distribution preferences for the selected team, allowing you to manage distribution for yourself or team members.</li>
            </ul>
        </li>
    </ol>

    <h3>Assigning Distribution Preferences</h3>
    <ol>
        <li>In the spreadsheet, locate the row corresponding to the desired <strong>Document Type</strong> and <strong>Discipline</strong>.</li>
        <li>Add one of the following codes under your name or a team member’s name:
            <ul>
                <li><strong>A</strong>: Approver (only one approver per document type/discipline is allowed).</li>
                <li><strong>R</strong>: Review (indicates you wish to review the document).</li>
                <li><strong>I</strong>: Information (indicates you wish to receive the document for information).</li>
            </ul>
        </li>
    </ol>

    <h3>Saving Changes</h3>
    <ol>
        <li>After entering or editing a code (<code>A</code>, <code>R</code>, or <code>I</code>), click off the cell to confirm the change.</li>
        <li>The cell will turn <strong>orange</strong> to indicate an unsaved change.</li>
        <li>Click the <strong>Save Changes</strong> button to save your updates. The row will turn <strong>blue</strong> to confirm the change has been saved.</li>
        <li>If multiple consolidators/approvers are assigned to the same document type/discipline, the row will turn <strong>red</strong>, indicating a potential issue that needs resolution.</li>
    </ol>

    <h3>Discarding Changes</h3>
    <ul>
        <li>To revert any unsaved changes, click the <strong>Discard All Changes</strong> button. This will clear all orange-highlighted cells.</li>
    </ul>

    <h2>Managing Teams</h2>
    <ol>
        <li><strong>Access the Manage Teams Menu</strong>:
            <ul>
                <li>In the header panel, click <strong>Manage Teams</strong>.</li>
            </ul>
        </li>
        <li><strong>Team Management Options</strong>:
            <ul>
                <li><strong>Create a Team</strong>: Set up a new team to group users for easier document distribution management.</li>
                <li><strong>Join a Team</strong>: Add yourself to an existing team.</li>
                <li><strong>Leave a Team</strong>: Remove yourself from a team.</li>
                <li><strong>Manage Team Members</strong>: As a team lead, add or remove users from your team.</li>
            </ul>
        </li>
        <li><strong>Using Teams</strong>:
            <ul>
                <li>Teams allow you to manage document distribution for a group of users collectively, rather than individually.</li>
                <li>Select a team from the dropdown above the spreadsheet to view and edit distribution preferences for all team members.</li>
            </ul>
        </li>
    </ol>

    <h2>Generating an Action Report</h2>
    <ol>
        <li><strong>Access the Action Report Menu</strong>:
            <ul>
                <li>In the header panel, click <strong>Action Report</strong>.</li>
            </ul>
        </li>
        <li><strong>Search for Document Distribution</strong>:
            <ul>
                <li>Enter a full or partial <strong>document number</strong> that includes the <strong>Document Type Code</strong>, <strong>Discipline Code</strong>, and <strong>Contractor Code</strong>.</li>
                <li>Click the <strong>Search</strong> button.</li>
                <li>The system will display a report showing which users are set to receive the document for <strong>Approval (A)</strong>, <strong>Review (R)</strong>, or <strong>Information (I)</strong>.</li>
            </ul>
        </li>
    </ol>

    <h2>Tips for Effective Use</h2>
    <ul>
        <li><strong>Check for Errors</strong>: If a row turns <strong>red</strong>, review the document type/discipline to ensure only one approver is assigned.</li>
        <li><strong>Save Frequently</strong>: Always click <strong>Save Changes</strong> after making updates to ensure your preferences are recorded.</li>
        <li><strong>Use Teams for Efficiency</strong>: Create or join teams to streamline distribution management for groups of users.</li>
        <li><strong>Verify Document Details</strong>: When using the Action Report, ensure the document number includes the correct codes to get accurate results.</li>
    </ul>

    <h2>Troubleshooting</h2>
    <ul>
        <li><strong>Tree View Not Visible</strong>: Click the <strong>Distribution Matrix</strong> menu in the header panel to display the Spreadsheets tree.</li>
        <li><strong>Changes Not Saving</strong>: Ensure you click off the edited cell to confirm the change (cell turns orange), then click <strong>Save Changes</strong>.</li>
        <li><strong>Red Rows</strong>: Check for multiple approvers assigned to the same document type/discipline and resolve by keeping only one approver.</li>
    </ul>

    <h2>Support</h2>
    <p>For assistance, contact the system administrator or IT support team.</p>

    <div class="note">
        <p>This user guide provides a comprehensive overview of the FusionLive Document Distribution System. For additional help or to provide feedback, please contact your system administrator.</p>
    </div>

</asp:Content>

