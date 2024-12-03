using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class SecurityManager
{
   
   
    public static bool IsDebuggerAttached()
    {
        return Debugger.IsAttached || Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING") == "1";
    }

    /// <summary>
    /// Exits the application if a debugger is detected.
    /// </summary>
    public static void PreventDebugging()
    {
        if (IsDebuggerAttached())
        {
            Console.WriteLine("Unauthorized debugging attempt detected. Exiting application.");
            Environment.Exit(1);
        }
    }


}
