using System;
using Axolotl.State;
using Axolotl.Protocol;
using Axolotl.Ratchet;
using Axolotl.ECC;
using Functional.Maybe;
using System.Collections.Generic;
using System.Security.Cryptography;
using Axolotl.Util;

namespace Axolotl
{
	public class SessionCipher
	{
		public static object SESSION_LOCK = new object();

		private ISessionStore _sessionStore;
		private SessionBuilder _sessionBuilder;
		private IPreKeyStore _preKeyStore;
		private AxolotlAddress _remoteAddress;

		/// <summary>
		/// Construct a SessionCipher for encrypt/decrypt operations on a session.
		/// In order to use SessionCipher, a session must have already been created
		/// and stored using <see cref="SessionBuilder"/>.
		/// </summary>
		/// <param name="sessionStore">The <see cref="ISessionStore"/> that contains a session for this recipient.</param>
		/// <param name="preKeyStore">Pre key store.</param>
		/// <param name="signedPreKeyStore">Signed pre key store.</param>
		/// <param name="identityKeyStore">Identity key store.</param>
		/// <param name="remoteAddress">The remote address that messages will be encrypted to or decrypted from.</param>
		public SessionCipher(ISessionStore sessionStore, IPreKeyStore preKeyStore,
		                     ISignedPreKeyStore signedPreKeyStore, IIdentityKeyStore identityKeyStore,
		                     AxolotlAddress remoteAddress)
		{
			_sessionStore = sessionStore;
			_preKeyStore = preKeyStore;
			_remoteAddress = remoteAddress;
			_sessionBuilder = new SessionBuilder(sessionStore, preKeyStore, signedPreKeyStore, identityKeyStore, remoteAddress);
		}

		public SessionCipher(IAxolotlStore store, AxolotlAddress remoteAddress)
			: this(store, store, store, store, remoteAddress)
		{
		}

		/// <summary>
		/// Encrypt a message.
		/// </summary>
		/// <param name="paddedMessage">The plaintext message bytes, optionally padded to a constant multiple.</param>
		public CiphertextMessage Encrypt(byte[] paddedMessage)
		{
			lock(SESSION_LOCK)
			{
				SessionRecord sessionRecord = _sessionStore.LoadSession(_remoteAddress);
				SessionState sessionState = sessionRecord.SessionState;
				ChainKey chainKey = sessionState.SenderChainKey;
				MessageKeys messageKeys = chainKey.GetMessageKeys();
				ECPublicKey senderEphemeral = sessionState.SenderRatchetKey;
				UInt32 previousCounter = sessionState.PreviousCounter;
				UInt32 sessionVersion = sessionState.GetSessionVersion();

				byte[] ciphertextBody = GetCiphertext(sessionVersion, messageKeys, paddedMessage);
				CiphertextMessage ciphertextMessage = new WhisperMessage(sessionVersion, messageKeys.MacKey,
					                                      senderEphemeral, chainKey.Index,
					                                      previousCounter, ciphertextBody,
					                                      sessionState.GetLocalIdentityKey(),
					                                      sessionState.GetRemoteIdentityKey());

				if(sessionState.HasUnacknowledgedPreKeyMessage())
				{
					SessionState.UnacknowledgedPreKeyMessageItems items = sessionState.GetUnacknowledgedPreKeyMessageItems();
					UInt32 localRegistrationId = sessionState.LocalRegistrationId;

					ciphertextMessage = new PreKeyWhisperMessage(sessionVersion, localRegistrationId, items.PreKeyId,
						items.SignedPreKeyId, items.BaseKey,
						sessionState.GetLocalIdentityKey(),
						(WhisperMessage)ciphertextMessage);
				}

				sessionState.SenderChainKey = chainKey.GetNextChainKey();
				_sessionStore.StoreSession(_remoteAddress, sessionRecord);
				return ciphertextMessage;
			}
		}

		public byte[] Decrypt(PreKeyWhisperMessage ciphertext)
		{
			return Decrypt(ciphertext, new NullDecryptionCallback());
		}

		public byte[] Decrypt(PreKeyWhisperMessage ciphertext, IDecryptionCallback callback)
		{
			lock(SESSION_LOCK)
			{
				SessionRecord sessionRecord = _sessionStore.LoadSession(_remoteAddress);
				Maybe<UInt32> unsignedPreKeyId = _sessionBuilder.Process(sessionRecord, ciphertext);
				byte[] plaintext = Decrypt(sessionRecord, ciphertext.Message);

				callback.HandlePlaintext(plaintext);

				_sessionStore.StoreSession(_remoteAddress, sessionRecord);

				if(unsignedPreKeyId.HasValue)
				{
					_preKeyStore.RemovePreKey(unsignedPreKeyId.Value);
				}

				return plaintext;
			}
		}

		public byte[] Decrypt(WhisperMessage ciphertext)
		{
			return Decrypt(ciphertext, new NullDecryptionCallback());
		}

		public byte[] Decrypt(WhisperMessage ciphertext, IDecryptionCallback callback)
		{
			lock(SESSION_LOCK)
			{

				if(!_sessionStore.ContainsSession(_remoteAddress))
				{
					throw new NoSessionException("No session for: " + _remoteAddress);
				}

				SessionRecord sessionRecord = _sessionStore.LoadSession(_remoteAddress);
				byte[] plaintext = Decrypt(sessionRecord, ciphertext);

				callback.HandlePlaintext(plaintext);

				_sessionStore.StoreSession(_remoteAddress, sessionRecord);

				return plaintext;
			}
		}

		private byte[] Decrypt(SessionRecord sessionRecord, WhisperMessage ciphertext)
		{
			lock(SESSION_LOCK)
			{
				var previousStates = new List<SessionState>(sessionRecord.PreviousStates);
				var exceptions = new List<Exception>();

				try
				{
					var sessionState = new SessionState(sessionRecord.SessionState);
					byte[] plaintext = Decrypt(sessionState, ciphertext);

					sessionRecord.SetState(sessionState);
					return plaintext;
				}
				catch(InvalidMessageException e)
				{
					exceptions.Add(e);
				}

				// TODO: Check~
				foreach(var state in previousStates)
				{
					try
					{
						var promotedState = new SessionState(state);
						byte[] plainText = Decrypt(promotedState, ciphertext);
						sessionRecord.PreviousStates.Remove(state);
						return plainText;
					}
					catch(InvalidMessageException e)
					{
						exceptions.Add(e);
					}

				}

				throw new InvalidMessageException("No valid sessions.", exceptions);
			}
		}

		private byte[] Decrypt(SessionState sessionState, WhisperMessage ciphertextMessage)
		{
			if(!sessionState.HasSenderChain())
			{
				throw new InvalidMessageException("Uninitialized session!");
			}

			if(ciphertextMessage.MessageVersion != sessionState.GetSessionVersion())
			{
				throw new InvalidMessageException(string.Format("Message version {0}, but session version {1}",
					ciphertextMessage.MessageVersion,
					sessionState.GetSessionVersion()));
			}

			UInt32 messageVersion = ciphertextMessage.MessageVersion;
			ECPublicKey theirEphemeral = ciphertextMessage.SenderRatchetKey;
			UInt32 counter = ciphertextMessage.Counter;
			ChainKey chainKey = GetOrCreateChainKey(sessionState, theirEphemeral);
			MessageKeys messageKeys = GetOrCreateMessageKeys(sessionState, theirEphemeral,
				                          chainKey, counter);

			ciphertextMessage.VerifyMac(messageVersion,
				sessionState.GetRemoteIdentityKey(),
				sessionState.GetLocalIdentityKey(),
				messageKeys.MacKey);

			byte[] plaintext = GetPlaintext(messageVersion, messageKeys, ciphertextMessage.Body);

			sessionState.ClearUnacknowledgedPreKeyMessage();

			return plaintext;
		}

		public UInt32 GetRemoteRegistrationId()
		{
			lock(SESSION_LOCK)
			{
				var record = _sessionStore.LoadSession(_remoteAddress);
				return record.SessionState.RemoteRegistrationId;
			}
		}

		public UInt32 GetSessionVersion()
		{
			lock(SESSION_LOCK)
			{
				if(!_sessionStore.ContainsSession(_remoteAddress))
				{
					throw new InvalidOperationException(string.Format("No session for ({0})!", _remoteAddress));
				}

				SessionRecord record = _sessionStore.LoadSession(_remoteAddress);
				return record.SessionState.GetSessionVersion();
			}
		}

		private byte[] GetCiphertext(UInt32 version, MessageKeys messageKeys, byte[] plaintext)
		{
			try
			{
				ICryptoTransform cipher;

				if(version >= 3)
				{
					cipher = GetCipher(messageKeys.CipherKey, messageKeys.Iv);
				}
				else
				{
					cipher = GetCipher(messageKeys.CipherKey, messageKeys.Counter);
				}

				return cipher.TransformFinalBlock(plaintext, 0, plaintext.Length);
			}
			catch(Exception e)
			{
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		private byte[] GetPlaintext(UInt32 version, MessageKeys messageKeys, byte[] cipherText)
		{
			try
			{
				ICryptoTransform cipher;

				if(version >= 3)
				{
					cipher = GetCipher(messageKeys.CipherKey, messageKeys.Iv);
				}
				else
				{
					cipher = GetCipher(messageKeys.CipherKey, messageKeys.Counter);
				}

				return cipher.TransformFinalBlock(cipherText, 0, cipherText.Length);
			}
			catch(Exception e)
			{
				throw new InvalidMessageException(e);
			}
		}

		private ChainKey GetOrCreateChainKey(SessionState sessionState, ECPublicKey theirEphemeral)
		{
			try
			{
				if(sessionState.HasReceiverChain(theirEphemeral))
				{
					return sessionState.GetReceiverChainKey(theirEphemeral);
				}
				else
				{
					RootKey rootKey = sessionState.RootKey;
					ECKeyPair ourEphemeral = sessionState.SenderRatchetKeyPair;
					Tuple<RootKey, ChainKey> receiverChain = rootKey.CreateChain(theirEphemeral, ourEphemeral);
					ECKeyPair ourNewEphemeral = Curve.GenerateKeyPair();
					Tuple<RootKey, ChainKey> senderChain = receiverChain.Item1.CreateChain(theirEphemeral, ourNewEphemeral);

					sessionState.RootKey = senderChain.Item1;
					sessionState.AddReceiverChain(theirEphemeral, receiverChain.Item2);
					sessionState.PreviousCounter = Math.Max(sessionState.SenderChainKey.Index - 1, 0);
					sessionState.SetSenderChain(ourNewEphemeral, senderChain.Item2);

					return receiverChain.Item2;
				}
			}
			catch(InvalidKeyException e)
			{
				throw new InvalidMessageException(e);
			}
		}

		private MessageKeys GetOrCreateMessageKeys(SessionState sessionState, ECPublicKey theirEphemeral, ChainKey chainKey, UInt32 counter)
		{
			if(chainKey.Index > counter)
			{
				if(sessionState.HasMessageKeys(theirEphemeral, counter))
				{
					return sessionState.RemoveMessageKeys(theirEphemeral, counter);
				}
				else
				{
					throw new DuplicateMessageException("Received message with old counter: " +
					chainKey.Index + " , " + counter);
				}
			}

			if(counter - chainKey.Index > 2000)
			{
				throw new InvalidMessageException("Over 2000 messages into the future!");
			}

			while(chainKey.Index < counter)
			{
				MessageKeys messageKeys = chainKey.GetMessageKeys();
				sessionState.SetMessageKeys(theirEphemeral, messageKeys);
				chainKey = chainKey.GetNextChainKey();
			}

			sessionState.SetReceiverChainKey(theirEphemeral, chainKey.GetNextChainKey());
			return chainKey.GetMessageKeys();
		}

		private ICryptoTransform GetCipher(byte[] key, byte[] iv)
		{
			var aes = SymmetricAlgorithm.Create();

			aes.Mode = CipherMode.ECB;
			aes.Key = key;
			aes.IV = iv;
			aes.Padding = PaddingMode.PKCS7;
			ICryptoTransform encryptor = aes.CreateEncryptor();

			return encryptor;
		}

		private ICryptoTransform GetCipher(byte[] key, UInt32 counter)
		{
			var aes = SymmetricAlgorithm.Create();

			aes.Mode = CipherMode.ECB;
			aes.Key = key;
		
			byte[] ivBytes = ByteUtil.IntToByteArray((int)counter);
			if(ivBytes.Length != 16)
				throw new InvalidOperationException();

			aes.IV = ivBytes;

			aes.Padding = PaddingMode.None;
			ICryptoTransform encryptor = aes.CreateEncryptor();

			return encryptor;
		}
	}
}

