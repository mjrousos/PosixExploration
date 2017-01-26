using System;
using System.Diagnostics;
using System.IO;
using Mono.Unix;
using Mono.Posix;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        WriteHeader();
    }

    private static void WriteHeader()
    {
        Log("----------------------------", includeTime: false);
        Log("- .NET Core POSIX Explorer -", includeTime: false);
        Log("----------------------------", includeTime: false);
        Log("", includeTime: false);
        Log($"Enumerating files under current working path ({Environment.CurrentDirectory})");

        try
        {
            AnalyzeFileSystemEntry(UnixFileSystemInfo.GetFileSystemEntry(UnixDirectoryInfo.GetCurrentDirectory()));
        }
        catch (Exception exc)
        {
            Log($"Caught unexpected exception: {exc}");
        }
        Log();
        Log("Done");
        if (Debugger.IsAttached) Debugger.Break();
    }

    private static void AnalyzeFileSystemEntry(UnixFileSystemInfo info, int nestDepth = 0)
    {
        var buffer = new string(' ', nestDepth * 2);
        var linkDest = string.Empty;
        if (info.IsSymbolicLink)
        {
            var symLink = (UnixSymbolicLinkInfo)info;
            linkDest = $" (=> {symLink.ContentsPath})";
        }
        Log($"{info.FileType}\t{FilePermissionsAsString(info.FileAccessPermissions)}\t{buffer}{info.Name}{linkDest}");
        if (info.IsDirectory)
        {
            UnixDirectoryInfo dirInfo = (UnixDirectoryInfo)info;
            foreach (var child in dirInfo.GetFileSystemEntries())
            {
                AnalyzeFileSystemEntry(child, nestDepth + 1);
            }
        }
    }

    private static string FilePermissionsAsString(FileAccessPermissions fileAccessPermissions)
    {
        StringBuilder ret = new StringBuilder();
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.UserRead)) ? "r" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.UserWrite)) ? "w" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.UserExecute)) ? "x" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.GroupRead)) ? "r" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.GroupWrite)) ? "w" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.GroupExecute)) ? "x" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.OtherRead)) ? "r" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.OtherWrite)) ? "w" : "-");
        ret.Append((fileAccessPermissions.HasFlag(FileAccessPermissions.OtherExecute)) ? "x" : "-");

        return ret.ToString();
    }

    private static object logLock = new object();
    private static void Log(string message = "", ConsoleColor? color = null, bool includeTime = true)
    {
        var formattedMessage = $"{(includeTime ? $"[{DateTime.Now.ToString("HH:mm:ss")}] " : string.Empty)}{message}";
        lock (logLock)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
                Console.WriteLine(formattedMessage);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(formattedMessage);
            }
        }
    }
}
