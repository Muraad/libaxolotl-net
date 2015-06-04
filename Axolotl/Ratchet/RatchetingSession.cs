using System;
using System.IO;
using System.Linq;
using System.Text;
using Functional.Maybe;
using Axolotl.ECC;
using Axolotl.KDF;
using Axolotl.State;
using Axolotl.Util;

namespace Axolotl.Ratchet
{
	public class RatchetingSession
	{
		// Complete
		public static void InitializeSession(SessionState sessionState,
		                                     UInt32 sessionVersion,
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
					                          UInt32 sessionVersion,
					                          AliceAxolotlParameters parameters)
		{
			try {
				sessionState.SessionVersion = sessionVersion;
				sessionState.RemoteIdentityKey = parameters.TheirIdentityKey;
				sessionState.LocalIdentityKey = parameters.OurIdentityKey.PublicKey;

				ECKeyPair             sendingRatchetKey = Curve.GenerateKeyPair();

				byte[] secrets;
				using(var stream = new MemoryStream())
				{
					byte[] buf;
					int offset = 0;

					if (sessionVersion >= 3) {
						buf = GetDiscontinuityBytes();
						stream.Write(buf, offset, buf.Length);
						offset = buf.Length;
					}

					buf = Curve.CalculateAgreement(parameters.TheirSignedPreKey,
					                               parameters.OurIdentityKey.PrivateKey);

					stream.Write(buf, offset, buf.Length);
					offset = buf.Length;

					buf = Curve.CalculateAgreement(parameters.TheirIdentityKey.PublicKey,
					                               parameters.OurBaseKey.PrivateKey);

					stream.Write(buf, offset, buf.Length);
					offset += buf.Length;

					buf = Curve.CalculateAgreement(parameters.TheirSignedPreKey,
					                               parameters.OurBaseKey.PrivateKey);

					stream.Write(buf, offset, buf.Length);
					offset += buf.Length;

					if (sessionVersion >= 3 && parameters.TheirOneTimePreKey.IsSomething()) {
						parameters.TheirOneTimePreKey.Do(pKey => { 
							buf = Curve.CalculateAgreement(pKey,
							                               parameters.OurBaseKey.PrivateKey);
							stream.Write(buf, offset, buf.Length); 
						});
					}

					secrets = stream.ToArray();
				}


				DerivedKeys             derivedKeys  = CalculateDerivedKeys(sessionVersion, secrets);
				Tuple<RootKey, ChainKey> sendingChain = derivedKeys.RootKey.CreateChain(parameters.TheirRatchetKey, sendingRatchetKey);

				sessionState.AddReceiverChain(parameters.TheirRatchetKey, derivedKeys.ChainKey);
				sessionState.SetSenderChain(sendingRatchetKey, sendingChain.Item2);
				sessionState.RootKey = sendingChain.Item1;
			} catch (Exception e) {
				throw new Exception("wtf: " + e);
			}
		}

		public static void InitializeSession (SessionState sessionState,
					                          UInt32 sessionVersion,
					                          BobAxolotlParameters parameters)
		{
			try {
				sessionState.SessionVersion = sessionVersion;
				sessionState.RemoteIdentityKey = parameters.TheirIdentityKey;
				sessionState.LocalIdentityKey = parameters.OurIdentityKey.PublicKey;

				byte[] secrets;

				using(var stream = new MemoryStream())
				{
					byte[] buffer;
					int offset = 0;

					if (sessionVersion >= 3) {
						buffer = GetDiscontinuityBytes();
						stream.Write(buffer, offset, buffer.Length);
						offset = buffer.Length;
					}

					buffer = Curve.CalculateAgreement(parameters.TheirIdentityKey.PublicKey,
					                                  parameters.OurSignedPreKey.PrivateKey);
					stream.Write(buffer, offset, buffer.Length);
					offset += buffer.Length;

					buffer = Curve.CalculateAgreement(parameters.TheirBaseKey,
					                                  parameters.OurIdentityKey.PrivateKey);
					stream.Write(buffer, offset, buffer.Length);
					offset += buffer.Length;

					buffer = Curve.CalculateAgreement(parameters.TheirBaseKey,
					                                  parameters.OurSignedPreKey.PrivateKey);
					stream.Write(buffer, offset, buffer.Length);
					offset += buffer.Length;

					if (sessionVersion >= 3 && parameters.OurOneTimePreKey.IsSomething()) {

						parameters.OurOneTimePreKey.Do(otpK => {
							buffer = Curve.CalculateAgreement(parameters.TheirBaseKey, otpK.PrivateKey);
							stream.Write(buffer, offset, buffer.Length);
							offset += buffer.Length;
						});
					}

					secrets = stream.ToArray();
				}


				DerivedKeys derivedKeys = CalculateDerivedKeys(sessionVersion, secrets);

				sessionState.SetSenderChain(parameters.OurRatchetKey, derivedKeys.ChainKey);
				sessionState.RootKey = derivedKeys.RootKey;
			} catch (Exception e) {
				throw new Exception("wtf " + e);
			}
		}

		private static bool IsAlice(ECPublicKey ourKey, ECPublicKey theirKey) {
			return ourKey.CompareTo(theirKey) < 0;
		}

		private static byte[] GetDiscontinuityBytes() {
			byte[] discontinuity = new byte[32];
			discontinuity = discontinuity.Select (i => (byte)0xFF).ToArray ();
			return discontinuity;
		}

		private static DerivedKeys CalculateDerivedKeys(UInt32 sessionVersion, byte[] masterSecret) {
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

