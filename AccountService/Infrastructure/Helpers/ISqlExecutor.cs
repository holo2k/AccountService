namespace AccountService.Infrastructure.Helpers;

public interface ISqlExecutor
{
    Task<int> ExecuteScalarIntAsync(string sql, params object[] parameters);
}