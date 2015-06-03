using System;
using ProtoBuf;

namespace Axolotl.SessionStructure
{
	[ProtoContract]
	public class PendingKeyExchange
	{
		[ProtoMember(1)]
		public UInt32? Sequence { get; set; }

		[ProtoMember(2)]
		public byte[]  LocalBaseKey { get; set; }

		[ProtoMember(3)]
		public byte[]  LocalBaseKeyPrivate { get; set; }

		[ProtoMember(4)]
		public byte[]  LocalRatchetKey { get; set; }

		[ProtoMember(5)]
		public byte[]  LocalRatchetKeyPrivate { get; set; }

		[ProtoMember(7)]
		public byte[]  LocalIdentityKey { get; set; }

		[ProtoMember(8)]
		public byte[]  LocalIdentityKeyPrivate { get; set; }
	}
}

