# Automatización OrangeHRM (C# .NET 8 + Selenium + xUnit)

Solución completa de automatización de pruebas end-to-end para **OrangeHRM**, desarrollada en **C# (.NET 8)** con **Selenium WebDriver**, **xUnit**, **Page Object Model (POM)**, aserciones en Base de Datos con **ADO.NET y Dapper**, y **ejecución paralela cross-browser (1 ventana en Chrome y 1 ventana en Edge simultáneas)**.

---

## 🚀 Requerimientos Implementados

1. **Ejercicio 1 – Flujo ABM Completo en OrangeHRM**:
   - **Alta (Create)**: Creación de empleado con nombre, apellido y avatar en PIM, resolviendo automáticamente un ID único y validando la creación en UI (`EmployeeProfileLoaded`).
   - **Modificación (Update)**: Edición de datos (`middleName`) directamente en el perfil del empleado y validación de persistencia en UI (`GetMiddleNameValue`).
   - **Baja (Delete)**: Búsqueda del empleado por su ID exacto, confirmación en el modal y validación en UI de que no figura en la grilla (`IsEmployeeDeleted`).

2. **Ejercicio 2 – Conexión a Base de Datos y Aserciones (ADO.NET + Dapper)**:
   - Helper de arquitectura `DatabaseHelper` que soporta conexión real a servidor de BD (`ORANGEHRM_DB_CONNECTION`) y modo simulado en memoria (`Microsoft.Data.Sqlite`) para ejecuciones determinísticas en CI/CD y demos públicas.
   - En cada etapa (Alta, Modificación, Baja) se realiza una consulta SQL mediante Dapper y ADO.NET y un `Assert` que verifica el cambio en la base de datos.

3. **Ejercicio 3 – Paralelismo en Múltiples Navegadores (Chrome + Edge)**:
   - Configuración de xUnit mediante clases paralelas dedicadas (`ChromeEmployeeAbmTests` y `EdgeEmployeeAbmTests`).
   - Límite de concurrencia configurado exactamente a **2 hilos** (`maxParallelThreads: 2`): **1 ventana en Google Chrome y 1 ventana en Microsoft Edge simultáneamente**.

---

## 📁 Estructura del Proyecto

```text
VM_Login_Bot/
├── OrangeHrmAutomation.sln               # Solución principal de .NET
├── OrangeHrm.Tests/
│   ├── OrangeHrm.Tests.csproj            # Configuración y dependencias NuGet
│   ├── appsettings.json                  # Parámetros y credenciales configurables
│   ├── AssemblyInfo.cs                   # Configuración de concurrencia xUnit (2 hilos)
│   ├── xunit.runner.json                 # Configuración del ejecutor de pruebas
│   ├── Helpers/
│   │   ├── ConfigHelper.cs               # Lectura centralizada de configuración
│   │   ├── DatabaseHelper.cs             # Capa ADO.NET / Dapper / SQLite
│   │   ├── DriverFactory.cs              # Inicialización de ChromeDriver y EdgeDriver
│   │   └── WaitHelper.cs                 # Waits dinámicos explícitos (spinners, toasts)
│   ├── Models/
│   │   └── EmployeeModel.cs              # Modelo de datos de Empleado
│   ├── Pages/                            # Page Object Model (POM)
│   │   ├── BasePage.cs                   # Clase base con esperas comunes y toasts
│   │   ├── LoginPage.cs                  # Interacción con la pantalla de autenticación
│   │   ├── DashboardPage.cs              # Navegación hacia módulos (PIM, etc.)
│   │   ├── PimPage.cs                    # Acciones y validaciones de PIM (ABM)
│   │   └── DirectoryPage.cs              # Búsqueda y validaciones en directorio
│   ├── TestData/
│   │   └── profile.jpg                   # Imagen para carga de avatar de empleado
│   └── Tests/
│       ├── BaseTest.cs                   # Setup/Teardown común y login
│       ├── DatabaseHelperTests.cs        # Tests unitarios de base de datos
│       └── EmployeeAbmTests.cs           # Tests ABM concurrentes para Chrome y Edge
└── README.md
```

---

## 💻 Comandos de Ejecución (`dotnet test`)

```powershell
# Compilar la solución
dotnet build

# Ejecutar el flujo ABM completo (1 navegador en Chrome y 1 en Edge simultáneamente)
dotnet test

# Ejecutar en modo Headless (sin levantar ventanas visuales, ideal para CI/CD)
$env:CI = "true"; dotnet test

# Ejecutar únicamente en Google Chrome
dotnet test --filter "FullyQualifiedName~Chrome"

# Ejecutar únicamente en Microsoft Edge
dotnet test --filter "FullyQualifiedName~Edge"

# Ejecutar pruebas unitarias de arquitectura de BD (ADO.NET / Dapper)
dotnet test --filter "FullyQualifiedName~DatabaseHelperTests"
```