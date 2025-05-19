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