using OpenQA.Selenium;
using OrangeHrm.Tests.Helpers;
using OrangeHrm.Tests.Pages;

namespace OrangeHrm.Tests.Tests;

public abstract class BaseTest : IDisposable
{
    protected string AdminUser => ConfigHelper.Username;
    protected string AdminPassword => ConfigHelper.Password;
    protected const string TestPhotoPath = "TestData/profile.jpg";

    protected readonly DatabaseHelper DbHelper;

    protected BaseTest()
    {
        DbHelper = new DatabaseHelper(ConfigHelper.DatabaseConnectionString);
    }

    protected void LoginAsAdmin(IWebDriver driver)
    {
        var loginPage = new LoginPage(driver);
        var dashboardPage = new DashboardPage(driver);

        loginPage.OpenLoginPage();
        loginPage.Login(AdminUser, AdminPassword);

        if (!dashboardPage.IsDashboardLoaded())
        {
            throw new InvalidOperationException("No se logró cargar el Dashboard de OrangeHRM después del inicio de sesión.");
        }
    }

    public void Dispose()
    {
        DbHelper.Dispose();
        GC.SuppressFinalize(this);
    }
}
