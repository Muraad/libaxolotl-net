using System;
using Axolotl.Groups.State;
using Axolotl.Groups.Ratchet;
using Axolotl.Protocol;

namespace Axolotl.Groups
{
	public class GroupCipher
	{
		private object LOCK = new object();

		private ISenderKeyStore _senderKeyStore;
		private SenderKeyName _senderKeyId;

		public GroupCipher (ISenderKeyStore senderKeyStore, SenderKeyName senderKeyId)
		{
			_senderKeyStore = senderKeyStore;
			_senderKeyId = senderKeyId;
		}

		public byte[] encrypt(byte[] paddedPlaintext) 
		{
			lock(LOCK) {
				// TODO
	//			try {
					var record         = _senderKeyStore.LoadSenderKey(_senderKeyId);
					var senderKeyState = record.GetSenderKeyState();
					var senderKey      = senderKeyState.SenderChainKey.GetSenderMessageKey();
					var ciphertext     = GetCipherText(senderKey.Iv, senderKey.CipherKey, paddedPlaintext);

					var senderKeyMessage = new SenderKeyMessage(senderKeyState.KeyId,
					                                            senderKey.Iteration,
					                                            ciphertext,
					                                            senderKeyState.SigningKeyPrivate);

					senderKeyState.SenderChainKey = senderKeyState.SenderChainKey.GetNext();
					_senderKeyStore.StoreSenderKey(_senderKeyId, record);

					return senderKeyMessage.Serialize();
	//			} catch (InvalidKeyIdException e) {
	//				throw new NoSessionException(e);
	//			}
			}
		}

		public byte[] Decrypt(byte[] senderKeyMessageBytes, IDecryprionCallback callback)
		{
			//TODO Try/catch and stuff
			lock (LOCK) {
				var record = _senderKeyStore.LoadSenderKey(_senderKeyId);

				if (record.IsEmpty) {
					throw new Exception("No sender key for: " + _senderKeyId);
				}

				var senderKeyMessage = new SenderKeyMessage(senderKeyMessageBytes);
				var senderKeyState   = record.GetSenderKeyState(senderKeyMessage.KeyId);

				senderKeyMessage.VerifySignature(senderKeyState.SigningKeyPublic);

				var senderKey = GetSenderKey(senderKeyState, senderKeyMessage.Iteration);

				byte[] plaintext = GetPlainText(senderKey.Iv, senderKey.CipherKey, senderKeyMessage.CipherText);

				callback.HandlePlaintext(plaintext);

				_senderKeyStore.StoreSenderKey(_senderKeyId, record);

				return plaintext;
			}
		}

		byte[] GetCipherText (byte[] iv, byte[] cipherKey, byte[] paddedPlaintext)
		{
			throw new NotImplementedException ();
		}

		private SenderMessageKey GetSenderKey (SenderKeyState senderKeyState, int iteration)
		{
			var senderChainKey = senderKeyState.SenderChainKey;

			if (senderChainKey.Iteration > iteration) {
				if (senderKeyState.HasSenderMessageKey(iteration)) {
					return senderKeyState.RemoveSenderMessageKey(iteration);
				} else {
					throw new Exception("Received message with old counter: " +
					                                    senderChainKey.Iteration + " , " + iteration);
				}
			}

			if (senderChainKey.Iteration - iteration > 2000) {
				throw new Exception("Over 2000 messages into the future!");
			}

			while (senderChainKey.Iteration < iteration) {
				senderKeyState.AddSenderMessageKey(senderChainKey.GetSenderMessageKey());
				senderChainKey = senderChainKey.GetNext();
			}

			senderKeyState.SenderChainKey = senderChainKey.GetNext();
			return senderChainKey.GetSenderMessageKey();
		}

		private byte[] GetPlainText (byte[] iv, byte[] key, byte[] ciphertext)
		{
			throw new NotImplementedException ();
		}
	}
}

