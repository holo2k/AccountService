using System.Data;
using AccountService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Helpers;

public class SqlExecutor : ISqlExecutor
{
    private readonly AppDbContext _dbContext;

    public SqlExecutor(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> ExecuteScalarIntAsync(string sql, params object[] parameters)
    {
        var conn = _dbContext.Database.GetDbConnection();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = $"@p{i}";
            param.Value = parameters[i];
            cmd.Parameters.Add(param);
        }

        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
            return 0;

        return Convert.ToInt32(result);
    }
}