#!/bin/bash

# List of paths (both directories and files) to remove
DIRS_AND_FILES=(
    "code/TaskManagerDesktop/obj/"
    "code/TaskManagerDesktop/bin/"
    "code/TaskManager/bin/"
    "code/TaskManager/obj/"
    "code/TaskManager/bin/"
    "code/TaskManager.Tests/bin/"
    "code/TaskManager.Tests/obj/"
    "code/.vs/"
)

FILES=(
    "code/G4CapstoneProject.sln.DotSettings.user"
)

# Loop through and remove each directory
for dir in "${DIRS_AND_FILES[@]}"; do
    if [ -d "$dir" ]; then
        echo "Deleting directory: $dir"
        rm -rf "$dir"
    else
        echo "Skipping: $dir (not found)"
    fi
done

# Loop through and remove each file
for file in "${FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "Deleting file: $file"
        rm -f "$file"
    else
        echo "Skipping: $file (not found)"
    fi
done

echo "Cleanup complete!"
