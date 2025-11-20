#!/bin/sh
# Auto-install pre-commit hooks if available
# This script is called automatically during build

# Check if we're in a git repository
if [ ! -d .git ]; then
    echo "Not in a git repository, skipping pre-commit hook installation"
    exit 0
fi

# Check if pre-commit hooks are already installed
if [ -f .git/hooks/pre-commit ] && grep -q "pre-commit" .git/hooks/pre-commit 2>/dev/null; then
    echo "✓ Pre-commit hooks already installed"
    exit 0
fi

# Try to install pre-commit hooks
if command -v pre-commit >/dev/null 2>&1; then
    echo "Installing pre-commit hooks..."
    if pre-commit install; then
        echo "✓ Pre-commit hooks installed successfully"
        echo "  Hooks will now run automatically on git commit"
    else
        echo "⚠ Failed to install pre-commit hooks"
        echo "  You can install manually with: pre-commit install"
    fi
else
    echo "⚠ pre-commit is not installed"
    echo "  Install with: pip install pre-commit"
    echo "  Then run: pre-commit install"
    echo "  Or use ./run-checks.sh to run checks manually"
fi

