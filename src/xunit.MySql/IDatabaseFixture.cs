using Microsoft.EntityFrameworkCore;
using System;
using Xunit.MySql.Services;
using Xunit.MySql.Versions;

namespace Xunit.MySql
{
    /// <summary>
    /// Interface for Database Fixture.
    /// </summary>
    /// <typeparam name="TX">The DbContext.</typeparam>
    /// <typeparam name="TS">The MySql Service.</typeparam>
    public interface IDatabaseFixture<TX, out TS> : IDisposable
        where TX : DbContext
        where TS : IMySqlService<IMySqlVersion>
    {
    }
}
