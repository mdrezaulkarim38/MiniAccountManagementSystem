# 💼 MiniAccountManagementSystem

A modern ASP.NET Core MVC application for managing:

- 🎯 **Chart of Accounts**
- 🧾 **Voucher Entries (Journal / Payment / Receipt)**
- 🔐 **User Roles & Permissions**
- 📤 **Excel Export via DataTables**

---

## ✨ Key Features

✅ User Authentication (Register / Login / Logout)  
✅ Role-Based Access Control: Admin, Accountant, Viewer  
✅ Chart of Accounts CRUD using Stored Procedures  
✅ Voucher Entry Module with multi-row Debit/Credit  
✅ Admin User Management (Assign roles)  
✅ Toastr Notifications for Feedback  
✅ DataTables Integration with Search, Pagination, and Export to Excel

---

## 🧰 Technologies Used

- ASP.NET Core MVC 8
- SQL Server + Stored Procedures
- ADO.NET (for DB interaction)
- Bootstrap 5
- Toastr.js (for notifications)
- DataTables (jQuery plugin)

---

## ⚙️ Setup Instructions

### 🔗 1. Clone the Repository

```bash
git clone https://github.com/yourusername/MiniAccountManagementSystem.git
cd MiniAccountManagementSystem
````
---
### ⚙️ 2. Configure Database Connection

Open `appsettings.json` and update your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MiniAccountDB;User Id=sa;Password=yourpassword;TrustServerCertificate=True;"
}
```

---

### 🗂 3. Run SQL Setup Scripts (Manually)

> ⚠️ First You need to run command **dotnet ef database update** for create Identity tables then you need to run sql command 

You must manually execute the SQL scripts in order using **SSMS** or any SQL client:

1. **Tables**
2. **Initial Data Inserts** (`VoucherTypes`, `AspNetRoles`)
3. **Table-Valued Types**
4. **Stored Procedures**

All scripts are available in the **📦 SQL Setup** section below.

---

### ▶️ 4. Run the App

```bash
dotnet ef database update
dotnet run
```

---

## 📦 SQL Setup

### 🏗️ Tables

```sql
CREATE TABLE ChartOfAccounts (
    AccountId INT PRIMARY KEY IDENTITY,
    AccountName NVARCHAR(MAX),
    ParentId INT NULL,
    FOREIGN KEY (ParentId) REFERENCES ChartOfAccounts(AccountId)
);
```

```sql
CREATE TABLE VoucherTypes (
    Id INT PRIMARY KEY IDENTITY,
    TypeName NVARCHAR(50) NOT NULL
);

INSERT INTO VoucherTypes (TypeName) VALUES ('Journal'), ('Payment'), ('Receipt');
```

```sql
CREATE TABLE Vouchers (
    VoucherId INT PRIMARY KEY IDENTITY,
    VoucherDate DATE NOT NULL,
    ReferenceNo NVARCHAR(100),
    VoucherTypeId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (VoucherTypeId) REFERENCES VoucherTypes(Id)
);
```

```sql
CREATE TABLE VoucherEntries (
    EntryId INT PRIMARY KEY IDENTITY,
    VoucherId INT FOREIGN KEY REFERENCES Vouchers(VoucherId),
    AccountId INT FOREIGN KEY REFERENCES ChartOfAccounts(AccountId),
    DebitAmount DECIMAL(18,2),
    CreditAmount DECIMAL(18,2)
);
```

```sql
INSERT INTO [TestingDB].[dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES 
('8485E8E3-3CEB-4A6D-A65D-9F3F01A912CB', 'Viewer', 'VIEWER', '7F456208-BBBF-4AFC-9891-54C0B411FE3E'),
('85AE03E1-BBFD-4079-AA2A-D56C53F588FA', 'Admin', 'ADMIN', 'B2FD17BB-A6D1-4C33-B069-F9E2EAA04A0C'),
('B0F361CB-8736-432A-9D6D-D8883CB28A8D', 'Accountant', 'ACCOUNTANT', '090D62B3-156C-435F-8403-EF1DBAC0CBCB');
```

---

### 📄 Table-Valued Type

```sql
CREATE TYPE VoucherEntryType AS TABLE (
    AccountId INT,
    DebitAmount DECIMAL(18,2),
    CreditAmount DECIMAL(18,2)
);
```

---

### ⚙️ Stored Procedures

#### 🧮 `sp_ManageChartOfAccounts`

```sql
CREATE PROCEDURE sp_ManageChartOfAccounts
    @AccountId INT = NULL,
    @AccountName NVARCHAR(100),
    @ParentId INT = NULL,
    @Operation NVARCHAR(10)
AS
BEGIN
    IF @Operation = 'INSERT'
        INSERT INTO ChartOfAccounts (AccountName, ParentId) VALUES (@AccountName, @ParentId)
    ELSE IF @Operation = 'UPDATE'
        UPDATE ChartOfAccounts SET AccountName = @AccountName, ParentId = @ParentId WHERE AccountId = @AccountId
    ELSE IF @Operation = 'DELETE'
        DELETE FROM ChartOfAccounts WHERE AccountId = @AccountId
END
```

#### 💰 `sp_SaveVoucher`

```sql
CREATE PROCEDURE sp_SaveVoucher
    @VoucherDate DATE,
    @ReferenceNo NVARCHAR(100),
    @VoucherTypeId INT,
    @VoucherEntries VoucherEntryType READONLY
AS
BEGIN
    INSERT INTO Vouchers (VoucherDate, ReferenceNo, VoucherTypeId)
    VALUES (@VoucherDate, @ReferenceNo, @VoucherTypeId);

    DECLARE @VoucherId INT = SCOPE_IDENTITY();

    INSERT INTO VoucherEntries (VoucherId, AccountId, DebitAmount, CreditAmount)
    SELECT @VoucherId, AccountId, DebitAmount, CreditAmount FROM @VoucherEntries;
END
```

#### 🔐 `sp_SeedRoles`

```sql
CREATE PROCEDURE sp_SeedRoles
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());

    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Accountant')
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Accountant', 'ACCOUNTANT', NEWID());

    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Viewer')
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Viewer', 'VIEWER', NEWID());
END
```

#### 🔁 `sp_AssignUserToRole`

```sql
CREATE PROCEDURE sp_AssignUserToRole
    @UserId NVARCHAR(450),
    @RoleName NVARCHAR(256)
AS
BEGIN
    DECLARE @RoleId NVARCHAR(450)
    SELECT @RoleId = Id FROM AspNetRoles WHERE Name = @RoleName

    IF NOT EXISTS (
        SELECT 1 FROM AspNetUserRoles 
        WHERE UserId = @UserId AND RoleId = @RoleId
    )
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId)
    END
END
```

---

## 🔐 Roles & Permissions

| Role           | View CoA | Edit CoA | View Vouchers | Create Vouchers | Manage Users |
| -------------- | -------- | -------- | ------------- | --------------- | ------------ |
| **Admin**      | ✅        | ✅        | ✅             | ✅               | ✅            |
| **Accountant** | ✅        | ✅        | ✅             | ✅               | ❌            |
| **Viewer**     | ❌        | ❌        | ✅             | ❌               | ❌            |

---

## 📦 Modules Summary

* 🗂 **Chart of Accounts**: Parent-child hierarchy with stored procedure for insert/update/delete
* 🧾 **Voucher Entry**: Debit/Credit transactions, dynamic rows, DataTables
* 👥 **User Management**: Admin can assign/reassign roles
* 📤 **Excel Export**: DataTables with export-to-excel, search, sort, pagination
* 🎯 **Toastr Notifications**: Used for all feedback (create/edit/delete)

---

## 📤 Export Feature (DataTables)

* Searchable, paginated, exportable
* No backend dependency
* Button integration using CDN

---

## 👨‍💻 Developed By

**Rezaul Karim**
📧 Email: `mdrezaulkarim31295@gmail.com`
🐙 GitHub: [@mdrezaulkarim38](https://github.com/mdrezaulkarim38)
