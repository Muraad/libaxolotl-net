using System;
using Axolotl.Util;

namespace Axolotl.KDF
{
	public class DerivedRootSecrets
	{
		public static int SIZE = 64;

		public byte[] RootKey { get; private set; }
		public byte[] ChainKey { get; private set; }

		public DerivedRootSecrets (byte[] okm)
		{
			byte[][] keys = ByteUtil.Split (okm, 32, 32);
			RootKey = keys [0];
			ChainKey = keys [1];	
		}
	}
}

