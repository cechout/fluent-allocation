# Fluent Allocation

Fluent Allocation shares limited places fairly, based on everyone's own list of preferences. The idea started at school: every student hands in a list of courses in order of preference, every course has a limited number of seats, and every student needs a fixed number of courses in the end. The app works through the preferences from the top and gives every student their highest choices that still have room. Where a course has more applicants than seats left, a lottery decides.

The original version was built for exactly that case. From the WinUI version on, the same principle works in any setting where people compete for limited places, for example when the employees of a company pick their vacation days.


## 📖 Project History & Architecture
The first version of this project [`v1.0.0`](https://github.com/cechout/fluent-allocation/releases/tag/v1.0.0) was written in WPF as a school project at the Richard Wossidlo Gymnasium in Waren (Müritz), from September 2023 to March 2024, under the name **Kurszuteilung**. It assigns students to their courses, has a German interface, and is kept in the `Kurszuteilung/` folder exactly as it was built.

For version [`v2.0.0`](https://github.com/cechout/fluent-allocation/releases/tag/v2.0.0), the UI framework, the code structure and the scope change:
* **WinUI 3:** Windows App SDK components replace the WPF controls, and the interface is in English.
* **MVVM:** The allocation logic is separated from the user interface. The code is divided into Models, Views, and ViewModels, which communicate via data binding and commands.
* **General allocation:** Students and courses become people and the places they compete for, so the app fits any setting where limited places have to be shared fairly.


## ⚙️ Core Mechanics
### The Allocation
1. All students are read in with their priorities, and all courses with their capacity. A priority that repeats an earlier one, or names a course that does not exist, is skipped, and the later priorities of that student move up.
2. For priority 1, every course takes the students who put it first. If there are more of them than seats, a lottery picks who gets in. Then the same happens for priority 2 with the seats that are left, and so on.
3. A student who already has enough courses drops out of the following rounds. A student who still lacks courses after the last priority is listed separately, so they can be placed by hand.


## 🛠️ How to Build

### 1. Prerequisites
To build and run this project, it is highly recommended to use **Visual Studio 2026** with the **.NET 10 SDK**.
Before opening the solution, make sure you have the following workloads installed via the **Visual Studio Installer**:

* **.NET Desktop Development**
* **Windows application development** (Make sure that the "Windows App SDK C# Templates" are checked in the optional components on the right side).

### 2. Clone the Repository
```ps
git clone https://github.com/cechout/fluent-allocation.git
```

### 3. Build and Run
* Open the solution file in Visual Studio.
* Right-click on the Solution in the Solution Explorer and select **Restore NuGet Packages** (Visual Studio usually does this automatically on the first build).
* Right-click on the `FluentAllocation` project in the Solution Explorer and select `Set as Startup Project`.
* In the top toolbar, change the Solution Platform from `Any CPU` to `x64`. *Note: WinUI 3 projects do not support 'Any CPU' builds.*
* Press `F5` to build and run the application.

And now you're good to go!
