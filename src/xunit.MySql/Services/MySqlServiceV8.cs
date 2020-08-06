using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit.MySql.Utilities;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Services
{
    /// <summary>
    /// The Service implementation for MySql v8.
    /// </summary>
    /// <typeparam name="TV">The specific MySql version.</typeparam>
    public class MySqlServiceV8<TV> : BaseMySqlService<TV>, IMySqlService<TV> where TV : IMySqlVersion
    {
        private readonly string baseDirectory;
        private readonly string serverDirectory;
        private readonly string dataDirectory;
        private readonly string dataRootDirectory;
        private readonly string messagesDirectory;

        public MySqlServiceV8(ILoggerFactory factory) : base(factory)
        {
            baseDirectory = Path.Combine(FileUtils.GetBaseDir(), ToString());
            serverDirectory = Path.Combine(baseDirectory, "server");
            dataRootDirectory = Path.Combine(serverDirectory, "data");
            dataDirectory = Path.Combine(serverDirectory, Guid.NewGuid().ToString());
            messagesDirectory = Path.Combine(serverDirectory, "english");
        }

        protected override bool CreateDirectories()
        {
            var directories = new List<string> { baseDirectory, serverDirectory, dataRootDirectory, dataDirectory, messagesDirectory };
            return CreateDirectories(directories);
        }

        protected override string GetExecutionFilePath()
        {
            return Path.Combine(serverDirectory, Executable);
        }

        protected override string[] CreateExecutionArgumentsString()
        {
            // https://dev.mysql.com/doc/refman/8.0/en/server-options.html
            var arguments = new string[]
            {
                "--standalone",
                "--console",
                $"--basedir=\"{baseDirectory}\"", // MySQL installation directory
                //$"--character-set-server={"utf8mb4"}", // >= 8.0.1, utf8mb4 is default
                $"--datadir=\"{dataDirectory}\"", // MySQL server data directory
                "--enable-named-pipe",
                $"--lc-messages={"en_US"}", // locale to use for error messages, the default is en_US
                $"--lc-messages-dir=\"{messagesDirectory}\"", // directory where error messages are located
                $"--port={Port}",
                "--skip-grant-tables", // start without using the privilege system at all
                //$"--log-error-verbosity={"3"}", // The verbosity for handling events, >= 8.0.4 default is 2
               // https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html
                "--innodb_fast_shutdown=2",
                "--innodb_doublewrite=OFF",
                "--innodb_log_file_size=4194304",
                "--innodb_data_file_path=ibdata1:10M;ibdata2:10M:autoextend"
            };

            return arguments;
        }

        protected override bool ExtractMySqlFiles()
        {
            try
            {
                foreach (string file in MySqlVersion.Files)
                    FileUtils.WriteFileToFolder(serverDirectory, MySqlVersion.ResourceFolder, file);

                File.Move(Path.Combine(serverDirectory, "errmsg.sys"), Path.Combine(messagesDirectory, "errmsg.sys"));
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
