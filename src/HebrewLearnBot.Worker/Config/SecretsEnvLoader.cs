using DotNetEnv;

namespace HebrewLearnBot.Worker.Config;

public static class SecretsEnvLoader
{
    /// <summary>
    /// Загружает secrets.env, если он есть. Файл ищется вверх по дереву каталогов от каталога
    /// сборки: при `dotnet run` рабочим каталогом становится папка проекта, а secrets.env лежит
    /// в корне репозитория. В контейнере файла нет — переменные приходят через env_file.
    /// </summary>
    public static void Load()
    {
        var path = FindSecretsEnv();
        if (path is null)
        {
            return;
        }

        // NoClobber: переменные окружения, заданные снаружи (например POSTGRES_HOST из
        // docker compose), важнее содержимого файла.
        Env.NoClobber().Load(path);
    }

    private static string? FindSecretsEnv()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "secrets.env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
