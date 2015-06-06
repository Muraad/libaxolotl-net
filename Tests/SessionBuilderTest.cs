using NUnit.Framework;
using System;
using Axolotl;
using Axolotl.State;
using Axolotl.ECC;
using Axolotl.Protocol;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Tests
{
	[TestFixture()]
	public class SessionBuilderTest
	{
		private static AxolotlAddress ALICE_ADDRESS = new AxolotlAddress("+14151111111", 1);
		private static AxolotlAddress BOB_ADDRESS   = new AxolotlAddress("+14152222222", 1);

		[Test()]
		public void TestBasicPreKeyV2 ()
		{
			IAxolotlStore   aliceStore         = new TestInMemoryAxolotlStore();
			SessionBuilder aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			IAxolotlStore bobStore      = new TestInMemoryAxolotlStore();
			ECKeyPair    bobPreKeyPair = Curve.GenerateKeyPair();
			PreKeyBundle bobPreKey     = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                                              31337, bobPreKeyPair.PublicKey,
			                                              0, null, null,
			                                              bobStore.GetIdentityKeyPair().PublicKey);

			aliceSessionBuilder.Process(bobPreKey);

			Assert.True(aliceStore.ContainsSession(BOB_ADDRESS));
			Assert.True(aliceStore.LoadSession(BOB_ADDRESS).SessionState.GetSessionVersion() == 2);

			var originalMessage    = "L'homme est condamné à être libre";
			var aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			var outgoingMessage    = aliceSessionCipher.Encrypt(Encoding.UTF8.GetBytes(originalMessage));

			Assert.True(outgoingMessage.GetKeyType() == CiphertextMessage.PREKEY_TYPE);

			PreKeyWhisperMessage incomingMessage = new PreKeyWhisperMessage(outgoingMessage.Serialize());
			bobStore.StorePreKey((UInt32)31337, new PreKeyRecord(bobPreKey.PreKeyID, bobPreKeyPair));

			var bobSessionCipher = new SessionCipher(bobStore, ALICE_ADDRESS);
			var plaintext        = bobSessionCipher.Decrypt(incomingMessage);

			Assert.True(bobStore.ContainsSession(ALICE_ADDRESS));
			Assert.True(bobStore.LoadSession(ALICE_ADDRESS).SessionState.GetSessionVersion() == 2);
			Assert.True(originalMessage.Equals(new string(plaintext.Select(x => (char)x).ToArray())));

			var bobOutgoingMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));
			Assert.True(bobOutgoingMessage.GetKeyType() == CiphertextMessage.WHISPER_TYPE);

			var alicePlaintext = aliceSessionCipher.Decrypt((WhisperMessage)bobOutgoingMessage);
			Assert.True(new string(alicePlaintext.Select(x => (char)x).ToArray()).Equals(originalMessage));

			RunInteraction(aliceStore, bobStore);

			aliceStore          = new TestInMemoryAxolotlStore();
			aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);
			aliceSessionCipher  = new SessionCipher(aliceStore, BOB_ADDRESS);

			bobPreKeyPair = Curve.GenerateKeyPair();
			bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(),
			                             1, 31338, bobPreKeyPair.PublicKey,
			                             0, null, null, bobStore.GetIdentityKeyPair().PublicKey	);

			bobStore.StorePreKey((UInt32)31338, new PreKeyRecord(bobPreKey.PreKeyID, bobPreKeyPair));
			aliceSessionBuilder.Process(bobPreKey);

			outgoingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			try {
				bobSessionCipher.Decrypt(new PreKeyWhisperMessage(outgoingMessage.Serialize()));
				throw new InvalidOperationException("shouldn't be trusted!");
			} catch (UntrustedIdentityException uie) {
				bobStore.SaveIdentity(ALICE_ADDRESS.Name, new PreKeyWhisperMessage(outgoingMessage.Serialize()).IdentityKey);
			}

			plaintext = bobSessionCipher.Decrypt(new PreKeyWhisperMessage(outgoingMessage.Serialize()));

			Assert.True(new string(plaintext.Select(x => (char)x).ToArray()).Equals(originalMessage));

			bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                             31337, Curve.GenerateKeyPair().PublicKey,
			                             0, null, null,
			                             aliceStore.GetIdentityKeyPair().PublicKey);

			try {
				aliceSessionBuilder.Process(bobPreKey);
				throw new InvalidOperationException("shoulnd't be trusted!");
			} catch (UntrustedIdentityException uie) {
				Assert.Pass();
			}
		}

		private void RunInteraction(IAxolotlStore aliceStore, IAxolotlStore bobStore)
		{
			SessionCipher aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			SessionCipher bobSessionCipher   = new SessionCipher(bobStore, ALICE_ADDRESS);

			String originalMessage = "smert ze smert";
			CiphertextMessage aliceMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			Assert.True(aliceMessage.GetKeyType() == CiphertextMessage.WHISPER_TYPE);

			byte[] plaintext = bobSessionCipher.Decrypt(new WhisperMessage(aliceMessage.Serialize()));
			Assert.True(new string(plaintext.Select(x => (char)x).ToArray()).Equals(originalMessage));

			var bobMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			Assert.True(bobMessage.GetKeyType() == CiphertextMessage.WHISPER_TYPE);

			plaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobMessage.Serialize()));
			Assert.True(new string(plaintext.Select(x => (char)x).ToArray()).Equals(originalMessage));

			for (int i=0;i<10;i++) {
				String loopingMessage = ("What do we mean by saying that existence precedes essence? " +
				                         "We mean that man first of all exists, encounters himself, " +
				                         "surges up in the world--and defines himself aftward. " + i);
				var aliceLoopingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (loopingMessage));

				byte[] loopingPlaintext = bobSessionCipher.Decrypt(new WhisperMessage(aliceLoopingMessage.Serialize()));
				Assert.True (new String (loopingPlaintext.Select(x => (char)x).ToArray()).Equals (Encoding.UTF8.GetBytes (loopingMessage)));
			}

			for (int i = 0; i < 10; i++) {
				String loopingMessage = ("What do we mean by saying that existence precedes essence? " +
				                         "We mean that man first of all exists, encounters himself, " +
				                         "surges up in the world--and defines himself aftward. " + i);
				var bobLoopingMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (loopingMessage));

				byte[] loopingPlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobLoopingMessage.Serialize()));
				Assert.True (new String (loopingPlaintext.Select(x => (char)x).ToArray()).Equals (Encoding.UTF8.GetBytes (loopingMessage)));
			}

			var aliceOutOfOrderMessages = new HashSet<Tuple<String, CiphertextMessage>>();

			for (int i = 0; i < 10; i++) {
				string loopingMessage = ("What do we mean by saying that existence precedes essence? " +
				                         "We mean that man first of all exists, encounters himself, " +
				                         "surges up in the world--and defines himself aftward. " + i);
				var aliceLoopingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (loopingMessage));

				aliceOutOfOrderMessages.Add(new Tuple<string, CiphertextMessage>(loopingMessage, aliceLoopingMessage));
			}

			for (int i = 0; i < 10; i++) {
				string loopingMessage = ("What do we mean by saying that existence precedes essence? " +
				                         "We mean that man first of all exists, encounters himself, " +
				                         "surges up in the world--and defines himself aftward. " + i);
				var aliceLoopingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (loopingMessage));

				byte[] loopingPlaintext = bobSessionCipher.Decrypt(new WhisperMessage(aliceLoopingMessage.Serialize()));
				Assert.True (new string (loopingPlaintext.Select(x => (char)x).ToArray()).Equals (Encoding.UTF8.GetBytes (loopingMessage)));
			}

			for (int i=0;i<10;i++) {
				String loopingMessage = ("You can only desire based on what you know: " + i);
				CiphertextMessage bobLoopingMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (loopingMessage));

				byte[] loopingPlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobLoopingMessage.Serialize()));
				Assert.True (new string (loopingPlaintext.Select(x => (char)x).ToArray()).Equals (Encoding.UTF8.GetBytes (loopingMessage)));
			}

			foreach (var aliceOutOfOrderMessage in aliceOutOfOrderMessages) {
				byte[] outOfOrderPlaintext = bobSessionCipher.Decrypt(new WhisperMessage(aliceOutOfOrderMessage.Item2.Serialize()));
				Assert.True(new string(outOfOrderPlaintext.Select(x => (char)x).ToArray()).Equals(aliceOutOfOrderMessage.Item1));
			}
		}
	}
}
