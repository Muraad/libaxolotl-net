using System;
using Axolotl.Groups.State;
using Axolotl.Protocol;
using Axolotl.Util;

namespace Axolotl.Groups
{
	public class GroupSessionBuilder
	{
		private ISenderKeyStore _senderKeyStore;

		public GroupSessionBuilder (ISenderKeyStore senderKeyStore)
		{
			_senderKeyStore = senderKeyStore;
		}

		public void Process(SenderKeyName senderKeyName, SenderKeyDistributionMessage senderKeyDistributionMessage) 
		{
			lock (GroupCipher.LOCK) 
			{
				var senderKeyRecord = _senderKeyStore.LoadSenderKey(senderKeyName);
				senderKeyRecord.AddSenderKeyState(senderKeyDistributionMessage.Id,
				                                  senderKeyDistributionMessage.Iteration,
				                                  senderKeyDistributionMessage.ChainKey,
				                                  senderKeyDistributionMessage.SignatureKey);

				_senderKeyStore.StoreSenderKey(senderKeyName, senderKeyRecord);
			}
		}

		public SenderKeyDistributionMessage Create(SenderKeyName senderKeyName) 
		{
			lock (GroupCipher.LOCK) 
			{
				try {
					var senderKeyRecord = _senderKeyStore.LoadSenderKey(senderKeyName);

					if (senderKeyRecord.IsEmpty) {
						senderKeyRecord.SetSenderKeyState(KeyHelper.GenerateSenderKeyId(),
						                                  0,
						                                  KeyHelper.GenerateSenderKey(),
						                                  KeyHelper.GenerateSenderSigningKey());
						_senderKeyStore.StoreSenderKey(senderKeyName, senderKeyRecord);
					}

					SenderKeyState state = senderKeyRecord.GetSenderKeyState();

					return new SenderKeyDistributionMessage(state.KeyId,
					                                        state.SenderChainKey.Iteration,
					                                        state.SenderChainKey.Seed,
					                                        state.SigningKeyPublic);

				} catch (Exception e) {
					throw new Exception("GroupSessuinBuilder ex: " + e);
				}
			}
		}
	}
}

