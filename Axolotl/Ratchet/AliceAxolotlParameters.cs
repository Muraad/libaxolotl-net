using System;
using Axolotl.ECC;

namespace Axolotl.Ratchet
{
	public class AliceAxolotlParameters
	{
		public IdentityKeyPair OurIdentityKey { get; private set; }
		public ECKeyPair OurBaseKey { get; private set; }

		public IdentityKey TheirIdentityKey { get; private set; }
		public ECPublicKey TheirSignedPreKey { get; private set; }
		public ECPublicKey TheirOneTimePreKey { get; private set; }
		public ECPublicKey TheirRatchetKey { get; private set; }

		public AliceAxolotlParameters (IdentityKeyPair ourIdentityKey, ECKeyPair ourBaseKey,
		                               IdentityKey theirIdentityKey, ECPublicKey theirSignedPreKey,
		                               ECPublicKey theirRatchetKey, ECPublicKey theirOneTimePreKey)
		{
			OurIdentityKey = ourIdentityKey;
			OurBaseKey = ourBaseKey;
			TheirIdentityKey = theirIdentityKey;
			TheirSignedPreKey = theirSignedPreKey;
			TheirRatchetKey = theirRatchetKey;
			TheirOneTimePreKey = theirOneTimePreKey;

			if (ourIdentityKey == null || ourBaseKey == null || theirIdentityKey == null ||
				theirSignedPreKey == null || theirRatchetKey == null || theirOneTimePreKey == null) 
			{

				throw new Exception ("Null values!");
			}
		}
	}
}

