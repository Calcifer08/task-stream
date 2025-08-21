namespace TaskStream.ApiGateway.Extensions;

public static class ConfigurationExtensions
{
    public static string GetRequired(this IConfiguration config, string key)
    {
        return config[key]
            ?? throw new InvalidOperationException($"Обязательный параметр конфигурации '{key}' отсутствует");
    }
}