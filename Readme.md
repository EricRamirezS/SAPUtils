# SAPUtils - SAP Business One Add-on Development Library

## Overview

SAPUtils is a specialized C# library designed to simplify and speed up the development of add-ons for SAP Business One.
Built for .NET Framework 4.8.1, this comprehensive toolkit abstracts away the complexities of the SAP Business One SDK,
providing developers with intuitive and type-safe interfaces for working with user tables, fields, forms, and other SAP
BO components.

## Key Features

### User-Defined Tables and Fields Management

- **Declarative Table Definitions**: Define SAP Business One user tables using C# classes with simple attributes:
  ```csharp
  [UserTable("TABLE_NAME", "Custom Table")]
  public class Comuna : UserTableObjectModel<Comuna>, IAuditable {
      [AlphaUserTableField] 
      public string CountyName { get; set; }
      
      [NumberUserTableField]
      public int PostalCode { get; set; }

      [AlphaUserTableField(LinkedTableType = typeof(Region))]
      public string Region { get; set; }

      public Datetime CreatedAt { get; set; }
      public Datetime UpdatedAt { get; set; }
      public string CreatedBy { get; set; }
      public string UpdatedBy { get; set; }
      public bool Active { get; set; }
  }
  ```

- **Rich Field Type Support**: Specialized attributes for different SAP field types:
    - `AlphaUserTableFieldAttribute` - Text fields
    - `NumericUserTableFieldAttribute` - Numeric values
    - `DateTimeUserTableFieldAttribute` - Date and time values
    - `BooleanUserTableFieldAttribute` - Yes/No fields
    - `MemoUserTableFieldAttribute` - Large text fields
    - And many more specialized field types (Price, Percentage, Quantity, etc.)

- **Field Validation & Constraints**: Define field requirements, size limits, and valid values directly in attributes
- **Linked Objects**: Easily create references to SAP system objects or other user-defined tables

### Auditable Models

- **Automatic Field Tracking**: Implement interfaces to automatically add audit fields:
    - `IAuditableDate` - Adds creation and modification date/time fields
    - `IAuditableUser` - Adds created-by and updated-by user fields
    - `ISoftDeletable` - Adds an Active field for soft deletion

### Form Management

- **Advanced Form Compatibility (SAPUtils.Forms.UserForm)**:
    - **Modern Form Support**: Use `.srf` form files instead of the classic `.b1f` format, eliminating the need for
      accompanying `.b1s` files
    - **Legacy Framework Benefits**: Leverages the underlying SAP form handling logic from the B1 Framework
    - **Best of Both Worlds**: Combines the modern SRF format with the powerful event management system of SAP's native
      framework
    - **Simplified Development**: Get all the automatic event handling and form lifecycle management without being
      locked into the legacy form format
- **Form Event Handling**: Simplified event system for SAP Business One forms
- **UI Controls Access**: Type-safe access to form controls
- **Framework Integration**: Built on top of SAPbouiCOM.Framework for reliability

### Menu Creation and Management

## Overview

SAPUtils is a comprehensive C# library designed to simplify and streamline the development of add-ons for SAP Business
One. This library provides a set of utilities, attributes, and models that reduce boilerplate code and standardize
common patterns when working with the SAP Business One SDK.

## Features

### User-Defined Tables and Fields

- **Attribute-based UDT/UDF definition**: Define your user tables and fields using C# attributes
- **Type-safe field handling**: Strongly-typed attributes for different field types (text, numeric, date, boolean)
- **Validation support**: Built-in validation and field value constraints
- **Linked objects**: Easy linking to SAP system objects or other user-defined tables