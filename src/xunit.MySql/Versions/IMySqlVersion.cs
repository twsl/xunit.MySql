using System;
using System.Collections.Generic;

namespace Xunit.MySql.Versions
{
    /// <summary>
    /// Interface for MySql Version.
    /// </summary>
    public interface IMySqlVersion
    {
        Version Version { get; }

        List<string> Files { get; }

        string ResourceFolder { get; }
    }
}
