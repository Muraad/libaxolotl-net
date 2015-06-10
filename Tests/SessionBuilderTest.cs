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
		static DateTime Jan1st1970 = new DateTime
			(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		static long currentMillis = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

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
			bobStore.StorePreKey((UInt32)31337, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));

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

			bobStore.StorePreKey((UInt32)31338, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
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

		[Test()]
		public void TestBasicPreKeyV3()
		{
			var aliceStore         		 = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder		 = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore           		 = new TestInMemoryAxolotlStore();
			var bobPreKeyPair      		 = Curve.GenerateKeyPair();
			var bobSignedPreKeyPair		 = Curve.GenerateKeyPair();
			var bobSignedPreKeySignature = Curve.CalculateSignature(bobStore.GetIdentityKeyPair().PrivateKey,
			                                                                 bobSignedPreKeyPair.PublicKey.Serialize());

			var bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                                          31337, bobPreKeyPair.PublicKey,
			                                          22, bobSignedPreKeyPair.PublicKey,
			                                          bobSignedPreKeySignature,
			                                          bobStore.GetIdentityKeyPair().PublicKey);

			aliceSessionBuilder.Process(bobPreKey);

			Assert.True(aliceStore.ContainsSession(BOB_ADDRESS));
			Assert.True(aliceStore.LoadSession(BOB_ADDRESS).SessionState.GetSessionVersion() == 3);

			var originalMessage    = "L'homme est condamné à être libre";
			var aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			var outgoingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			Assert.True(outgoingMessage.GetKeyType() == CiphertextMessage.PREKEY_TYPE);

			PreKeyWhisperMessage incomingMessage = new PreKeyWhisperMessage(outgoingMessage.Serialize());
			bobStore.StorePreKey(31337, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
			bobStore.StoreSignedPreKey(22, new SignedPreKeyRecord(22, currentMillis, bobSignedPreKeyPair, bobSignedPreKeySignature));

			SessionCipher bobSessionCipher = new SessionCipher(bobStore, ALICE_ADDRESS);
			byte[] plaintext = bobSessionCipher.Decrypt(incomingMessage, new TestDecryptionCallback());

			Assert.True(bobStore.ContainsSession(ALICE_ADDRESS));
			Assert.True(bobStore.LoadSession(ALICE_ADDRESS).SessionState.GetSessionVersion() == 3);
			Assert.True(bobStore.LoadSession(ALICE_ADDRESS).SessionState.AliceBaseKey != null);
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));

			CiphertextMessage bobOutgoingMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));
			Assert.True(bobOutgoingMessage.GetKeyType() == CiphertextMessage.WHISPER_TYPE);

			var alicePlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobOutgoingMessage.Serialize()));
			Assert.True (Encoding.UTF8.GetString(alicePlaintext).Equals (originalMessage));

			RunInteraction(aliceStore, bobStore);

			aliceStore          = new TestInMemoryAxolotlStore();
			aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);
			aliceSessionCipher  = new SessionCipher(aliceStore, BOB_ADDRESS);

			bobPreKeyPair            = Curve.GenerateKeyPair();
			bobSignedPreKeyPair      = Curve.GenerateKeyPair();
			bobSignedPreKeySignature = Curve.CalculateSignature(bobStore.GetIdentityKeyPair().PrivateKey, bobSignedPreKeyPair.PublicKey.Serialize());
			bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(),
			                             1, 31338, bobPreKeyPair.PublicKey,
			                             23, bobSignedPreKeyPair.PublicKey, bobSignedPreKeySignature,
			                             bobStore.GetIdentityKeyPair().PublicKey);

			bobStore.StorePreKey(31338, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
			bobStore.StoreSignedPreKey(23, new SignedPreKeyRecord(23, currentMillis, bobSignedPreKeyPair, bobSignedPreKeySignature));
			aliceSessionBuilder.Process(bobPreKey);

			outgoingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			try {
				plaintext = bobSessionCipher.Decrypt(new PreKeyWhisperMessage(outgoingMessage.Serialize()));
				throw new InvalidOperationException("shouldn't be trusted!");
			} catch (UntrustedIdentityException uie) {
				bobStore.SaveIdentity(ALICE_ADDRESS.Name, new PreKeyWhisperMessage(outgoingMessage.Serialize()).IdentityKey);
			}

			plaintext = bobSessionCipher.Decrypt(new PreKeyWhisperMessage(outgoingMessage.Serialize()));
			Assert.True(Encoding.UTF8.GetString(plaintext).Equals(originalMessage));

			bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                             31337, Curve.GenerateKeyPair().PublicKey,
			                             23, bobSignedPreKeyPair.PublicKey, bobSignedPreKeySignature,
			                             aliceStore.GetIdentityKeyPair().PublicKey);

			try {
				aliceSessionBuilder.Process(bobPreKey);
				throw new InvalidOperationException("shoulnd't be trusted!");
			} catch (UntrustedIdentityException uie) {
				Assert.Pass();
			}
		}

		[Test()]
		public void TestBadSignedPreKeySignature()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			IIdentityKeyStore bobIdentityKeyStore = new TestInMemoryIdentityKeyStore();

			ECKeyPair bobPreKeyPair            = Curve.GenerateKeyPair();
			ECKeyPair bobSignedPreKeyPair      = Curve.GenerateKeyPair();
			byte[]    bobSignedPreKeySignature = Curve.CalculateSignature(bobIdentityKeyStore.GetIdentityKeyPair().PrivateKey,
			                                                              bobSignedPreKeyPair.PublicKey.Serialize());


			for (int i = 0; i < bobSignedPreKeySignature.Length * 8; i++) 
			{
				var modifiedSignature = new byte[bobSignedPreKeySignature.Length];

				Array.Copy (bobSignedPreKeySignature, 0, modifiedSignature, 0, modifiedSignature.Length);

				modifiedSignature[i/8] ^= (byte)(0x01 << (i % 8));

				PreKeyBundle bobPreKey = new PreKeyBundle(bobIdentityKeyStore.GetLocalRegistrationId(), 1,
				                                          31337, bobPreKeyPair.PublicKey,
				                                          22, bobSignedPreKeyPair.PublicKey, modifiedSignature,
				                                          bobIdentityKeyStore.GetIdentityKeyPair().PublicKey);

				try {
					aliceSessionBuilder.Process(bobPreKey);
					throw new InvalidOperationException("Accepted modified device key signature!");
				} catch (InvalidKeyException ike) {
					Assert.Pass();
				}
			}
		}

		[Test()]
		public void TestRepeatBundleMessageV2()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore = new TestInMemoryAxolotlStore();

			var bobPreKeyPair            = Curve.GenerateKeyPair();
			var bobSignedPreKeyPair      = Curve.GenerateKeyPair();
			var bobSignedPreKeySignature = Curve.CalculateSignature(bobStore.GetIdentityKeyPair().PrivateKey,
			                                                              bobSignedPreKeyPair.PublicKey.Serialize());

			var bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                                          31337, bobPreKeyPair.PublicKey,
			                                          0, null, null,
			                                          bobStore.GetIdentityKeyPair().PublicKey);

			bobStore.StorePreKey(31337, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
			bobStore.StoreSignedPreKey(22, new SignedPreKeyRecord(22, currentMillis, bobSignedPreKeyPair, bobSignedPreKeySignature));

			aliceSessionBuilder.Process(bobPreKey);

			var originalMessage    = "L'homme est condamné à être libre";
			var aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			var outgoingMessageOne = aliceSessionCipher.Encrypt(Encoding.UTF8.GetBytes(originalMessage));
            var outgoingMessageTwo = aliceSessionCipher.Encrypt(Encoding.UTF8.GetBytes(originalMessage));

			Assert.True(outgoingMessageOne.GetKeyType() == CiphertextMessage.PREKEY_TYPE);

			var incomingMessage = new PreKeyWhisperMessage(outgoingMessageOne.Serialize());

			var bobSessionCipher = new SessionCipher(bobStore, ALICE_ADDRESS);

			var        plaintext        = bobSessionCipher.Decrypt(incomingMessage);
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));

            var bobOutgoingMessage = bobSessionCipher.Encrypt(Encoding.UTF8.GetBytes(originalMessage));

			var alicePlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobOutgoingMessage.Serialize()));
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(alicePlaintext)));

			// The test

			var incomingMessageTwo = new PreKeyWhisperMessage(outgoingMessageTwo.Serialize());

			plaintext = bobSessionCipher.Decrypt(incomingMessageTwo);
            Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));

    		bobOutgoingMessage = bobSessionCipher.Encrypt(Encoding.UTF8.GetBytes(originalMessage));
			alicePlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobOutgoingMessage.Serialize()));
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(alicePlaintext)));
		}

		[Test()]
		public void TestRepeatBundleMessageV3()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore = new TestInMemoryAxolotlStore();

			var bobPreKeyPair            = Curve.GenerateKeyPair();
			var bobSignedPreKeyPair      = Curve.GenerateKeyPair();
			var bobSignedPreKeySignature = Curve.CalculateSignature(bobStore.GetIdentityKeyPair().PrivateKey,
			                                                              bobSignedPreKeyPair.PublicKey.Serialize());

			PreKeyBundle bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                                          31337, bobPreKeyPair.PublicKey,
			                                          22, bobSignedPreKeyPair.PublicKey, bobSignedPreKeySignature,
			                                          bobStore.GetIdentityKeyPair().PublicKey);

			bobStore.StorePreKey(31337, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
			bobStore.StoreSignedPreKey(22, new SignedPreKeyRecord(22, currentMillis, bobSignedPreKeyPair, bobSignedPreKeySignature));

			aliceSessionBuilder.Process(bobPreKey);

			var originalMessage    = "L'homme est condamné à être libre";
			var aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			var outgoingMessageOne = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));
			var outgoingMessageTwo = aliceSessionCipher.Encrypt(Encoding.UTF8.GetBytes (originalMessage));

			Assert.True(outgoingMessageOne.GetKeyType() == CiphertextMessage.PREKEY_TYPE);
			Assert.True(outgoingMessageTwo.GetKeyType() == CiphertextMessage.PREKEY_TYPE);

			var incomingMessage = new PreKeyWhisperMessage(outgoingMessageOne.Serialize());

			var bobSessionCipher = new SessionCipher(bobStore, ALICE_ADDRESS);

			var plaintext = bobSessionCipher.Decrypt(incomingMessage);
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));

			var bobOutgoingMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			var alicePlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobOutgoingMessage.Serialize()));
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(alicePlaintext)));

			// The test

			var incomingMessageTwo = new PreKeyWhisperMessage(outgoingMessageTwo.Serialize());

			plaintext = bobSessionCipher.Decrypt(new PreKeyWhisperMessage(incomingMessageTwo.Serialize()));
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));

			bobOutgoingMessage = bobSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));
			alicePlaintext = aliceSessionCipher.Decrypt(new WhisperMessage(bobOutgoingMessage.Serialize()));
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString (alicePlaintext)));
		}

		[Test()]
		public void TestBadMessageBundle()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore = new TestInMemoryAxolotlStore();

			var bobPreKeyPair            = Curve.GenerateKeyPair();
			var bobSignedPreKeyPair      = Curve.GenerateKeyPair();
			var bobSignedPreKeySignature = Curve.CalculateSignature(bobStore.GetIdentityKeyPair().PrivateKey,
			                                                              bobSignedPreKeyPair.PublicKey.Serialize());

			PreKeyBundle bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                                          31337, bobPreKeyPair.PublicKey,
			                                          22, bobSignedPreKeyPair.PublicKey, bobSignedPreKeySignature,
			                                          bobStore.GetIdentityKeyPair().PublicKey);

			bobStore.StorePreKey(31337, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
			bobStore.StoreSignedPreKey(22, new SignedPreKeyRecord(22, currentMillis, bobSignedPreKeyPair, bobSignedPreKeySignature));

			aliceSessionBuilder.Process(bobPreKey);

			var originalMessage    = "L'homme est condamné à être libre";
			var aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			var outgoingMessageOne = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			Assert.True(outgoingMessageOne.GetKeyType() == CiphertextMessage.PREKEY_TYPE);

			var goodMessage = outgoingMessageOne.Serialize();
			var badMessage  = new byte[goodMessage.Length];

			Array.Copy(goodMessage, 0, badMessage, 0, badMessage.Length);

			badMessage[badMessage.Length-10] ^= 0x01;

			var incomingMessage  = new PreKeyWhisperMessage(badMessage);
			var bobSessionCipher = new SessionCipher(bobStore, ALICE_ADDRESS);

			var plaintext = new byte[0];

			try {
				plaintext = bobSessionCipher.Decrypt(incomingMessage);
				throw new InvalidOperationException("Decrypt should have failed!");
			} catch (InvalidMessageException e) {
				Assert.Pass ();
			}

			Assert.True(bobStore.ContainsPreKey(31337));

			plaintext = bobSessionCipher.Decrypt(new PreKeyWhisperMessage(goodMessage));

			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));
			Assert.True(!bobStore.ContainsPreKey(31337));
		}

		[Test()]
		public void TestBasicKeyExchange()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore          = new TestInMemoryAxolotlStore();
			var bobSessionBuilder = new SessionBuilder(bobStore, ALICE_ADDRESS);

			var aliceKeyExchangeMessage      = aliceSessionBuilder.Process();
			Assert.True(aliceKeyExchangeMessage != null);

			var aliceKeyExchangeMessageBytes = aliceKeyExchangeMessage.Serialize();
			var bobKeyExchangeMessage        = bobSessionBuilder.Process(new KeyExchangeMessage(aliceKeyExchangeMessageBytes));

			Assert.True(bobKeyExchangeMessage != null);

			var bobKeyExchangeMessageBytes = bobKeyExchangeMessage.Serialize();
			var response = aliceSessionBuilder.Process(new KeyExchangeMessage(bobKeyExchangeMessageBytes));

			Assert.True(response == null);
			Assert.True(aliceStore.ContainsSession(BOB_ADDRESS));
			Assert.True(bobStore.ContainsSession(ALICE_ADDRESS));

			RunInteraction(aliceStore, bobStore);

			aliceStore              = new TestInMemoryAxolotlStore();
			aliceSessionBuilder     = new SessionBuilder(aliceStore, BOB_ADDRESS);
			aliceKeyExchangeMessage = aliceSessionBuilder.Process();

			try {
				bobKeyExchangeMessage = bobSessionBuilder.Process(aliceKeyExchangeMessage);
				throw new InvalidOperationException("This identity shouldn't be trusted!");
			} catch (UntrustedIdentityException uie) {
				bobStore.SaveIdentity(ALICE_ADDRESS.Name, aliceKeyExchangeMessage.IdentityKey);
				bobKeyExchangeMessage = bobSessionBuilder.Process(aliceKeyExchangeMessage);
			}

			Assert.True(aliceSessionBuilder.Process(bobKeyExchangeMessage) == null);

			RunInteraction(aliceStore, bobStore);
		}

		[Test()]
		public void TestSimultaneousKeyExchange()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore          = new TestInMemoryAxolotlStore();
			var bobSessionBuilder = new SessionBuilder(bobStore, ALICE_ADDRESS);

			var aliceKeyExchange = aliceSessionBuilder.Process();
			var bobKeyExchange   = bobSessionBuilder.Process();

			Assert.True(aliceKeyExchange != null);
			Assert.True(bobKeyExchange != null);

			var aliceResponse = aliceSessionBuilder.Process(bobKeyExchange);
			var bobResponse   = bobSessionBuilder.Process(aliceKeyExchange);

			Assert.True(aliceResponse != null);
			Assert.True(bobResponse != null);

			var aliceAck = aliceSessionBuilder.Process(bobResponse);
			var bobAck   = bobSessionBuilder.Process(aliceResponse);

			Assert.True(aliceAck == null);
			Assert.True(bobAck == null);

			RunInteraction(aliceStore, bobStore);
		}

		[Test()]
		public void TestOptionalOneTimePreKey()
		{
			var aliceStore          = new TestInMemoryAxolotlStore();
			var aliceSessionBuilder = new SessionBuilder(aliceStore, BOB_ADDRESS);

			var bobStore = new TestInMemoryAxolotlStore();

			var bobPreKeyPair            = Curve.GenerateKeyPair();
			var bobSignedPreKeyPair      = Curve.GenerateKeyPair();
			var bobSignedPreKeySignature = Curve.CalculateSignature(bobStore.GetIdentityKeyPair().PrivateKey,
			                                                              bobSignedPreKeyPair.PublicKey.Serialize());

			var bobPreKey = new PreKeyBundle(bobStore.GetLocalRegistrationId(), 1,
			                                          0, null,
			                                          22, bobSignedPreKeyPair.PublicKey,
			                                          bobSignedPreKeySignature,
			                                          bobStore.GetIdentityKeyPair().PublicKey);

			aliceSessionBuilder.Process(bobPreKey);

			Assert.True(aliceStore.ContainsSession(BOB_ADDRESS));
			Assert.True(aliceStore.LoadSession(BOB_ADDRESS).SessionState.GetSessionVersion() == 3);

			var originalMessage    = "L'homme est condamné à être libre";
			var aliceSessionCipher = new SessionCipher(aliceStore, BOB_ADDRESS);
			var outgoingMessage = aliceSessionCipher.Encrypt (Encoding.UTF8.GetBytes (originalMessage));

			Assert.True(outgoingMessage.GetKeyType() == CiphertextMessage.PREKEY_TYPE);

			var incomingMessage = new PreKeyWhisperMessage(outgoingMessage.Serialize());
			Assert.True(!incomingMessage.PreKeyId.HasValue);

			bobStore.StorePreKey(31337, new PreKeyRecord(bobPreKey.PreKeyId, bobPreKeyPair));
			bobStore.StoreSignedPreKey(22, new SignedPreKeyRecord(22, currentMillis, bobSignedPreKeyPair, bobSignedPreKeySignature));

			var bobSessionCipher = new SessionCipher(bobStore, ALICE_ADDRESS);
			var plaintext        = bobSessionCipher.Decrypt(incomingMessage);

			Assert.True(bobStore.ContainsSession(ALICE_ADDRESS));
			Assert.True(bobStore.LoadSession(ALICE_ADDRESS).SessionState.GetSessionVersion() == 3);
			Assert.True(bobStore.LoadSession(ALICE_ADDRESS).SessionState.AliceBaseKey != null);
			Assert.True(originalMessage.Equals(Encoding.UTF8.GetString(plaintext)));
		}

		class TestDecryptionCallback : IDecryptionCallback
		{
			public void HandlePlaintext(byte[] plaintext) 
			{
				// TODO
				//Assert.True(originalMessage.Equals(new string(plaintext)));
				//Assert.False(bobStore.ContainsSession(ALICE_ADDRESS));
			}
		}
	}
}

