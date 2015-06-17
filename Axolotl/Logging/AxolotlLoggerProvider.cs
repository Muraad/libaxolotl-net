using System;

namespace Axolotl.Logging
{
  public class AxolotlLoggerProvider {

    private static IAxolotlLogger _provider;

    public static IAxolotlLogger GetProvider() 
    {
      return _provider;
    }

    public static void SetProvider(IAxolotlLogger provider) 
    {
      AxolotlLoggerProvider._provider = provider;
    }
  }
}

