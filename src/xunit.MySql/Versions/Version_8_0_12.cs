using System;
using System.Collections.Generic;

namespace Xunit.MySql.Versions
{
    /// <summary>
    /// MySql Version 8.0.12.
    /// </summary>
    public class Version_8_0_12 : IMySqlVersion
    {
        public Version Version => new Version(8, 0, 12);

        public List<string> Files => new List<string>() { "errmsg.sys", "libeay32.dll", "mysqld.exe", "ssleay32.dll" };

        public string ResourceFolder => $"v{Version.ToString(3).Replace(".", "_")}";
    }
}
