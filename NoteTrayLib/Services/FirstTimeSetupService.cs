using Serilog;

namespace NoteTrayLib.Services;

public class FirstTimeSetupService
{
    private readonly IFullTextSearchService _searchService;
    private readonly UserPreferenceService _userPrefs;
    private bool _firstRun;

    public FirstTimeSetupService(IFullTextSearchService searchService, UserPreferenceService userPrefs)
    {
        _searchService = searchService;
        _userPrefs = userPrefs;
        if (_userPrefs.FirstRunFlag == null)
        {
            _userPrefs.FirstRunFlag = true;
            _firstRun = true;
        }; 
    }
    
    public void Setup()
    {
        if (_firstRun)
        {
            Log.Debug("Performing first time setup");

            // Build the initial search index from our base path
            if (_userPrefs.BasePath != null)
            {
                // Flush the index before building to remove any old data
                _searchService.BuildIndex(_userPrefs.BasePath, true);
            }
            
            // Disable future "first runs"
            _userPrefs.FirstRunFlag = false;
            
            // Prevent any further calls this session
            _firstRun = false;
        }
    }

    public void Reset()
    {
        Log.Debug("Resetting first time setup status");
        _userPrefs.FirstRunFlag = true;
        _firstRun = true; 
    }
}