function createTeam(){
    window.location.href = "Create.aspx";
}

function OnSelectionChanged(s, e) {
    var selectedId = s.GetSelectedKeysOnPage()[0]; // Get the selected row's ID
    console.log("Hiii");
    if (selectedId) {
        window.location.href = "/TeamsV2/View.aspx?Id=" + selectedId; // Redirect client-side
    }
}

function onDeleteClicked(s, e) {

    var rowIndex = e.visibleIndex; // Get the row index
    s.GetRowValues(rowIndex, "Id", function (value) {
        if (value) {
            document.getElementById("PageContent_hiddenTeamId").value = value; // Store the ID
            deleteCheckPopup.Show(); // Display the popup after storing the ID
        }
    });

}

function onDelete(s, e) {
    deleteCheckPopup.Hide(); // Close the popup first
}

function onCancelDelete(s, e) {
    deleteCheckPopup.Hide();
}
