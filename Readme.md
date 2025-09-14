![NuGet Version](https://img.shields.io/nuget/v/EricRamirezS.SAPUtils)

# SAPUtils

**SAPUtils** — A C# utility library to accelerate SAP Business One add-on development.
Built for .NET Framework 4.8.1, SAPUtils provides attribute-based UDT/UDF definitions, auditable models, form helpers (SRF support), menu helpers and other common utilities that remove boilerplate when working with the SAP Business One SDK.

---

## Table of contents

- [Installation](#Installation)
- [Overview](#overview)
- [Key features](#key-features)
- [Requirements](#requirements)
- [Installation & build](#installation--build)
- [Quickstart](#quickstart)
- [Core concepts & API (high-level)](#core-concepts--api-high-level)
- [Form handling (.srf) notes](#form-handling-srf-notes)
- [Recommended workflow](#recommended-workflow)
- [Testing & debugging](#testing--debugging)
- [Contributing](#contributing)
- [License](#license)
- [Contact & support](#contact--support)
- [Roadmap & ideas](#roadmap--ideas)

---

## Installation

You can install the library from NuGet:

### Using NuGet Package Manager (Visual Studio)

```powershell
Install-Package EricRamirezS.SAPUtils -Version 0.1.0
```

### Using .NET CLI

```bash
dotnet add package EricRamirezS.SAPUtils --version 0.1.0
```

## Overview

SAPUtils is intended to be a lightweight, pragmatic toolkit for developers building SAP Business One add-ons. It emphasizes:

- Declarative UDT/UDF definitions via C# attributes (reduce SQL/SDK boilerplate).
- Type-safe mapping between C# models and SAP user tables/fields.
- Built-in support for audit fields and soft-delete semantics.
- Simplified form lifecycle & event handling, with modern `.srf` support.

---

## Key features

- **Attribute-based UDT/UDF definition** — annotate C# classes and properties to describe SAP user tables and fields (text, numeric, date/time, boolean, memo, price, percentage, quantity, etc.).
- **Auditable & soft-deletable models** — interfaces that add standard audit fields (created/updated dates and users) and an `Active` flag for soft deletes.
- **Form management** — helpers around SAPbouiCOM (UserForm abstraction) that let you load `.srf` forms and wire events with less plumbing.
- **Menu & UI helpers** — utilities to register menus and access controls in a type-safe way.
- **Validation & field constraints** — support to declare size limits, required flags and linked-table relationships at attribute-level.

---

## Requirements

- Windows OS with SAP Business One client + SDK installed.
- .NET Framework **4.8.1** (project target).
- Visual Studio 2019/2022 (or similar) for development.
- References to `SAPbobsCOM` and `SAPbouiCOM` assemblies must be present in the consuming project.
- This library depends on **SAP Business One SDK**. Users must download and reference `SAPBusinessOneSDK.dll` in their projects. It is **not included** in this NuGet package due to licensing restrictions.

---

## Installation & build

1. Clone the repo:
```bash
git clone https://github.com/EricRamirezS/SAPUtils.git
cd SAPUtils
```

2. Open `SAPUtils.sln` in Visual Studio.

3. Ensure NuGet packages are restored (if any) and add references to SAP Business One assemblies (usually found in the SAP client installation folder).

4. Build the solution (Debug/Release as needed).

---

## Quickstart

### Initialize the library
```csharp
using SAPUtils;

[STAThread]
public static void Main(string[] args) {
    SapAddon.Instance(args);
    
    // Later in the code, you can safely retrieve the same instance
    var addon = SapAddon.Instance();
    var logger = addon.Logger;
}
```


### Define a user table model
```csharp
using SAPUtils.Core;

[UserTable("MyModel", "Model")]
public class MyModel : UserTableObjectModel<MyModel>, IAuditable
{
    [AlphaUserTableField(Size = 250)]
    public string Comments { get; set; }

    [NumberUserTableField(Description = "Postal code", Required = true)]
    public int PostalCode { get; set; }

    [AlphaUserTableField(LinkedTableType = typeof(Region))]
    public string Region { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool Active { get; set; }
}
```

Then initialize the models with
```csharp
Type[] userDataTypes = {
    typeof(Zona)
};

SapAddon.Instance().InitializeUserTables(userDataTypes);
```

### Create form

Create a .srf form, then create your form controller class just like a Normal B1f form controller class.

```csharp
using SAPUtils.Forms;

namespace MyProject.Forms {
    [Form("MyProject.Forms.MyForm", "Forms/MyForm.srf")]
    public class MyForm : UserForm {
        // Build your class like a normal b1f form controller class.
        public override void OnInitializeComponent() {
        }
    }
}
```
### Logs

The ILogger interface provides a unified way to log messages, objects, and exceptions at multiple severity levels:

- Trace — Detailed debug/tracing information.
- Debug — Diagnostic info useful during development.
- Info — General application events.
- Warning — Potential issues or unusual events.
- Error — Errors preventing normal operation.
- Critical — Severe errors requiring immediate attention.

It supports:

- Logging simple messages with formatting.
- Logging via a delegate (Func<string>) to delay evaluation until needed.
- Logging objects directly.
- Logging exceptions with optional messages.
- Automatic caller information (CallerMemberName, CallerFilePath, CallerLineNumber) for context.

```csharp
ILogger logger = SapAddon.Instance().Logger; // or your concrete implementation

// Trace a simple message
logger.Trace("Entering method {0}", nameof(MyMethod));

// Trace using a delegate (lazy evaluation)
logger.Trace(() => $"Calculating values for user {userId}");

// Log an object
logger.DebugObject(myObject);

// Log an info message
logger.Info("Service started successfully");

// Log a warning
logger.Warning("Configuration file not found, using defaults");

// Log an error with exception
try
{
    DoRiskyOperation();
}
catch (Exception ex)
{
    logger.Error("Failed to complete operation", ex);
}

// Log critical issues
logger.Critical("Database connection failed, application cannot continue");

```
### SapClass

`SapClass` is a static helper class designed to give global access to commonly used SAP Business One objects:

- `SAPbobsCOM.Company` → the main DI API company object.

- `SAPbouiCOM.Application` → the UI API application object.

- `ILogger` → the project-wide logger instance.

It essentially wraps `SapAddon.Instance()` (your singleton addon class) and exposes its main members as static properties.

Key idea:

By using `using static` directive, you can avoid repeatedly typing `SapAddon.Instance().Company` or `SapAddon.Instance().Logger` and instead use short, readable names like `Company`, `App`, or `log`.

#### example

```csharp
using static SAPUtils.Utils.SapClass;

public class Example
{
    public void Run()
    {
        // Access SAP B1 Company directly
        string companyName = Company.CompanyName;
        
        // Access SAP B1 UI Application
        App.MessageBox("Hello SAP Business One!");

        // Log messages
        log.Info("Connected to SAP B1 company: {0}", companyName);
        
        // You can also use alternative names
        application.MessageBox("Another way to access UI");
        Log.Debug("Debugging info here");
    }
}
```
> All aliases (App, app, application, Log, log) are interchangeable — choose the style that fits your coding conventions.


#### Benefits

1. **Shorter code** — avoids repetitive singleton calls.

2. **Consistent access** — all parts of your project reference the same Company, Application, and Logger.

3. **Readability** — makes code cleaner, especially in large addons where these objects are used everywhere.

4. **Multiple aliases** — Company/company, Application/App/app/application, Logger/Log/log allow you to write in the style you prefer.

---

## Core concepts & API (high-level)

- `UserTableAttribute` / `UserTable("CODE","Description")` — annotate a class as representing an SAP user table.
- `AlphaUserTableFieldAttribute`, `NumericUserTableFieldAttribute`, `DateTimeUserTableFieldAttribute`, `BooleanUserTableFieldAttribute`, `MemoUserTableFieldAttribute` — property-level field metadata.
- `UserTableObjectModel<T>` — base class mapping a C# POCO to the SAP table lifecycle.
- `IAuditableDate`, `IAuditableUser`, `ISoftDeletable` — standard audit and active/inactive fields.
- `SAPUtils.Forms.UserForm` — abstraction for form loading and event handling, with `.srf` support.

---

## Form handling (.srf) notes

- SAPUtils favors `.srf` format for form layouts and plugs them into event handling mechanisms. Consider converting older `.b1s` forms to `.srf`.

---

## Recommended workflow

1. Create C# POCO models for UDTs/UDFs using the attribute approach.
2. Use the library helper to create/upgrade tables in non-production environments.
3. Implement UI forms using `.srf` and `UserForm` abstractions.
4. Implement business logic using `SAPbobsCOM` objects; encapsulate raw SDK calls into services.
5. Version migrations: treat field removals/renames carefully; prefer additive migrations.

---

## Testing & debugging

- Mock `SAPbobsCOM` and `SAPbouiCOM` dependencies for unit testing.
- Use a dedicated SAP sandbox company DB for integration tests.
- Log table/field creation steps for auditing.

---

## Contributing

1. Fork the repo.
2. Create a `feature/` or `bugfix/` branch.
3. Add tests for new behavior if possible.
4. Include a clear PR description and migration plan if schema changes are involved.

---

## License

This project is licensed under the **MIT License**.

---

## Contact & support

- Open an issue in the repository for bugs or feature requests.

---

## Roadmap & ideas

- Automated migrations for UDT/UDF changes.
- Integration test harness for sandbox environment.
- Publish as NuGet packages.
- Sample add-on showing full end-to-end usage.
- API reference generation and how-to examples.

## Others

[SAPbobsCOM (Data Interface API):](https://help.sap.com/doc/089315d8d0f8475a9fc84fb919b501a3/10.0/en-US/SDKHelp/SAPbobsCOM_P.html)
This API allows you to interact programmatically with SAP Business One's data layer, enabling operations such as creating, reading, updating, and deleting business objects like invoices, orders, and business partners. You can find the API reference here:
SAP Help Portal.

[SAPbouiCOM (User Interface API):](https://help.sap.com/doc/089315d8d0f8475a9fc84fb919b501a3/10.0/en-US/SDKHelp/SAPbouiCOM_P.html) This API enables you to customize and extend the SAP Business One client interface, allowing you to create and manage forms, handle events, and interact with UI elements. The documentation is available here:
SAP Help Portal.