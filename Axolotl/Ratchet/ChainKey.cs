using System;
using Axolotl.KDF;
using System.Security.Cryptography;
using System.Text;

namespace Axolotl.Ratchet
{
	public class ChainKey
	{
		// Looks like done

		private byte[] MESSAGE_KEY_SEED = { 0x01 };
		private byte[] CHAIN_KEY_SEED   = { 0x02 };

		private HKDF _kdf;

		public byte[] Key { get; private set; }

		public UInt32 Index { get; private set; }

		public ChainKey(HKDF kdf, byte[] key, UInt32 index)
		{
			_kdf = kdf;
			Key = key;
			Index = index;
		}

		public MessageKeys GetMessageKeys()
		{
			byte[] inputKeyMaterial = GetBaseMaterial(MESSAGE_KEY_SEED);
			byte[] keyMaterialBytes = _kdf.DeriveSecrets(inputKeyMaterial, Encoding.UTF8.GetBytes("WhisperMessageKeys"), DerivedMessageSecrets.SIZE);
			DerivedMessageSecrets keyMaterial = new DerivedMessageSecrets(keyMaterialBytes);

			return new MessageKeys(keyMaterial.CipherKey, keyMaterial.MacKey, keyMaterial.Iv, Index);
		}

		public ChainKey GetNextChainKey()
		{
			byte[] nextKey = GetBaseMaterial(CHAIN_KEY_SEED);
			return new ChainKey(_kdf, nextKey, Index + 1);
		}

		private byte[] GetBaseMaterial(byte[] seed)
		{
			try
			{
				using(var hmac = new HMACSHA256(Key))
				{
					var res = hmac.ComputeHash(seed);
					return res;
					// TODO Check
					//return hmac.TransformFinalBlock(seed, 0, seed.Length);
				}
			}
			catch(Exception e)
			{
				throw new InvalidOperationException("Assertion error", e);
			}
		}
	}
}

