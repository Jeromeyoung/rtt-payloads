using System;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;

namespace CsWMI
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
            if (args.Length < 3)
            {
                Console.WriteLine(" [x] Invalid number of arguments");
                Console.WriteLine("     Usage: WMI.exe <targetMachine> <command> <method>");
                return;
            }

            string target = args[0];
            string command = args[1];
            string method = args[2];

            ManagementScope scope = null;
            ManagementBaseObject result = null;

            try
            {
                scope = new ManagementScope($@"\\{target}\root\cimv2");
                scope.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(" [x] {0}", e.Message);
                return;
            }

            if (method.ToLower() == "processcallcreate")
            {
                result = ProcessCallCreate(scope, command);
            }

            Console.WriteLine(" [*] Return Value: {0}", result["returnValue"]);
            Console.WriteLine(" [*] ProcessId: {0}", result["ProcessId"]);
        }

        public static ManagementBaseObject ProcessCallCreate(ManagementScope scope, string command)
        {
            ManagementBaseObject result = null;

            try
            {
                ManagementClass mClass = new ManagementClass(scope, new ManagementPath("Win32_Process"), new ObjectGetOptions());
                ManagementBaseObject parameters = mClass.GetMethodParameters("Create");
                PropertyDataCollection properties = parameters.Properties;
                parameters["CommandLine"] = command;

                result = mClass.InvokeMethod("Create", parameters, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(" [x] {0}", e.Message);
            }

            return result;
        }
    }
}
