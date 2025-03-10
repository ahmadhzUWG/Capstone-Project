function setupDropdownSearch(inputId, dropdownId) {
    var input = document.getElementById(inputId);
    var dropdown = document.getElementById(dropdownId);

    var originalOptions = Array.from(dropdown.querySelectorAll("option"));

    input.addEventListener("keyup", function () {
        var searchValue = input.value.toLowerCase();

        var filteredOptions = originalOptions.filter(function (option) {
            return option.getAttribute("data-removed") !== "true" &&
                option.text.toLowerCase().includes(searchValue);
        });

        dropdown.innerHTML = "";

        if (filteredOptions.length === 0) {
            var noOption = document.createElement("option");
            noOption.text = "No matching managers";
            noOption.disabled = true;
            dropdown.add(noOption);
        } else {
            filteredOptions.forEach(function (option) {
                dropdown.add(option);
            });
        }
    });
}


function setupSearch(inputId, containerId) {
    var input = document.getElementById(inputId);
    input.addEventListener("keyup", function () {
        let searchValue = input.value.toLowerCase();
        let checkboxes = document.querySelectorAll("#" + containerId + " .employee-box");
        checkboxes.forEach(function (box) {
            let label = box.querySelector(".form-check-label").textContent.toLowerCase();
            box.style.display = label.includes(searchValue) ? "block" : "none";
        });
    });
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
    setupDropdownSearch("searchBoxManagers", "managerDropdown");
    setupSearch("searchBoxEmployees", "employeeContainer");
});
