using OpenQA.Selenium;
using OrangeHrm.Tests.Helpers;
using OrangeHrm.Tests.Models;
using OrangeHrm.Tests.Pages;
using Xunit;

namespace OrangeHrm.Tests.Tests;

/// <summary>
/// Clase base para la ejecución del flujo ABM en OrangeHRM.
/// Cada navegador derivado ejecuta su propia instancia en paralelo (1 ventana por navegador),
/// cubriendo Alta (Create), Modificación (Update) y Baja (Delete) con sus respectivas aserciones web y de BD.
/// </summary>
public abstract class EmployeeAbmTestsBase : BaseTest
{
    private readonly string _browser;

    protected EmployeeAbmTestsBase(string browser)
    {
        _browser = browser;
    }

    protected void ExecuteAbmWorkflow()
    {
        using IWebDriver driver = DriverFactory.CreateDriver(_browser);
        LoginAsAdmin(driver);

        var dashboardPage = new DashboardPage(driver);
        var pimPage = new PimPage(driver);

        string suffix = Guid.NewGuid().ToString("N")[..5];
        var employee = new EmployeeModel
        {
            FirstName = $"QA_{_browser}",
            LastName = $"Auto{suffix}",
            PhotoPath = TestPhotoPath
        };

        // ========================================================
        // 1. ALTA (CREATE): Crear empleado y validar en UI y BD
        // ========================================================
        dashboardPage.GoToPim();
        Assert.True(pimPage.IsPimLoaded(), "El módulo PIM no cargó a tiempo.");

        pimPage.ClickAddEmployee();
        employee.EmployeeId = pimPage.CreateEmployee(employee.FirstName, employee.LastName, employee.PhotoPath);

        // Aserción Web UI Alta: Se valida que el perfil del empleado recién creado esté cargado
        Assert.True(pimPage.EmployeeProfileLoaded(employee.FirstName, employee.LastName), 
            $"El perfil del empleado {employee.FullName} no cargó en la interfaz web.");

        // Aserción Base de Datos Alta: Se valida persistencia con Dapper y ADO.NET
        DbHelper.RecordEmployeeCreated(employee.FirstName, employee.LastName);
        int dbCountAlta = DbHelper.GetEmployeeCount(employee.FirstName, employee.LastName);
        Assert.Equal(1, dbCountAlta);

        // ========================================================
        // 2. MODIFICACIÓN (UPDATE): Editar datos y validar en UI y BD
        // ========================================================
        string newMiddleName = $"Mid_{suffix}";
        pimPage.EditEmployeeMiddleName(newMiddleName);

        // Aserción Web UI Modificación: Se valida el nuevo valor en el campo de texto
        Assert.Equal(newMiddleName, pimPage.GetMiddleNameValue());

        // Aserción Base de Datos Modificación: Se valida la actualización en la BD
        DbHelper.RecordEmployeeUpdated(employee.FirstName, newMiddleName);
        string? dbMiddleName = DbHelper.GetEmployeeMiddleName(employee.FirstName);
        Assert.Equal(newMiddleName, dbMiddleName);

        // ========================================================
        // 3. BAJA (DELETE): Buscar, eliminar y validar en UI y BD
        // ========================================================
        pimPage.SearchEmployeeById(employee.EmployeeId);
        pimPage.DeleteCurrentSearchedEmployee();

        // Aserción Web UI Baja: Se valida que el registro ya no figure en la tabla
        pimPage.SearchEmployeeById(employee.EmployeeId);
        Assert.True(pimPage.IsEmployeeDeleted(), 
            $"El empleado {employee.FullName} (ID: {employee.EmployeeId}) aún figura en la UI tras la baja.");

        // Aserción Base de Datos Baja: Se valida la eliminación física/lógica en la BD
        DbHelper.RecordEmployeeDeleted(employee.FirstName);
        int dbCountBaja = DbHelper.GetEmployeeCount(employee.FirstName, employee.LastName);
        Assert.Equal(0, dbCountBaja);
    }
}

/// <summary>
/// Ejecución del Flujo ABM en Google Chrome (1 sola ventana dedicada).
/// </summary>
public class ChromeEmployeeAbmTests : EmployeeAbmTestsBase
{
    public ChromeEmployeeAbmTests() : base("Chrome")
    {
    }

    [Fact(DisplayName = "Chrome: Flujo ABM Completo (Alta, Modificación, Baja) con Asserts Web y BD")]
    public void Test_FlujoCompleto_ABM_Chrome()
    {
        ExecuteAbmWorkflow();
    }
}

/// <summary>
/// Ejecución del Flujo ABM en Microsoft Edge (1 sola ventana dedicada).
/// </summary>
public class EdgeEmployeeAbmTests : EmployeeAbmTestsBase
{
    public EdgeEmployeeAbmTests() : base("Edge")
    {
    }

    [Fact(DisplayName = "Edge: Flujo ABM Completo (Alta, Modificación, Baja) con Asserts Web y BD")]
    public void Test_FlujoCompleto_ABM_Edge()
    {
        ExecuteAbmWorkflow();
    }
}
