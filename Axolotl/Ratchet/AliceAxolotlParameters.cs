using System;
using Axolotl.ECC;
using Functional.Maybe;

namespace Axolotl.Ratchet
{
	public class AliceAxolotlParameters
	{
		public IdentityKeyPair OurIdentityKey { get; private set; }
		public ECKeyPair OurBaseKey { get; private set; }

		public IdentityKey TheirIdentityKey { get; private set; }
		public ECPublicKey TheirSignedPreKey { get; private set; }
		public Maybe<ECPublicKey> TheirOneTimePreKey { get; private set; }
		public ECPublicKey TheirRatchetKey { get; private set; }

		public AliceAxolotlParameters (IdentityKeyPair ourIdentityKey, ECKeyPair ourBaseKey,
		                               IdentityKey theirIdentityKey, ECPublicKey theirSignedPreKey,
		                               ECPublicKey theirRatchetKey, Maybe<ECPublicKey> theirOneTimePreKey)
		{
			OurIdentityKey = ourIdentityKey;
			OurBaseKey = ourBaseKey;
			TheirIdentityKey = theirIdentityKey;
			TheirSignedPreKey = theirSignedPreKey;
			TheirRatchetKey = theirRatchetKey;
			TheirOneTimePreKey = theirOneTimePreKey;

			if (ourIdentityKey == null || ourBaseKey == null || theirIdentityKey == null ||
				theirSignedPreKey == null || theirRatchetKey == null) 
			{

				throw new Exception ("Null values!");
			}
		}

		public static Builder NewBuilder()
		{
			return new Builder ();
		}

		public class Builder 
		{
			private IdentityKeyPair       _ourIdentityKey;
			private ECKeyPair             _ourBaseKey;

			private IdentityKey           _theirIdentityKey;
			private ECPublicKey           _theirSignedPreKey;
			private ECPublicKey           _theirRatchetKey;
			private Maybe<ECPublicKey>    _theirOneTimePreKey;

			public Builder SetOurIdentityKey(IdentityKeyPair ourIdentityKey) {
				_ourIdentityKey = ourIdentityKey;
				return this;
			}

			public Builder SetOurBaseKey(ECKeyPair ourBaseKey) {
				_ourBaseKey = ourBaseKey;
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

			public Builder SetTheirSignedPreKey(ECPublicKey theirSignedPreKey) {
				_theirSignedPreKey = theirSignedPreKey;
				return this;
			}

			public Builder SetTheirOneTimePreKey(Maybe<ECPublicKey> theirOneTimePreKey) {
				_theirOneTimePreKey = theirOneTimePreKey;
				return this;
			}

			public AliceAxolotlParameters Create() {
				return new AliceAxolotlParameters(_ourIdentityKey, _ourBaseKey, _theirIdentityKey,
				                                  _theirSignedPreKey, _theirRatchetKey, _theirOneTimePreKey);
			}
		}
	}
}

