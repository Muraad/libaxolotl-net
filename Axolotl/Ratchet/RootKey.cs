using System;
using System.Linq;
using System.Text;
using Axolotl.ECC;
using Axolotl.KDF;
using Axolotl.State;

namespace Axolotl.Ratchet
{
	public class RootKey
	{
		// Looks complete
		private HKDF   _kdf;

		public byte[] Key { get; private set; }

		public RootKey (HKDF kdf, byte[] key)
		{
			_kdf = kdf;
			Key = key;
		}

		public Tuple<RootKey, ChainKey> CreateChain(ECPublicKey theirRatchetKey, ECKeyPair ourRatchetKey)
		{
			byte[] sharedSecret = Curve.CalculateAgreement (theirRatchetKey, ourRatchetKey.PrivateKey);
			byte[] derivedSecretBytes = _kdf.DeriveSecrets (sharedSecret, Key, Encoding.UTF8.GetBytes ("WhisperRatchet"), DerivedRootSecrets.SIZE);
			var derivedSecrets = new DerivedRootSecrets (derivedSecretBytes);

			RootKey newRootKey = new RootKey (_kdf, derivedSecrets.RootKey);
			ChainKey newChainKey = new ChainKey (_kdf, derivedSecrets.ChainKey, 0);

			return new Tuple<RootKey, ChainKey> (newRootKey, newChainKey);
		}
	}
}

