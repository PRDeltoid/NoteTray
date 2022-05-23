using Serilog;

namespace NoteTrayLib.Services;

public class StartupService
{
    private readonly StartupScanService _startupScanner;
    private readonly FirstTimeSetupService _firstTimeSetup;
    private static bool _hasRun = false;
    
    public StartupService(StartupScanService startupScanner, FirstTimeSetupService firstTimeSetup)
    {
        _startupScanner = startupScanner;
        _firstTimeSetup = firstTimeSetup;
    }

    public Task PerformStartup()
    {
        // Only allow Startup to be run once
        if (_hasRun == false)
        {
            Log.Debug("Performing Startup");
            _hasRun = true;
            return Task.WhenAll(new List<Task>()
            {
                _firstTimeSetup.PerformSetup(),
                _startupScanner.StartScan()
            });
        }

        return Task.CompletedTask;
    }
}