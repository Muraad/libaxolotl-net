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

		public int KeyId { get; set; }

		public byte[] CipherText { get; set; }

		public int Iteration { get; set; }


		private byte[] _serialized;
		private int _messageVersion;

		public SenderKeyMessage(byte[] serialized)
		{
			try
			{
				byte[][] messageParts = ByteUtil.Split(serialized, 1, serialized.Length - 1 - SIGNATURE_LENGTH, SIGNATURE_LENGTH);
				byte version = messageParts[0][0];
				byte[] message = messageParts[1];
				byte[] signature = messageParts[2];

				if(ByteUtil.HighBitsToInt(version) < 3)
				{
					throw new Exception("Legacy message: " + ByteUtil.HighBitsToInt(version));
				}

				if(ByteUtil.HighBitsToInt(version) > CURRENT_VERSION)
				{
					throw new Exception("Unknown version: " + ByteUtil.HighBitsToInt(version));
				}
					
				WhisperProtos.SenderKeyMessage senderKeyMessage;

				using(var stream = new MemoryStream())
				{
					stream.Write(message, 0, message.Length);
					senderKeyMessage = Serializer.Deserialize<WhisperProtos.SenderKeyMessage>(stream);
				}
					
				// TODO: not nullable
				if(senderKeyMessage.id == null ||
				   senderKeyMessage.iteration == null ||
				   senderKeyMessage.ciphertext == null)
				{
					throw new Exception("Incomplete message.");
				}

				_serialized = serialized;
				_messageVersion = ByteUtil.HighBitsToInt(version);
				KeyId = (int)senderKeyMessage.id;
				Iteration = (int)senderKeyMessage.iteration;
				CipherText = senderKeyMessage.ciphertext;
			}
			catch(Exception e)
			{
				throw new Exception("exception " + e);
			}
		}

		public SenderKeyMessage(int keyId, int iteration, byte[] ciphertext, ECPrivateKey signatureKey)
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
					throw new Exception("Invalid signature!");
				}

			}
			catch(Exception e)
			{
				throw new Exception("exception " + e);
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
				throw new Exception("exception " + e);
			}
		}

		public override int GetKeyType()
		{
			return CiphertextMessage.SENDERKEY_TYPE;
		}
	}
}

