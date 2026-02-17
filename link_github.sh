#!/bin/bash

# Exit on error
set -e

# Initialize git if not already initialized
if [ ! -d ".git" ]; then
    git init
fi

# Add remote origin (force update if it already exists)
git remote remove origin 2>/dev/null || true
git remote add origin https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition.git

# Add all files and commit
if [ -z "$(git status --porcelain)" ]; then
    echo "No changes to commit."
else
    git add .
    git commit -m "Initial commit"
fi

# Push to main branch (create if it doesn't exist)
git branch -M main
git push -u origin main
