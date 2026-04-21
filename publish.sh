#!/bin/bash
# VM Workflow - Publish Script
# Produces a release build and zips it to ~/Desktop/lockci/

set -e

ROOT="$(cd "$(dirname "$0")" && pwd)"
OUT_DIR="$ROOT/publish-out"
DEST="$HOME/Desktop/lockci"
ZIPNAME="vm-workflow-$(date +%Y%m%d).zip"

echo "==> Cleaning previous output..."
rm -rf "$OUT_DIR"

echo "==> Publishing VMWorkflow.API (Release)..."
dotnet publish "$ROOT/src/VMWorkflow.API/VMWorkflow.API.csproj" \
    -c Release \
    -o "$OUT_DIR"

echo "==> Copying docs..."
cp -r "$ROOT/docs/"* "$OUT_DIR/" 2>/dev/null || true

echo "==> Writing startup script..."
cat > "$OUT_DIR/start.sh" << 'EOF'
#!/bin/bash
# Start VM Workflow
# Access at: http://localhost:5000
export ASPNETCORE_ENVIRONMENT=Production
cd "$(dirname "$0")"
./VMWorkflow.API
EOF
chmod +x "$OUT_DIR/start.sh"

echo "==> Zipping to $DEST/$ZIPNAME..."
mkdir -p "$DEST"
cd "$ROOT"
zip -r "$DEST/$ZIPNAME" publish-out/ -x "*.pdb"

echo "==> Cleaning up..."
rm -rf "$OUT_DIR"

echo ""
echo "Done! Published to: $DEST/$ZIPNAME"
