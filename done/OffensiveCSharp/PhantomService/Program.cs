using System;
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace PhantomService
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

        public static void Main(string[] args)
        {
            string usage = "PhantomService.exe (audit|remove)";
            if (args.Length == 1 && args[0].ToLower() == "audit")
            {
                RemovePhantomServices(false);
            }
            else if (args.Length == 1 && args[0].ToLower() == "remove")
            {
                RemovePhantomServices(true);
            }
            else
            {
                Console.WriteLine(usage);
            }

        }

        static void RemovePhantomServices(bool remove)
        {
            Console.OutputEncoding = Encoding.Unicode;
            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController service in services)
            {
                string serviceName = service.ServiceName;

                if (Encoding.UTF8.GetByteCount(serviceName) != serviceName.Length)
                {
                    Console.WriteLine("[*] Found non-ASCII service: " + service.ServiceName);
                    if (remove)
                    {
                        try
                        {
                            ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                            InstallContext Context = new InstallContext(null, null);
                            ServiceInstallerObj.Context = Context;
                            ServiceInstallerObj.ServiceName = service.ServiceName;
                            ServiceInstallerObj.Uninstall(null);
                            Console.WriteLine();
                        }
                        catch (Win32Exception w)
                        {
                            Console.WriteLine("[-] Failed to remove {0} -> {1}", service.ServiceName, w.Message);
                        }
                    }
                }
            }
        }
    }
}

