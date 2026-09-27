using System.Text.Json;

namespace OrangeHrm.Tests.Helpers;

public static class ConfigHelper
{
    private static readonly JsonDocument? ConfigDoc;

    static ConfigHelper()
    {
        string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (File.Exists(appSettingsPath))
        {
            try
            {
                string json = File.ReadAllText(appSettingsPath);
                ConfigDoc = JsonDocument.Parse(json);
            }
            catch
            {
                ConfigDoc = null;
            }
        }
    }

    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("ORANGEHRM_BASE_URL")
        ?? GetConfigString("OrangeHrm", "BaseUrl")
        ?? "https://opensource-demo.orangehrmlive.com/";

    public static string Username =>
        Environment.GetEnvironmentVariable("ORANGEHRM_USERNAME")
        ?? GetConfigString("OrangeHrm", "Username")
        ?? "Admin";

    public static string Password =>
        Environment.GetEnvironmentVariable("ORANGEHRM_PASSWORD")
        ?? GetConfigString("OrangeHrm", "Password")
        ?? "admin123";

    public static string? DatabaseConnectionString =>
        Environment.GetEnvironmentVariable("ORANGEHRM_DB_CONNECTION")
        ?? GetConfigString("Database", "ConnectionString");

    public static bool IsHeadless =>
        Environment.GetEnvironmentVariable("CI") == "true"
        || (GetConfigBool("Execution", "Headless") ?? false);

    public static int DefaultTimeoutSeconds =>
        GetConfigInt("Execution", "DefaultTimeoutSeconds") ?? 15;

    private static string? GetConfigString(string section, string key)
    {
        if (ConfigDoc == null) return null;
        if (ConfigDoc.RootElement.TryGetProperty(section, out var sectionEl) &&
            sectionEl.TryGetProperty(key, out var keyEl))
        {
            return keyEl.GetString();
        }
        return null;
    }

    private static bool? GetConfigBool(string section, string key)
    {
        if (ConfigDoc == null) return null;
        if (ConfigDoc.RootElement.TryGetProperty(section, out var sectionEl) &&
            sectionEl.TryGetProperty(key, out var keyEl))
        {
            return keyEl.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }
        return null;
    }

    private static int? GetConfigInt(string section, string key)
    {
        if (ConfigDoc == null) return null;
        if (ConfigDoc.RootElement.TryGetProperty(section, out var sectionEl) &&
            sectionEl.TryGetProperty(key, out var keyEl) &&
            keyEl.TryGetInt32(out int val))
        {
            return val;
        }
        return null;
    }
}
