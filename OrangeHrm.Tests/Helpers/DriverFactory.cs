using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;

namespace OrangeHrm.Tests.Helpers;

public static class DriverFactory
{
    public static IWebDriver CreateDriver(string browserName, bool headless = false)
    {
        string? remoteUrl = Environment.GetEnvironmentVariable("SELENIUM_REMOTE_URL");

        if (!string.IsNullOrEmpty(remoteUrl))
        {
            return CreateRemoteDriver(browserName, remoteUrl);
        }

        return browserName.ToLowerInvariant() switch
        {
            "chrome" => CreateChromeDriver(headless),
            "edge" => CreateEdgeDriver(headless),
            _ => throw new ArgumentException($"Navegador no soportado: '{browserName}'. Utilice 'Chrome' o 'Edge'.")
        };
    }

    private static IWebDriver CreateChromeDriver(bool headless)
    {
        var options = new ChromeOptions();
        options.AddArguments(
            "--start-maximized",
            "--disable-notifications",
            "--disable-popup-blocking",
            "--disable-infobars",
            "--no-sandbox",
            "--disable-dev-shm-usage"
        );

        if (headless || Environment.GetEnvironmentVariable("CI") == "true")
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;

        var driver = new ChromeDriver(service, options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        return driver;
    }

    private static IWebDriver CreateEdgeDriver(bool headless)
    {
        var options = new EdgeOptions();
        options.AddArguments(
            "--start-maximized",
            "--disable-notifications",
            "--disable-popup-blocking",
            "--no-sandbox",
            "--disable-dev-shm-usage"
        );

        if (headless || Environment.GetEnvironmentVariable("CI") == "true")
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string localDriver = Path.Combine(baseDir, "msedgedriver.exe");
        string cacheDriver = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cache", "selenium", "msedgedriver", "win64", "154.0.4258.37", "msedgedriver.exe");

        EdgeDriverService service;
        if (File.Exists(localDriver))
        {
            service = EdgeDriverService.CreateDefaultService(baseDir, "msedgedriver.exe");
        }
        else if (File.Exists(cacheDriver))
        {
            service = EdgeDriverService.CreateDefaultService(Path.GetDirectoryName(cacheDriver)!, "msedgedriver.exe");
        }
        else
        {
            service = EdgeDriverService.CreateDefaultService();
        }

        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;

        var driver = new EdgeDriver(service, options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        return driver;
    }

    private static IWebDriver CreateRemoteDriver(string browserName, string remoteUrl)
    {
        DriverOptions options = browserName.ToLowerInvariant() switch
        {
            "edge" => new EdgeOptions(),
            _ => new ChromeOptions()
        };

        return new RemoteWebDriver(new Uri(remoteUrl), options.ToCapabilities(), TimeSpan.FromSeconds(60));
    }
}
