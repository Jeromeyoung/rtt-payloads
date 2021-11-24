using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpEDRChecker
{
    public class Program
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

        public static void Main(string[] args)
        {
            try
            {
                bool isAdm = PrivilegeChecker.PrivCheck();
                PrintIntro(isAdm);
                var summary = ProcessChecker.CheckProcesses();
                summary += ProcessChecker.CheckCurrentProcessModules();
                summary += DirectoryChecker.CheckDirectories();
                summary += ServiceChecker.CheckServices();
                summary += DriverChecker.CheckDrivers();
                PrintOutro(summary);
#if DEBUG
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine("[-] Error running SharpEDRChecker: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void PrintIntro(bool isAdm)
        {
            if (isAdm)
            {
                Console.WriteLine($"\n##################################################################");
                Console.WriteLine("   [!][!][!] Welcome to SharpEDRChecker by @PwnDexter [!][!][!]");
                Console.WriteLine("[+][+][+] Running as admin, all checks will be performed [+][+][+]");
                Console.WriteLine($"##################################################################\n");
            }
            else
            {
                Console.WriteLine($"\n###################################################################################################");
                Console.WriteLine("                    [!][!][!] Welcome to SharpEDRChecker by @PwnDexter [!][!][!]");
                Console.WriteLine("[-][-][-] Not running as admin, some privileged metadata and processes may not be checked [-][-][-]");
                Console.WriteLine($"###################################################################################################\n");
            }
        }

        private static void PrintOutro(string summary)
        {
            Console.WriteLine($"################################");
            Console.WriteLine($"[!][!][!] TLDR Summary [!][!][!]");
            Console.WriteLine($"################################");
            Console.WriteLine($"{summary}");
            Console.WriteLine($"#######################################");
            Console.WriteLine("[!][!][!] EDR Checks Complete [!][!][!]");
            Console.WriteLine($"#######################################\n");
        }
    }
}

