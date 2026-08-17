# Building CarboLifeCalc — the "Duo" setup

One source tree, two builds. Every project multi-targets, so a single build of the
solution produces both sets of binaries side by side.

| Target framework | Revit versions | Rhino | Charting stack             |
| ---------------- | -------------- | ----- | -------------------------- |
| `net48`          | 2023 - 2024    | 7     | LiveCharts2 + SkiaSharp 2.88.9 |
| `net8.0-windows` | 2025 and up    | 7     | LiveCharts2 + SkiaSharp 3.119.0 |

Revit 2024 is the last Revit on .NET Framework 4.8, and Revit 2025 the first on .NET.
The split is a Revit-generation split, not just a framework one.

## Building

```bash
msbuild "CarboLifeCalcNET8.sln" /t:Rebuild /p:Configuration=Release /p:Platform=x64 /m
```

Use Visual Studio's `MSBuild.exe` rather than `dotnet build` — `CarboLifeAPI` has
`COMReference` items (Excel interop) that need the .NET Framework MSBuild to resolve.

`Debug`/`Release` × `x64`/`Any CPU` all build. Both target frameworks are built by each
invocation; to build just one, add `/p:TargetFramework=net48`.

### Prerequisites

- Visual Studio 2022 or newer with the .NET desktop workload, and the .NET 8 SDK
- **Revit 2023** and **Revit 2025** installed — the builds reference their API
  assemblies directly out of `C:\Program Files\Autodesk\Revit <year>\`
- **Rhino 7** for `CarboCroc`

## Output directories

`$(TargetFramework)` is appended to every output path, so the two builds never collide.
The interesting one is the application folder, where all projects deposit their
binaries plus the data files:

```
CarboLifeCalc\bin\x64\Release\net48\            <- Revit 2023-2024 + standalone exe
CarboLifeCalc\bin\x64\Release\net8.0-windows\   <- Revit 2025+ + standalone exe
```

Same pattern for `bin\Release\`, `bin\Debug\` and `bin\x64\Debug\`.

## Where the two builds diverge

Almost nothing is conditional. The whole divergence is:

### `Directory.Build.props`

Sets `$(RevitApiDir)` — Revit 2023 for `net48`, Revit 2025 for `net8.0-windows` — which
every `RevitAPI` / `RevitAPIUI` / `AdWindows` hint path goes through. Also forces the 4.8
build to x64 (`Prefer32Bit false`), raises `LangVersion` (the SDK would otherwise default
.NET Framework to C# 7.3), and turns on generated binding redirects.

### `Shared\RevitCompat.cs`

Revit 2024 widened element ids to 64 bit, adding `ElementId(Int64)` and `ElementId.Value`.
Revit 2023 has neither. Ids are stored as `Int64` throughout `CarboLifeAPI` to match the
newer API, and this file narrows at the Revit boundary — `long.ToElementId()` and
`ElementId.LongValue()`. Linked into `CarboLifeRevit` and `CarboCircle`.

Compiling against 2023 still yields a binary that runs on 2024, because 2024 kept the
32-bit members.

### `Shared\IsExternalInit.cs`

C# 9 `record` types (`RevitActivator`) need this attribute; .NET Framework does not ship
it. Linked into every project for `net48` by `Directory.Build.props`.

### `CarboLifeUI\NativeDependencies.cs`

`Preload()`, compiled to a no-op on .NET 8. Called from `CarboLifeApp.OnStartup`,
`CarboCircleApp.OnStartup` and `MainWindow`'s constructor — whichever runs first does the
work. Two problems it solves for the 4.8 build:

1. Windows resolves `DllImport("libSkiaSharp")` against the *process* directory (Revit's
   install folder), the system directories and PATH — never the folder holding the
   assembly. It loads the native by absolute path instead.
2. Binding redirects do not reach a Revit add-in: the CLR reads `Revit.exe.config`, so the
   redirects in `CarboLifeCalc.exe.config` never apply. SkiaSharp binds against
   `System.Runtime.CompilerServices.Unsafe 4.0.4.1` while the folder ships 6.0.0.0, which
   surfaces as a `FileNotFoundException` inside `SKObject`'s type initializer and *looks*
   like "libSkiaSharp cannot be loaded". An `AssemblyResolve` handler resolves by simple
   name from the add-in folder. It only resolves assemblies that actually sit next to it,
   so it cannot interfere with Revit's own loading.

### `#if NETFRAMEWORK` blocks in shared source

- `RevitActivator.xaml.cs` — offers Revit 2023-2024 on 4.8, 2025-2027 on .NET 8. The XAML
  rows are year-agnostic and unused ones collapse.
- `CarboLifeMainWindow.Window_Loaded` — `CodePagesEncodingProvider` registration; the
  legacy code pages are built into .NET Framework.

## Gotchas

**SkiaSharp is pinned per framework, deliberately.** `LiveChartsCore` 2.0.0-rc5.4 is
compiled against SkiaSharp 2.88.0.0. .NET Core rolls assembly versions forward at
runtime, which is how the NET8 build gets away with 3.119.0; .NET Framework binds
strictly, so a 3.x SkiaSharp makes `SolidColorPaint(SKColor)` bind against a different
`SKColor` type and throw `MissingMethodException` the moment a chart is built. Keep the
4.8 build on the version LiveCharts' nuspec asks for.

**After changing a SkiaSharp version, delete every `bin` and `obj` by hand.** The native
`libSkiaSharp.dll` copies (output root plus the `x86`/`x64`/`arm64` subfolders) are not
removed by `msbuild /t:Clean`, so a downgrade leaves 3.x natives beside 2.88 managed
assemblies. That throws either `InvalidOperationException` in
`SkiaSharpVersion.CheckNativeLibraryCompatible` or `DllNotFoundException` in
`LibraryLoader.LoadLocalLibrary`.

**`CarboLifeCalc\libSkiaSharp.dll` is checked in for the NET8 build only.** It is the
3.119.0 win-x64 native and is excluded from the 4.8 output. For 4.8,
`Directory.Build.targets` mirrors the package's `x64\libSkiaSharp.dll` into the output
root, which is what the standalone exe needs.

**Never set the 4.8 build back to AnyCPU/32-bit.** Revit and Rhino are x64, and only the
x64 native is copied to the output root.

## Smoke-testing the 4.8 chart stack without launching Revit

Windows PowerShell 5.1 is a 64-bit .NET Framework process whose directory is not the
output folder — the same conditions a Revit add-in sees:

```powershell
$out = "CarboLifeCalc\bin\x64\Release\net48"
$ui = [System.Reflection.Assembly]::LoadFrom("$out\CarboLifeUI.dll")
[CarboLifeUI.NativeDependencies]::Preload()

# forces SkiaSharp to load through the AssemblyResolve handler
$gb = $ui.GetType("CarboLifeUI.UI.GraphBuilder")
$c  = $gb.GetMethod("getSKColour", [System.Reflection.BindingFlags]"NonPublic,Static").Invoke($null, @([int]99))

# exercises the native library
$img  = [SkiaSharp.SKImage]::FromBitmap((New-Object SkiaSharp.SKBitmap 64,64))
$img.Encode().Size
```
