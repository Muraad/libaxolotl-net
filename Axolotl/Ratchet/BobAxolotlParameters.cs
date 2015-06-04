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
		                             ECKeyPair ourRatchetKey, Maybe<ECKeyPair> ourOneTimePreKey,
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

		public static Builder NewBuilder()
		{
			return new Builder ();
		}

		public class Builder {
			private IdentityKeyPair     _ourIdentityKey;
			private ECKeyPair           _ourSignedPreKey;
			private Maybe<ECKeyPair>    _ourOneTimePreKey;
			private ECKeyPair           _ourRatchetKey;

			private IdentityKey         _theirIdentityKey;
			private ECPublicKey         _theirBaseKey;

			public Builder SetOurIdentityKey(IdentityKeyPair ourIdentityKey) {
				_ourIdentityKey = ourIdentityKey;
				return this;
			}

			public Builder SetOurSignedPreKey(ECKeyPair ourSignedPreKey) {
				_ourSignedPreKey = ourSignedPreKey;
				return this;
			}

			public Builder SetOurOneTimePreKey(Maybe<ECKeyPair> ourOneTimePreKey) {
				_ourOneTimePreKey = ourOneTimePreKey;
				return this;
			}

			public Builder SetTheirIdentityKey(IdentityKey theirIdentityKey) {
				_theirIdentityKey = theirIdentityKey;
				return this;
			}

			public Builder SetTheirBaseKey(ECPublicKey theirBaseKey) {
				_theirBaseKey = theirBaseKey;
				return this;
			}

			public Builder SetOurRatchetKey(ECKeyPair ourRatchetKey) {
				_ourRatchetKey = ourRatchetKey;
				return this;
			}

			public BobAxolotlParameters Create() {
				return new BobAxolotlParameters(_ourIdentityKey, _ourSignedPreKey, _ourRatchetKey,
				                                _ourOneTimePreKey, _theirIdentityKey, _theirBaseKey);
			}
		}
	}
}

