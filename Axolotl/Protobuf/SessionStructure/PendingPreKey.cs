using System;
using ProtoBuf;

namespace Axolotl.SessionStructure
{
	[ProtoContract]
	public class PendingPreKey
	{
		[ProtoMember(1)]
		public UInt32 preKeyId { get; set; }

		[ProtoMember(2)]
		public Int32  signedPreKeyId { get; set; }

		[ProtoMember(3)]
		public byte[] baseKey { get; set; }
	}
}

