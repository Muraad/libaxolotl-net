using System;

namespace Axolotl.Groups.State
{
	public interface ISenderKeyStore 
	{
		void StoreSenderKey(SenderKeyName senderKeyName, SenderKeyRecord record);

		SenderKeyRecord LoadSenderKey(SenderKeyName senderKeyName);
	}
}

