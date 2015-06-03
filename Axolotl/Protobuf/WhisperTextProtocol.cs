using System;
using ProtoBuf;

namespace Axolotl.WhisperProtos
{
	[ProtoContract]
	public class WhisperMessage
	{
		[ProtoMember(1)]
		public byte[] RatchetKey { get; set; }
		[ProtoMember(2)]
		public UInt32? Counter { get; set; }
		[ProtoMember(3)]
		public UInt32? PreviousCounter { get; set; }
		[ProtoMember(4)]
		public byte[] Ciphertext { get; set; }
	}

	[ProtoContract]
	public class PreKeyWhisperMessage {
		[ProtoMember(5)]
		public UInt32? registrationId { get; set; }
		[ProtoMember(1)]
		public UInt32? preKeyId       { get; set; }
		[ProtoMember(6)]
		public UInt32? signedPreKeyId { get; set; }
		[ProtoMember(2)]
		public byte[]  baseKey        { get; set; }
		[ProtoMember(3)]
		public byte[]  identityKey    { get; set; }
		[ProtoMember(4)]
		public byte[]  message        { get; set; } // WhisperMessage
	}

	[ProtoContract]
	public class KeyExchangeMessage {
		[ProtoMember(1)]
		public UInt32? id               { get; set; }
		[ProtoMember(2)]
		public byte[]  baseKey          { get; set; }
		[ProtoMember(3)]
		public byte[]  ratchetKey       { get; set; }
		[ProtoMember(4)]
		public byte[]  identityKey      { get; set; }
		[ProtoMember(5)]
		public byte[]  baseKeySignature { get; set; }
	}

	[ProtoContract]
	public class SenderKeyMessage {
		[ProtoMember(1)]
		public UInt32? id         { get; set; }
		[ProtoMember(2)]
		public UInt32? iteration  { get; set; }
		[ProtoMember(3)]
		public byte[]  ciphertext { get; set; }
	}
	[ProtoContract]
	public class SenderKeyDistributionMessage {
		[ProtoMember(1)]
		public UInt32? id         { get; set; }
		[ProtoMember(2)]
		public UInt32? iteration  { get; set; }
		[ProtoMember(3)]
		public byte[]  chainKey   { get; set; }
		[ProtoMember(4)]
		public byte[]  signingKey { get; set; }
	}
}