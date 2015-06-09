using NUnit.Framework;
using System;
using Axolotl.State;
using System.Collections.Generic;
using Axolotl.Protocol;
using Axolotl;
using System.Text;

namespace Tests
{
	[TestFixture()]
	public class SessionCipherTest
	{
		static DateTime Jan1st1970 = new DateTime
			(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		static long CurrentMillis {
			get { return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds; }
		}

		[Test()]
		public void TestBasicSessionV2()
		{
			var aliceSessionRecord = new SessionRecord();
			var bobSessionRecord   = new SessionRecord();

			// TODO
			//InitializeSessionsV2(aliceSessionRecord.SessionState, bobSessionRecord.SessionState);
			RunInteraction(aliceSessionRecord, bobSessionRecord);
		}

		[Test()]
		public void TestBasicSessionV3()
		{
			var aliceSessionRecord = new SessionRecord();
			var bobSessionRecord   = new SessionRecord();

			// TODO
			//InitializeSessionsV3(aliceSessionRecord.SessionState, bobSessionRecord.SessionState);
			RunInteraction(aliceSessionRecord, bobSessionRecord);
		}

		private void RunInteraction(SessionRecord aliceSessionRecord, SessionRecord bobSessionRecord)
		{
			var aliceStore = new TestInMemoryAxolotlStore();
			var bobStore   = new TestInMemoryAxolotlStore();

			aliceStore.StoreSession(new AxolotlAddress("+14159999999", 1), aliceSessionRecord);
			bobStore.StoreSession(new AxolotlAddress("+14158888888", 1), bobSessionRecord);

			var aliceCipher    = new SessionCipher(aliceStore, new AxolotlAddress("+14159999999", 1));
			var bobCipher      = new SessionCipher(bobStore, new AxolotlAddress("+14158888888", 1));

			var alicePlaintext = Encoding.UTF8.GetBytes ("This is a plaintext message.");
			var message        = aliceCipher.Encrypt(alicePlaintext);
			var bobPlaintext   = bobCipher.Decrypt(new WhisperMessage(message.Serialize()));

			Assert.True(ArrayComparer.Compare(alicePlaintext, bobPlaintext));

			var bobReply = Encoding.UTF8.GetBytes ("This is a message from Bob.");
			var reply         = bobCipher.Encrypt(bobReply);
			var receivedReply = aliceCipher.Decrypt(new WhisperMessage(reply.Serialize()));

			Assert.True(ArrayComparer.Compare(bobReply, receivedReply));

			var aliceCiphertextMessages = new List<CiphertextMessage>();
			var alicePlaintextMessages  = new List<byte[]>();

			for (int i = 0; i < 50; i++) {
				alicePlaintextMessages.Add (Encoding.UTF8.GetBytes (string.Format ("смерть за смерть {0}", i)));
				aliceCiphertextMessages.Add(aliceCipher.Encrypt(Encoding.UTF8.GetBytes (string.Format ("смерть за смерть {0}", i))));
			}

			long seed = CurrentMillis;

			aliceCiphertextMessages.Sort ((x, y) => new Random((int)CurrentMillis + DateTime.UtcNow.Day).Next (-1, 2));
			alicePlaintextMessages.Sort ((x, y) => new Random ((int)CurrentMillis + DateTime.UtcNow.Minute).Next (-1, 2));

			for (int i = 0; i < aliceCiphertextMessages.Count / 2; i++) {
				byte[] receivedPlaintext = bobCipher.Decrypt(new WhisperMessage(aliceCiphertextMessages[i].Serialize()));
				Assert.True(ArrayComparer.Compare(receivedPlaintext, alicePlaintextMessages[i]));
			}

			var bobCiphertextMessages = new List<CiphertextMessage>();
			var bobPlaintextMessages  = new List<byte[]>();

			for (int i=0;i<20;i++) {
				bobPlaintextMessages.Add(Encoding.UTF8.GetBytes (string.Format ("смерть за смерть {0}", i)));
				bobCiphertextMessages.Add(bobCipher.Encrypt(Encoding.UTF8.GetBytes (string.Format ("смерть за смерть {0}", i))));
			}

			seed = CurrentMillis;

			bobCiphertextMessages.Sort ((x, y) => new Random((int)CurrentMillis + DateTime.UtcNow.Day).Next (-1, 2));
			bobPlaintextMessages.Sort ((x, y) => new Random ((int)CurrentMillis + DateTime.UtcNow.Minute).Next (-1, 2));

			for (int i = 0; i < bobCiphertextMessages.Count / 2; i++) {
				byte[] receivedPlaintext = aliceCipher.Decrypt(new WhisperMessage(bobCiphertextMessages[i].Serialize()));
				Assert.True(ArrayComparer.Compare(receivedPlaintext, bobPlaintextMessages[i]));
			}

			for (int i = aliceCiphertextMessages.Count / 2; i < aliceCiphertextMessages.Count; i++) {
				byte[] receivedPlaintext = bobCipher.Decrypt(new WhisperMessage(aliceCiphertextMessages[i].Serialize()));
				Assert.True(ArrayComparer.Compare(receivedPlaintext, alicePlaintextMessages[i]));
			}

			for (int i=bobCiphertextMessages.Count / 2; i < bobCiphertextMessages.Count; i++) {
				byte[] receivedPlaintext = aliceCipher.Decrypt(new WhisperMessage(bobCiphertextMessages[i].Serialize()));
				Assert.True(ArrayComparer.Compare(receivedPlaintext, bobPlaintextMessages[i]));
			}
		}
	}
}

