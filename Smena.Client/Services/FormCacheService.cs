using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Smena.Client.Services;

/// <summary>
/// Lightweight cache for form field values.
/// Stores data in %LOCALAPPDATA%/Smena.Client/form-cache.json so that
/// form inputs survive application restarts without blocking the UI.
/// </summary>
public sealed class FormCacheService : IDisposable
{
    private static readonly string CacheDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Smena.Client");

    private static readonly string CachePath = Path.Combine(CacheDir, "form-cache.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

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

    public string? Get(string key)
    {
        return _cache.TryGetValue(key, out var val) ? val : null;
    }

    public int GetInt(string key, int defaultValue = -1)
    {
        if (_cache.TryGetValue(key, out var val) &&
            int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return defaultValue;
    }

    public void SetInt(string key, int value)
    {
        Set(key, value.ToString(CultureInfo.InvariantCulture));
    }

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

    public void Flush() => FlushToDisk();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _flushTimer.Stop();
        _flushTimer.Dispose();
        FlushToDisk();
    }

    // ── JSON I/O ────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        if (!File.Exists(CachePath)) return;

        try
        {
            var json = File.ReadAllBytes(CachePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict == null) return;

            foreach (var kvp in dict)
            {
                _cache[kvp.Key] = kvp.Value;
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

            var snapshot = new Dictionary<string, string>(_cache, StringComparer.OrdinalIgnoreCase);
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);

            lock (_writeLock)
            {
                File.WriteAllText(CachePath, json);
            }
        }
        catch
        {
            // Best-effort: don't crash if disk is unavailable.
        }
    }
}
