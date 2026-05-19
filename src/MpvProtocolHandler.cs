using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MpvProtocolHandler
{
    internal static class Program
    {
        private const string LogFileName = "mpv-protocol-handler.log";

        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Log("No URL argument was provided.");
                    return 2;
                }

                string rawUrl = string.Join(" ", args).Trim().Trim('"');
                string mediaUrl = ExtractMediaUrl(rawUrl);
                string playerPath = FindPlayer();

                if (playerPath == null)
                {
                    Log("No mpv player was found. Set MPV_PROTOCOL_PLAYER or install mpv.net/mpv in a standard location.");
                    return 3;
                }

                StartPlayer(playerPath, mediaUrl);
                Log("Started: " + playerPath + " " + mediaUrl);
                return 0;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                return 1;
            }
        }

        private static string ExtractMediaUrl(string rawUrl)
        {
            string url = Uri.UnescapeDataString(rawUrl ?? string.Empty).Trim().Trim('"');
            string payload;

            if (url.StartsWith("mpv://play/", StringComparison.OrdinalIgnoreCase))
            {
                payload = url.Substring("mpv://play/".Length);
                return DecodeBase64Url(payload);
            }

            if (url.StartsWith("mpv://", StringComparison.OrdinalIgnoreCase))
            {
                payload = url.Substring("mpv://".Length);

                if (payload.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    payload.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return payload;
                }

                try
                {
                    return DecodeBase64Url(payload);
                }
                catch
                {
                    return payload;
                }
            }

            return url;
        }

        private static string DecodeBase64Url(string text)
        {
            string base64 = text.Replace('-', '+').Replace('_', '/');

            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
                case 1:
                    throw new FormatException("Invalid base64url payload length.");
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }

        private static string FindPlayer()
        {
            string envPath = Environment.GetEnvironmentVariable("MPV_PROTOCOL_PLAYER");
            if (IsFile(envPath))
            {
                return envPath;
            }

            string ownDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(ownDir, "mpvnet.exe"),
                Path.Combine(ownDir, "mpv.exe"),
                @"C:\Program Files\mpv.net\mpvnet.exe",
                @"C:\Program Files (x86)\mpv.net\mpvnet.exe",
                @"C:\Program Files\mpv\mpv.exe",
                @"C:\Program Files (x86)\mpv\mpv.exe"
            };

            foreach (string candidate in candidates)
            {
                if (IsFile(candidate))
                {
                    return candidate;
                }
            }

            return FindOnPath("mpvnet.exe") ?? FindOnPath("mpv.exe");
        }

        private static string FindOnPath(string fileName)
        {
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

            foreach (string dir in path.Split(Path.PathSeparator).Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                try
                {
                    string candidate = Path.Combine(dir.Trim('"'), fileName);
                    if (IsFile(candidate))
                    {
                        return candidate;
                    }
                }
                catch
                {
                    // Ignore malformed PATH entries.
                }
            }

            return null;
        }

        private static bool IsFile(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        private static void StartPlayer(string playerPath, string mediaUrl)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = playerPath,
                Arguments = QuoteArg(mediaUrl),
                WorkingDirectory = Path.GetDirectoryName(playerPath),
                UseShellExecute = false
            };

            Process.Start(startInfo);
        }

        private static string QuoteArg(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static void Log(string message)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), LogFileName);
                File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine);
            }
            catch
            {
                // Logging must never block protocol handling.
            }
        }
    }
}
