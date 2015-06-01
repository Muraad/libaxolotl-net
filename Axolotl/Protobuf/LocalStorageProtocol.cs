using System;
using ProtoBuf;
using System.Collections.Generic;

namespace Axolotl.Protobuf
{
	[ProtoContract]
	public class SessionStructure
	{
		[ProtoContract]
		public class Chain {
			[ProtoMember(1)]
			public byte[] senderRatchetKey { get; set; }
			[ProtoMember(2)]
			public byte[] senderRatchetKeyPrivate { get; set; }

			[ProtoContract]
			public class ChainKey {
				[ProtoMember(1)]
				public UInt32 index { get; set; }
				[ProtoMember(2)]
				public byte[] key { get; set; }
			}

			[ProtoMember(3)]
			public ChainKey chainKey;

			[ProtoContract]
			public class MessageKey {
				[ProtoMember(1)]
				public UInt32 index { get; set; }
				[ProtoMember(2)]
				public byte[] cipherKey { get; set; }
				[ProtoMember(3)]
				public byte[] macKey { get; set; }
				[ProtoMember(4)]
				public byte[] iv { get; set; }
			}

			[ProtoMember(4)]
			public List<MessageKey> messageKeys { get; set; }
		}

		[ProtoContract]
		public class PendingKeyExchange {
			[ProtoMember(1)]
			public UInt32 sequence1 { get; set; }
			[ProtoMember(2)]
			public byte[]  localBaseKey { get; set; }
			[ProtoMember(3)]
			public byte[]  localBaseKeyPrivate { get; set; }
			[ProtoMember(4)]
			public byte[]  localRatchetKey { get; set; }
			[ProtoMember(5)]
			public byte[]  localRatchetKeyPrivate { get; set; }
			[ProtoMember(7)]
			public byte[]  localIdentityKey { get; set; }
			[ProtoMember(8)]
			public byte[]  localIdentityKeyPrivate { get; set; }
		}

		[ProtoContract]
		public class PendingPreKey {
			[ProtoMember(1)]
			public UInt32 preKeyId { get; set; }
			[ProtoMember(2)]
			public Int32  signedPreKeyId { get; set; }
			[ProtoMember(3)]
			public byte[]  baseKey { get; set; }
		}

		[ProtoMember(1)]
		public UInt32 sessionVersion { get; set; }
		[ProtoMember(2)]
		public byte[] localIdentityPublic { get; set; }
		[ProtoMember(3)]
		public byte[] remoteIdentityPublic { get; set; }

		[ProtoMember(4)]
		public byte[] rootKey { get; set; }
		[ProtoMember(5)]
		public UInt32 previousCounter { get; set; }

		[ProtoMember(6)]
		public Chain senderChain { get; set; }
		[ProtoMember(7)]
		public List<Chain> receiverChains { get; set; }

		[ProtoMember(8)]
		public PendingKeyExchange pendingKeyExchange { get; set; }
		[ProtoMember(9)]
		public PendingPreKey      pendingPreKey { get; set; }

		[ProtoMember(10)]
		public UInt32 remoteRegistrationId { get; set; }
		[ProtoMember(11)]
		public UInt32 localRegistrationId { get; set; }

		[ProtoMember(12)]
		public bool needsRefresh { get; set; }
		[ProtoMember(13)]
		public byte[] aliceBaseKey { get; set; }
	}

	[ProtoContract]
	public class RecordStructure {
		[ProtoMember(1)]
		public SessionStructure currentSession { get; set; }
		[ProtoMember(2)]
		public List<SessionStructure> previousSessions { get; set;}
	}

	[ProtoContract]
	public class PreKeyRecordStructure {
		[ProtoMember(1)]
		public UInt32 id { get; set; }
		[ProtoMember(2)]
		public byte[] publicKey { get; set; }
		[ProtoMember(3)]
		public byte[] privateKey { get; set; }
	}

	[ProtoContract]
	public class SignedPreKeyRecordStructure {
		[ProtoMember(1)]
		public UInt32 id { get; set; }
		[ProtoMember(2)]
		public byte[] publicKey { get; set; }
		[ProtoMember(3)]
		public byte[] privateKey { get; set; }
		[ProtoMember(4)]
		public byte[] signature { get; set; }
		//[ProtoMember(5)]
		//public fixed64 timestamp  = 5; // UNDONE
	}

	[ProtoContract]
	public class IdentityKeyPairStructure {
		[ProtoMember(1)]
		public byte[] publicKey { get; set; }
		[ProtoMember(2)]
		public byte[] privateKey { get; set; }
	}

	[ProtoContract]
	public class SenderKeyStateStructure {

		[ProtoContract]
		public class SenderChainKey {
			[ProtoMember(1)]
			public UInt32 iteration { get; set; }
			[ProtoMember(2)]
			public byte[] seed { get; set; }
		}

		[ProtoContract]
		public class SenderMessageKey {
			[ProtoMember(1)]
			public UInt32 iteration { get; set; }
			[ProtoMember(2)]
			public byte[] seed { get; set; }
		}

		[ProtoContract]
		public class SenderSigningKey {
			[ProtoMember(1)]
			public byte[] PublicKey { get; set; }
			[ProtoMember(2)]
			public byte[] PrivateKey { get; set; }
		}

		[ProtoMember(1)]
		public UInt32 senderKeyId { get; set; }
		[ProtoMember(2)]
		public SenderChainKey senderChainKey { get; set; }
		[ProtoMember(3)]
		public SenderSigningKey senderSigningKey { get; set; }
		[ProtoMember(4)]
		public List<SenderMessageKey> senderMessageKeys { get; set; }
	}

	[ProtoContract]
	public class SenderKeyRecordStructure {
		[ProtoMember(1)]
		public List<SenderKeyStateStructure> senderKeyStates { get; set; }
	}
}