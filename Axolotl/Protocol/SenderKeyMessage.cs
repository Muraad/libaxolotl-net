using System;
using System.IO;
using Axolotl.ECC;
using Axolotl.Util;
using ProtoBuf;

namespace Axolotl.Protocol
{
	// Complete

	public class SenderKeyMessage : CiphertextMessage
	{
		private const int SIGNATURE_LENGTH = 64;

		public UInt32 KeyId { get; set; }

		public byte[] CipherText { get; set; }

		public UInt32 Iteration { get; set; }


		private byte[] _serialized;
		private int _messageVersion;

		public SenderKeyMessage(byte[] serialized)
		{
			try
			{
				byte[][] messageParts = ByteUtil.Split(serialized, 1, serialized.Length - 1 - SIGNATURE_LENGTH, SIGNATURE_LENGTH);
				byte version = messageParts[0][0];
				byte[] message = messageParts[1];

				if(ByteUtil.HighBitsToInt(version) < 3)
				{
					throw new LegacyMessageException("Legacy message: " + ByteUtil.HighBitsToInt(version));
				}

				if(ByteUtil.HighBitsToInt(version) > CURRENT_VERSION)
				{
					throw new InvalidVersionException("Unknown version: " + ByteUtil.HighBitsToInt(version));
				}
					
				WhisperProtos.SenderKeyMessage senderKeyMessage;

				using(var stream = new MemoryStream())
				{
					stream.Write(message, 0, message.Length);
					senderKeyMessage = Serializer.Deserialize<WhisperProtos.SenderKeyMessage>(stream);
				}
					
				if(!senderKeyMessage.id.HasValue ||
				   !senderKeyMessage.iteration.HasValue ||
				   senderKeyMessage.ciphertext == null)
				{
					throw new InvalidMessageException("Incomplete message.");
				}

				_serialized = serialized;
				_messageVersion = ByteUtil.HighBitsToInt(version);
				KeyId = senderKeyMessage.id.Value;
				Iteration = senderKeyMessage.iteration.Value;
				CipherText = senderKeyMessage.ciphertext;
			}
			catch(Exception e)
			{
				throw new InvalidMessageException(e);
			}
		}

		public SenderKeyMessage(UInt32 keyId, UInt32 iteration, byte[] ciphertext, ECPrivateKey signatureKey)
		{
			byte[] version = { ByteUtil.IntsToByteHighAndLow(CURRENT_VERSION, CURRENT_VERSION) };

			var messageObj = new WhisperProtos.SenderKeyMessage {
				id = (uint)keyId,
				iteration = (uint)iteration,
				ciphertext = ciphertext,
			};

			byte[] message;

			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<WhisperProtos.SenderKeyMessage>(stream, messageObj);
				message = stream.ToArray();
			}

			byte[] signature = GetSignature(signatureKey, ByteUtil.Combine(version, message));

			_serialized = ByteUtil.Combine(version, message, signature);
			_messageVersion = CURRENT_VERSION;
			KeyId = keyId;
			Iteration = iteration;
			CipherText = ciphertext;
		}

		public override byte[] Serialize()
		{
			return _serialized;
		}

		public void VerifySignature(ECPublicKey signatureKey)
		{
			try
			{
				byte[][] parts = ByteUtil.Split(_serialized, _serialized.Length - SIGNATURE_LENGTH, SIGNATURE_LENGTH);

				if(!Curve.VerifySignature(signatureKey, parts[0], parts[1]))
				{
					throw new InvalidMessageException("Invalid signature!");
				}

			}
			catch(Exception e)
			{
				throw new InvalidMessageException(e);
			}
		}

		private byte[] GetSignature(ECPrivateKey signatureKey, byte[] serialized)
		{
			try
			{
				return Curve.CalculateSignature(signatureKey, serialized);
			}
			catch(Exception e)
			{
				throw new InvalidOperationException("Assertion error: " + e);
			}
		}

		public override int GetKeyType()
		{
			return CiphertextMessage.SENDERKEY_TYPE;
		}
	}
}

