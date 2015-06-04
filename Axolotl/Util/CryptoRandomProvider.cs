using System;
using System.Security.Cryptography;

namespace Axolotl.Util
{
	public class CryptoRandomProvider : RandomNumberGenerator
	{
		private RandomNumberGenerator rnd;

		public CryptoRandomProvider ()
		{
			rnd = RandomNumberGenerator.Create ();
		}

		public override void GetBytes (byte[] data)
		{
			rnd.GetBytes (data);
		}

		public double NextDouble()
		{
			byte[] buffer = new byte[4];
			rnd.GetBytes (buffer);
			return (double)BitConverter.ToUInt32 (buffer, 0) / UInt32.MaxValue;
		}

		public int Next(int minValue, int maxValue)
		{
			return (int)Math.Round (NextDouble () * (maxValue - minValue - 1)) + minValue;
		}

		public int Next(int maxValue)
		{
			return Next (0, Int32.MaxValue);
		}

		public override void GetNonZeroBytes (byte[] data)
		{
			rnd.GetNonZeroBytes (data);
		}
	}
}

