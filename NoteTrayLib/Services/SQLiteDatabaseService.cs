using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using Serilog;

namespace NoteTrayLib.Services;

public class SQLiteDatabaseService : IDatabaseService
{
    private readonly string _databaseFile;

    public SQLiteDatabaseService(string databaseFileName)
    {
        _databaseFile = databaseFileName;
    }

    public int ExecuteNonQuery(string commandString, object parameters = null)
    {
        try
        {
            using SQLiteConnection con = new SQLiteConnection($"Data Source={_databaseFile}");
            con.Open();
            return con.Execute(commandString, parameters);
        }
        catch (Exception e)
        {
            Log.Error("Exception: {e}", e);
            return 0;
        } 
    }
    
    public IEnumerable<T> ExecuteQuery<T>(string commandString, object parameters = null)
    {
        try
        {
            using SQLiteConnection con = new SQLiteConnection($"Data Source={_databaseFile}");
            con.Open();

            return con.Query<T>(commandString, parameters);
        }
        catch (Exception e)
        {
            Log.Error("Exception: {e}", e);
            return Enumerable.Empty<T>();
        } 
    }
}