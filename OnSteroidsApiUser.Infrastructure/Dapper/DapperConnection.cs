using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;
using System.Collections.Concurrent;
using System.Data;

namespace OnSteroidsApiUser.Infrastructure.Dapper;

public class DapperConnection(
    IOptions<AppSettings> appSettings,
    ILogger<DapperConnection> logger
) : IDapperConnection
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly ILogger<DapperConnection> _logger = logger;
    private static readonly ConcurrentDictionary<string, HashSet<string>> _procParameterCache = new(StringComparer.OrdinalIgnoreCase);

    private string ConnectionString => _appSettings.SqlSettings?.ConnectionString
        ?? throw new InvalidOperationException("SQL connection string is not configured.");

    public static void InvalidateProcedureCache() => _procParameterCache.Clear();

    private async Task<DynamicParameters?> FilterParametersForProcedureAsync(
        SqlConnection connection,
        DynamicParameters? parameters,
        string procedureName,
        CommandType commandType)
    {
        if (parameters == null || commandType != CommandType.StoredProcedure)
            return parameters;

        if (!_procParameterCache.TryGetValue(procedureName, out var allowedParams))
        {
            try
            {
                var names = await connection.QueryAsync<string>(
                    "SELECT name FROM sys.parameters WHERE object_id = OBJECT_ID(@ProcName)",
                    new { ProcName = procedureName });
                var set = new HashSet<string>(
                    names.Select(n => n.TrimStart('@')),
                    StringComparer.OrdinalIgnoreCase);
                if (set.Count > 0)
                {
                    allowedParams = set;
                    _procParameterCache[procedureName] = set;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not inspect sys.parameters for {ProcedureName}; forwarding all parameters.", procedureName);
                return parameters;
            }
        }

        if (allowedParams == null || allowedParams.Count == 0)
            return parameters;

        if (parameters.ParameterNames.All(n => allowedParams.Contains(n.TrimStart('@'))))
            return parameters;

        var filtered = new DynamicParameters();
        foreach (var name in parameters.ParameterNames)
        {
            var normalized = name.TrimStart('@');
            if (allowedParams.Contains(normalized))
            {
                filtered.Add(name, parameters.Get<object?>(name));
            }
        }
        return filtered;
    }

    private Dictionary<string, object> ExtractParameters(DynamicParameters? parameters)
    {
        var dict = new Dictionary<string, object>();
        if (parameters != null)
        {
            foreach (var name in parameters.ParameterNames)
            {
                dict[name] = parameters.Get<object>(name);
            }
        }
        return dict;
    }

    public async Task<int> Execute(DynamicParameters parameters, string procedureName, CommandType commandType = CommandType.StoredProcedure)
    {
        var pDict = ExtractParameters(parameters);
        if (procedureName != "spApiLogs_Insert")
            _logger.LogInformation("Executing DB {ProcedureName} with params: {Params}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(pDict));
        
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            var effectiveParams = await FilterParametersForProcedureAsync(connection, parameters, procedureName, commandType) ?? parameters;
            var rows = await connection.ExecuteAsync(procedureName, effectiveParams, commandType: commandType);
            
            if (procedureName != "spApiLogs_Insert")
                _logger.LogInformation("Executed DB {ProcedureName}, affected rows: {Rows}", procedureName, rows);
                
            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute procedure {ProcedureName} with params {Params}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(pDict));
            throw;
        }
    }

    public async Task<T> Query<T>(DynamicParameters parameters, string procedureName, CommandType commandType = CommandType.StoredProcedure)
    {
        var pDict = ExtractParameters(parameters);
        _logger.LogInformation("Executing DB Query {ProcedureName} with params: {Params}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(pDict));
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            var effectiveParams = await FilterParametersForProcedureAsync(connection, parameters, procedureName, commandType) ?? parameters;
            var result = await connection.QueryFirstOrDefaultAsync<T>(procedureName, effectiveParams, commandType: commandType);
            _logger.LogInformation("Executed DB Query {ProcedureName}, returned data: {Data}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(result));
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query procedure {ProcedureName} with params {Params}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(pDict));
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAll<T>(DynamicParameters? parameters, string procedureName, CommandType commandType = CommandType.StoredProcedure)
    {
        var pDict = ExtractParameters(parameters);
        _logger.LogInformation("Executing DB QueryAll {ProcedureName} with params: {Params}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(pDict));
        using var connection = new SqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            var effectiveParams = await FilterParametersForProcedureAsync(connection, parameters, procedureName, commandType);
            var result = await connection.QueryAsync<T>(procedureName, effectiveParams, commandType: commandType);
            _logger.LogInformation("Executed DB QueryAll {ProcedureName}, returned count: {Count}, data: {Data}", procedureName, result?.Count() ?? 0, Newtonsoft.Json.JsonConvert.SerializeObject(result));
            return result ?? Enumerable.Empty<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query all procedure {ProcedureName} with params {Params}", procedureName, Newtonsoft.Json.JsonConvert.SerializeObject(pDict));
            throw;
        }
    }
}
