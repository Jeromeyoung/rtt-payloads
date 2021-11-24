using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CsEnv
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
                Console.WriteLine("     Usage: CsEnv.exe <variable> <value> <target>");
                return;
            }

            string variable = args[0];
            string value = args[1];
            string target = args[2];

            SetEnvVar(variable, value, target);
        }

        private static void SetEnvVar(string variable, string value, string target)
        {
            EnvironmentVariableTarget envTarget;

            switch (target) {
                case "user":
                    envTarget = EnvironmentVariableTarget.User;
                    break;

                case "machine":
                    envTarget = EnvironmentVariableTarget.Machine;
                    break;

                case "process":
                    envTarget = EnvironmentVariableTarget.Process;
                    break;

                default:
                    return;
                
            }

            try
            {
                Environment.SetEnvironmentVariable(variable, value, envTarget);
            }
            catch (Exception e)
            {
                Console.WriteLine(" [x] {0}", e.Message);
            }

        }
    }
}
