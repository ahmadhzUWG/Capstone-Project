#!/bin/bash

# List of directories to remove
DIRS=(
    "code/TaskManagerDesktop/obj/"
    "code/TaskManagerDesktop/bin/"
    "code/TaskManager/bin/"
    "code/TaskManager/obj/"
    "code/TaskManager/bin/"
    "code/TaskManager.Tests/bin/"
    "code/TaskManager.Tests/obj/"
    "code/.vs/"
)

# Loop through and remove each directory
for dir in "${DIRS[@]}"; do
    if [ -d "$dir" ]; then
        echo "Deleting: $dir"
        rm -rf "$dir"
    else
        echo "Skipping: $dir (not found)"
    fi
done

echo "Cleanup complete!"
