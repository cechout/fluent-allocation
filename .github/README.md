# Fluent Allocation

Fluent Allocation assigns students to the courses they chose. Every student hands in a list of courses in order of preference, every course has a limited number of seats, and every student needs a fixed number of courses in the end. The app works through the preferences from the top and gives every student their highest choices that still have room. Where a course has more applicants than seats left, a lottery decides.


## 📖 Project History & Architecture
The first version of this project was written in WPF as a school project at the Richard Wossidlo Gymnasium in Waren (Müritz), from September 2023 to March 2024, under the name **Kurszuteilung**. It is kept in this repository as version `v1.0.0`, in the `Kurszuteilung/` folder, exactly as it was built, with its German interface.

Version `v2.0.0` will be a rewrite in WinUI 3 with the MVVM pattern and an English interface, following the other two apps of this family, [Fluent Math](https://github.com/cechout/fluent-math) and [FluentSensors](https://github.com/cechout/fluent-sensors). It is not in the repository yet.


## ⚙️ Core Mechanics
### The Allocation
1. The source workbook is read in: all students with their priorities, and all courses with their capacity. A priority that repeats an earlier one, or names a course that does not exist, is skipped, and the later priorities of that student move up.
2. For priority 1, every course takes the students who put it first. If there are more of them than seats, a lottery picks who gets in. Then the same happens for priority 2 with the seats that are left, and so on.
3. A student who already has enough courses drops out of the following rounds. A student who still lacks courses after the last priority is listed separately, so they can be placed by hand.

### The Source Workbook
The workbook needs two sheets. Sheet names and header labels do not matter, only the order of the columns.

* **Sheet 1, students:** an Id, the name, any number of attributes (for example the class), then one column per priority holding a course Id.
* **Sheet 2, courses:** an Id, the name, and the number of seats.

The result is written into a new or an existing workbook: every student with their courses, every course with its students, optionally all students grouped by an attribute, and the students who could not be given all their courses.


## 🛠️ How to Run

### 1. Prerequisites
Version 1 automates Microsoft Excel and keeps its working data in a local SQL Server database, so it needs:

* **Windows** with **Microsoft Excel** installed
* **SQL Server Express LocalDB**, with its default `MSSQLLocalDB` instance ([download](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb))
* the **.NET 8 SDK**, or **Visual Studio 2022** (17.14 or later, which opens `.slnx` solutions) with the **.NET Desktop Development** workload

Excel is only needed to run the app, not to build it.

### 2. Clone the Repository
```ps
git clone https://github.com/cechout/fluent-allocation.git
```

### 3. Build and Run
From the command line:
```ps
dotnet run --project Kurszuteilung/Kurszuteilung.csproj
```

Or open `FluentAllocation.slnx` in Visual Studio, set `Kurszuteilung` as the startup project and press `F5`.

### 4. Try It
[`Samples/students-and-courses.xlsx`](../Samples/students-and-courses.xlsx) holds 60 made-up students in three classes, eight courses and four priorities per student, with a few courses deliberately in high demand so the lottery has something to decide. Pick it as the source file (`Quell-Excel-Datei`) and fill in the numbers version 1 asks for:

| Field | Value |
| :--- | :--- |
| Schüler | 60 |
| Fächer | 8 |
| Attribute | 1 |
| Prioritäten | 4 |
| Benötigte Fächer | 2 |
| Gruppieren (optional) | `Class` |

A handful of students usually end up without their second course; they are listed on the last sheet of the result, `FF-Schüler`.
