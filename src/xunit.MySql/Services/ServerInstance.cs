using Microsoft.Extensions.Logging;
using System;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Services
{
    public class ServerInstance<TS> : IDisposable where TS: IMySqlService<IMySqlVersion>
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private static volatile Lazy<ServerInstance<TS>> _instance;
        private static volatile int _instanceCount = 0;
        private bool _alreadyDisposed = false;
        private readonly Lazy<TS> _server;

        public TS Server => _server.Value;

        public static ServerInstance<TS> Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Lazy<ServerInstance<TS>>(() => new ServerInstance<TS>());
                _instanceCount++;
                return _instance.Value;
            }
        }

        private ServerInstance()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Information)
                    .AddDebug();

            });
            _logger = _loggerFactory.CreateLogger<ServerInstance<TS>>();

            _server = new Lazy<TS>(() =>
            {
                var instance = (TS)Activator.CreateInstance(typeof(TS), new object[] { _loggerFactory });
                bool started = instance.Start().Result;
                if (!started || instance.ProcessId == -1)
                {
                    _logger.LogCritical("MySQL Server Initialization Exception");
                    throw new ObjectDisposedException("MySQL Server");
                }
                return instance;
            });

            AppDomain.CurrentDomain.DomainUnload += (s, e) => {
                _logger.LogDebug("Disposing server instance");
                _server.Value.Dispose();
            };
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            if (--_instanceCount == 0) // No more references to this object.
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_alreadyDisposed) return;

            if (disposing)
            {
                _instance = null; // Allow GC to dispose of this instance.
                                  // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            _alreadyDisposed = true;
        }
    }
}
