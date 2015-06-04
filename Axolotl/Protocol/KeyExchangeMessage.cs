using System;
using Axolotl.ECC;
using Axolotl.Util;
using System.IO;
using ProtoBuf;

namespace Axolotl.Protocol
{
	// Full Complete

	public class KeyExchangeMessage
	{
		public static int INITIATE_FLAG = 0x01;
		public static int RESPONSE_FLAG = 0X02;
		public static int SIMULTAENOUS_INITIATE_FLAG = 0x04;

		public int         Version { get; private set; }

		public int         MaxVersion { get; private set; }

		public UInt32         Sequence { get; private set; }

		public UInt32         Flags { get; private set; }

		public ECPublicKey BaseKey { get; private set; }

		public byte[]      BaseKeySignature { get; private set; }

		public ECPublicKey RatchetKey { get; private set; }

		public IdentityKey IdentityKey { get; private set; }

		public byte[] _serialized;

		public bool IsResponce
		{
			get
			{ 
				return (Flags & RESPONSE_FLAG) != 0; 
			}
		}

		public bool IsInitiate
		{
			get
			{
				return (Flags & INITIATE_FLAG) != 0;
			}
		}

		public bool IsResponseForSimultaneousInitiate
		{
			get
			{
				return (Flags & SIMULTAENOUS_INITIATE_FLAG) != 0;
			}
		}

		public KeyExchangeMessage(int messageVersion, UInt32 sequence, UInt32 flags,
		                           ECPublicKey baseKey, byte[] baseKeySignature,
		                           ECPublicKey ratchetKey,
		                           IdentityKey identityKey)
		{
			MaxVersion = CiphertextMessage.CURRENT_VERSION;
			Version = messageVersion;
			Sequence = sequence;
			Flags = flags;
			BaseKey = baseKey;
			BaseKeySignature = baseKeySignature;
			RatchetKey = ratchetKey;
			IdentityKey = identityKey;

			byte[] version = { ByteUtil.IntsToByteHighAndLow(Version, MaxVersion) };
			var keyExchangeMsg = new WhisperProtos.KeyExchangeMessage {
				id = (Sequence << 5) | Flags,
				baseKey = BaseKey.Serialize(),
				ratchetKey = RatchetKey.Serialize(),
				identityKey = IdentityKey.Serialize()
			};

			if(Version >= 3)
			{
				keyExchangeMsg.baseKeySignature = BaseKeySignature;
			}

			byte[] bytes;
			using(var stream = new MemoryStream())
			{
				Serializer.Serialize(stream, keyExchangeMsg);
				bytes = stream.ToArray();
			}
			_serialized = ByteUtil.Combine(version, bytes);
		}

		public KeyExchangeMessage(byte[] serialized)
		{
			try
			{
				byte[][] parts = ByteUtil.Split(serialized, 1, serialized.Length - 1);
				Version = ByteUtil.HighBitsToInt(parts[0][0]);
				MaxVersion = ByteUtil.LowBitsToInt(parts[0][0]);

				if(Version <= CiphertextMessage.UNSUPPORTED_VERSION)
				{
					throw new LegacyMessageException("Unsupported legacy version: " + Version);
				}

				if(Version > CiphertextMessage.CURRENT_VERSION)
				{
					throw new InvalidMessageException("Unknown version: " + Version);
				}

				WhisperProtos.KeyExchangeMessage message;
				using(var stream = new MemoryStream())
				{
					stream.Write(parts[1], 0, parts[1].Length);
					message = Serializer.Deserialize<WhisperProtos.KeyExchangeMessage>(stream);
				}

				if(!message.id.HasValue || message.baseKey == null ||
				    message.ratchetKey == null || message.identityKey == null ||
				    (Version >= 3 && message.baseKeySignature == null))
				{
					throw new InvalidMessageException("Some required fields missing!");
				}

				Sequence = message.id.Value >> 5;
				Flags = message.id.Value & 0x1f;
				_serialized = serialized;
				BaseKey = Curve.DecodePoint(message.baseKey, 0);
				BaseKeySignature = message.baseKeySignature;
				RatchetKey = Curve.DecodePoint(message.ratchetKey, 0);
				IdentityKey = new IdentityKey(message.identityKey, 0);
			}
			catch(InvalidMessageException e)
			{
				throw new Exception("Invalid message exception: " + e);
			}
		}

		public byte[] Serialize()
		{
			return _serialized;
		}
	}
}

