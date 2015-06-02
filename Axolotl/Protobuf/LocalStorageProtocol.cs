using System;
using ProtoBuf;
using System.Collections.Generic;
using Axolotl.SessionStructure;

namespace Axolotl.State
{
	[ProtoContract]
	public class SessionStructure
	{
		[ProtoMember(1)]
		public UInt32 SessionVersion { get; set; }

		[ProtoMember(2)]
		public byte[] LocalIdentityPublic { get; set; }

		[ProtoMember(3)]
		public byte[] RemoteIdentityPublic { get; set; }

		[ProtoMember(4)]
		public byte[] RootKey { get; set; }

		[ProtoMember(5)]
		public UInt32 PreviousCounter { get; set; }

		[ProtoMember(6)]
		public Chain SenderChain { get; set; }

		[ProtoMember(7)]
		public List<Chain> ReceiverChains { get; set; }

		[ProtoMember(8)]
		public PendingKeyExchange PendKeyExchange { get; set; }

		[ProtoMember(9)]
		public PendingPreKey      PendPreKey { get; set; }

		[ProtoMember(10)]
		public UInt32 RemoteRegistrationId { get; set; }

		[ProtoMember(11)]
		public UInt32 LocalRegistrationId { get; set; }

		[ProtoMember(12)]
		public bool NeedsRefresh { get; set; }

		[ProtoMember(13)]
		public byte[] AliceBaseKey { get; set; }
	}

	[ProtoContract]
	public class RecordStructure
	{
		[ProtoMember(1)]
		public SessionStructure CurrentSession { get; set; }

		[ProtoMember(2)]
		public List<SessionStructure> PreviousSessions { get; set; }
	}

	[ProtoContract]
	public class PreKeyRecordStructure
	{
		[ProtoMember(1)]
		public UInt32 Id { get; set; }

		[ProtoMember(2)]
		public byte[] PublicKey { get; set; }

		[ProtoMember(3)]
		public byte[] PrivateKey { get; set; }
	}

	[ProtoContract]
	public class SignedPreKeyRecordStructure
	{
		[ProtoMember(1)]
		public UInt32 Id { get; set; }

		[ProtoMember(2)]
		public byte[] PublicKey { get; set; }

		[ProtoMember(3)]
		public byte[] PrivateKey { get; set; }

		[ProtoMember(4)]
		public byte[] Signature { get; set; }
		//[ProtoMember(5)]
		//public fixed64 timestamp  = 5; // UNDONE
	}

	[ProtoContract]
	public class IdentityKeyPairStructure
	{
		[ProtoMember(1)]
		public byte[] publicKey { get; set; }

		[ProtoMember(2)]
		public byte[] privateKey { get; set; }
	}

	[ProtoContract]
	public class SenderKeyStateStructure
	{

		[ProtoContract]
		public class SenderChainKey
		{
			[ProtoMember(1)]
			public UInt32 Iteration { get; set; }

			[ProtoMember(2)]
			public byte[] Seed { get; set; }
		}

		[ProtoContract]
		public class SenderMessageKey
		{
			[ProtoMember(1)]
			public UInt32 Iteration { get; set; }

			[ProtoMember(2)]
			public byte[] Seed { get; set; }
		}

		[ProtoContract]
		public class SenderSigningKey
		{
			[ProtoMember(1)]
			public byte[] PublicKey { get; set; }

			[ProtoMember(2)]
			public byte[] PrivateKey { get; set; }
		}

		[ProtoMember(1)]
		public UInt32 SenderKeyId { get; set; }

		[ProtoMember(2)]
		public SenderChainKey senderChainKey { get; set; }

		[ProtoMember(3)]
		public SenderSigningKey senderSigningKey { get; set; }

		[ProtoMember(4)]
		public List<SenderMessageKey> senderMessageKeys { get; set; }
	}

	[ProtoContract]
	public class SenderKeyRecordStructure
	{
		[ProtoMember(1)]
		public List<SenderKeyStateStructure> senderKeyStates { get; set; }
	}
}