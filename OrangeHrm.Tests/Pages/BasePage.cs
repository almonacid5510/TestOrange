using OpenQA.Selenium;
using OrangeHrm.Tests.Helpers;

namespace OrangeHrm.Tests.Pages;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WaitHelper Wait;

    protected readonly By ToastMessage = By.CssSelector(".oxd-toast, .oxd-text--toast-message");

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WaitHelper(driver);
    }

    public string GetCurrentUrl() => Driver.Url;

    public bool IsToastMessageDisplayed(string? expectedText = null, int timeoutSeconds = 5)
    {
        try
        {
            if (Wait.IsElementVisible(ToastMessage, timeoutSeconds))
            {
                if (string.IsNullOrEmpty(expectedText)) return true;
                var toast = Driver.FindElement(ToastMessage);
                return toast.Text.Contains(expectedText, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }
}
