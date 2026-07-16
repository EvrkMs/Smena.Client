using System.Text;

namespace Smena.Client.Helpers;

/// <summary>
/// Лог ТОЛЬКО ошибок (сознательное решение: полный лог забивает диск, а типовые
/// проблемы вида "минус не сошёлся" отсекает UI ещё до сервера). Пишет в
/// %LOCALAPPDATA%\Smena.Client\logs\errors.log; при превышении 512 КБ текущий
/// файл переезжает в errors.log.old (одно поколение), так что суммарный размер
/// ограничен ~1 МБ. Ошибки самого логирования глотаются — лог не должен ронять
/// приложение и не должен требовать проверок у вызывающих.
/// </summary>
internal static class ErrorLog
{
    private const long MaxBytes = 512 * 1024;
    private static readonly object Sync = new();
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Smena.Client", "logs", "errors.log");

    public static void Write(string context, Exception ex) => Write($"{context}: {ex}");

    public static void Write(string message)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);

                var file = new FileInfo(LogPath);
                if (file.Exists && file.Length > MaxBytes)
                {
                    var old = LogPath + ".old";
                    File.Delete(old);
                    File.Move(LogPath, old);
                }

                File.AppendAllText(
                    LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}",
                    Encoding.UTF8);
            }
        }
        catch
        {
            // Никогда не роняем приложение из-за проблем с записью лога.
        }
    }
}
