using System.Collections.Generic;

namespace NoteTrayLib.Services;

public interface IDatabaseService
{
    int ExecuteNonQuery(string commandString, object parameters = null);
    IEnumerable<T> ExecuteQuery<T>(string commandString, object parameters = null);
}