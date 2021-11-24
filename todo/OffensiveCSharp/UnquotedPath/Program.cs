using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace UnqoutedPath
{
    class Program
    {
        private static string GetServiceInstallPath(string serviceName)
        {
            RegistryKey regkey;
            regkey = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName));

            if (regkey.GetValue("ImagePath") == null)
                return "Not Found";
            else
                return regkey.GetValue("ImagePath").ToString();
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
            List<string> vulnSvcs = new List<string>();
            RegistryKey services = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\");
            foreach (var service in services.GetSubKeyNames())
            {
                RegistryKey imagePath = services.OpenSubKey(service);
                foreach (var value in imagePath.GetValueNames())
                {
                    string path = Convert.ToString(imagePath.GetValue("ImagePath"));
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (!path.Contains("\"") && path.Contains(" ")) //If path is unquoted and has a space...
                        {
                            if (!path.Contains("System32") && !path.Contains("system32") && !path.Contains("SysWow64")) //...and is not System32/SysWow64
                            {
                                vulnSvcs.Add(path);
                            }
                        }
                    }
                    
                }
                
            }
            List<string> distinctPaths = vulnSvcs.Distinct().ToList();
            if (!distinctPaths.Any())
            {
                Console.WriteLine("[-] Couldn't find any unquoted services paths");
            }
            else
            {
                Console.WriteLine("[+] Unquoted service paths found");
                foreach (string path in distinctPaths)
                {
                    Console.WriteLine(path);
                }
            }
        }
    }
}

