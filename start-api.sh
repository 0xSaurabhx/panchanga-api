#!/bin/bash

# Panchanga API Launcher Script
# This script provides multiple ways to run the Panchanga API

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "🌙 Panchanga API Launcher"
echo "========================="

# Check if .NET runtime is available
if command -v dotnet &> /dev/null; then
    echo "✅ .NET runtime detected"
    DOTNET_AVAILABLE=true
else
    echo "❌ .NET runtime not found"
    DOTNET_AVAILABLE=false
fi

# Function to run portable version
run_portable() {
    if [ "$DOTNET_AVAILABLE" = true ]; then
        echo "🚀 Starting Panchanga API (Portable)"
        cd "$SCRIPT_DIR/publish/portable"
        exec dotnet PanchangaApi.dll "$@"
    else
        echo "❌ Cannot run portable version: .NET runtime not installed"
        echo "   Install .NET 8.0 runtime: https://dotnet.microsoft.com/download"
        exit 1
    fi
}

# Function to test if self-contained version works
test_self_contained() {
    cd "$SCRIPT_DIR/publish/linux-x64-fixed" 2>/dev/null || return 1
    
    # Quick test - try to get version info
    timeout 5s ./PanchangaApi --version >/dev/null 2>&1
    return $?
}

# Function to run self-contained version
run_self_contained() {
    echo "🚀 Starting Panchanga API (Self-Contained)"
    
    # Test if self-contained works first
    echo "🔍 Testing self-contained compatibility..."
    if ! test_self_contained; then
        echo "⚠️  Self-contained version is incompatible with this system"
        echo "   (This is a known issue on some Linux distributions)"
        
        if [ "$DOTNET_AVAILABLE" = true ]; then
            echo "🔄 Automatically switching to portable version..."
            echo ""
            run_portable "$@"
        else
            echo "❌ Cannot fallback: .NET runtime not available"
            echo "   Please install .NET 8.0 runtime:"
            echo "   curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore"
            exit 1
        fi
    else
        echo "✅ Self-contained version is compatible"
        cd "$SCRIPT_DIR/publish/linux-x64-fixed"
        exec ./PanchangaApi "$@"
    fi
}

# Function to run from source
run_from_source() {
    if [ "$DOTNET_AVAILABLE" = true ]; then
        echo "🚀 Starting Panchanga API (From Source)"
        cd "$SCRIPT_DIR/src/PanchangaApi"
        exec dotnet run "$@"
    else
        echo "❌ Cannot run from source: .NET SDK not installed"
        exit 1
    fi
}

# Parse command line arguments
case "${1:-auto}" in
    "portable"|"-p"|"--portable")
        shift
        run_portable "$@"
        ;;
    "self-contained"|"sc"|"-s"|"--self-contained")
        shift
        run_self_contained "$@"
        ;;
    "source"|"src"|"--source")
        shift
        run_from_source "$@"
        ;;
    "auto"|"")
        echo "🔍 Auto-detecting best option..."
        if [ "$DOTNET_AVAILABLE" = true ]; then
            echo "   Using portable version (most reliable)"
            run_portable "$@"
        else
            echo "   Attempting self-contained version"
            run_self_contained "$@"
        fi
        ;;
    "help"|"-h"|"--help")
        echo ""
        echo "Usage: $0 [option] [dotnet-args...]"
        echo ""
        echo "Options:"
        echo "  auto, (default)     Auto-detect best deployment method"
        echo "  portable, -p        Use portable deployment (requires .NET runtime)"
        echo "  self-contained, -s  Use self-contained deployment (with auto-fallback)"
        echo "  source, src         Run from source code (requires .NET SDK)"
        echo "  help, -h            Show this help message"
        echo ""
        echo "Examples:"
        echo "  $0                                    # Auto-detect and run"
        echo "  $0 portable                          # Use portable version"
        echo "  $0 -s --urls http://0.0.0.0:8080     # Self-contained (falls back if needed)"
        echo ""
        echo "Note: Self-contained mode automatically falls back to portable if incompatible"
        echo ""
        echo "Default URLs when running:"
        echo "  HTTP:  http://localhost:5000"
        echo "  HTTPS: https://localhost:5001"
        echo ""
        ;;
    *)
        echo "❌ Unknown option: $1"
        echo "   Use '$0 help' for usage information"
        exit 1
        ;;
esac
