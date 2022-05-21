namespace NoteTrayLib.Services;

public class StartupService
{
    private static bool _hasRun = false;
    
    public StartupService(StartupScanService startupScanner, FirstTimeSetupService firstTimeSetup)
    {
        // Only allow StartupService to be initialized once
        if (_hasRun == false)
        {
            // Initial setup
            firstTimeSetup.Setup();
            startupScanner.Scan();
            _hasRun = true;
        }
    }
}