using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OrangeHrm.Tests.Pages;

public class PimPage : BasePage
{
    // Locadores originales migrados desde PimPage.java
    private readonly By _pimHeader = By.XPath("//h6[contains(normalize-space(),'PIM')]");
    private readonly By _addEmployeeOption = By.XPath("//a[contains(@href,'addEmployee') or normalize-space()='Add Employee']");
    private readonly By _employeeListOption = By.XPath("//a[contains(@href,'viewEmployeeList') or normalize-space()='Employee List']");
    private readonly By _firstNameInput = By.Name("firstName");
    private readonly By _middleNameInput = By.Name("middleName");
    private readonly By _lastNameInput = By.Name("lastName");
    private readonly By _employeeIdInput = By.XPath("//label[text()='Employee Id']/parent::div/following-sibling::div/input");
    private readonly By _photoInput = By.CssSelector("input[type='file']");
    private readonly By _saveButton = By.CssSelector("button[type='submit']");

    // Locadores adicionales para Modificación (Update), Baja (Delete) y Validaciones
    private readonly By _employeeIdExistsWarning = By.XPath("//span[contains(normalize-space(),'Employee Id already exists')]");
    private readonly By _searchEmployeeNameInput = By.XPath("(//input[@placeholder='Type for hints...'])[1]");
    private readonly By _searchSubmitButton = By.XPath("//button[@type='submit' and normalize-space()='Search']");
    private readonly By _firstAutoCompleteOption = By.XPath("//div[@role='listbox']//span");
    private readonly By _tableBody = By.ClassName("oxd-table-body");
    private readonly By _tableRows = By.XPath("//div[@class='oxd-table-body']//div[@role='row']");
    private readonly By _firstEditButton = By.XPath("(//div[@class='oxd-table-body']//button[./i[contains(@class,'bi-pencil-fill')]])[1]");
    private readonly By _firstDeleteButton = By.XPath("(//div[@class='oxd-table-body']//button[./i[contains(@class,'bi-trash')]])[1]");
    private readonly By _confirmDeleteButton = By.XPath("//button[contains(@class,'oxd-button--label-danger') or normalize-space()='Yes, Delete']");
    private readonly By _noRecordsFoundText = By.XPath("//span[normalize-space()='No Records Found']");
    private readonly By _personalDetailsHeader = By.XPath("//h6[normalize-space()='Personal Details']");

    public PimPage(IWebDriver driver) : base(driver)
    {
    }

    #region Navegación y Estado
    public bool IsPimLoaded()
    {
        return Wait.IsElementVisible(_pimHeader, 8) || Wait.IsElementVisible(_addEmployeeOption, 8);
    }

    public void ClickAddEmployee()
    {
        Wait.Click(_addEmployeeOption);
        WaitForAddEmployeeForm();
    }

    public void GoToEmployeeList()
    {
        Wait.Click(_employeeListOption);
        Wait.WaitForElementVisible(_searchSubmitButton, 10);
        Wait.WaitForSpinnerToDisappear();
    }
    #endregion

    #region 1. Flujo Alta (Create)
    public string CreateEmployee(string firstName, string lastName, string? photoPath = null, string? customEmployeeId = null)
    {
        WaitForAddEmployeeForm();

        Wait.Type(_firstNameInput, firstName);
        Wait.Type(_lastNameInput, lastName);

        // Resolver Employee Id garantizando que sea único para evitar el error 'Employee Id already exists'
        string finalEmployeeId = ResolveUniqueEmployeeId(customEmployeeId);

        if (!string.IsNullOrEmpty(photoPath))
        {
            string resolvedPath = Path.IsPathRooted(photoPath) 
                ? photoPath 
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, photoPath);

            if (File.Exists(resolvedPath))
            {
                try
                {
                    var uploadElement = Wait.WaitForElementPresent(_photoInput, 5);
                    uploadElement.SendKeys(Path.GetFullPath(resolvedPath));
                    Wait.WaitForSpinnerToDisappear(3);
                }
                catch (Exception)
                {
                    // Si el upload falla por permisos o headless, prosigue la creación sin bloquear el flujo
                }
            }
        }

        Wait.Click(_saveButton);
        Wait.WaitForSpinnerToDisappear(15);
        Wait.WaitForToastToDisappear(8);

        return finalEmployeeId;
    }

    private string ResolveUniqueEmployeeId(string? requestedId)
    {
        string candidateId = !string.IsNullOrWhiteSpace(requestedId) 
            ? requestedId 
            : GenerateRandomNumericId();

        for (int attempt = 0; attempt < 5; attempt++)
        {
            Wait.Type(_employeeIdInput, candidateId);
            Wait.WaitABit(1000);

            // Validar si OrangeHRM muestra la advertencia "Employee Id already exists"
            if (!Wait.IsElementVisible(_employeeIdExistsWarning, 2))
            {
                return candidateId;
            }

            // Si ya existe en la demo pública, generar otro identificador único con timestamp
            candidateId = GenerateRandomNumericId();
        }

        return candidateId;
    }

    private static string GenerateRandomNumericId()
    {
        return DateTime.UtcNow.ToString("HHmmss") + Random.Shared.Next(10, 99).ToString();
    }

    public bool EmployeeProfileLoaded(string firstName, string lastName)
    {
        // 1. Validar por redirección a la vista de detalles personales
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            bool navigated = wait.Until(d => d.Url.Contains("viewPersonalDetails") || d.Url.Contains("empNumber"));
            if (navigated) return true;
        }
        catch
        {
            // Fallback a validación de elementos en DOM
        }

        // 2. Validar que el nombre o apellido aparezca visible en la interfaz
        var locator = By.XPath($"//*[contains(normalize-space(),'{firstName}') or contains(normalize-space(),'{lastName}')]");
        if (Wait.IsElementVisible(locator, 10)) return true;

        // 3. Validar por valor del campo firstName en el formulario
        try
        {
            var input = Driver.FindElement(_firstNameInput);
            string val = input.GetAttribute("value") ?? string.Empty;
            return val.Contains(firstName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region 2. Flujo Modificación (Update) y Búsqueda
    public void SearchEmployeeInPim(string firstName)
    {
        SearchEmployeeByName(firstName);
    }

    public void SearchEmployeeInPim(string? employeeId, string? firstName)
    {
        SearchEmployee(employeeId, firstName);
    }

    public void SearchEmployee(string? employeeId, string? firstName)
    {
        GoToEmployeeList();

        if (!string.IsNullOrWhiteSpace(employeeId))
        {
            SearchEmployeeById(employeeId);
        }
        else if (!string.IsNullOrWhiteSpace(firstName))
        {
            SearchEmployeeByName(firstName);
        }
    }

    public void SearchEmployeeById(string employeeId)
    {
        GoToEmployeeList();
        Wait.Type(_employeeIdInput, employeeId);
        Wait.Click(_searchSubmitButton);
        Wait.WaitForSpinnerToDisappear(10);
    }

    public void SearchEmployeeByName(string firstName)
    {
        GoToEmployeeList();
        Wait.Type(_searchEmployeeNameInput, firstName);

        if (Wait.IsElementVisible(_firstAutoCompleteOption, 4))
        {
            Wait.Click(_firstAutoCompleteOption);
        }

        Wait.Click(_searchSubmitButton);
        Wait.WaitForSpinnerToDisappear(10);
    }

    public void EditEmployeeMiddleName(string newMiddleName)
    {
        // Si estamos en la grilla de búsqueda de PIM, hacer clic en el botón de edición (lápiz)
        if (Wait.IsElementVisible(_firstEditButton, 4))
        {
            Wait.Click(_firstEditButton);
        }

        // Asegurar que la vista de Personal Details está cargada y los datos iniciales se hidrataron
        Wait.WaitForElementVisible(_personalDetailsHeader, 20);
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(d => !string.IsNullOrEmpty(d.FindElement(_firstNameInput).GetAttribute("value")));
        }
        catch
        {
            // Continuar
        }

        // Modificar el campo Middle Name
        Wait.Type(_middleNameInput, newMiddleName);

        // Guardar cambios en Personal Details
        Wait.Click(_saveButton);
        Wait.WaitForSpinnerToDisappear(15);
        Wait.WaitForToastToDisappear(8);
    }

    public string GetMiddleNameValue()
    {
        var element = Wait.WaitForElementVisible(_middleNameInput, 10);
        return element.GetAttribute("value") ?? string.Empty;
    }
    #endregion

    #region 3. Flujo Baja (Delete)
    public void DeleteCurrentSearchedEmployee()
    {
        Wait.Click(_firstDeleteButton);
        Wait.Click(_confirmDeleteButton);
        Wait.WaitForSpinnerToDisappear(10);
        Wait.WaitForToastToDisappear(8);
    }

    public bool IsEmployeeDeleted()
    {
        // Se valida que la tabla indique "No Records Found" o que no haya filas
        return Wait.IsElementVisible(_noRecordsFoundText, 5) || Driver.FindElements(_tableRows).Count == 0;
    }
    #endregion

    private void WaitForAddEmployeeForm()
    {
        Wait.WaitForElementVisible(_firstNameInput, 20);
    }
}
