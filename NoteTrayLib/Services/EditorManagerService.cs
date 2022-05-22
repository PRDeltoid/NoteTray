using System.Diagnostics;
using Serilog;

namespace NoteTrayLib.Services;

public class EditorManagerService
{
    private readonly string _editCommandTemplate;
    private Process p = new Process();

    public EditorManagerService(UserPreferenceService userPreferences)
    {
        _editCommandTemplate = userPreferences.EditorCommand ?? @"Notepad.exe ""{0}""";
    }

    public Process OpenInEditor(string fullPath)
    {
        string composedEditCommand = string.Format(_editCommandTemplate, fullPath);
        
        Log.Debug($"Executing command \'{composedEditCommand}\'");

        ProcessStartInfo startInfo = new ProcessStartInfo(composedEditCommand);

        p = Process.Start(startInfo);
        // Wait for the process to startup so anything dependent on the process can safely interact with it
        p.WaitForInputIdle();
        return p;
    }
}