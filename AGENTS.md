# Fluent Allocation Development Guidelines

This project shares limited places fairly, based on everyone's own list of preferences. It started as a school tool:
every student hands in a priority list, every course has a capacity, and every student needs a fixed number of
courses; the allocation walks the priorities from the top and draws lots wherever a course has more applicants than
seats. The original WPF version 1, `Kurszuteilung/`, does exactly that, reading a source workbook through Excel and
writing the allocation back into one. `FluentAllocation/` rewrites it as a C#/.NET 10 WinUI 3 desktop app with MVVM,
and that rewrite is no longer tied to students and courses: the same principle has to work in any setting, for
example employees picking their vacation days. It ships unpackaged and self-contained through an installer and as a
portable zip, and needs no elevation.

- In the WinUI version, keep names, UI strings and docs general. Nothing there may assume a school, students or
  courses; those words belong to the WPF version 1 only.
- Keep the work scoped to what was asked. Avoid opportunistic refactors, formatting churn, dependency
  bumps and drive-by renames.
- Read the surrounding code before adding an abstraction. In the WinUI version prefer the MVVM patterns the file
  already uses: a view binds to a view model, and the view model holds no WinUI type it does not need.
- Preserve existing comments verbatim when you change the code around them.
- Everything is English: code, comments, commit messages, UI strings and release notes. The one exception is the UI
  of the WPF version 1, which is German and stays German.
- Always follow `.editorconfig`. Text files are LF; `.gitattributes` pins the checkout, and the CI
  `format` job fails on CRLF.
- `Kurszuteilung/` is the WPF version 1. Once v1.0.0 is released it is kept for history only; it is not developed
  further and changes to it are not accepted. Its UI and its content, the imprint included, stay exactly as they
  were built at school.
- Never attach `Kurszuteilung/Database/main_database.mdf` to a database server directly. A newer LocalDB upgrades
  the file in place and older ones can no longer open it. The file is a template: the app copies it once per
  Windows user to `%LocalAppData%\Kurszuteilung\Database` and only ever attaches that copy.
- Build and publish `FluentAllocation/FluentAllocation.csproj`, never `FluentAllocation.slnx`. The solution also
  contains the WPF version 1.
- WinUI 3 has no `Any CPU` configuration, so every build and publish of the WinUI project names a platform. `x64`
  is the one that ships.
- Prefer targeted search over full file reads, and cap any command whose output could be large.

## Comment Style

A comment explains what a piece of code does and why it exists, not how the framework works underneath.
Weight scales with how non-obvious the reason is. A helper whose name already says everything needs no
comment at all.

The points below describe the style the code currently follows. They are recommendations, not entry
requirements: bring your own style if you prefer one, and expect these to shift over time.

- XML documentation comments are not used anywhere in this codebase. No `/// <summary>`, `/// <param>`,
  `/// <returns>` or `/// <inheritdoc/>`; plain `//` single-line comments throughout.
- Section headers inside a class are written as `// === section name ===`, lowercase, three equals signs,
  with a blank line above and below, rather than as multi-line banner dividers.
- Related fields are grouped under a `// topic name` or `// --- topic name ---` subheader, with short
  trailing comments on the individual lines.
- A comment that only restates the line below it, or that defends an option nobody took, is usually
  better left out.
- Comments are kept short: one or two lines for what the code does and the one thing that is not
  obvious, the unit once on a group line, a trailing comment in a few words, and no history of how the
  code got to where it is.

Four tags mark code that is not ordinary. Use each only for what it names:

- `// --- workaround: short name ---` for a bug in the platform or a third-party library, never for one
  of ours. State the problem, link the issue or the confirmed repro, then state the fix.
- `// --- memory leak: short name ---` for the WinUI 3 retained-instance pattern, where a window is
  hidden and reused because a real close never releases it.
- `// --- revisit: short name ---` for a deliberate temporary shape that has a named trigger for
  changing it.
- `// KNOWN UNRELIABLE:` for an external data source whose values cannot be trusted, paired with
  `(unreliable)` on the matching UI label.

XAML comments put two spaces inside the markers (`<!--  note  -->`). When the text does not fit on one
line it becomes a block with the markers on their own lines, rather than several stacked one-liners. A
double hyphen inside a comment is an XML parse error, so anomaly tags are written plainly there:
`<!--  workaround: short name  -->`.

The WPF version 1 predates these conventions and is left as it was written.

## Project Structure

```text
FluentAllocation/
├── Assets/       the app icon
├── Distribution/ the channel the running build came from, installer or portable: AppDistribution
├── Engines/      the allocation, UI-free: SourceWorkbookReader, AllocationEngine, ResultWorkbookWriter, and
│                 AllocationPipeline, which runs the three in one go
├── Models/       Participant, Option, SheetValue, AllocationInput, AllocationRequest, AllocationResult,
│                 AllocationException (a message meant for the user)
├── Persistence/  what survives a restart, as json under %LocalAppData%\FluentAllocation, or in a Persistence
│                 folder next to the exe in the portable build:
│   ├── Models/   the file shapes, whose initial values are the defaults: AppSettingsData
│   └── Services/ PersistenceService (the disk), AppDataFolder, and the live SettingsService
├── Properties/   PublishProfiles, launchSettings
├── ViewModels/   SettingsViewModel
└── Views/        AllocationPage, SourceHelpPage, InputHelpPage, SettingsPage

Kurszuteilung/
├── Classes/      the allocation pipeline and its helpers: EvaluateC runs it, Functions1 reads the workbook
│                 into the database, Functions2 holds the allocation queries, Functions3 writes the result,
│                 Excel wraps the Excel COM automation, Globals carries the form input
├── Database/     main_database.mdf and .ldf, an empty LocalDB database that only carries the schema; the
│                 template the app copies per user
├── Pages/        Menu, Evaluate, Setup and Input (help pages), Imprint
├── Icons/        UI icons and the app icon
└── Images/       the screenshots the help pages show

FluentAllocation.Tests/   the engine tests, plain net10.0, no reference to the app

Samples/          example source workbooks with made-up people: employees and vacation weeks for the WinUI
                  version, students and courses for the WPF version 1
Setup/            the Inno Setup script of the WPF version 1 and the script that builds its installer
```

`MainWindow` holds the `NavigationView` and the `Frame` the pages are shown in, extends into the title bar
with a plain 32 px `Grid` rather than the WinUI `TitleBar`, carries Mica, applies the theme and sizes the window
through `WinUIEx.WindowManager`. The navigation groups Allocation under General and the two help sections under
Help, with Settings in the footer. Every page but the settings is cached, so it keeps its content across a
navigation.

`.github/` holds the workflows, the issue and pull request templates and the public README.

## Build

Build with the .NET CLI. No project has a COM reference, so `dotnet build` resolves everything it needs and none
of the workflows carry an MSBuild setup step.

WinUI 3 has no `Any CPU` configuration, so always pass `-p:Platform=x64`.

```powershell
dotnet restore FluentAllocation/FluentAllocation.csproj -p:Platform=x64 -r win-x64 -p:SelfContained=true
dotnet build FluentAllocation/FluentAllocation.csproj --no-restore -c Release -p:Platform=x64 -r win-x64 -p:SelfContained=true
```

Publish only through the publish profile, and only after a full build pass. The XAML compiler resolves `x:Bind`
against project-local types in `MarkupCompilePass2`, which needs the `LocalAssembly` that a build produces;
publishing a fresh checkout without one fails with `WMC1509` or `WMC9999`.

```powershell
dotnet publish FluentAllocation/FluentAllocation.csproj --no-build -c Release -p:Platform=x64 -p:PublishProfile=win-x64
```

### The WPF version 1

The WPF version 1 builds with the .NET CLI. Excel is referenced through the primary interop assembly from NuGet
rather than a COM reference, so neither Visual Studio's MSBuild nor an Office installation is needed to build it.

```powershell
dotnet build Kurszuteilung/Kurszuteilung.csproj -c Release
```

Running it needs Microsoft Excel and SQL Server Express LocalDB (the `MSSQLLocalDB` instance) on the machine. The
source workbook format is explained on the Setup help pages inside the app; `Samples/` holds a workbook that
follows it.

The installer is built by one script, which publishes the app self-contained for `win-x64` and compiles
`Setup/KurszuteilungWPF_Setup.iss` with Inno Setup 6, passing it the `<Version>` from the `.csproj`:

```powershell
.\Setup\build-wpf-installer.ps1
```

It writes `Setup/Output/Kurszuteilung_Installer.exe`. The installer warns when Excel or LocalDB is missing but does
not stop.

The `format` check runs `dotnet format whitespace --verify-no-changes` against the `.editorconfig`. It
only passes because `.gitattributes` pins the checkout to LF: Git for Windows sets `core.autocrlf=true`
in its system config and the GitHub runner has the same default, so without that pin the working tree is
CRLF and every line of every file is reported as a violation. Note that `dotnet format` takes no MSBuild
properties; passing `-p:Platform=x64` makes it print its usage help and exit non-zero, so the check would
silently never run.

## Test

`FluentAllocation.Tests/` covers the reader, the allocation, the writer and whole runs against files on
disk, plus both workbooks in `Samples/` under many seeds. It targets plain `net10.0` and links the
sources it tests rather than referencing the app, which is a `WinExe` on a Windows target framework and
cannot be referenced from a plain library, so the suite runs on any dotnet runner. The lottery takes a
`Random`, so a test seeds it.

```powershell
dotnet test FluentAllocation.Tests/FluentAllocation.Tests.csproj
```

What is not covered is the interface: no test opens a window. The bar for a change is that the tests
stay green, that the build stays green, and that the screen it touches was opened in a running app and
looked at. Report what you did not verify instead of implying it passed.

## Commit & Push

Run these before committing, and stage only what belongs to the change:

```powershell
git status --short
git diff --check
```

Branches are `feature/`, `fix/`, `chore/`, `refactor/` or `docs/` followed by a short name. Releases use
their own `update/` branch.

A one-line commit message is recommended. Pull requests are squash merged, so the individual commits are
collapsed into one anyway and only the pull request title survives on `main`. The issue reference is
appended in parentheses.

```text
feat: read the source workbook without Excel (#3)
fix: keep the priorities of a student in order after a duplicate is dropped (#3)
refactor: split the allocation out of the page
chore: add an example source workbook
build: v2.1.0
```

## Open a PR

Pull requests target `main` and are squash merged, so the pull request title becomes the commit message
on `main` and has to carry the same prefix a hand-written commit would.

Reference issues without closing them, as `Part of #N`. An issue is closed by hand once the change is
released, not by the merge.

Size the description by the diff, and check the length against this ladder before posting:

- A fix with one cause gets one or two bullets and nothing else. No paragraph.
- A small feature, or a fix whose behaviour change would surprise a reader, gets two sentences and up to
  three bullets.
- A branch that adds a subsystem or a distribution channel gets the full shape.

The opening paragraph carries context the diff cannot. When the title and two bullets already carry it,
the heading stands alone above the bullets. Do not argue the fix in the body: why a change is correct
belongs in the code comment, and if it seems worth saying in both places, the comment is what is missing.

**The prefix decides what reaches users.** Release notes are drafted from `feat:` and `fix:` pull
requests only. `chore:`, `build:`, `docs:` and `refactor:` are left out, because someone downloading the
app cannot notice them.

## Things That Must Not Change

These are load-bearing. Breaking one of them fails at runtime, on a user machine, or invisibly in a
repository setting, not in the build.

- **`PublishTrimmed=False` stays.** Trimming strips the WinRT and COM interop types the Windows App SDK
  resolves at runtime, and the app crashes on start. The `.csproj` sets it to `False` as well, so a publish
  that bypasses the profile cannot hit the trap either.
- **Publish only through the profile in `FluentAllocation/Properties/PublishProfiles/`.** That is what makes a
  publish in CI apply the exact same settings as a local one.
- **Deselect Windows App SDK components with `ExcludeAssets`, never by deleting files from the publish
  output.** Every component ships a package fragment that becomes part of the embedded WinRT manifest; deleting
  only the binaries leaves the manifest declaring classes that are gone, and the process does not start. The
  excluded component versions move together with `Microsoft.WindowsAppSDK`.
- **Never prune the WinUI `.mui` locale folders.** `Microsoft.ui.xaml.dll` keeps its resources only there and
  loads the folder of the Windows UI language; without it the app dies the moment the visual tree is built.

- **No `COMReference` in any project.** `ResolveComReference` exists only in the .NET Framework MSBuild, so a COM
  reference makes `dotnet build` fail with `MSB4803`, and it needs the type library registered on the build machine,
  which the GitHub runner does not have.
- **`System.Data.SQLite` stays in the WPF version 1**, although nothing in it uses SQLite. It is what brings in
  `System.Data.SqlClient` (through `System.Data.SQLite.EF6` and `EntityFramework`), which the database code needs.
- **The `.gitignore` negations stay.** The stock template ignores `*.mdf` and `*.ldf`, which would drop the
  database the WPF version 1 copies next to its executable and fail every fresh build. The macOS `Icon` rule and
  `*.pubxml` would swallow the app icon and the publish profiles of the WinUI version; both are undone for
  `FluentAllocation/` ahead of time.
- **The CodeQL default setup stays switched off in the repository settings.** It collides with the
  advanced setup in `codeql.yml`.
- **Required checks report on every pull request.** `build` and `format` are required on `main`, so
  `pr-build.yml` skips work through a job condition and never through `paths-ignore`; a workflow skipped by
  path filtering never reports and leaves the pull request unmergeable.
- **`<Version>` in a `.csproj` is the only place a version is written**, the WinUI one for v2.0.0 on and the WPF
  one for v1. The installer script reads it from there and passes it to Inno Setup, so the version in Installed
  apps can never drift from the app. It is bumped in its own release pull request, never in a feature branch.
- **The installer name is a contract.** `Kurszuteilung_Installer.exe` is what the Inno Setup script produces and
  what the release notes of the WPF version 1 tell people to download.
- **The WPF version 1 never writes next to its executable.** An installed copy sits in Program Files, where a normal user
  cannot write; the database works from the per-user copy in `%LocalAppData%\Kurszuteilung`, which is also why the
  app runs without elevation.
- **The installer leaves `%LocalAppData%\Kurszuteilung` behind on uninstall.** LocalDB keeps the database registered
  by its file path, so deleting the file makes the next attach at that path fail after a reinstall.
