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
		public static void InitializeSession(SessionState sessionState,
		                                     UInt32 sessionVersion,
		                                     SymmetricAxolotlParameters parameters)
		{
			try {
				if (IsAlice(parameters.OurBaseKey.PublicKey, parameters.TheirBaseKey)) 
				{
					var aliceParams = AliceAxolotlParameters.NewBuilder();

					aliceParams.SetOurBaseKey(parameters.OurBaseKey)
							.SetOurIdentityKey(parameters.OurIdentityKey)
							.SetTheirRatchetKey(parameters.TheirRatchetKey)
							.SetTheirIdentityKey(parameters.TheirIdentityKey)
							.SetTheirSignedPreKey(parameters.TheirBaseKey)
							.SetTheirOneTimePreKey(Maybe<ECPublicKey>.Nothing);

					RatchetingSession.InitializeSession(sessionState, sessionVersion, aliceParams.Create());
				} else 
				{
					var bobParams = BobAxolotlParameters.NewBuilder();
					bobParams.SetOurIdentityKey(parameters.OurIdentityKey)
							.SetOurRatchetKey(parameters.OurRatchetKey)
							.SetOurSignedPreKey(parameters.OurBaseKey)
							.SetOurOneTimePreKey(Maybe<ECKeyPair>.Nothing)
							.SetTheirBaseKey(parameters.TheirBaseKey)
							.SetTheirIdentityKey(parameters.TheirIdentityKey);

					RatchetingSession.InitializeSession(sessionState, sessionVersion, bobParams.Create());
				}
			} catch (Exception e) {
				throw new InvalidOperationException ("Asserion error", e);
			}
		}

		public static void InitializeSession (SessionState sessionState,
					                          UInt32 sessionVersion,
					                          AliceAxolotlParameters parameters)
		{
			try {
				sessionState.SetSessionVersion(sessionVersion);
				sessionState.SetRemoteIdentityKey(parameters.TheirIdentityKey);
				sessionState.SetLocalIdentityKey(parameters.OurIdentityKey.PublicKey);

				ECKeyPair             sendingRatchetKey = Curve.GenerateKeyPair();

				byte[] secrets;
				using(var stream = new MemoryStream())
				using(var sw = new StreamWriter(stream))
				{
					byte[] buf;
					int offset = 0;

					if (sessionVersion >= 3) {
						buf = GetDiscontinuityBytes();
						sw.Write(buf);
						offset = buf.Length;
					}

					buf = Curve.CalculateAgreement(parameters.TheirSignedPreKey,
					                               parameters.OurIdentityKey.PrivateKey);

					sw.Write(buf);
					offset = buf.Length;

					buf = Curve.CalculateAgreement(parameters.TheirIdentityKey.PublicKey,
					                               parameters.OurBaseKey.PrivateKey);

					sw.Write(buf);
					offset += buf.Length;

					buf = Curve.CalculateAgreement(parameters.TheirSignedPreKey,
					                               parameters.OurBaseKey.PrivateKey);

					sw.Write(buf);
					offset += buf.Length;

					if (sessionVersion >= 3 && parameters.TheirOneTimePreKey.IsSomething()) {
						parameters.TheirOneTimePreKey.Do(pKey => { 
							buf = Curve.CalculateAgreement(pKey,
							                               parameters.OurBaseKey.PrivateKey);
							sw.Write(buf); 
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
				throw new InvalidOperationException("Assertion error" + e);
			}
		}

		public static void InitializeSession (SessionState sessionState,
					                          UInt32 sessionVersion,
					                          BobAxolotlParameters parameters)
		{
			try {
				sessionState.SetSessionVersion(sessionVersion);
				sessionState.SetRemoteIdentityKey(parameters.TheirIdentityKey);
				sessionState.SetLocalIdentityKey(parameters.OurIdentityKey.PublicKey);

				byte[] secrets;

				using(var stream = new MemoryStream())
				{
					byte[] buffer;

					if (sessionVersion >= 3) {
						buffer = GetDiscontinuityBytes();
						stream.Write(buffer, 0, buffer.Length);
					}

					buffer = Curve.CalculateAgreement(parameters.TheirIdentityKey.PublicKey,
					                                  parameters.OurSignedPreKey.PrivateKey);
					stream.Write(buffer, 0, buffer.Length);

					buffer = Curve.CalculateAgreement(parameters.TheirBaseKey,
					                                  parameters.OurIdentityKey.PrivateKey);
					stream.Write(buffer, 0, buffer.Length);

					buffer = Curve.CalculateAgreement(parameters.TheirBaseKey,
					                                  parameters.OurSignedPreKey.PrivateKey);
					stream.Write(buffer, 0, buffer.Length);

					if (sessionVersion >= 3 && parameters.OurOneTimePreKey.IsSomething()) {

						parameters.OurOneTimePreKey.Do(otpK => {
							buffer = Curve.CalculateAgreement(parameters.TheirBaseKey, otpK.PrivateKey);
							stream.Write(buffer, 0, buffer.Length);
						});
					}

					secrets = stream.ToArray();
				}


				DerivedKeys derivedKeys = CalculateDerivedKeys(sessionVersion, secrets);

				sessionState.SetSenderChain(parameters.OurRatchetKey, derivedKeys.ChainKey);
				sessionState.RootKey = derivedKeys.RootKey;
			} catch (Exception e) {
				throw new InvalidOperationException("Assertion error", e);
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

