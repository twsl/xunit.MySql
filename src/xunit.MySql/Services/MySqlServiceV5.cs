using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit.MySql.Utilities;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Services
{
    /// <summary>
    /// The Service implementation for MySql v5.
    /// </summary>
    /// <typeparam name="TV">The specific MySql version.</typeparam>
    public class MySqlServiceV5<TV> : BaseMySqlService<TV>, IMySqlService<TV> where TV: IMySqlVersion
    {
        private readonly string baseDirectory;
        private readonly string serverDirectory;
        private readonly string dataDirectory;
        private readonly string dataRootDirectory;
        private readonly string messagesDirectory;

        public MySqlServiceV5(ILoggerFactory factory) : base(factory)
        {
            baseDirectory = Path.Combine(FileUtils.GetBaseDir(), ToString());
            serverDirectory = Path.Combine(baseDirectory, "server");
            dataRootDirectory = Path.Combine(serverDirectory, "data");
            dataDirectory = Path.Combine(serverDirectory, Guid.NewGuid().ToString());
            messagesDirectory = serverDirectory;
        }

        protected override bool CreateDirectories()
        {
            var directories = new List<string> { baseDirectory, serverDirectory, dataRootDirectory, dataDirectory };
            return CreateDirectories(directories);
        }

        protected override string GetExecutionFilePath()
        {
            return Path.Combine(serverDirectory, Executable);
        }

        protected override string[] CreateExecutionArgumentsString()
        {
            // https://dev.mysql.com/doc/refman/5.7/en/server-options.html
            var arguments = new string[]
            {
                "--standalone",
                "--console",
                $"--basedir=\"{baseDirectory}\"", // MySQL installation directory
                $"--datadir=\"{dataDirectory}\"", // MySQL server data directory
                "--enable-named-pipe",
                $"--lc-messages-dir=\"{messagesDirectory}\"", // directory where error messages are located
                $"--port={Port}",
                "--skip-grant-tables", // start without using the privilege system at all
               // https://dev.mysql.com/doc/refman/5.7/en/innodb-parameters.html
                "--innodb_fast_shutdown=2",
                "--innodb_doublewrite=OFF",
                "--innodb_log_file_size=1048576",
                "--innodb_data_file_path=ibdata1:10M;ibdata2:10M:autoextend"
            };
            return arguments;
        }

        protected override bool ExtractMySqlFiles()
        {
            try
            {
                foreach(string file in MySqlVersion.Files)
                    FileUtils.WriteFileToFolder(serverDirectory, MySqlVersion.ResourceFolder, file);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception while extracting: {ex.Message}");
                return false;
            }
            return true;
        }

        protected override bool RemoveDirectories()
        {
            try
            {
                var directory = new DirectoryInfo(baseDirectory);
                if (directory.Exists)
                {
                    directory.Delete(true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not delete instances file: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
