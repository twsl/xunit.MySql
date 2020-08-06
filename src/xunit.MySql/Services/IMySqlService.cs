using System;
using System.Threading.Tasks;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Services
{
    /// <summary>
    /// Interface for MySql Services.
    /// </summary>
    /// <typeparam name="TV">The IMySqlVersion.</typeparam>
    public interface IMySqlService<out TV> : IDisposable where TV : IMySqlVersion
    {
        string Executable { get; }
        TV MySqlVersion { get; }
        int ProcessId { get; }
        int Port { get; set; }
        string GetConnectionString();
        string GetConnectionString(string database);
        Task<bool> Start(bool initialze = true, bool force = false);
        Task<bool> Shutdown();
    }
}
