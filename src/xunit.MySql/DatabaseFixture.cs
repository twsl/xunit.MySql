using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Reflection;
using Xunit.MySql.Services;
using Xunit.MySql.Versions;
using System.Threading.Tasks;

#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#else
using JetBrains.Annotations;
#endif

namespace Xunit.MySql
{
    /// <summary>
    /// The xunit Database Fixture providing access to a database.
    /// </summary>
    /// <typeparam name="TX">The DbContext.</typeparam>
    /// <typeparam name="TS">The MySql Service.</typeparam>
    public class DatabaseFixture<TX, TS> : IDatabaseFixture<TX, TS>
        where TX : DbContext
        where TS : IMySqlService<IMySqlVersion>
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public TS Server => ServerInstance<TS>.Instance.Server;
        public Version Version => Server.MySqlVersion.Version;

        public TX Context { get; private set; }

        public string DatabaseName { get; set; }

        public DatabaseFixture()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Information)
                    .AddDebug();

            });
            _logger = _loggerFactory.CreateLogger<DatabaseFixture<TX, TS>>();

            var options = CreateOptions<TX>();

            Context = CreateDbContextInstance(options);
            Context.Database.OpenConnection();

            DatabaseName = $"test{Guid.NewGuid().ToString().Replace("-", string.Empty)}";

            Context = CreateDatabase(Context, DatabaseName);

            //Context.Database.EnsureDeleted();
            Context.Database.Migrate();
            // Do not call EnsureCreated before Migrate.
            Context.Database.EnsureCreated();
        }

        public DbContextOptions<T> CreateOptions<T>(string databaseName = null) where T : TX
        {
            string name = typeof(T).GetTypeInfo().Assembly.GetName().Name;

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            string connectionString = string.IsNullOrWhiteSpace(databaseName) ? Server.GetConnectionString() : Server.GetConnectionString(databaseName);

            builder.UseLoggerFactory(_loggerFactory);
            builder.UseMySql(connectionString, mysqlOptions =>
            {
                mysqlOptions.MigrationsAssembly(name);
                mysqlOptions.ServerVersion(Version, ServerType.MySql);
            });
            return builder.Options;
        }

        public T CreateDatabase<T>([NotNull] T context, string databaseName) where T : TX
        {
            // https://docs.microsoft.com/en-us/ef/core/querying/raw-sql
            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $"CREATE DATABASE {databaseName};";
            _ = cmd.ExecuteNonQuery();
            context.Dispose();
            _logger.LogDebug($"Created database {databaseName}");

            string name = typeof(T).GetTypeInfo().Assembly.GetName().Name;

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseLoggerFactory(_loggerFactory);
            builder.EnableSensitiveDataLogging(true);
            builder.UseMySql(Server.GetConnectionString(databaseName), mysqlOptions =>
            {
                mysqlOptions.MigrationsAssembly(name);
                mysqlOptions.ServerVersion(Version, ServerType.MySql);
            });
            var options = (DbContextOptions<T>)(object)builder.Options;

            var returnContext = CreateDbContextInstance<T>(options);
            returnContext.Database.OpenConnection();

            return (T)(object)returnContext;
        }

        public virtual T CreateDbContextInstance<T>(DbContextOptions<T> options) where T : TX
        {
            _logger.LogDebug($"Created DbContext {typeof(T)}");
            return (T)Activator.CreateInstance(typeof(T), new object[] { options });
        }

        public void Dispose()
        {
            _logger.LogDebug($"Drop database {DatabaseName}");
            var cmd = Context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $"DROP DATABASE {DatabaseName};";
            cmd.ExecuteNonQuery();
        }
    }
}
