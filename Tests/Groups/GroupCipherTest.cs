using System;
using System.Text;
using Axolotl;
using Axolotl.Groups;
using Axolotl.Protocol;
using NUnit.Framework;

namespace Tests
{
	[TestFixture()]
	public class GroupCipherTest
	{
		private static AxolotlAddress SENDER_ADDRESS = new AxolotlAddress("+14150001111", 1);
		private static SenderKeyName  GROUP_SENDER   = new SenderKeyName("nihilist history reading group", SENDER_ADDRESS);

		[Test()]
		public void TestNoSession ()
		{
			InMemorySenderKeyStore aliceStore = new InMemorySenderKeyStore();
			InMemorySenderKeyStore bobStore   = new InMemorySenderKeyStore();

			GroupSessionBuilder aliceSessionBuilder = new GroupSessionBuilder(aliceStore);
			GroupSessionBuilder bobSessionBuilder   = new GroupSessionBuilder(bobStore);

			GroupCipher aliceGroupCipher = new GroupCipher(aliceStore, GROUP_SENDER);
			GroupCipher bobGroupCipher   = new GroupCipher(bobStore, GROUP_SENDER);

			SenderKeyDistributionMessage sentAliceDistributionMessage     = aliceSessionBuilder.Create(GROUP_SENDER);
			SenderKeyDistributionMessage receivedAliceDistributionMessage = new SenderKeyDistributionMessage(sentAliceDistributionMessage.Serialize());

			//bobSessionBuilder.Process(GROUP_SENDER, receivedAliceDistributionMessage);

			byte[] ciphertextFromAlice = aliceGroupCipher.Encrypt (Encoding.UTF8.GetBytes ("smert ze smert"));
			try {
				byte[] plaintextFromAlice  = bobGroupCipher.Decrypt(ciphertextFromAlice);
				throw new InvalidOperationException("Should be no session!");
			} catch (NoSessionException e) {
				Assert.Pass ();
			}
		}
	}
}

