using OpenQA.Selenium;
using OrangeHrm.Tests.Helpers;

namespace OrangeHrm.Tests.Pages;

public class LoginPage : BasePage
{
    private const string DefaultBaseUrl = "https://opensource-demo.orangehrmlive.com/";

    // Locadores idénticos a los definidos en Java (LoginPage.java)
    private readonly By _usernameInput = By.Name("username");
    private readonly By _passwordInput = By.Name("password");
    private readonly By _loginButton = By.CssSelector("button[type='submit']");

    public LoginPage(IWebDriver driver) : base(driver)
    {
    }

    public void OpenLoginPage(string? url = null)
    {
        string targetUrl = url ?? ConfigHelper.BaseUrl;
        Driver.Navigate().GoToUrl(targetUrl);
        WaitForLoginPageToBeReady();
    }

    public void Login(string? username = null, string? password = null)
    {
        string finalUser = username ?? ConfigHelper.Username;
        string finalPass = password ?? ConfigHelper.Password;

        Wait.Type(_usernameInput, finalUser);
        Wait.Type(_passwordInput, finalPass);
        Wait.Click(_loginButton);
    }

    public bool IsLoginPageLoaded()
    {
        return Wait.IsElementVisible(_usernameInput, 10);
    }

    private void WaitForLoginPageToBeReady()
    {
        Wait.WaitForElementVisible(_usernameInput, 25);
    }
}
