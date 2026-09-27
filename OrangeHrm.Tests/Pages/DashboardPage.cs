using OpenQA.Selenium;

namespace OrangeHrm.Tests.Pages;

public class DashboardPage : BasePage
{
    // Locadores idénticos a los definidos en Java (DashboardPage.java)
    private readonly By _pimMenu = By.XPath("//span[normalize-space()='PIM']");
    private readonly By _directoryMenu = By.XPath("//span[normalize-space()='Directory']");
    private readonly By _dashboardHeader = By.XPath("//h6[normalize-space()='Dashboard']");

    public DashboardPage(IWebDriver driver) : base(driver)
    {
    }

    public void GoToPim()
    {
        Wait.Click(_pimMenu);
    }

    public void GoToDirectory()
    {
        Wait.Click(_directoryMenu);
    }

    public bool IsDashboardLoaded()
    {
        return Wait.IsElementVisible(_dashboardHeader, 10);
    }
}
