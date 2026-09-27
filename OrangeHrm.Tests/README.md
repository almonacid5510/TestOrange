# OrangeHRM Test Automation Framework (C# .NET 8 + Selenium + xUnit)

Framework de automatización de pruebas end-to-end de nivel Senior para **OrangeHRM**, migrado desde Java/Serenity BDD a **C# (.NET 8)** utilizando **Selenium WebDriver**, **xUnit**, **Page Object Model (POM)**, **ADO.NET / Dapper** y **paralelismo cross-browser (Chrome y Edge)**.

---

## 📁 1. Estructura del Proyecto

```text
c:\Users\Juan\VM_Login_Bot\
│
├── OrangeHrmAutomation.sln               # Solución de .NET
├── OrangeHrm.Tests/                      # Proyecto de pruebas C# (.NET 8)
│   ├── OrangeHrm.Tests.csproj            # Configuración de paquetes NuGet y SDK
│   ├── AssemblyInfo.cs                   # Configuración de paralelismo por clase para xUnit
│   ├── xunit.runner.json                 # Configuración de threads y algoritmo paralelo
│   │
│   ├── Models/                           # Modelos de datos
│   │   └── EmployeeModel.cs              # Representación del empleado para ABM
│   │
│   ├── Pages/                            # Page Object Model (POM estándar C#)
│   │   ├── BasePage.cs                   # Clase base con esperas explícitas y utilidades
│   │   ├── LoginPage.cs                  # Mapeo de Login (reciclado de Java)
│   │   ├── DashboardPage.cs              # Mapeo de Dashboard y navegación (reciclado de Java)
│   │   ├── PimPage.cs                    # Mapeo PIM: Alta, Búsqueda, Edición y Baja
│   │   └── DirectoryPage.cs              # Mapeo Directory: Búsqueda y validación (reciclado de Java)
│   │
│   ├── Helpers/                          # Arquitectura y utilidades de soporte
│   │   ├── DriverFactory.cs              # Creación de WebDrivers (Chrome, Edge, Headless, Grid)
│   │   ├── DatabaseHelper.cs             # Capa de BD con ADO.NET y Dapper (Real / Simulación)
│   │   └── WaitHelper.cs                 # Esperas explícitas con WebDriverWait y fallback JS
│   │
│   ├── Tests/                            # Suites de pruebas xUnit
│   │   ├── BaseTest.cs                   # Setup/Teardown y ciclo de vida de Driver y BD
│   │   ├── CreateEmployeeTests.cs        # Caso 1: Alta (Create) - Paralelo Chrome/Edge
│   │   ├── UpdateEmployeeTests.cs        # Caso 2: Modificación (Update) - Paralelo Chrome/Edge
│   │   ├── DeleteEmployeeTests.cs        # Caso 3: Baja (Delete) - Paralelo Chrome/Edge
│   │   ├── FullLifecycleAbmTests.cs      # Flujo E2E ABM Completo (Alta -> Modif -> Baja)
│   │   └── DatabaseHelperTests.cs        # Tests unitarios de conexión y consultas SQL
│   │
│   └── TestData/                         # Datos de prueba estáticos
│       └── profile.jpg                   # Imagen de perfil para carga de avatar
```

---

## 🔄 2. Migración y Reciclaje de Locadores desde Java

Se conservaron fielmente los selectores originales definidos en las páginas de Java (`LoginPage.java`, `DashboardPage.java`, `PimPage.java`, `DirectoryPage.java`):

| Página | Elemento | Selector reciclado de Java |
|---|---|---|
| **LoginPage** | Input Usuario | `By.Name("username")` |
| **LoginPage** | Input Password | `By.Name("password")` |
| **LoginPage** | Botón Login | `By.CssSelector("button[type='submit']")` |
| **DashboardPage** | Menú PIM | `By.XPath("//span[normalize-space()='PIM']")` |
| **DashboardPage** | Menú Directory | `By.XPath("//span[normalize-space()='Directory']")` |
| **DashboardPage** | Título Dashboard | `By.XPath("//h6[normalize-space()='Dashboard']")` |
| **PimPage** | Header PIM | `By.XPath("//h6[contains(normalize-space(),'PIM')]")` |
| **PimPage** | Opción Add Employee | `By.XPath("//a[contains(@href,'addEmployee') or normalize-space()='Add Employee']")` |
| **PimPage** | Input First Name | `By.Name("firstName")` |
| **PimPage** | Input Last Name | `By.Name("lastName")` |
| **PimPage** | Input Foto Perfil | `By.CssSelector("input[type='file']")` |
| **PimPage** | Botón Save | `By.CssSelector("button[type='submit']")` |
| **DirectoryPage** | Input Autocomplete | `By.XPath("(//input[@placeholder='Type for hints...'])[1]")` |
| **DirectoryPage** | Sugerencia Lista | `By.XPath("//div[@role='listbox']//span")` |
| **DirectoryPage** | Botón Buscar | `By.CssSelector("button[type='submit']")` |

---

## ⚙️ 3. Requerimientos Implementados

### Ejercicio 1 – Flujo ABM Completo en OrangeHRM:
- **Alta (Create)**:
  - Navega a PIM -> Add Employee.
  - Completa nombre, apellido y sube la foto de perfil.
  - Valida en UI que el perfil del empleado ha sido creado (`pimPage.EmployeeProfileLoaded(...)`).
- **Modificación (Update)**:
  - Busca el empleado por nombre en la lista de PIM con el input de búsqueda interactivo.
  - Abre el formulario de edición y modifica su segundo nombre (`middleName`).
  - Guarda los cambios y valida en UI que el nuevo valor persiste (`pimPage.GetMiddleNameValue()`).
- **Baja (Delete)**:
  - Localiza el empleado en la tabla de resultados.
  - Presiona el botón de eliminar, confirma la acción en el diálogo modal emergente (`Yes, Delete`).
  - Valida en UI que el registro ya no existe (`pimPage.IsEmployeeDeleted()`, mostrando `No Records Found`).
- **Flujo Completo E2E**:
  - `FullLifecycleAbmTests` ejecuta el ciclo de vida íntegro en una misma sesión transaccional.

### Ejercicio 2 – Conexión a Base de Datos y Aserciones (ADO.NET / Dapper):
- La clase `DatabaseHelper` proporciona una arquitectura dual:
  1. **Conexión Real**: Permite configurar un Connection String real (`ORANGEHRM_DB_CONNECTION`) conectándose vía `SqlConnection` de ADO.NET.
  2. **Modo Simulado / SQLite en memoria**: Diseñado para entornos CI/CD y pruebas sobre la demo pública de OrangeHRM (la cual bloquea el puerto 3306 hacia internet). Utiliza `Microsoft.Data.Sqlite` ejecutando sentencias SQL reales (`CREATE TABLE`, `INSERT`, `UPDATE`, `DELETE`, `SELECT COUNT(1)`).
- En cada prueba se realiza la verificación con aserciones xUnit:
  - **Alta**: `int dbCount = DbHelper.GetEmployeeCount(emp.FirstName, emp.LastName); Assert.Equal(1, dbCount);`
  - **Modificación**: `string? dbMiddle = DbHelper.GetEmployeeMiddleName(emp.FirstName); Assert.Equal(newMiddleName, dbMiddle);`
  - **Baja**: `int dbCount = DbHelper.GetEmployeeCount(emp.FirstName, emp.LastName); Assert.Equal(0, dbCount);`

### Ejercicio 3 – Paralelismo en Múltiples Navegadores:
- Configurado con `[Theory]` e `[InlineData("Chrome")]` / `[InlineData("Edge")]`.
- Paralelización por clase (`CollectionBehavior.CollectionPerClass`) en `AssemblyInfo.cs` y `xunit.runner.json` con `maxParallelThreads: 6`.
- Cada prueba es **autocontenida y aislada**: genera sufijos aleatorios (`Guid`) para que los hilos concurrentes de Chrome y Edge jamás colisionen con los mismos datos en el sistema.

---

## 🚀 4. Comandos de Ejecución (`dotnet test`)

### Restaurar y Compilar la Solución:
```powershell
dotnet build
```

### Ejecutar todas las pruebas (Lanzamiento paralelo de los 3 casos en Chrome y Edge):
```powershell
dotnet test
```

### Ejecutar en modo Headless (ideal para CI/CD):
```powershell
$env:CI = "true"
dotnet test
```

### Ejecutar solo las pruebas de Base de Datos (ADO.NET / Dapper):
```powershell
dotnet test --filter "FullyQualifiedName~DatabaseHelperTests"
```

### Ejecutar solo el caso de Alta (Chrome y Edge):
```powershell
dotnet test --filter "FullyQualifiedName~CreateEmployeeTests"
```

### Ejecutar solo el caso de Modificación (Chrome y Edge):
```powershell
dotnet test --filter "FullyQualifiedName~UpdateEmployeeTests"
```

### Ejecutar solo el caso de Baja (Chrome y Edge):
```powershell
dotnet test --filter "FullyQualifiedName~DeleteEmployeeTests"
```

### Ejecutar el Flujo Completo E2E (Alta -> Modificación -> Baja):
```powershell
dotnet test --filter "FullyQualifiedName~FullLifecycleAbmTests"
```

### Filtrar por navegador específico:
```powershell
# Solo pruebas en Google Chrome
dotnet test --filter "browser=Chrome"

# Solo pruebas en Microsoft Edge
dotnet test --filter "browser=Edge"
```

---

## 📦 5. Paquetes NuGet Utilizados

- `Selenium.WebDriver` (v4.25.0) - Control del navegador mediante protocolo W3C WebDriver.
- `Selenium.Support` (v4.25.0) - Soporte para WebDriverWait y Page Factory.
- `DotNetSeleniumExtras.WaitHelpers` (v3.11.0) - Condiciones esperadas (`ExpectedConditions`).
- `xunit` (v2.9.2) - Framework de pruebas unitarias y de integración.
- `xunit.runner.visualstudio` (v2.8.2) - Integrador del test runner con Visual Studio y `dotnet test`.
- `Microsoft.NET.Test.Sdk` (v17.11.1) - SDK de pruebas de .NET.
- `Dapper` (v2.1.35) - Micro ORM de alto rendimiento para mapeo objeto-relacional y consultas SQL.
- `Microsoft.Data.SqlClient` (v5.2.2) - Proveedor ADO.NET oficial para SQL Server.
- `Microsoft.Data.Sqlite` (v8.0.10) - Proveedor ADO.NET para SQLite en memoria.
- `FluentAssertions` (v6.12.1) - Aserciones fluidas legibles.
