using System;
using Axolotl.State;
using Functional.Maybe;
using Axolotl.Protocol;

namespace Axolotl
{
	class SessionBuilder
	{
		// UNDONE

		public SessionBuilder(ISessionStore sessionStore, IPreKeyStore preKeyStore, ISignedPreKeyStore signedPreKeyStore, IIdentityKeyStore identityKeyStore, AxolotlAddress remoteAddress)
		{
			throw new NotImplementedException();
		}

		public Maybe<UInt32> Process(SessionRecord sessionRecord, PreKeyWhisperMessage ciphertext)
		{
			throw new NotImplementedException();
		}
	}

}

