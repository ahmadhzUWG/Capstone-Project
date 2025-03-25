function setupSearch(inputId, containerId) {
    var input = document.getElementById(inputId);
    if (!input) {
        console.error("Element with id '" + inputId + "' not found.");
        return;
    }
    input.addEventListener("keyup", function () {
        let searchValue = input.value.toLowerCase();
        let checkboxes = document.querySelectorAll("#" + containerId + " .employee-box");
        checkboxes.forEach(function (box) {
            let label = box.querySelector(".form-check-label").textContent.toLowerCase();
            box.style.display = label.includes(searchValue) ? "block" : "none";
        });
    });
}
function clearValidationError(fieldName) {
    var $field = $('[name="' + fieldName + '"]');
    if ($field.length) {
        $field.removeClass("input-validation-error");

        $field.trigger("change");
    }

    var $errorSpan = $('[data-valmsg-for="' + fieldName + '"]');
    $errorSpan.empty()
        .removeClass("field-validation-error")
        .addClass("field-validation-valid");
}

document.addEventListener("DOMContentLoaded", function () {
    var managerDropdown = document.getElementById("managerDropdown");
    if (managerDropdown) {
        managerDropdown.addEventListener("change", function () {
            let selectedManagerId = this.value;
            let employeeCheckboxes = document.querySelectorAll(".employee-box");

            employeeCheckboxes.forEach(function (box) {
                let checkbox = box.querySelector(".employee-checkbox");
                let userId = box.getAttribute("data-user-id");

                if (userId === selectedManagerId) {
                    checkbox.checked = false;
                    box.style.display = "none";
                } else {
                    box.style.display = "block";
                }
            });
        });
    }
});

document.querySelectorAll(".employee-checkbox").forEach(function (checkbox) {
    checkbox.addEventListener("change", function () {
        let userId = this.value;
        let managerOption = document.querySelector("#managerDropdown option[data-user-id='" + userId + "']");
        if (this.checked) {
            if (managerOption) {
                managerOption.style.display = "none";
                managerOption.setAttribute("data-removed", "true");
                if (managerOption.selected) {
                    document.getElementById("managerDropdown").value = "";
                }
            }
        } else {
            if (managerOption) {
                managerOption.style.display = "block";
                managerOption.setAttribute("data-removed", "false");
            }
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    setupSearch("searchBoxEmployees", "employeeContainer");
});

