using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#else
using JetBrains.Annotations;
#endif

namespace Xunit.MySql.Extensions
{
    /// <summary>
    /// Extensions class for Process
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Creates a list of all child processes of <paramref name="process"/>.
        /// </summary>
        /// <param name="process">The Process object.</param>
        /// <returns>The list of child processes.</returns>
        public static IEnumerable<Process> GetChildProcesses([NotNull]this Process process)
        {
            var children = new List<Process>();
            var mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");

            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }

            return children;
        }
    }
}
