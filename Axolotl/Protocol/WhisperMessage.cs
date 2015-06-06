using System;
using Axolotl.Util;
using Axolotl.ECC;
using System.IO;
using ProtoBuf;
using System.Security.Cryptography;

namespace Axolotl.Protocol
{
	//Complete

	public class WhisperMessage : CiphertextMessage
	{
		private const int MAC_LENGTH = 8;

		public UInt32         MessageVersion { get; private set; }

		public ECPublicKey SenderRatchetKey { get; private set; }

		public UInt32      Counter { get; private set; }

		public UInt32      PreviousCounter { get; private set; }

		public byte[]      Body { get; private set; }

		private byte[] _serialized;

		public WhisperMessage(byte[] serialized)
		{
			try
			{
				byte[][] messageParts = ByteUtil.Split(serialized, 1, serialized.Length - 1 - MAC_LENGTH, MAC_LENGTH);
				byte version = messageParts[0][0];
				byte[] message = messageParts[1];

				if(ByteUtil.HighBitsToUInt(version) <= CiphertextMessage.UNSUPPORTED_VERSION)
				{
					throw new LegacyMessageException("Legacy message: " + ByteUtil.HighBitsToUInt(version));
				}

				if(ByteUtil.HighBitsToUInt(version) > CURRENT_VERSION)
				{
					throw new InvalidVersionException("Unknown version: " + ByteUtil.HighBitsToUInt(version));
				}

				WhisperProtos.WhisperMessage whisperMessage;

				using(var stream = new MemoryStream())
				{
					stream.Write(message, 0, message.Length);
					whisperMessage = Serializer.Deserialize<WhisperProtos.WhisperMessage>(stream);
				}

				if(whisperMessage.Ciphertext == null ||
				   !whisperMessage.Counter.HasValue ||
				   whisperMessage.RatchetKey == null)
				{
					throw new InvalidMessageException("Incomplete message");
				}

				_serialized = serialized;
				SenderRatchetKey = Curve.DecodePoint(whisperMessage.RatchetKey, 0);
				MessageVersion = (UInt32)ByteUtil.HighBitsToUInt(version);
				Counter = whisperMessage.Counter.Value;
				PreviousCounter = whisperMessage.PreviousCounter.Value;
				Body = whisperMessage.Ciphertext;
			}
			catch(Exception e)
			{
				throw new InvalidMessageException(e);
			}
		}

		public WhisperMessage(UInt32 messageVersion, byte[] macKey, ECPublicKey senderRatchetKey,
		                      UInt32 counter, UInt32 previousCounter, byte[] ciphertext,
		                      IdentityKey senderIdentityKey,
		                      IdentityKey receiverIdentityKey)
		{
			byte[] version = { ByteUtil.IntsToByteHighAndLow(messageVersion, CURRENT_VERSION) };

			var messageObj = new WhisperProtos.WhisperMessage {
				RatchetKey = senderRatchetKey.Serialize(),
				Counter = counter,
				PreviousCounter = previousCounter,
				Ciphertext = ciphertext
			};

			byte[] message;

			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<WhisperProtos.WhisperMessage>(stream, messageObj);
				message = stream.ToArray();
			}


			byte[] mac = GetMac(messageVersion, senderIdentityKey, receiverIdentityKey, macKey,
				             ByteUtil.Combine(version, message));

			_serialized = ByteUtil.Combine(version, message, mac);
			SenderRatchetKey = senderRatchetKey;
			Counter = counter;
			PreviousCounter = previousCounter;
			Body = ciphertext;
			MessageVersion = messageVersion;
		}

		private byte[] GetMac(UInt32 messageVersion,
		                      IdentityKey senderIdentityKey,
		                      IdentityKey receiverIdentityKey,
		                      byte[] macKey, byte[] serialized)
		{
			try
			{
				byte[] fullMac;
				using(var mac = new HMACSHA256(macKey))
				{
					if(messageVersion >= 3)
					{
						var SIk = senderIdentityKey.PublicKey.Serialize();
						var RIk = receiverIdentityKey.PublicKey.Serialize();
						mac.TransformBlock(SIk, 0, SIk.Length, null, 0);
						mac.TransformBlock(RIk, 0, RIk.Length, null, 0);
					}
					fullMac = mac.TransformFinalBlock(serialized, 0, serialized.Length);
				}

				return ByteUtil.Trim(fullMac, MAC_LENGTH);
			}
			catch(Exception e)
			{
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		public void VerifyMac(UInt32 messageVersion, IdentityKey senderIdentityKey,
		                      IdentityKey receiverIdentityKey, byte[] macKey)
		{
			byte[][] parts = ByteUtil.Split(_serialized, _serialized.Length - MAC_LENGTH, MAC_LENGTH);
			byte[] ourMac = GetMac(messageVersion, senderIdentityKey, receiverIdentityKey, macKey, parts[0]);
			byte[] theirMac = parts[1];

			if(!CompareArraysExhaustively(ourMac, theirMac))
			{
				throw new InvalidMessageException("Bad Mac!");
			}
		}

		public bool IsLegacy(byte[] message)
		{
			return message != null && message.Length >= 1 &&
			ByteUtil.HighBitsToUInt(message[0]) <= CiphertextMessage.UNSUPPORTED_VERSION;
		}

		public override byte[] Serialize()
		{
			return _serialized;
		}

		public override UInt32 GetKeyType()
		{
			return CiphertextMessage.WHISPER_TYPE;
		}

		private bool CompareArraysExhaustively(byte[] first, byte[] second)
		{
			if(first.Length != second.Length)
			{
				return false;
			}
			bool ret = true;
			for(int i = 0; i < first.Length; i++)
			{
				ret = ret & (first[i] == second[i]);
			}
			return ret;
		}
	}
}

