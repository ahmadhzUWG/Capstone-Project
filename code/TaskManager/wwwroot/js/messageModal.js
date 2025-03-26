// wwwroot/js/confirm-dialog.js

// Global variables to hold the current object type and id.
var currentObjectType = '';
var currentObjectId = '';

// Function to show the generic Bootstrap modal.
// It accepts an object type (like 'group', 'user', or 'project') and the corresponding id.
function showConfirmDialog(objectType, objectId) {
    currentObjectType = objectType;
    currentObjectId = objectId;
    var myModal = new bootstrap.Modal(document.getElementById('confirmModal'));
    myModal.show();
}

// When the DOM is fully loaded, wire up the event for the modal's Delete button.
document.addEventListener("DOMContentLoaded", function () {
    var confirmDeleteButton = document.getElementById('confirmDelete');
    if (confirmDeleteButton) {
        confirmDeleteButton.addEventListener('click', function () {
            console.log('Delete confirmed for ' + currentObjectType + ' id: ' + currentObjectId);
            // Hide the modal.
            var modalEl = document.getElementById('confirmModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            // Construct the form ID based on the object type and id, then submit the form.
            var formId = 'deleteForm-' + currentObjectType + '-' + currentObjectId;
            var deleteForm = document.getElementById(formId);
            if (deleteForm) {
                deleteForm.submit();
            }
        });
    }
});
