# Employee Management System with C# Windows Forms (.NET Framework 4.7) 

- BUILD APPLICATION USING C#
- BUILD AND DESIGN MAIN FORM
- BUILD AND DESIGN DASHBOARD FORM
- BUILD LOGIN FORM
- BUILD EMPLOYEE MANAGEMNT SYSTEM USING C#
- SQL DATABASE
- CRUD
  
# Login/Register form view:
![1](https://github.com/milen92sl/EmployeeManagementSystem/assets/58393766/666b02e9-5885-4256-b94d-b50bffbdac56)

# Dashboard form view: 
![2](https://github.com/milen92sl/EmployeeManagementSystem/assets/58393766/3a252197-1e37-46bb-beb2-05855ac12c1f)

# Employee Management form view: 
![3](https://github.com/milen92sl/EmployeeManagementSystem/assets/58393766/aecdbc6e-f585-4aac-8609-a9d48f1a1ddb)


# Salary Management form view: 
![4](https://github.com/milen92sl/EmployeeManagementSystem/assets/58393766/47feaaac-96b0-4dd3-a116-bc7f9816ba6f)



---

# Database: Oracle (Docker)

The project was migrated from SQL Server LocalDB (`.mdf`) to **Oracle Database 23ai Free**
running in Docker, using the fully managed ODP.NET driver
(`Oracle.ManagedDataAccess` — no Oracle Client install needed).

## 1. Start the database

```powershell
# first time: create the schema and load sample data
powershell -ExecutionPolicy Bypass -File db\setup-db.ps1 -Seed

# every time after that
powershell -ExecutionPolicy Bypass -File db\setup-db.ps1
```

The script starts the `ems-oracle` container, waits for it to report healthy (the
first start takes a few minutes), then runs **Flyway** to bring the schema up to date.

It is safe to re-run: Flyway applies only migrations that have not been applied yet
and records each one in `flyway_schema_history`. Running it twice prints
`Schema "EMS" is up to date` and leaves your data alone.

| Switch      | Effect                                                                   |
|-------------|--------------------------------------------------------------------------|
| *(none)*    | Applies pending migrations. Non-destructive.                              |
| `-Seed`     | **Deletes all rows** and loads `db/seed/dev-seed.sql`. Local use only.    |
| `-Recreate` | **Drops every object** (`flyway clean`) and migrates from scratch.        |

### Changing the schema

Migrations are append-only. **Never edit an applied migration** — Flyway records its
checksum and will refuse to run again. Add a new file instead:

```
db/migrations/
├─ V1__initial_schema.sql        already applied - do not touch
└─ V2__your_change.sql           add this
```

```powershell
docker compose -f db\docker-compose.yml --profile migrate run --rm flyway info
docker compose -f db\docker-compose.yml --profile migrate run --rm flyway migrate
```

## 2. Connection details

| Setting  | Value                       |
|----------|-----------------------------|
| Host     | `localhost`                 |
| Port     | `1521`                      |
| Service  | `FREEPDB1` (pluggable DB)   |
| Schema   | `ems` / `Ems_Pass2026`      |
| SYS      | `sys` / `Oracle_Sys2026`    |

The application reads its connection string from **`App.config`**:

```xml
<connectionStrings>
  <add name="OracleDb"
       providerName="Oracle.ManagedDataAccess.Client"
       connectionString="User Id=ems;Password=Ems_Pass2026;Data Source=localhost:1521/FREEPDB1;Connection Timeout=30;" />
</connectionStrings>
```

## 3. Build and run

```powershell
msbuild EmployeeManagementSystem.sln -t:restore
msbuild EmployeeManagementSystem.sln -p:Configuration=Debug
.\EmployeeManagementSystem\bin\Debug\EmployeeManagementSystem.exe
```

Seeded login: **admin / admin**.

## 4. Tests

```powershell
msbuild EmployeeManagementSystem.sln -t:restore
msbuild EmployeeManagementSystem.sln -p:Configuration=Debug
vstest.console.exe EmployeeManagementSystem.Tests\bin\Debug\net472\EmployeeManagementSystem.Tests.dll
```

`dotnet test` does **not** work here: the application project is a legacy-format
`.csproj` and the dotnet CLI only builds SDK-style projects. Build with MSBuild and run
with `vstest.console.exe` (or just use the Test Explorer in Visual Studio).

The suite has three kinds of test:

| Kind                 | Needs a database | What it protects                                              |
|----------------------|------------------|---------------------------------------------------------------|
| `SqlCatalogTests`    | no               | XML parsing, and that every statement key a repository asks for exists |
| `DesignerSafetyTests`| no               | that opening a form in the VS designer touches nothing         |
| `*RepositoryTests`   | **yes**          | real CRUD, name-based parameter binding, soft delete           |

Integration tests connect to `localhost:1521/FREEPDB1` by default. Point them elsewhere
with the `EMS_TEST_CONNECTION` environment variable. **When no database is reachable they
skip rather than fail**, with a message telling you to run `db\setup-db.ps1`.

One test is named `KNOWN_GAP_password_is_stored_in_clear_text`. It asserts current
*wrong* behaviour on purpose, so that fixing the finding makes it fail and forces the
test to be rewritten — documentation that fails loudly when it goes stale. There used to
be a second one for duplicate `employee_id`; migration V2 closed that gap, and the test
is now `Duplicate_active_employee_id_is_rejected_by_the_database`.

## 5. What the database enforces

The schema, not just the UI, now rejects bad data:

| Rule                                        | Enforced by                      | Migration |
|---------------------------------------------|----------------------------------|-----------|
| `username` is unique                        | `UQ_USERS_USERNAME`              | V1        |
| an **active** `employee_id` is unique       | `UQ_EMPLOYEES_ACTIVE_ID`         | V2        |
| `status` is `Active` or `Inactive`          | `CK_EMPLOYEES_STATUS`            | V3        |
| `salary >= 0`                               | `CK_EMPLOYEES_SALARY`            | V3        |

The uniqueness index is function-based:

```sql
CREATE UNIQUE INDEX uq_employees_active_id
  ON employees (CASE WHEN delete_date IS NULL THEN employee_id END);
```

Oracle does not index rows where the expression is NULL, so soft-deleted rows are
outside the constraint. That keeps two behaviours the application depends on: an
`employee_id` can be reused after a delete, and the deleted history may hold the same id
more than once.

`Data/OracleErrors.cs` turns the resulting Oracle codes into sentences a user can act
on, so nobody sees `ORA-00001: unique constraint (EMS.UQ_EMPLOYEES_ACTIVE_ID) violated`:

| Oracle code | Becomes                    | Shown as   |
|-------------|----------------------------|------------|
| `ORA-00001` | `DuplicateKeyException`    | a warning  |
| `ORA-02290` | `DataRuleViolationException` | a warning |
| `ORA-12899` | `DataRuleViolationException` | a warning |

Anything else keeps its own message and is reported as an error.

The repositories still pre-check with `ExistsByEmployeeId` / `UsernameExists`, because
that gives a better message before any work is done. The database constraint is what
closes the race when two people add the same id at the same moment.

## 6. Useful container commands

```powershell
docker compose -f db\docker-compose.yml logs -f      # watch startup
docker exec -it ems-oracle sqlplus ems/Ems_Pass2026@localhost:1521/FREEPDB1
docker compose -f db\docker-compose.yml stop         # stop, keep data
docker compose -f db\docker-compose.yml down -v      # delete container AND data
```

---

# Architecture

```
EmployeeManagementSystem/
├─ Program.cs             entry point; initialises AppServices, then shows LoginForm
├─ AppServices.cs         composition root - the only place that news up repositories
├─ App.config             the one connection string
│
├─ Models/                plain data, no behaviour
│   ├─ Employee.cs
│   ├─ EmployeeStatus.cs  the "Active"/"Inactive" constants
│   └─ User.cs
│
├─ Data/                  everything that talks to Oracle
│   ├─ IDbConnectionFactory.cs / OracleConnectionFactory.cs
│   ├─ SqlCatalog.cs      loads Sql/*.xml and serves statements by key
│   ├─ OracleParams.cs    Dapper parameter bag that forces BindByName
│   ├─ IEmployeeRepository.cs / EmployeeRepository.cs
│   └─ IUserRepository.cs / UserRepository.cs
│
├─ Sql/                   >>> every SQL statement in the application <<<
│   ├─ EmployeeStatements.xml
│   └─ UserStatements.xml
│
├─ Forms/                 top-level windows
│   ├─ LoginForm, RegisterForm, MainForm
│   └─ UiMessage.cs       all MessageBox calls in one place
│
└─ Views/                 the three panels MainForm swaps between
    ├─ DataView.cs        base class: design-time guard, threading, error reporting
    ├─ EmployeeGrid.cs    grid binding, columns addressed by name
    └─ DashboardView, EmployeeView, SalaryView
```

The dependency direction is one-way: `Forms`/`Views` -> `Data` -> `Models`. Nothing in
`Data` references WinForms, so the repositories can be exercised without a UI.

## Where the SQL lives

Every statement sits in `Sql/*.xml`, keyed by `namespace.id`:

```xml
<statements namespace="Employee">
  <statement id="CountByStatus">
    SELECT COUNT(id)
    FROM   employees
    WHERE  delete_date IS NULL
    AND    status = :status
  </statement>
</statements>
```

and is used like this:

```csharp
connection.ExecuteScalar<int>(
    _sql.Get("Employee.CountByStatus"),
    OracleParams.New().Set("status", status));
```

`SqlCatalog` reads the folder once at startup and fails loudly on a duplicate key, an
empty statement, or a missing namespace. `Get()` on an unknown key throws and lists
every key it does know.

The XML files are copied next to the executable, so **a statement can be corrected in
`bin\Debug\Sql\` and takes effect on the next run, with no rebuild.**

## Why not iBATIS?

The XML-mapper idea is right; the library is not. Apache retired iBATIS in 2010 and the
.NET port (`IBatisNet.DataMapper`) has had no release since - no fixes, no security
patches, no support for current tooling. Its successor MyBatis never got an official
.NET version.

What this project uses instead is **Dapper + a ~180-line `SqlCatalog`**, which keeps the
part that mattered (SQL out of C#, in one reviewable folder) and drops the part that did
not (a dead dependency). Dapper is the most widely used micro-ORM in .NET, actively
maintained, and does the row-to-object mapping that made iBATIS worth having.

If you specifically need dynamic SQL (`<if>`, `<where>` inside the XML), the closest
living option is **SmartSql**; it was not used here because none of these nine
statements need it.

## Notes on the ADO.NET details

* Bind variables are Oracle `:name` style. `OracleParams` sets
  `OracleCommand.BindByName = true` - ODP.NET otherwise binds **by position**, which
  quietly mismatches arguments the moment a parameter is reordered. Dapper exposes no
  other hook for that flag, which is why the class exists.
* `DefaultTypeMap.MatchNamesWithUnderscores = true` (set in `AppServices`) is what maps
  `employee_id` -> `EmployeeId`.
* Oracle `NUMBER` arrives as `decimal`; Dapper converts it to `int` for the model.

## Notes on the migration from SQL Server

* `System.Data.SqlClient` -> `Oracle.ManagedDataAccess.Client`.
* The connection string used to be duplicated in six files and pointed at another
  machine's `employee.mdf`. It now lives once in `App.config`.
* Connections are opened per operation inside `using` blocks instead of being held in a
  long-lived field.
* The data layer no longer shows MessageBoxes; it throws, and the UI reports.
* Views no longer query the database from their constructors, so the Visual Studio
  designer no longer tries to open a connection while you edit a form. See below.

## Keeping the Visual Studio designer working

Opening `MainForm` in the designer instantiates `DashboardView`, `EmployeeView` and
`SalaryView` for real, inside `devenv.exe`, where `Program.Main` never ran and
`AppServices` is therefore empty. Two rules keep that from blowing up:

1. **Never resolve `AppServices` in a constructor.** Forms and views hold an optional
   injected repository and fall back to `AppServices` from a *property*, so
   construction never needs the container.
2. **Capture design-time detection in the constructor.**
   `LicenseManager.UsageMode` only reports `Designtime` *while a component is being
   constructed* - by the time `OnLoad` runs it is back to `Runtime`. Checking it in both
   places gives two different answers, which is exactly how this broke: the constructor
   skipped wiring the repository, then `OnLoad` decided it was not design time after
   all and dereferenced the null it had just left behind.

   `DataView` therefore stores the answer once (`_createdByDesigner`), ORs it with the
   standard `DesignMode` property (which is false for nested controls, so neither check
   is sufficient alone), and additionally refuses to load when no repository is
   available.
* Grid rows are read through `DataBoundItem` instead of `row.Cells[4]`.
* Employee photos are written to a `Directory` folder next to the executable instead of
  a hard-coded `C:\Users\milen\source\repos\...` path.
