using System;

namespace Axolotl.Protocol
{
	public abstract class CiphertextMessage
	{
		// Complete

		public static UInt32 UNSUPPORTED_VERSION = 1;
		public static UInt32 CURRENT_VERSION     = 3;

		public static UInt32 WHISPER_TYPE                = 2;
		public static UInt32 PREKEY_TYPE                 = 3;
		public static UInt32 SENDERKEY_TYPE              = 4;
		public static UInt32 SENDERKEY_DISTRIBUTION_TYPE = 5;

		// This should be the worst case (worse than V2).  
		// So not always accurate, but good enough for padding.
		public static UInt32 ENCRYPTED_MESSAGE_OVERHEAD = 53;

		public abstract byte[] Serialize();

		public abstract UInt32 GetKeyType();
	}
}

