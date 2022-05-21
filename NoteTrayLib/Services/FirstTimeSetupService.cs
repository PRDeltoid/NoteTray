using Serilog;

namespace NoteTrayLib.Services;

public class FirstTimeSetupService
{
    private readonly UserPreferenceService _userPrefs;
    private bool _firstRun;

    public FirstTimeSetupService(UserPreferenceService userPrefs)
    {
        _userPrefs = userPrefs;
        if (_userPrefs.TryGetPreference<bool>("firstRun", out _firstRun) == false)
        {
            _userPrefs.SetPreference("firstRun", true);
            _firstRun = true;
        }; 
    }
    
    public void Setup()
    {
        if (_firstRun)
        {
            Log.Debug("Performing first time setup");
            // Disable future "first runs"
            _userPrefs.SetPreference("firstRun", false);
            // Prevent any further calls this session
            _firstRun = false;
        }
    }

    public void Reset()
    {
        Log.Debug("Resetting first time setup status");
        _userPrefs.SetPreference("firstRun", true);
        _firstRun = true; 
    }
}