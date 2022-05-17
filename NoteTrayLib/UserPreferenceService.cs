using System.Collections.Generic;
using System.Linq;
using NoteTrayLib.Services;
using Serilog;

namespace NoteTrayLib;

public class UserPreferenceService
{
    private const string TableName = "preferences";
    private readonly IDatabaseService _database;

    public UserPreferenceService(IDatabaseService database)
    {
        _database = database;
        CreatePreferenceTableIfAbsent();
    }

    public bool TryGetPreference<T>(string prefKey, out T value)
    {
        Log.Debug($@"SELECT Value FROM {TableName} WHERE Name='{prefKey}'");
        var parameters = new { name = prefKey };
        IEnumerable<T> pref = _database.ExecuteQuery<T>($@"SELECT Value FROM {TableName} WHERE Name=@name", parameters);

        if (pref.Any())
        {
            value = pref.First();
            return true;
        }

        value = default;
        return false;
    }

    public void SetPreference<T>(string prefKey, T value)
    {
        Log.Debug($@"INSERT INTO {TableName}(Name, Value) VALUES({prefKey}, {value}) ON CONFLICT(Name) DO UPDATE SET Value='{value}'");
        var parameters = new { value, name = prefKey };
        _database.ExecuteNonQuery($@"INSERT INTO {TableName}(Name, Value) VALUES(@name, @value) ON CONFLICT(Name) DO UPDATE SET Value=@value", parameters);
    }

    private void CreatePreferenceTableIfAbsent()
    {
        Log.Debug($@"CREATE TABLE IF NOT EXISTS {TableName}(
            Name TEXT PRIMARY KEY,
            Value TEXT
            )");
        _database.ExecuteNonQuery(
            @$"CREATE TABLE IF NOT EXISTS {TableName}(
                Name TEXT PRIMARY KEY,
                Value TEXT
              )");
    }
}