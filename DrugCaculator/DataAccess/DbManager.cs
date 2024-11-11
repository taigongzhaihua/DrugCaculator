using Dapper;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace DrugCaculator.DataAccess;
public class DbManager(string connectionString)
{
    // 创建表的方法
    public void CreateTableIfNotExists(string tableName, params (string, string)[] columnsDefinitions)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        var columns = string.Join(", ", columnsDefinitions.Select(cd => $"{cd.Item1} {cd.Item2}"));
        var sql = $"CREATE TABLE IF NOT EXISTS {tableName} ({columns});";
        connection.Execute(sql);
    }

    // 执行SQL命令的方法
    public void Execute(string sql)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        connection.Execute(sql);
    }


    // 通用查询方法
    public IEnumerable<T> Query<T>(string tableName, string whereClause = "", params object[] parameters)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        var sql = $"SELECT * FROM {tableName} {whereClause}";
        var dynamicParameters = BuildDynamicParameters(parameters);
        return connection.Query<T>(sql, dynamicParameters);
    }

    // 通用插入方法
    public int Insert(string tableName, params (string, object)[] columnValues)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        var columns = string.Join(", ", columnValues.Select(cv => cv.Item1));
        var values = string.Join(", ", columnValues.Select(cv => "@" + cv.Item1));
        var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values}); SELECT last_insert_rowid();";
        var parameters = new DynamicParameters();
        foreach (var (key, value) in columnValues)
        {
            parameters.Add(key, value);
        }
        return connection.QuerySingle<int>(sql, parameters);
    }

    // 通用更新方法
    public void Update(string tableName, (string, object)[] columnValues, string whereClause, params object[] parameters)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        var setClause = string.Join(", ", columnValues.Select(cv => $"{cv.Item1} = @{cv.Item1}"));
        var sql = $"UPDATE {tableName} SET {setClause} {whereClause}";
        var dynamicParameters = BuildDynamicParameters(parameters);
        foreach (var (key, value) in columnValues)
        {
            dynamicParameters.Add(key, value);
        }
        connection.Execute(sql, dynamicParameters);
    }

    // 通用删除方法
    public void Delete(string tableName, string whereClause, params object[] parameters)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        var sql = $"DELETE FROM {tableName} {whereClause}";
        var dynamicParameters = BuildDynamicParameters(parameters);
        connection.Execute(sql, dynamicParameters);
    }

    // 构建 DynamicParameters
    private static DynamicParameters BuildDynamicParameters(params object[] parameters)
    {
        var dynamicParameters = new DynamicParameters();
        for (var i = 0; i < parameters.Length; i += 2)
        {
            if (i + 1 < parameters.Length)
            {
                dynamicParameters.Add(parameters[i].ToString()!, parameters[i + 1]);
            }
        }
        return dynamicParameters;
    }
}