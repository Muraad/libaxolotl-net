using System;
using Axolotl.State;
using Axolotl.ECC;

namespace Axolotl.Ratchet
{
	public class RatchetingSession
	{
		public RatchetingSession ()
		{
		}

		public static void InitializeSession(SessionState sessionState,
		                                     int sessionVersion,
		                                     SymmetricAxolotlParameters parameters)
		{
			if (IsAlice(parameters.OurBaseKey.PublicKey, parameters.TheirBaseKey)) 
			{
				//AliceAxolotlParameters.Builder aliceParameters = AliceAxolotlParameters.newBuilder();

				var alice = new AliceAxolotlParameters (
					parameters.OurIdentityKey,
					parameters.OurBaseKey,
					parameters.TheirIdentityKey,
					parameters.TheirBaseKey,
					parameters.TheirRatchetKey,
					Absent ()); // UNDONE: Optional.<ECPublicKey>absent()

				RatchetingSession.InitializeSession(sessionState, sessionVersion, alice);
			} else {
			//	BobAxolotlParameters.Builder bobParameters = BobAxolotlParameters.newBuilder();
				var bob = new BobAxolotlParameters (
					parameters.OurIdentityKey,
					parameters.OurBaseKey,
					new ECKeyPair (null, null), // UNDONE : like alice but for ECKeyPair
					parameters.OurRatchetKey,
					parameters.TheirIdentityKey,
					parameters.TheirBaseKey);

				RatchetingSession.InitializeSession(sessionState, sessionVersion, bob);
			}
		}

		public static void InitializeSession (SessionState sessionState,
					                          int sessionVersion,
					                          AliceAxolotlParameters parameters)
		{
			// UNDONE
		}

		public static void InitializeSession (SessionState sessionState,
					                          int sessionVersion,
					                          BobAxolotlParameters parameters)
		{
			// UNDONE
		}

		private static bool IsAlice(ECPublicKey ourKey, ECPublicKey theirKey) {
			return ourKey.CompareTo(theirKey) < 0;
		}

		private static ECPublicKey Absent ()
		{
			throw new NotImplementedException ();
		}

		///
		// UNDONE
		///
	}
}

