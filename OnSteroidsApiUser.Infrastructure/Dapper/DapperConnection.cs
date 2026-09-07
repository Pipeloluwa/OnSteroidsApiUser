using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;
using System.Data;

namespace OnSteroidsApiUser.Infrastructure.Dapper;

public class DapperConnection(
    IOptions<AppSettings> appSettings,
    ILogger<DapperConnection> logger
) : IDapperConnection
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly ILogger<DapperConnection> _logger = logger;

    private string ConnectionString => _appSettings.SqlSettings?.ConnectionString
        ?? throw new InvalidOperationException("SQL connection string is not configured.");

    public async Task<int> Execute(DynamicParameters parameters, string procedureName, CommandType commandType = CommandType.StoredProcedure)
    {
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            return await connection.ExecuteAsync(procedureName, parameters, commandType: commandType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute procedure {ProcedureName}", procedureName);
            throw;
        }
    }

    public async Task<T> Query<T>(DynamicParameters parameters, string procedureName, CommandType commandType = CommandType.StoredProcedure)
    {
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            var result = await connection.QueryFirstOrDefaultAsync<T>(procedureName, parameters, commandType: commandType);
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query procedure {ProcedureName}", procedureName);
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAll<T>(DynamicParameters? parameters, string procedureName, CommandType commandType = CommandType.StoredProcedure)
    {
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            var result = await connection.QueryAsync<T>(procedureName, parameters, commandType: commandType);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query all procedure {ProcedureName}", procedureName);
            throw;
        }
    }
}
