using System;
using System.Collections.Generic;

namespace Xunit.MySql.Versions
{
    /// <summary>
    /// MySql Version 5.7.24.
    /// </summary>
    public class Version_5_7_24 : IMySqlVersion
    {
        public Version Version => new Version(5, 7, 24);

        public List<string> Files => new List<string>() { "errmsg.sys", "mysqld.exe" };

        public string ResourceFolder => $"v{Version.ToString(3).Replace(".", "_")}";
    }
}
