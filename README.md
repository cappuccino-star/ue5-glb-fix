# UE5 GLB Fix

修复部分 UE5.4 导出的 GLB 无法导入 Blender 的问题。借助 GPT 制作，分享给遇到类似问题的人。

## 使用

1. 下载 [Windows 64 位工具](GlbFix-Windows-x64.zip)，解压。
2. 将 GLB 文件拖到 `GlbFix.exe` 上，无需安装 Python 或 .NET。
3. 将生成的 `文件名_fixed.glb` 导入 Blender，检查模型。

**请保留原文件。修复会截断越界索引，或移除顶点属性数量不一致的网格片段，可能丢面。已有同名 `_fixed.glb` 会被覆盖。**

仅针对这两类问题，不保证解决所有 GLB 导入错误。适用于普通、未压缩、非稀疏的三角形 GLB；不支持其他复杂布局。

## 源码

Windows 版源码为 `Program.cs` 和 `GlbFix.csproj`，使用 .NET 8 SDK 构建：

```sh
dotnet publish GlbFix.csproj -c Release -r win-x64 --self-contained true -o publish
```



## English

A small tool made with GPT assistance for certain UE 5.4 GLB exports that fail to import into Blender. Download the Windows ZIP, extract it, and drag GLB files onto `GlbFix.exe`. Import the generated `_fixed.glb` into Blender.

Keep the original. Repair truncates out-of-range triangle indices or removes primitives with inconsistent vertex attribute counts. Geometry may be lost, and existing `_fixed.glb` outputs may be overwritten. This is a targeted workaround, not a general GLB validator. Inspect the result in Blender.

