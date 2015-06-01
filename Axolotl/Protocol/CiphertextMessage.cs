using System;

namespace Axolotl.Protocol
{
	public abstract class CiphertextMessage
	{
		public static int UNSUPPORTED_VERSION = 1;
		public static int CURRENT_VERSION     = 3;

		public static int WHISPER_TYPE                = 2;
		public static int PREKEY_TYPE                 = 3;
		public static int SENDERKEY_TYPE              = 4;
		public static int SENDERKEY_DISTRIBUTION_TYPE = 5;

		// This should be the worst case (worse than V2).  
		// So not always accurate, but good enough for padding.
		public static int ENCRYPTED_MESSAGE_OVERHEAD = 53;

		public abstract byte[] Serialize();

		public abstract int GetKeyType();
	}
}

