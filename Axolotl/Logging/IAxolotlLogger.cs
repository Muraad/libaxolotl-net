using System;

namespace Axolotl.Logging
{
	public interface IAxolotlLogger
	{
		void Log(DebugLevel priority, string tag, string message);
	}
}

