using System;
using Axolotl.ECC;
using Axolotl.State;

namespace Axolotl.Ratchet
{
	public class SymmetricAxolotlParameters
	{
		// Complete

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


		public static Builder NewBuilder ()
		{
			return new Builder ();
		}

		public class Builder {

			private ECKeyPair       _ourBaseKey;
			private ECKeyPair       _ourRatchetKey;
			private IdentityKeyPair _ourIdentityKey;

			private ECPublicKey     _theirBaseKey;
			private ECPublicKey     _theirRatchetKey;
			private IdentityKey     _theirIdentityKey;

			public Builder SetOurBaseKey(ECKeyPair ourBaseKey) {
				_ourBaseKey = ourBaseKey;
				return this;
			}

			public Builder SetOurRatchetKey(ECKeyPair ourRatchetKey) {
				_ourRatchetKey = ourRatchetKey;
				return this;
			}

			public Builder SetOurIdentityKey(IdentityKeyPair ourIdentityKey) {
				_ourIdentityKey = ourIdentityKey;
				return this;
			}

			public Builder SetTheirBaseKey(ECPublicKey theirBaseKey) {
				_theirBaseKey = theirBaseKey;
				return this;
			}

			public Builder SetTheirRatchetKey(ECPublicKey theirRatchetKey) {
				_theirRatchetKey = theirRatchetKey;
				return this;
			}

			public Builder SetTheirIdentityKey(IdentityKey theirIdentityKey) {
				_theirIdentityKey = theirIdentityKey;
				return this;
			}

			public SymmetricAxolotlParameters Create() {
				return new SymmetricAxolotlParameters(_ourBaseKey, _ourRatchetKey, _ourIdentityKey,
				                                      _theirBaseKey, _theirRatchetKey, _theirIdentityKey);
			}
		}
	}
}

