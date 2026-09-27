using OpenQA.Selenium;

namespace OrangeHrm.Tests.Pages;

public class DirectoryPage : BasePage
{
    // Locadores idénticos a los definidos en Java (DirectoryPage.java)
    private readonly By _employeeNameInput = By.XPath("(//input[@placeholder='Type for hints...'])[1]");
    private readonly By _firstSuggestion = By.XPath("//div[@role='listbox']//span");
    private readonly By _searchButton = By.CssSelector("button[type='submit']");

    public DirectoryPage(IWebDriver driver) : base(driver)
    {
    }

    public void SearchEmployee(string firstName)
    {
        Wait.Type(_employeeNameInput, firstName);

        if (Wait.IsElementVisible(_firstSuggestion, 5))
        {
            Wait.Click(_firstSuggestion);
        }

        Wait.WaitABit(1000);
        Wait.Click(_searchButton);
        Wait.WaitABit(3000);
    }

    public bool EmployeeAppearsInResults(string firstName, string lastName)
    {
        var locator = By.XPath($"//*[contains(normalize-space(),'{firstName}') or contains(normalize-space(),'{lastName}')]");
        return Driver.FindElements(locator).Count > 0;
    }
}
