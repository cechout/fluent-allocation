# Fluent Allocation Development Guidelines

This project assigns students to the courses they chose. Every student hands in a priority list, every course has a
capacity, and every student needs a fixed number of courses; the allocation walks the priorities from the top and
draws lots wherever a course has more applicants than seats. It is being rewritten as a C# WinUI 3 desktop app.
Until that rewrite exists, the repository holds only the original WPF version 1, `Kurszuteilung/`, which reads a
source workbook through Excel and writes the allocation back into one.

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
  the file in place and older ones can no longer open it; the app only attaches the copy the build puts next to
  the executable.
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
Kurszuteilung/
├── Classes/      the allocation pipeline and its helpers: EvaluateC runs it, Functions1 reads the workbook
│                 into the database, Functions2 holds the allocation queries, Functions3 writes the result,
│                 Excel wraps the Excel COM automation, Globals carries the form input
├── Database/     main_database.mdf and .ldf, an empty LocalDB database that only carries the schema
├── Pages/        Menu, Evaluate, Setup and Input (help pages), Imprint
├── Icons/        UI icons and the app icon
└── Images/       the screenshots the help pages show

Samples/          an example source workbook with made-up students
```

`.github/` holds the workflows, the issue and pull request templates and the public README.

## Build

The WPF version 1 builds with the .NET CLI. Excel is referenced through the primary interop assembly from NuGet
rather than a COM reference, so neither Visual Studio's MSBuild nor an Office installation is needed to build it.

```powershell
dotnet build Kurszuteilung/Kurszuteilung.csproj -c Release
```

Running it needs Microsoft Excel and SQL Server Express LocalDB (the `MSSQLLocalDB` instance) on the machine. The
source workbook format is explained on the Setup help pages inside the app; `Samples/` holds a workbook that
follows it.

The `format` check runs `dotnet format whitespace --verify-no-changes` against the `.editorconfig`. It
only passes because `.gitattributes` pins the checkout to LF: Git for Windows sets `core.autocrlf=true`
in its system config and the GitHub runner has the same default, so without that pin the working tree is
CRLF and every line of every file is reported as a violation. Note that `dotnet format` takes no MSBuild
properties; passing `-p:Platform=x64` makes it print its usage help and exit non-zero, so the check would
silently never run.

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
