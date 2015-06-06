using NUnit.Framework;
using System;
using Axolotl;
using Axolotl.State;
using Axolotl.ECC;
using Axolotl.Util;

namespace Tests
{
	public class TestInMemoryAxolotlStore : InMemoryAxolotlStore
	{
		public TestInMemoryAxolotlStore ()
			: base(GenerateIdentityKeyPair(), GenerateRegistrationId())
		{
		}

		private static IdentityKeyPair GenerateIdentityKeyPair() {
			ECKeyPair identityKeyPairKeys = Curve.GenerateKeyPair();

			return new IdentityKeyPair(new IdentityKey(identityKeyPairKeys.PublicKey),
			                           identityKeyPairKeys.PrivateKey);
		}

		private static UInt32 GenerateRegistrationId() {
			return KeyHelper.GenerateRegistrationId(false);
		}
	}
}

