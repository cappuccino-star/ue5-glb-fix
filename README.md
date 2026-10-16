# UE5 GLB Fix

A small tool made with GPT assistance to repair certain UE 5.4 GLB exports that fail to import into Blender. Shared for anyone facing the same issue.

## Usage

1. Download the [Windows x64 tool](GlbFix-Windows-x64.zip) and extract it.
2. Drag one or more GLB files onto `GlbFix.exe`. No Python or separate .NET installation is required.
3. Import the generated `filename_fixed.glb` into Blender and inspect the result.

**Keep your original files. Repair may remove geometry. Existing `_fixed.glb` outputs may be overwritten.**

The download is the original Windows build; its console messages are in Chinese.

## What it fixes

- Out-of-range indices: truncates indices from the first invalid triangle onward.
- Inconsistent vertex attribute counts: removes the affected primitive.

This is a targeted workaround, not a general GLB validator. It assumes ordinary, uncompressed, non-sparse triangle GLB files and does not support other complex layouts. A no-repair message does not guarantee a valid file or successful Blender import.

## Build

Install the .NET 8 SDK, then run:

```sh
dotnet publish GlbFix.csproj -c Release -r win-x64 --self-contained true -o publish
```

## Feedback

Please include your UE and Blender versions and the import error when reporting an issue. Only share models you have permission to publish.
