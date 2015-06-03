using System;
using Axolotl.ECC;
using Functional.Maybe;

namespace Axolotl.Ratchet
{
	public class BobAxolotlParameters
	{
		public IdentityKeyPair OurIdentityKey { get; private set; }
		public ECKeyPair OurSignedPreKey { get; private set; }
		public Maybe<ECKeyPair> OurOneTimePreKey { get; private set; }
		public ECKeyPair OurRatchetKey { get; private set; }

		public IdentityKey TheirIdentityKey { get; private set; }
		public ECPublicKey TheirBaseKey { get; private set; }

		public BobAxolotlParameters (IdentityKeyPair ourIdentityKey, ECKeyPair ourSignedPreKey,
		                             Maybe<ECKeyPair> ourOneTimePreKey, ECKeyPair ourRatchetKey,
		                             IdentityKey theirIdentityKey, ECPublicKey theirBaseKey)
		{
			OurIdentityKey = ourIdentityKey;
			OurSignedPreKey = ourSignedPreKey;
			OurOneTimePreKey = ourOneTimePreKey;
			OurRatchetKey = ourRatchetKey;
			TheirIdentityKey = theirIdentityKey;
			TheirBaseKey = theirBaseKey;

			if (ourIdentityKey == null || ourSignedPreKey == null || ourRatchetKey == null ||
			    theirIdentityKey == null || theirBaseKey == null)
			{
				throw new Exception("Null value!");
			}
		}
	}
}

