using System;
using System.Linq;
using System.Text;
using Axolotl.ECC;
using Axolotl.KDF;
using Axolotl.State;
using Axolotl.Util;
using Functional.Maybe;

namespace Axolotl.Ratchet
{
	public class RatchetingSession
	{
		public static void InitializeSession(SessionState sessionState,
		                                     int sessionVersion,
		                                     SymmetricAxolotlParameters parameters)
		{
			if (IsAlice(parameters.OurBaseKey.PublicKey, parameters.TheirBaseKey)) 
			{
				var alice = new AliceAxolotlParameters (
					parameters.OurIdentityKey,
					parameters.OurBaseKey,
					parameters.TheirIdentityKey,
					parameters.TheirBaseKey,
					parameters.TheirRatchetKey,
					Maybe<ECPublicKey>.Nothing);

				RatchetingSession.InitializeSession(sessionState, sessionVersion, alice);
			} else {	
				var bob = new BobAxolotlParameters (
					parameters.OurIdentityKey,
					parameters.OurBaseKey,
					Maybe<ECKeyPair>.Nothing,
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

		private static byte[] GetDiscontinuityBytes() {
			byte[] discontinuity = new byte[32];
			discontinuity = discontinuity.Select (i => (byte)0xFF).ToArray ();
			return discontinuity;
		}

		private static DerivedKeys CalculateDerivedKeys(int sessionVersion, byte[] masterSecret) {
			var kdf = HKDF.CreateFor (sessionVersion);
			byte[] derivedSecretBytes = kdf.DeriveSecrets (masterSecret, Encoding.UTF8.GetBytes ("WhisperText"), 64);
			byte[][] derivedSecrets = ByteUtil.Split (derivedSecretBytes, 32, 32);

			return new DerivedKeys (new RootKey (kdf, derivedSecrets [0]),
			                        new ChainKey (kdf, derivedSecrets [1], 0));
		}


		private class DerivedKeys
		{
			public RootKey RootKey { get; private set; }
			public ChainKey ChainKey { get; private set; }

			public DerivedKeys (RootKey rootKey, ChainKey chainKey)
			{
				RootKey = rootKey;
				ChainKey = chainKey;
			}
		}
	}
}

