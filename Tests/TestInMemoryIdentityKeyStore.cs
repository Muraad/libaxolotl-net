using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axolotl;
using Axolotl.ECC;
using Axolotl.Protocol;
using Axolotl.State;
using Axolotl.Util;
using NUnit.Framework;

namespace Tests
{
	public class TestInMemoryIdentityKeyStore : InMemoryIdentityKeyStore
	{
		public TestInMemoryIdentityKeyStore ()
			: base(GenerateIdentityKeyPair(), GenerateRegistrationId())
		{
		}

		private static IdentityKeyPair GenerateIdentityKeyPair() {
			var identityKeyPairKeys = Curve.GenerateKeyPair();

			return new IdentityKeyPair(new IdentityKey(identityKeyPairKeys.PublicKey),
			                           identityKeyPairKeys.PrivateKey);
		}

		private static UInt32 GenerateRegistrationId() {
			return KeyHelper.GenerateRegistrationId(false);
		}
	}
}

