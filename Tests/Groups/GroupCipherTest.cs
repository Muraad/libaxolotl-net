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
			var aliceStore = new InMemorySenderKeyStore();
			var bobStore   = new InMemorySenderKeyStore();

			var aliceSessionBuilder = new GroupSessionBuilder(aliceStore);
			var bobSessionBuilder   = new GroupSessionBuilder(bobStore);

			var aliceGroupCipher = new GroupCipher(aliceStore, GROUP_SENDER);
			var bobGroupCipher   = new GroupCipher(bobStore, GROUP_SENDER);

			var sentAliceDistributionMessage     = aliceSessionBuilder.Create(GROUP_SENDER);
			var receivedAliceDistributionMessage = new SenderKeyDistributionMessage(sentAliceDistributionMessage.Serialize());

			bobSessionBuilder.Process(GROUP_SENDER, receivedAliceDistributionMessage);

			var ciphertextFromAlice = aliceGroupCipher.Encrypt (Encoding.UTF8.GetBytes ("smert ze smert"));
			try {
				var plaintextFromAlice  = bobGroupCipher.Decrypt(ciphertextFromAlice);
				throw new InvalidOperationException("Should be no session!");
			} catch (NoSessionException e) {
				Assert.Pass ();
			}
		}
	}
}

