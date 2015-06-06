using System;
using Functional.Maybe;
using Axolotl.Protocol;
using Axolotl.State;
using Axolotl.Ratchet;
using Axolotl.ECC;
using Axolotl.Util;

namespace Axolotl
{
	public class SessionBuilder
	{
		// Check~
		//private static string TAG = ((object)this).GetType().Name;

		private ISessionStore      _sessionStore;
		private IPreKeyStore       _preKeyStore;
		private ISignedPreKeyStore _signedPreKeyStore;
		private IIdentityKeyStore  _identityKeyStore;
		private AxolotlAddress     _remoteAddress;

		public SessionBuilder(ISessionStore sessionStore,
		                      IPreKeyStore preKeyStore,
		                      ISignedPreKeyStore signedPreKeyStore,
		                      IIdentityKeyStore identityKeyStore,
		                      AxolotlAddress remoteAddress)
		{
			_sessionStore      = sessionStore;
			_preKeyStore       = preKeyStore;
			_signedPreKeyStore = signedPreKeyStore;
			_identityKeyStore  = identityKeyStore;
			_remoteAddress     = remoteAddress;
		}

		public SessionBuilder(IAxolotlStore store, AxolotlAddress remoteAddress) 
			: this(store, store, store, store, remoteAddress)
		{
		}

		public Maybe<UInt32> Process(SessionRecord sessionRecord, PreKeyWhisperMessage message)
		{
			var messageVersion = message.MessageVersion;
			var theirIdentityKey = message.IdentityKey;

			Maybe<UInt32> unsignedPreKeyId;

			if (!_identityKeyStore.IsTrustedIdentity(_remoteAddress.Name, theirIdentityKey)) 
			{
				throw new UntrustedIdentityException();
			}

			switch (messageVersion) 
			{
				case 2:  unsignedPreKeyId = ProcessV2(sessionRecord, message); break;
				case 3:  unsignedPreKeyId = ProcessV3(sessionRecord, message); break;
				default: throw new InvalidOperationException("Unknown version: " + messageVersion);
			}

			_identityKeyStore.SaveIdentity(_remoteAddress.Name, theirIdentityKey);
			return unsignedPreKeyId;
		}

		private Maybe<UInt32> ProcessV2 (SessionRecord sessionRecord, PreKeyWhisperMessage message)
		{
			if (message.PreKeyId.IsNothing()) {
				throw new InvalidKeyIdException("V2 message requires one time prekey id!");
			}

			if (!_preKeyStore.ContainsPreKey(message.PreKeyId.Value) &&
			    _sessionStore.ContainsSession(_remoteAddress))
			{
				Console.WriteLine("TAG" + "We've already processed the prekey part of this V2 session, letting bundled message fall through...");
				return Maybe<UInt32>.Nothing;
			}

			var ourPreKey = _preKeyStore.LoadPreKey(message.PreKeyId.Value).KeyPair;

			var bobParams = BobAxolotlParameters.NewBuilder();

			bobParams.SetOurIdentityKey(_identityKeyStore.GetIdentityKeyPair())
					.SetOurSignedPreKey(ourPreKey)
					.SetOurRatchetKey(ourPreKey)
					.SetOurOneTimePreKey(Maybe<ECKeyPair>.Nothing)
					.SetTheirIdentityKey(message.IdentityKey)
					.SetTheirBaseKey(message.BaseKey);

			if (!sessionRecord.IsFresh) sessionRecord.ArchiveCurrentState();

			RatchetingSession.InitializeSession(sessionRecord.SessionState, message.MessageVersion, bobParams.Create());

			sessionRecord.SessionState.LocalRegistrationId 	= _identityKeyStore.GetLocalRegistrationId();
			sessionRecord.SessionState.RemoteRegistrationId = message.RegistrationId;
			sessionRecord.SessionState.AliceBaseKey 		= message.BaseKey.Serialize();

			if (message.PreKeyId.Value != Medium.MAX_VALUE) {
				return message.PreKeyId;
			} else {
				return Maybe<UInt32>.Nothing;
			}
		}

		private Maybe<UInt32> ProcessV3(SessionRecord sessionRecord, PreKeyWhisperMessage message)
		{
			if (sessionRecord.HasSessionState(message.MessageVersion, message.BaseKey.Serialize())) 
			{
				// TODO: LOG // TAG ?
				Console.WriteLine("SessionBuilder : " + "We've already setup a session for this V3 message, letting bundled message fall through...");
				return Maybe<UInt32>.Nothing;
			}

			var ourSignedPreKey = _signedPreKeyStore.LoadSignedPreKey(message.SignedPreKeyId).keyPair;

			var bobParams = BobAxolotlParameters.NewBuilder ();
			bobParams.SetTheirBaseKey(message.BaseKey)
						.SetTheirIdentityKey(message.IdentityKey)
						.SetOurIdentityKey(_identityKeyStore.GetIdentityKeyPair())
						.SetOurSignedPreKey(ourSignedPreKey)
						.SetOurRatchetKey(ourSignedPreKey);

			if (message.PreKeyId.IsSomething()) {
				message.PreKeyId.Do (pKid => {
					bobParams.SetOurOneTimePreKey(_preKeyStore.LoadPreKey(pKid).KeyPair.ToMaybe());
				});
			} else {
				bobParams.SetOurOneTimePreKey(Maybe<ECKeyPair>.Nothing);
			}

			if (!sessionRecord.IsFresh) sessionRecord.ArchiveCurrentState();

			RatchetingSession.InitializeSession(sessionRecord.SessionState, message.MessageVersion, bobParams.Create());

			sessionRecord.SessionState.LocalRegistrationId  = _identityKeyStore.GetLocalRegistrationId();
			sessionRecord.SessionState.RemoteRegistrationId = message.RegistrationId;
			sessionRecord.SessionState.AliceBaseKey 		= message.BaseKey.Serialize();

			if (message.PreKeyId.IsSomething() && 
				message.PreKeyId.Value != Medium.MAX_VALUE) {
				return message.PreKeyId;
			} else {
				return Maybe<UInt32>.Nothing;
			}
		}

		public void Process(PreKeyBundle preKey)
		{
			lock (SessionCipher.SESSION_LOCK) 
			{
				if (!_identityKeyStore.IsTrustedIdentity(_remoteAddress.Name, preKey.IdentityKey)) {
					throw new UntrustedIdentityException();
				}

				if (preKey.SignedPreKeyPublic != null &&
				    !Curve.VerifySignature(preKey.IdentityKey.PublicKey,
				                       preKey.SignedPreKeyPublic.Serialize(),
				                       preKey.SignedPreKeySignature))
				{
					throw new InvalidKeyException("Invalid signature on device key!");
				}

				if (preKey.GetSignedPreKey() == null && preKey.GetPreKey() == null) {
					throw new InvalidKeyException("Both signed and unsigned prekeys are absent!");
				}

				bool              	  supportsV3           = preKey.GetSignedPreKey() != null;
				SessionRecord         sessionRecord        = _sessionStore.LoadSession(_remoteAddress);
				ECKeyPair             ourBaseKey           = Curve.GenerateKeyPair();
				ECPublicKey           theirSignedPreKey    = supportsV3 ? preKey.GetSignedPreKey() : preKey.GetPreKey();
				Maybe<ECPublicKey> theirOneTimePreKey      = preKey.GetPreKey().ToMaybe();
				Maybe<UInt32>     theirOneTimePreKeyId 	   = theirOneTimePreKey.IsSomething() ? preKey.PreKeyID.ToMaybe() :Maybe<UInt32>.Nothing;

				var aliceParams = AliceAxolotlParameters.NewBuilder();

				aliceParams.SetOurBaseKey(ourBaseKey)
						.SetOurIdentityKey(_identityKeyStore.GetIdentityKeyPair())
						.SetTheirIdentityKey(preKey.IdentityKey)
						.SetTheirSignedPreKey(theirSignedPreKey)
						.SetTheirRatchetKey(theirSignedPreKey)
						.SetTheirOneTimePreKey(supportsV3 ? theirOneTimePreKey : Maybe<ECPublicKey>.Nothing);

				if (!sessionRecord.IsFresh) sessionRecord.ArchiveCurrentState();

				RatchetingSession.InitializeSession(sessionRecord.SessionState,
				                                    (UInt32)(supportsV3 ? 3 : 2),
				                                    aliceParams.Create());

				sessionRecord.SessionState.SetUnacknowledgedPreKeyMessage(theirOneTimePreKeyId, preKey.SignedPreKeyID, ourBaseKey.PublicKey);
				sessionRecord.SessionState.LocalRegistrationId 	= _identityKeyStore.GetLocalRegistrationId();
				sessionRecord.SessionState.RemoteRegistrationId = preKey.RegistrationID;
				sessionRecord.SessionState.AliceBaseKey 		= ourBaseKey.PublicKey.Serialize();

				_sessionStore.StoreSession(_remoteAddress, sessionRecord);
				_identityKeyStore.SaveIdentity(_remoteAddress.Name, preKey.IdentityKey);
			}
		}

		public KeyExchangeMessage Process(KeyExchangeMessage message)
		{
			lock (SessionCipher.SESSION_LOCK) {
				if (!_identityKeyStore.IsTrustedIdentity(_remoteAddress.Name, message.IdentityKey)) {
					throw new UntrustedIdentityException();
				}

				KeyExchangeMessage responseMessage = null;

				if (message.IsInitiate)   responseMessage = ProcessInitiate(message);
				else                      ProcessResponse(message);

				return responseMessage;
			}
		}

		private KeyExchangeMessage ProcessInitiate(KeyExchangeMessage message) 
		{
			UInt32           flags         = KeyExchangeMessage.RESPONSE_FLAG;
			SessionRecord sessionRecord = _sessionStore.LoadSession(_remoteAddress);

			if (message.Version >= 3 &&
			    !Curve.VerifySignature(message.IdentityKey.PublicKey,
			                       message.BaseKey.Serialize(),
			                       message.BaseKeySignature))
			{
				throw new InvalidKeyException("Bad signature!");
			}

			var builder = SymmetricAxolotlParameters.NewBuilder();

			if (!sessionRecord.SessionState.HasPendingKeyExchange) {
				builder.SetOurIdentityKey(_identityKeyStore.GetIdentityKeyPair())
							.SetOurBaseKey(Curve.GenerateKeyPair())
							.SetOurRatchetKey(Curve.GenerateKeyPair());
			} else {
				builder.SetOurIdentityKey(sessionRecord.SessionState.PendingKeyExchangeIdentityKey)
							.SetOurBaseKey(sessionRecord.SessionState.PendingKeyExchangeBaseKey)
							.SetOurRatchetKey(sessionRecord.SessionState.PendingKeyExchangeRatchetKey);
				flags |= KeyExchangeMessage.SIMULTAENOUS_INITIATE_FLAG;
			}

			builder.SetTheirBaseKey(message.BaseKey)
						.SetTheirRatchetKey(message.RatchetKey)
						.SetTheirIdentityKey(message.IdentityKey);

			var parameters = builder.Create();

			if (!sessionRecord.IsFresh) sessionRecord.ArchiveCurrentState();

			RatchetingSession.InitializeSession(sessionRecord.SessionState,
			                                    (UInt32)Math.Min(message.MaxVersion, CiphertextMessage.CURRENT_VERSION),
			                                    parameters);

			_sessionStore.StoreSession(_remoteAddress, sessionRecord);
			_identityKeyStore.SaveIdentity(_remoteAddress.Name, message.IdentityKey);

			byte[] baseKeySignature = Curve.CalculateSignature(parameters.OurIdentityKey.PrivateKey,
			                                                   parameters.OurBaseKey.PublicKey.Serialize());

			return new KeyExchangeMessage(sessionRecord.SessionState.GetSessionVersion(),
			                              message.Sequence, flags,
			                              parameters.OurBaseKey.PublicKey,
			                              baseKeySignature, parameters.OurRatchetKey.PublicKey,
			                              parameters.OurIdentityKey.PublicKey);
		}

		private void ProcessResponse(KeyExchangeMessage message)
		{
			var sessionRecord                  = _sessionStore.LoadSession(_remoteAddress);
			var sessionState                   = sessionRecord.SessionState;
			var hasPendingKeyExchange          = sessionState.HasPendingKeyExchange;
			var isSimultaneousInitiateResponse = message.IsResponseForSimultaneousInitiate;

			if (!hasPendingKeyExchange || sessionState.PendingKeyExchangeSequence != message.Sequence) {
				Console.WriteLine("TAG : " + "No matching sequence for response. Is simultaneous initiate response: " + isSimultaneousInitiateResponse);
				if (!isSimultaneousInitiateResponse) throw new StaleKeyExchangeException();
				else                                 return;
			}

			var parameters = SymmetricAxolotlParameters.NewBuilder();

			parameters.SetOurBaseKey(sessionRecord.SessionState.PendingKeyExchangeBaseKey)
					.SetOurRatchetKey(sessionRecord.SessionState.PendingKeyExchangeRatchetKey)
					.SetOurIdentityKey(sessionRecord.SessionState.PendingKeyExchangeIdentityKey)
					.SetTheirBaseKey(message.BaseKey)
					.SetTheirRatchetKey(message.RatchetKey)
					.SetTheirIdentityKey(message.IdentityKey);

			if (!sessionRecord.IsFresh) sessionRecord.ArchiveCurrentState();

			RatchetingSession.InitializeSession(sessionRecord.SessionState,
			                                    (UInt32)Math.Min(message.MaxVersion, CiphertextMessage.CURRENT_VERSION),
			                                    parameters.Create());

			if (sessionRecord.SessionState.GetSessionVersion() >= 3 &&
			    !Curve.VerifySignature(message.IdentityKey.PublicKey,
			                       message.BaseKey.Serialize(),
			                       message.BaseKeySignature))
			{
				throw new InvalidKeyException("Base key signature doesn't match!");
			}

			_sessionStore.StoreSession(_remoteAddress, sessionRecord);
			_identityKeyStore.SaveIdentity(_remoteAddress.Name, message.IdentityKey);

		}

		public KeyExchangeMessage Process() {
			lock (SessionCipher.SESSION_LOCK) {
				try {
					UInt32          sequence         = (UInt32)KeyHelper.GetRandomSequence(65534) + 1;
					UInt32          flags            = KeyExchangeMessage.INITIATE_FLAG;
					ECKeyPair       baseKey          = Curve.GenerateKeyPair();
					ECKeyPair       ratchetKey       = Curve.GenerateKeyPair();
					IdentityKeyPair identityKey      = _identityKeyStore.GetIdentityKeyPair();
					byte[]          baseKeySignature = Curve.CalculateSignature(identityKey.PrivateKey, baseKey.PublicKey.Serialize());
					SessionRecord   sessionRecord    = _sessionStore.LoadSession(_remoteAddress);

					sessionRecord.SessionState.SetPendingKeyExchange(sequence, baseKey, ratchetKey, identityKey);
					_sessionStore.StoreSession(_remoteAddress, sessionRecord);

					return new KeyExchangeMessage(2, sequence, flags, baseKey.PublicKey, baseKeySignature,
					                              ratchetKey.PublicKey, identityKey.PublicKey);
				} catch (InvalidKeyException e) {
					throw new InvalidOperationException("Assertion error", e);
				}
			}
		}
	}
}

