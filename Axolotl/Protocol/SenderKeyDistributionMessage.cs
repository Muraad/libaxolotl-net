using System.IO;
using Axolotl.ECC;
using Axolotl.Util;
using ProtoBuf;
using System;

namespace Axolotl.Protocol
{
	public class SenderKeyDistributionMessage : CiphertextMessage
	{
		// Complete

		public UInt32 Id { get; private set; }

		public UInt32 Iteration { get; private set; }

		public byte[] ChainKey { get; private set; }

		public ECPublicKey SignatureKey { get; private set; }

		private readonly byte[] _serialized;

		public SenderKeyDistributionMessage(UInt32 id, UInt32 iteration, byte[] chainKey, ECPublicKey signatureKey)
		{
			byte[] version = { ByteUtil.IntsToByteHighAndLow(CURRENT_VERSION, CURRENT_VERSION) };

			var protobufObject = new WhisperProtos.SenderKeyDistributionMessage {
				id = id,
				iteration = iteration,
				chainKey = chainKey,
				signingKey = signatureKey.Serialize()
			};

			byte[] protobuf;
			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<WhisperProtos.SenderKeyDistributionMessage>(stream, protobufObject);
				protobuf = stream.ToArray();
			}
				
			Id = id;
			Iteration = iteration;
			ChainKey = chainKey;
			SignatureKey = signatureKey;
			_serialized = ByteUtil.Combine(version, protobuf);
		}

		public SenderKeyDistributionMessage(byte[] serialized)
		{
			try
			{
				byte[][] messageParts = ByteUtil.Split(serialized, 1, serialized.Length - 1);
				byte version = messageParts[0][0];
				byte[] message = messageParts[1];

				if(ByteUtil.HighBitsToInt(version) < CiphertextMessage.CURRENT_VERSION)
				{
					throw new LegacyMessageException("Legacy message: " + ByteUtil.HighBitsToInt(version));
				}

				if(ByteUtil.HighBitsToInt(version) > CURRENT_VERSION)
				{
					throw new InvalidMessageException("Unknown version: " + ByteUtil.HighBitsToInt(version));
				}
					
				WhisperProtos.SenderKeyDistributionMessage distributionMessage;

				using(var stream = new MemoryStream())
				{
					stream.Write(message, 0, message.Length);
					distributionMessage = Serializer.Deserialize<WhisperProtos.SenderKeyDistributionMessage>(stream);
				}

				if(!distributionMessage.id.HasValue ||
				   !distributionMessage.iteration.HasValue ||
				   distributionMessage.chainKey == null ||
				   distributionMessage.signingKey == null)
				{
					throw new InvalidMessageException("Incomplete message.");
				}

				_serialized = serialized;
				Id = distributionMessage.id.Value;
				Iteration = distributionMessage.iteration.Value;
				ChainKey = distributionMessage.chainKey;
				SignatureKey = Curve.DecodePoint(distributionMessage.signingKey, 0);
			}
			catch(Exception e)
			{
				throw new InvalidMessageException (e);
			}
		}

		public override byte[] Serialize()
		{
			return _serialized;
		}

		public override int GetKeyType()
		{
			return SENDERKEY_DISTRIBUTION_TYPE;
		}
	}
}

