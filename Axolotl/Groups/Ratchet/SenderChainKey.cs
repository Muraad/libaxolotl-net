using System;
using System.Security.Cryptography;

namespace Axolotl.Groups.Ratchet
{
	public class SenderChainKey
	{
		// Complete

		private static byte[] MESSAGE_KEY_SEED = { 0x01 };
		private static byte[] CHAIN_KEY_SEED   = { 0x02 };

		public UInt32 Iteration { get; private set; }
		public byte[] ChainKey { get; private set; }
		public byte[] Seed { get { return ChainKey; } }

		public SenderChainKey (UInt32 iteration, byte[] chainKey)
		{
			Iteration = iteration;
			ChainKey = chainKey;
		}

		public SenderMessageKey GetSenderMessageKey()
		{
			return new SenderMessageKey (Iteration, GetDerivative (MESSAGE_KEY_SEED, ChainKey));
		}

		public SenderChainKey GetNext() 
		{
			return new SenderChainKey (Iteration + 1, GetDerivative (CHAIN_KEY_SEED, ChainKey));
		}

		private byte[] GetDerivative (byte[] seed, byte[] key)
		{
			try {
				HMAC mac = new HMACSHA256(key);
				return mac.Hash;
			}
			catch {
				throw new Exception ("watafuk exception");
			}
		}
	}
}

