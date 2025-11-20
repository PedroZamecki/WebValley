#!/bin/sh
# Simulate pre-commit hooks for testing/CI
# This runs the same checks as pre-commit would

set -e

echo "Running pre-commit checks..."
echo ""

echo "==> Restoring .NET tools..."
dotnet tool restore
echo "✓ .NET tools restored"
echo ""

echo "==> Checking code format..."
if dotnet format --verify-no-changes; then
    echo "✓ Code is properly formatted"
else
    echo "✗ Code formatting issues found"
    echo "  Run: dotnet format"
    exit 1
fi
echo ""

echo "==> Building project..."
if dotnet build --no-restore; then
    echo "✓ Build succeeded"
else
    echo "✗ Build failed"
    exit 1
fi
echo ""

echo "==> Running tests..."
if dotnet test --no-build; then
    echo "✓ All tests passed"
else
    echo "✗ Tests failed"
    exit 1
fi
echo ""

echo "✓ All pre-commit checks passed!"

