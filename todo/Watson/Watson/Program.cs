using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Watson.Msrc;

namespace Watson
{
    class Program
    {
[DllImport("shell32.dll", SetLastError = true)]
static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

[DllImport("advapi32.dll", SetLastError = true)]
static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

public static string[] CommandLineToArgs(string commandLine)
{
    int argc;
    var argv = CommandLineToArgvW(commandLine, out argc);
    if (argv == IntPtr.Zero)
        throw new System.ComponentModel.Win32Exception();
    try
    {
        var args = new string[argc];
        for (var i = 0; i < args.Length; i++)
        {
            var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
            args[i] = Marshal.PtrToStringUni(p);
        }

        return args;
    }
    finally
    {
        Marshal.FreeHGlobal(argv);
    }
}

public static string Execute(string commandLine)
{
    var sw = new StringWriter();
    Console.SetOut(sw);
    Console.SetError(sw);

    try
    {
        Main(CommandLineToArgs(commandLine));
    }
    catch (Exception e)
    {
        return e.ToString();
    }

    return sw.ToString();
}

        static void Main(string[] args)
        {
            Info.PrintLogo();

            // Supported versions
            var supportedVersions = new Dictionary<int, string>()
            {
                { 10240, "1507" }, { 10586, "1511" }, { 14393, "1607" }, { 15063, "1703" }, { 16299, "1709" },
                { 17134, "1803" }, { 17763, "1809" }, { 18362, "1903" }, { 18363, "1909" }, { 19041, "2004" }
            };

            // Get OS Build number
            var buildNumber = Wmi.GetBuildNumber();
            if (buildNumber != 0)
            {
                var version = supportedVersions[buildNumber];
                Console.WriteLine(" [*] OS Version: {0} ({1})", version, buildNumber);
            }
            else
            {
                Console.Error.WriteLine(" [!] Could not retrieve Windows BuildNumber");
                return;
            }   

            if (!supportedVersions.ContainsKey(buildNumber))
            {
                Console.Error.WriteLine(" [!] Windows version not supported");
                return;
            }

            // List of KBs installed
            Console.WriteLine(" [*] Enumerating installed KBs...");
            var installedKBs = Wmi.GetInstalledKBs();

#if DEBUG
            Console.WriteLine();

            foreach (var kb in installedKBs)
            {
                Console.WriteLine(" {0}", kb);
            }
                
            Console.WriteLine();
#endif

            // List of Vulnerabilities
            var vulnerabiltiies = new VulnerabilityCollection();

            // Check each one
            CVE_2019_0836.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_0841.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1064.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1130.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1253.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1315.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1385.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1388.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2019_1405.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2020_0668.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2020_0683.Check(vulnerabiltiies, buildNumber, installedKBs);
            CVE_2020_1013.Check(vulnerabiltiies, buildNumber, installedKBs);

            // Print the results
            vulnerabiltiies.ShowResults();
        }
    }
}
