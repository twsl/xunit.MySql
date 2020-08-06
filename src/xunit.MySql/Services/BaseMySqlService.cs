using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.MySql.Utilities;
using Xunit.MySql.Versions;
using Xunit.MySql.Extensions;
using System.Threading;

#if NETSTANDARD2_0
using JetBrains.Annotations;
#endif
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif

namespace Xunit.MySql.Services
{
    /// <summary>
    /// The shared base implementation for MySql Services.
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    public abstract class BaseMySqlService<TV> : IMySqlService<TV> where TV : IMySqlVersion
    {
        private readonly ILogger logger;
        protected Process process;
        protected string instancesFile;

        protected ILogger Logger => logger;
        public string Executable => "mysqld.exe";
        public Version Version => MySqlVersion.Version;
        public int ProcessId => !process.HasExited ? process.Id : -1;

        public int Port { get; set; } = 3306;
        public int Timeout { get; set; } = 15_000;
        public TV MySqlVersion { get; }


        #region Abstract Implementation
        protected abstract bool CreateDirectories();

        protected abstract bool RemoveDirectories();

        protected abstract string GetExecutionFilePath();

        protected abstract bool ExtractMySqlFiles();

        protected abstract string[] CreateExecutionArgumentsString();
        #endregion

        public BaseMySqlService([NotNull] ILoggerFactory factory)
        {
            logger = factory?.CreateLogger(GetType().Name);
            instancesFile = Path.Combine(FileUtils.GetBaseDir(), "instances");
            MySqlVersion = (TV)Activator.CreateInstance(typeof(TV), new object[] { });
        }

        /// <summary>
        /// Get a connection string for the server (no database selected)
        /// </summary>
        /// <returns>A connection string for the server</returns>
        public string GetConnectionString() => $"Server=localhost;Port={Port};Protocol=pipe;SslMode=none;";

        /// <summary>
        /// Get a connection string for the server and a specified database
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        /// <returns>A connection string for the server and database</returns>
        public string GetConnectionString(string databaseName) => $"Server=localhost;Port={Port};Protocol=pipe;SslMode=none;Database={databaseName};";

        protected bool CreateDirectories([NotNull] List<string> directories)
        {
            foreach (string dir in directories)
            {
                var directory = new DirectoryInfo(dir);
                try
                {
                    if (directory.Exists)
                    {
                        directory.Delete(true);
                        logger.LogDebug($"Deleted directory \"{directory.FullName}\"!");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Could not delete directory \"{directory.FullName}\": {ex.Message}");
                    return false;
                }
                try
                {
                    directory.Create();

                    logger.LogInformation($"Created directory \"{directory.FullName}\"!");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Could not create directory \"{directory.FullName}\": {ex.Message}");
                    return false;
                }
            }
            return true;
        }

        public Task<bool> Start(bool initialze = true, bool force = false)
        {
            bool result = false;
            if (process != null && !process.HasExited)
            {
                if (force)
                {
                    Dispose();
                }
                else
                {
                    logger.LogInformation($"MySql Server {Version} is already running!");
                    return Task.FromResult(result);
                }
            }

            if (!CreateDirectories())
            {
                logger.LogCritical($"Failed to create all required directories!");
                return Task.FromResult(result);
            }
            if (!ExtractMySqlFiles())
            {
                logger.LogCritical($"Failed to extract all required files!");
                return Task.FromResult(result);
            }

            if (initialze)
            {
                string[] args = new[] { "--initialize" };
                using var initProcess = StartMySqlProcess(args);
                initProcess?.WaitForExit();
            }

            process = StartMySqlProcess(null);

            return WaitForStartup();
        }

        protected virtual Process StartMySqlProcess(string[] additionalArgs)
        {
            string[] arguments = CreateExecutionArgumentsString();
            var allArgs = additionalArgs?.Concat(arguments).ToArray() ?? arguments;
            string argumentsString = string.Join(" ", allArgs);

            void log(object s, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogDebug(e.Data);
                }
            }
            void logWarnMySql(object s, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogWarning(e.Data);
                }
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetExecutionFilePath(),
                    Arguments = argumentsString,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.OutputDataReceived += log;
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += logWarnMySql;

            logger.LogInformation($"Running {process.StartInfo.FileName} {argumentsString}");

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                File.WriteAllText(instancesFile, process.Id.ToString());
            }
            catch (Exception e)
            {
                logger.LogError($"Could not start server process {process.Id}: {e.Message}!");
                return null;
            }
            return process;
        }

        private Task<bool> WaitForStartup()
        {
            bool result = false;
            int sleepTime = 1000;
            Exception lastException = null;
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                using (var connection = new MySqlConnection(GetConnectionString()))
                {
                    do
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (Exception ex)
                        {
                            lastException = ex;
                            connection.Close();
                            logger.LogWarning($"Database connection not opened {ex.Message}");
                            logger.LogDebug($"Sleeping for {sleepTime}ms");
                            Thread.Sleep(sleepTime);
                        }

                        if (sw.ElapsedMilliseconds > 100000)
                        {
                            throw new Exception($"Server could not be started: {lastException?.Message}");
                        }
                    }
                    while (connection.State != ConnectionState.Open);
                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception occured while connecting {ex.Message}");
            }
            finally
            {
                sw.Stop();
                string message = result ? "established" : "failed";
                logger.LogInformation($"Database connection {message} after {sw.Elapsed.TotalMilliseconds}ms!");
            }
            return Task.FromResult(result);
        }

        public Task<bool> Shutdown()
        {
            Dispose();
            return Task.FromResult(true);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (process != null)
                        {
                            var children = process.GetChildProcesses();
                            process.Kill();
                            foreach (var child in children)
                                child.Kill();
                            process.WaitForExit();
                            process.Dispose();
                            process = null;
                            logger.LogDebug($"Killed database server process");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Could not kill database server process: {e.Message}");
                    }

                    try
                    {
                        RemoveDirectories();
                        logger.LogDebug($"Removed database directories");
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Could not remove database directories: {e.Message}");
                    }
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public override string ToString()
        {
            return $"MySqlService {Version}";
        }
    }
}
