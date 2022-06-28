using System.Diagnostics;
using Serilog;

namespace NoteTrayLib.Services;

public class EditorManagerService
{
    private readonly UserPreferenceService _userPreferences;
    private Process p = new Process();

    public EditorManagerService(UserPreferenceService userPreferences)
    {
        _userPreferences = userPreferences;
    }

    public Process OpenInEditor(string fullPath)
    {
        string composedEditCommand = string.Format(_userPreferences.EditorCommand ?? @"Notepad.exe ""{0}""", fullPath);
        
        Log.Debug($"Executing command \'{composedEditCommand}\'");

        ProcessStartInfo startInfo = new ProcessStartInfo(composedEditCommand, $"-n \"{fullPath}\"");

        p = Process.Start(startInfo);
        // Wait for the process to startup so anything dependent on the process can safely interact with it
        p.WaitForInputIdle();
        return p;
    }
}