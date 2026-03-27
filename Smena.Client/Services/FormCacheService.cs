using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Smena.Client.Services;

/// <summary>
/// Lightweight async CSV cache for form field values.
/// Stores data in %LOCALAPPDATA%/Smena.Client/form-cache.csv so that
/// form inputs survive application restarts without blocking the UI.
/// 
/// CSV format: Key,Value (both fields are escaped if they contain comma/quote/newline).
/// </summary>
public sealed class FormCacheService : IDisposable
{
    private static readonly string CacheDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Smena.Client");

    private static readonly string CachePath = Path.Combine(CacheDir, "form-cache.csv");

    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _writeLock = new();
    private volatile bool _dirty;
    private bool _disposed;
    private readonly System.Windows.Forms.Timer _flushTimer;

    public FormCacheService()
    {
        LoadFromDisk();

        // Periodic flush every 2 seconds if dirty — avoids per-keystroke I/O.
        _flushTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _flushTimer.Tick += (_, _) => FlushIfDirty();
        _flushTimer.Start();
    }

    /// <summary>
    /// Set a cached value. Non-blocking; actual write is deferred.
    /// </summary>
    public void Set(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        if (value == null)
        {
            _cache.TryRemove(key, out _);
        }
        else
        {
            _cache[key] = value;
        }

        _dirty = true;
    }

    /// <summary>
    /// Get a cached text value, or null if missing.
    /// </summary>
    public string? Get(string key)
    {
        return _cache.TryGetValue(key, out var val) ? val : null;
    }

    /// <summary>
    /// Get a cached integer value (e.g., selected index), or defaultValue if missing/invalid.
    /// </summary>
    public int GetInt(string key, int defaultValue = -1)
    {
        if (_cache.TryGetValue(key, out var val) &&
            int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// Set a cached integer value (e.g., ComboBox selected index).
    /// </summary>
    public void SetInt(string key, int value)
    {
        Set(key, value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Remove all cached values whose keys start with the given prefix.
    /// Useful for clearing a whole tab/form section.
    /// </summary>
    public void ClearPrefix(string prefix)
    {
        foreach (var key in _cache.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                _cache.TryRemove(key, out _);
            }
        }

        _dirty = true;
    }

    /// <summary>
    /// Force an immediate flush to disk. Call on form close.
    /// </summary>
    public void Flush()
    {
        FlushToDisk();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _flushTimer.Stop();
        _flushTimer.Dispose();
        FlushToDisk();
    }

    // ── CSV I/O ─────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        if (!File.Exists(CachePath)) return;

        try
        {
            var lines = File.ReadAllLines(CachePath, Encoding.UTF8);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var (key, value) = ParseCsvLine(line);
                if (!string.IsNullOrEmpty(key))
                {
                    _cache[key] = value;
                }
            }
        }
        catch
        {
            // Corrupt cache — ignore and start fresh.
        }
    }

    private void FlushIfDirty()
    {
        if (!_dirty) return;
        FlushToDisk();
    }

    private void FlushToDisk()
    {
        _dirty = false;

        try
        {
            Directory.CreateDirectory(CacheDir);

            var sb = new StringBuilder();
            foreach (var kvp in _cache)
            {
                sb.Append(EscapeCsv(kvp.Key));
                sb.Append(',');
                sb.Append(EscapeCsv(kvp.Value));
                sb.AppendLine();
            }

            lock (_writeLock)
            {
                File.WriteAllText(CachePath, sb.ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // Best-effort: don't crash if disk is unavailable.
        }
    }

    // ── CSV helpers ─────────────────────────────────────────────

    private static string EscapeCsv(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }

    private static (string Key, string Value) ParseCsvLine(string line)
    {
        var fields = new List<string>(2);
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++; // skip escaped quote
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            else
            {
                if (ch == '"')
                {
                    inQuotes = true;
                }
                else if (ch == ',')
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                    if (fields.Count == 1)
                    {
                        // Rest of line is value
                        var rest = line[(i + 1)..];
                        fields.Add(UnescapeCsvField(rest));
                        return (fields[0], fields[1]);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
        }

        fields.Add(sb.ToString());

        return fields.Count >= 2
            ? (fields[0], fields[1])
            : (fields.Count == 1 ? (fields[0], string.Empty) : (string.Empty, string.Empty));
    }

    private static string UnescapeCsvField(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.StartsWith('"') && trimmed.EndsWith('"') && trimmed.Length >= 2)
        {
            return trimmed[1..^1].Replace("\"\"", "\"");
        }

        return raw;
    }
}
