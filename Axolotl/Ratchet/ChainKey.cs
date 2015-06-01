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

		private HKDF   _kdf;

		public byte[] Key { get; private set; }
		public int Index { get; private set; }

		public ChainKey (HKDF kdf, byte[] key, int index)
		{
			_kdf = kdf;
			Key = key;
			Index = index;
		}

		public MessageKeys GetMessageKeys() {
			byte[] inputKeyMaterial = GetBaseMaterial (MESSAGE_KEY_SEED);
			byte[] keyMaterialBytes = _kdf.DeriveSecrets (inputKeyMaterial, Encoding.UTF8.GetBytes ("WhisperMessageKeys"), DerivedMessageSecrets.SIZE);
			DerivedMessageSecrets keyMaterial = new DerivedMessageSecrets (keyMaterialBytes);

			return new MessageKeys (keyMaterial.CipherKey, keyMaterial.MacKey, keyMaterial.Iv, Index);
		}

		private byte[] GetBaseMaterial(byte[] seed) {
			var hmac = HMACSHA256.Create ();
			hmac.Key = Key;
			return hmac.ComputeHash (seed);
			// TODO: check if right
		}
	}
}

