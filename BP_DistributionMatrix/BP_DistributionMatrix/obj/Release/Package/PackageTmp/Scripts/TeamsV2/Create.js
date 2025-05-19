for (var i = 0; i < s.GetItemCount(); i++) {
    if (s.GetItem(i).selected) { // Check if item is selected
        selectedListBox.AddItem(s.GetItem(i).text, s.GetItem(i).value); // Add to second list box
    }
}

function PerformCallbackAndShowPopup(s, e) {
    callbackPanel.PerformCallback(); // Calls the server-side event first
}

function viewPage() {
    window.location.href = "List.aspx";
}

function ShowPopup(s, e) {
    window.scrollTo({ top: 0, behavior: "smooth" });
    popupAddMembers.Show();

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
