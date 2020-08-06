using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.MySql.Tests.Models;

#if NETCOREAPP3_1
using System.Diagnostics.CodeAnalysis;
#else
using JetBrains.Annotations;
#endif

namespace Xunit.MySql.Tests.Infrastructure
{
    public class TestDbContext : DbContext
    {
        public virtual DbSet<TestModel> TestModels { get; set; }

        protected TestDbContext()
        {
        }

        public TestDbContext([NotNull] DbContextOptions options) : base(options)
        {
        }
    }

    public class TestDbContextDesignFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>()
                .UseMySql("Server=.;Initial Catalog=Test");

            return new TestDbContext(optionsBuilder.Options);
        }
    }
}
