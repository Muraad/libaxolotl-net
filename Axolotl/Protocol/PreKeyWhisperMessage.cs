using System;
using Axolotl.ECC;
using Axolotl.Util;
using System.IO;
using ProtoBuf;
using Functional.Maybe;

namespace Axolotl.Protocol
{
	// Full Complete

	public class PreKeyWhisperMessage : CiphertextMessage
	{
		public UInt32            MessageVersion { get; private set; }

		public UInt32            RegistrationId { get; private set; }

		public Maybe<UInt32>	 PreKeyId { get; private set; }

		public UInt32            SignedPreKeyId { get; private set; }

		public ECPublicKey       BaseKey { get; private set; }

		public IdentityKey       IdentityKey { get; private set; }

		public WhisperMessage    Message { get; private set; }

		private byte[] _serialized;

		public PreKeyWhisperMessage(byte[] serialized)
		{
			try
			{
				MessageVersion = (UInt32)ByteUtil.HighBitsToUInt(serialized[0]);

				if(MessageVersion > CiphertextMessage.CURRENT_VERSION)
				{
					throw new InvalidVersionException("Unknown version: " + MessageVersion);
				}

			
				WhisperProtos.PreKeyWhisperMessage preKeyWhisperMessage;
				using(var stream = new MemoryStream())
				{
					stream.Write(serialized, 1, serialized.Length - 1);
					preKeyWhisperMessage = Serializer.Deserialize<WhisperProtos.PreKeyWhisperMessage>(stream);
				}
			
				if((MessageVersion == 2 && !preKeyWhisperMessage.preKeyId.HasValue) ||
				   (MessageVersion == 3 && !preKeyWhisperMessage.signedPreKeyId.HasValue) ||
				   preKeyWhisperMessage.baseKey == null ||
				   preKeyWhisperMessage.identityKey == null ||
				   preKeyWhisperMessage.message == null)
				{
					throw new InvalidMessageException("Incomplete message.");
				}

				_serialized = serialized;
				RegistrationId = preKeyWhisperMessage.registrationId.Value;
				PreKeyId = preKeyWhisperMessage.preKeyId.Value.ToMaybe();
				SignedPreKeyId = preKeyWhisperMessage.signedPreKeyId.Value; //() ? preKeyWhisperMessage.getSignedPreKeyId() : -1;
				BaseKey = Curve.DecodePoint(preKeyWhisperMessage.baseKey, 0);
				IdentityKey = new IdentityKey(Curve.DecodePoint(preKeyWhisperMessage.identityKey, 0));
				Message = new WhisperMessage(preKeyWhisperMessage.message);
			}
			catch(InvalidKeyException e)
			{
				throw new InvalidMessageException(e);
			}
			catch(LegacyMessageException e)
			{
				throw new InvalidMessageException(e);
			}
		}

		public PreKeyWhisperMessage(UInt32 messageVersion, UInt32 registrationId, Maybe<UInt32> preKeyId,
		                            UInt32 signedPreKeyId, ECPublicKey baseKey, IdentityKey identityKey,
		                            WhisperMessage message)
		{
			MessageVersion = messageVersion;
			RegistrationId = registrationId;
			PreKeyId = preKeyId;
			SignedPreKeyId = signedPreKeyId;
			BaseKey = baseKey;
			IdentityKey = identityKey;
			Message = message;

			var preKeyMessage = new WhisperProtos.PreKeyWhisperMessage {
				signedPreKeyId = SignedPreKeyId,
				baseKey = BaseKey.Serialize(),
				identityKey = IdentityKey.Serialize(),
				message = Message.Serialize(),
				registrationId = registrationId
			};

			preKeyId.Do(pKid => preKeyMessage.preKeyId = pKid);

			byte[] versionBytes = { ByteUtil.IntsToByteHighAndLow(MessageVersion, CURRENT_VERSION) };

			byte[] messageBytes;
			using(var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, preKeyMessage);
				messageBytes = stream.ToArray();
			}

			_serialized = ByteUtil.Combine(versionBytes, messageBytes);
		}

		public override UInt32 GetKeyType()
		{
			return CiphertextMessage.PREKEY_TYPE;
		}

		public override byte[] Serialize()
		{
			return _serialized;
		}
	}
}

