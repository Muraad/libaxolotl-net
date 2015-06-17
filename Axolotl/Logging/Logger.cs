using System;
using System.Web;
using System.IO;

namespace Axolotl.Logging
{
  public class Logger
  {
    public static void v(string tag, string msg) 
    {
      Log(DebugLevel.VERBOSE, tag, msg);
    }

    public static void v(string tag, string msg, Exception e) 
    {
      Log(DebugLevel.VERBOSE, tag, msg + '\n' + e.StackTrace);
    }

    public static void d(string tag, string msg) 
    {
      Log(DebugLevel.DEBUG, tag, msg);
    }

    public static void d(string tag, string msg, Exception e) 
    {
      Log(DebugLevel.DEBUG, tag, msg + '\n' + e.StackTrace);
    }

    public static void i(string tag, string msg) 
    {
      Log(DebugLevel.INFO, tag, msg);
    }

    public static void i(string tag, string msg, Exception e) 
    {
      Log(DebugLevel.INFO, tag, msg + '\n' + e.StackTrace);
    }

    public static void w(string tag, string msg) 
    {
      Log(DebugLevel.WARNING, tag, msg);
    }

    public static void w(string tag, string msg, Exception e) 
    {
      Log(DebugLevel.WARNING, tag, msg + '\n' + e.StackTrace);
    }

    public static void w(string tag, Exception e) 
    {
      Log(DebugLevel.WARNING, tag, e.StackTrace);
    }

    public static void e(string tag, string msg) 
    {
      Log(DebugLevel.ERROR, tag, msg);
    }

    public static void e(string tag, string msg, Exception e) 
    {
      Log(DebugLevel.ERROR, tag, msg + '\n' + e.StackTrace);
    }

    private static string GetStackTraceString(Exception e) 
    {
      if (e == null) {
        return string.Empty;
      }
      return e.StackTrace;
    }

    private static void Log(DebugLevel priority, string tag, string msg) 
    {
      var logger = AxolotlLoggerProvider.GetProvider();

      if (logger != null) {
        logger.Log(priority, tag, msg);
      }
    }
  }
}

