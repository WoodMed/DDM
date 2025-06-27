// Change status label on page load
window.onload = function () {
    var folderId = new URLSearchParams(window.location.search).get("folderId");
    console.log(folderId);
};

function onCellValueChanged(s, e) {
    console.log("JAVASCRIPT CELL CHANGE");
}

// Save the spreadsheet to the db by posting to backend
function clickSaveAs(s, e) {
    // Simulate the click on "Save As"
    document.getElementById('PageContent_CallbackPanel_Spreadsheet_SSRC_T0G0I3').click();

    // Wait for the dialog box to load, then click "Download"
    setTimeout(function () {
        document.getElementById('PageContent_CallbackPanel_Spreadsheet_savefiledialog_MainPanel_dxssFormLayout_btnDownload_CD').click();
        document.getElementById('PageContent_CallbackPanel_Spreadsheet_savefiledialog_MainPanel_dxssFormLayout_rblTarget_RB1').click();
    }, 3000); // Adjust timeout as needed to allow the dialog to appear
}

function testData(s, e) {
    clientCallbackPanel.PerformCallback();
}

function revertSpreadsheet(s, e) {
    location.reload();
}


// Show the success label
function showSuccessLabel() {
    var label = document.getElementById("PageContent_StatusLabel");

    if (label) {
        label.style.display = "flex";
        label.style.transition = "none"
        label.innerText = "Spreadsheet saved!";
        label.style.color = "#76C576";
        label.style.opacity = "1";

        setTimeout(function () {
            label.style.transition = "opacity 2s ease-in-out";
            label.style.opacity = "0";
        }, 1000);
    }
}
// Show the error label
function showErrorLabel() {
    var label = document.getElementById("PageContent_StatusLabel");

    if (label) {
        label.style.display = "flex";
        label.style.transition = "none"
        label.innerText = "Error saving spreadsheet";
        label.style.color = "red";
        label.style.opacity = "1";
    }
}

function onTeamSelected(s, e) {
    // Get the selected item text and value
    var selectedTeam = s.GetSelectedItem().text;
    var selectedValue = s.GetSelectedItem().value;

    // Parse the current URL
    var currentUrl = window.location.href.split('?')[0]; // Base URL without query parameters
    var searchParams = new URLSearchParams(window.location.search); // Get current query parameters

    // Add or update the query parameter
    searchParams.set('TeamId', selectedValue);

    // Construct the new URL
    var newUrl = currentUrl + '?' + searchParams.toString();

    // Redirect to the updated URL
    window.location.href = newUrl;
}

function OnIndexChange(s, e) {
    var selectedListBox = ASPxClientControl.GetControlCollection().GetByName("listSelectedUsers");
    selectedListBox.ClearItems(); // Clear previous selections

    // Loop through all items in the main list box
    for (var i = 0; i < s.GetItemCount(); i++) {
        if (s.GetItem(i).selected) { // Check if item is selected
            selectedListBox.AddItem(s.GetItem(i).text, s.GetItem(i).value); // Add to second list box
        }
    }
}
