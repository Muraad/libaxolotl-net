using System;
using Axolotl.ECC;
using Axolotl.Util;
using System.IO;
using ProtoBuf;
using Functional.Maybe;

namespace Axolotl.Protocol
{
	public class PreKeyWhisperMessage : CiphertextMessage
	{
		private int               _version;
		private UInt32            _registrationId;
		private Maybe<UInt32>	  _preKeyId;
		private UInt32               _signedPreKeyId;
		private ECPublicKey       _baseKey;
		private IdentityKey       _identityKey;
		private WhisperMessage    _message;
		private byte[]            _serialized;

		public PreKeyWhisperMessage(byte[] serialized)
		{
			try {
				_version = ByteUtil.HighBitsToInt(serialized[0]);

				if (_version > CiphertextMessage.CURRENT_VERSION) {
					throw new Exception("Unknown version: " + _version);
				}

			
				WhisperProtos.PreKeyWhisperMessage preKeyWhisperMessage;
				using(var stream = new MemoryStream())
				{
					stream.Write(serialized, 1, serialized.Length - 1);
					preKeyWhisperMessage = Serializer.Deserialize<WhisperProtos.PreKeyWhisperMessage>(stream);
				}

				// TODO: not nullable
				if ((_version == 2 && !preKeyWhisperMessage.preKeyId.HasValue)        ||
				    (_version == 3 && !preKeyWhisperMessage.signedPreKeyId.HasValue)  ||
				    preKeyWhisperMessage.baseKey == null                              ||
				    preKeyWhisperMessage.identityKey == null                       	  ||
				    preKeyWhisperMessage.message == null)
				{
					throw new Exception("Incomplete message.");
				}

				_serialized     = serialized;
				_registrationId = preKeyWhisperMessage.registrationId.Value;
				_preKeyId       = preKeyWhisperMessage.preKeyId.Value.ToMaybe();
				_signedPreKeyId = preKeyWhisperMessage.signedPreKeyId.Value; //() ? preKeyWhisperMessage.getSignedPreKeyId() : -1;
				_baseKey        = Curve.DecodePoint(preKeyWhisperMessage.baseKey, 0);
				_identityKey    = new IdentityKey(Curve.DecodePoint(preKeyWhisperMessage.identityKey, 0));
				_message        = new WhisperMessage(preKeyWhisperMessage.message);
			} catch (Exception e) {
				throw new Exception("WTF :" + e);
			}
		}

		public override int GetKeyType ()
		{
			throw new NotImplementedException ();
		}

		public override byte[] Serialize ()
		{
			throw new NotImplementedException ();
		}
	}
}

