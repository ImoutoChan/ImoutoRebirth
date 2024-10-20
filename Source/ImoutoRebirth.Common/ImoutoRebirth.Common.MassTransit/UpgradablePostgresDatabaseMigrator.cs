using System.Transactions;
using Dapper;
using MassTransit;
using MassTransit.SqlTransport;
using MassTransit.SqlTransport.PostgreSql;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.MassTransit;

internal class UpgradablePostgresDatabaseMigrator : ISqlTransportDatabaseMigrator
{
    private const string DropViewsAndFunctionsSql =
        """
        DO $$
        DECLARE
            r RECORD;
            schema_name TEXT := '{0}';
        BEGIN
            FOR r IN (
                SELECT table_name 
                FROM information_schema.views
                WHERE table_schema = schema_name
            ) LOOP
                EXECUTE format('DROP VIEW IF EXISTS %I.%I CASCADE', schema_name, r.table_name);
            END LOOP;
        
            FOR r IN (
                SELECT routine_name, specific_name
                FROM information_schema.routines
                WHERE routine_schema = schema_name
                  AND routine_type = 'FUNCTION'
            ) LOOP
                EXECUTE format('DROP FUNCTION IF EXISTS %I.%I CASCADE', schema_name, r.routine_name);
            END LOOP;
        END $$;
        """;

    private readonly ISqlTransportDatabaseMigrator _defaultMigrator;
    private readonly ILogger<UpgradablePostgresDatabaseMigrator> _logger;

    public UpgradablePostgresDatabaseMigrator(
        PostgresDatabaseMigrator defaultMigrator, 
        ILogger<UpgradablePostgresDatabaseMigrator> logger)
    {
        _defaultMigrator = defaultMigrator;
        _logger = logger;
    }

    public async Task CreateInfrastructure(SqlTransportOptions options, CancellationToken ct)
    {
        try
        {
            await _defaultMigrator.CreateInfrastructure(options, ct);
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e,
                "Unable to migrate infrastructure for MassTransit. Retrying after removing functions and views.");
            
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
            await DropObsoleteViewsAndFunctions(options, ct);
            await _defaultMigrator.CreateInfrastructure(options, ct);
            
            transaction.Complete();
        }
    }

    private async Task DropObsoleteViewsAndFunctions(SqlTransportOptions options, CancellationToken ct)
    {
        await using var connection = PostgresSqlTransportConnection.GetDatabaseConnection(options);
        await connection.Open(ct).ConfigureAwait(false);

        try
        {
            await connection.Connection
                .ExecuteScalarAsync<int>(string.Format(DropViewsAndFunctionsSql, options.Schema))
                .ConfigureAwait(false);

            _logger.LogDebug("Functions and views were removed in schema {Schema}", options.Schema);
        }
        finally
        {
            await connection.Close().ConfigureAwait(false);
        }
    }

    public Task CreateDatabase(SqlTransportOptions options, CancellationToken ct) 
        => _defaultMigrator.CreateDatabase(options, ct);

    public Task DeleteDatabase(SqlTransportOptions options, CancellationToken ct) 
        => _defaultMigrator.DeleteDatabase(options, ct);
}
