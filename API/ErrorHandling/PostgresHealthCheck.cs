using Npgsql;
public class PostgresHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresHealthCheck> _logger;

    public PostgresHealthCheck(IConfiguration config, ILogger<PostgresHealthCheck> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
        _logger = logger;
    }

    public async Task<bool> CheckDatabaseConnection()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Database connection failed: {ex.Message}");
            return false;
        }
    }
}
