using System;
using Axolotl.ECC;
using Axolotl.State;

namespace Axolotl.Ratchet
{
	public class SymmetricAxolotlParameters
	{
		public ECKeyPair       OurBaseKey		{ get; private set; }
		public ECKeyPair       OurRatchetKey	{ get; private set; }
		public IdentityKeyPair OurIdentityKey	{ get; private set; }

		public ECPublicKey     TheirBaseKey		{ get; private set; }
		public ECPublicKey     TheirRatchetKey	{ get; private set; }
		public IdentityKey     TheirIdentityKey	{ get; private set; }

		public SymmetricAxolotlParameters (ECKeyPair ourBaseKey, ECKeyPair ourRatchetKey, 
		                                   IdentityKeyPair ourIdentityKey, ECPublicKey theirBaseKey, 
		                                   ECPublicKey theirRatchetKey, IdentityKey theirIdentityKey)
		{
			OurBaseKey = ourBaseKey;
			OurRatchetKey = ourRatchetKey;
			OurIdentityKey = ourIdentityKey;
			TheirBaseKey = theirBaseKey;
			TheirRatchetKey = theirRatchetKey;
			TheirIdentityKey = theirIdentityKey;

			if (ourBaseKey == null || ourRatchetKey == null || ourIdentityKey == null ||
			    theirBaseKey == null || theirRatchetKey == null || theirIdentityKey == null)
			{
				throw new Exception("Null values!");
			}
		}


	}
}

