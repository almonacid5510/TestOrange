using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace OrangeHrm.Tests.Helpers;

public class WaitHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _defaultWait;

    public WaitHelper(IWebDriver driver, int defaultTimeoutSeconds = 15)
    {
        _driver = driver;
        _defaultWait = new WebDriverWait(driver, TimeSpan.FromSeconds(defaultTimeoutSeconds));
    }

    public IWebElement WaitForElementVisible(By locator, int? timeoutSeconds = null)
    {
        var wait = timeoutSeconds.HasValue 
            ? new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds.Value)) 
            : _defaultWait;

        return wait.Until(ExpectedConditions.ElementIsVisible(locator));
    }

    public IWebElement WaitForElementClickable(By locator, int? timeoutSeconds = null)
    {
        var wait = timeoutSeconds.HasValue 
            ? new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds.Value)) 
            : _defaultWait;

        return wait.Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    public IWebElement WaitForElementPresent(By locator, int? timeoutSeconds = null)
    {
        var wait = timeoutSeconds.HasValue 
            ? new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds.Value)) 
            : _defaultWait;

        return wait.Until(ExpectedConditions.ElementExists(locator));
    }

    public bool WaitForElementToDisappear(By locator, int? timeoutSeconds = null)
    {
        var wait = timeoutSeconds.HasValue 
            ? new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds.Value)) 
            : _defaultWait;

        return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
    }

    public bool IsElementVisible(By locator, int timeoutSeconds = 5)
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(d => {
                var elements = d.FindElements(locator);
                return elements.Count > 0 && elements[0].Displayed;
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public void Click(By locator)
    {
        try
        {
            var element = WaitForElementClickable(locator);
            element.Click();
        }
        catch (Exception)
        {
            var element = WaitForElementPresent(locator);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
    }

    public void Type(By locator, string text)
    {
        var element = WaitForElementVisible(locator);
        element.Clear();
        // Clear via backspaces if Angular/React/Vue input has cached value
        element.SendKeys(Keys.Control + "a");
        element.SendKeys(Keys.Backspace);
        element.SendKeys(text);
    }

    public bool WaitForSpinnerToDisappear(int timeoutSeconds = 15)
    {
        try
        {
            var spinnerLocator = By.CssSelector(".oxd-loading-spinner, .oxd-form-loader");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(spinnerLocator));
        }
        catch (Exception)
        {
            return true;
        }
    }

    public bool WaitForToastToDisappear(int timeoutSeconds = 10)
    {
        try
        {
            var toastLocator = By.CssSelector(".oxd-toast");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(toastLocator));
        }
        catch (Exception)
        {
            return true;
        }
    }

    public bool WaitForUrlContains(string partialUrl, int timeoutSeconds = 15)
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(d => d.Url.Contains(partialUrl, StringComparison.OrdinalIgnoreCase));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public void WaitABit(int milliseconds)
    {
        Thread.Sleep(milliseconds);
    }
}
