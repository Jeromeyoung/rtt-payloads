using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace AbandonedCOMKeys
{
    public class AbandonedCOMKeys
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

        public static void Main()
        {
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_ClassicCOMClassSetting");

                List<string> inprocsvr32 = new List<string>();

                //Query all objects for their InProcSvr32 value and if not null, check that the file still exists
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    object inprocsvrVal = queryObj["InprocServer32"];
                    string inprocsvrStr = Convert.ToString(inprocsvrVal);
                    string resolvedEnvVars = Environment.ExpandEnvironmentVariables(inprocsvrStr);
                    string path = resolvedEnvVars.Trim('"');

                    if (path != null)
                    {
                        if (!File.Exists(path))
                        {
                            object clsidVal = queryObj["ComponentID"];
                            string clsidStr = Convert.ToString(clsidVal);
                            string missingKey = path + "," + clsidStr;
                            if (missingKey.StartsWith("C:")) //This filters out things like combase.dll
                                inprocsvr32.Add(missingKey);
                        }
                    }
                }

                List<string> distinct = inprocsvr32.Distinct().ToList();
                List<string> cleanList = distinct.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                foreach (string dll in cleanList) { Console.WriteLine(dll); }
                //Console.ReadKey();
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
            }
        }
    }
}
