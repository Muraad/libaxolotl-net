using System;
using ProtoBuf;
using System.Collections.Generic;

namespace Axolotl.SessionStructure
{
	[ProtoContract]
	public class Chain {
		[ProtoMember(1)]
		public byte[] SenderRatchetKey { get; set; }
		[ProtoMember(2)]
		public byte[] SenderRatchetKeyPrivate { get; set; }

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
}

