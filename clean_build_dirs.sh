#!/bin/bash

echo "Cleaning up build artifacts..."

# List of directories and files to remove
TO_REMOVE=(
    "code/TaskManager/bin/"
    "code/TaskManager/obj/"
    "code/TaskManagerDesktop/bin/"
    "code/TaskManagerDesktop/obj/"
    "code/TaskManagerData/bin/"
    "code/TaskManagerData/obj/"
    "code/TaskManager.Tests/bin/"
    "code/TaskManager.Tests/obj/"
    "code/.vs/"
    "code/.DS_Store"
    "code/TaskManager/.DS_Store"
    "code/TaskManager/Views/.DS_Store"
    "Documentation/Class Diagrams/.DS_Store"
    "code/G4CapstoneProject.sln.DotSettings.user"
)

# Remove each file/directory
for item in "${TO_REMOVE[@]}"; do
    if [ -d "$item" ]; then
        echo "Deleting directory: $item"
        rm -rf "$item"
    elif [ -f "$item" ]; then
        echo "Deleting file: $item"
        rm -f "$item"
    else
        echo "Skipping: $item (not found)"
    fi
done

echo "✅ Cleanup complete!"

