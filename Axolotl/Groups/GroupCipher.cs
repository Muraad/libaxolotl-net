using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Axolotl.Groups.Ratchet;
using Axolotl.Groups.State;
using Axolotl.Protocol;

namespace Axolotl.Groups
{
	// Completed
	public class GroupCipher
	{
		public static object LOCK = new object();

		private ISenderKeyStore _senderKeyStore;
		private SenderKeyName _senderKeyId;

		public GroupCipher(ISenderKeyStore senderKeyStore, SenderKeyName senderKeyId)
		{
			_senderKeyStore = senderKeyStore;
			_senderKeyId = senderKeyId;
		}

		public byte[] Encrypt(byte[] paddedPlaintext)
		{
			lock(LOCK)
			{
				try {
				var record = _senderKeyStore.LoadSenderKey(_senderKeyId);
				var senderKeyState = record.GetSenderKeyState();
				var senderKey = senderKeyState.SenderChainKey.GetSenderMessageKey();
				var ciphertext = GetCipherText(senderKey.Iv, senderKey.CipherKey, paddedPlaintext);

				var senderKeyMessage = new SenderKeyMessage(senderKeyState.KeyId,
									                       senderKey.Iteration,
									                       ciphertext,
									                       senderKeyState.SigningKeyPrivate);

				senderKeyState.SenderChainKey = senderKeyState.SenderChainKey.GetNext();
				_senderKeyStore.StoreSenderKey(_senderKeyId, record);

				return senderKeyMessage.Serialize();
				} 
				catch (Exception e) 
				{
					throw new Exception("wtf :" + e);
				}
			}
		}

		public byte[] Decrypt(byte[] senderKeyMessageBytes)
		{
			return Decrypt(senderKeyMessageBytes, new NullDecryptionCallback());
		}

		public byte[] Decrypt(byte[] senderKeyMessageBytes, IDecryprionCallback callback)
		{
			//TODO Try/catch and stuff
			lock(LOCK)
			{
				var record = _senderKeyStore.LoadSenderKey(_senderKeyId);

				if(record.IsEmpty)
				{
					throw new Exception("No sender key for: " + _senderKeyId);
				}

				var senderKeyMessage = new SenderKeyMessage(senderKeyMessageBytes);
				var senderKeyState = record.GetSenderKeyState(senderKeyMessage.KeyId);

				senderKeyMessage.VerifySignature(senderKeyState.SigningKeyPublic);

				var senderKey = GetSenderKey(senderKeyState, senderKeyMessage.Iteration);

				byte[] plaintext = GetPlainText(senderKey.Iv, senderKey.CipherKey, senderKeyMessage.CipherText);

				callback.HandlePlaintext(plaintext);

				_senderKeyStore.StoreSenderKey(_senderKeyId, record);

				return plaintext;
			}
		}

		private SenderMessageKey GetSenderKey(SenderKeyState senderKeyState, UInt32 iteration)
		{
			var senderChainKey = senderKeyState.SenderChainKey;

			if(senderChainKey.Iteration > iteration)
			{
				if(senderKeyState.HasSenderMessageKey(iteration))
				{
					return senderKeyState.RemoveSenderMessageKey(iteration);
				}
				else
				{
					throw new Exception("Received message with old counter: " +
					senderChainKey.Iteration + " , " + iteration);
				}
			}

			if(senderChainKey.Iteration - iteration > 2000)
			{
				throw new Exception("Over 2000 messages into the future!");
			}

			while(senderChainKey.Iteration < iteration)
			{
				senderKeyState.AddSenderMessageKey(senderChainKey.GetSenderMessageKey());
				senderChainKey = senderChainKey.GetNext();
			}

			senderKeyState.SenderChainKey = senderChainKey.GetNext();
			return senderChainKey.GetSenderMessageKey();
		}

		private byte[] GetPlainText(byte[] iv, byte[] key, byte[] ciphertext)
		{
			byte[] result; 

			using(var rijAlg = Rijndael.Create())
			{
				rijAlg.Key = key;
				rijAlg.IV = iv;

				var decryptor = rijAlg.CreateDecryptor(key, iv);

				using(var mStream = new MemoryStream())
					using(var dcStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
						using(var sReader = new StreamReader(dcStream))
						{
							var stringText = sReader.ReadToEnd();
							result = Encoding.UTF8.GetBytes(stringText);
						}
			}

			return result;
		}

		private byte[] GetCipherText(byte[] iv, byte[] cipherKey, byte[] paddedPlaintext)
		{
			byte[] result;

			using(var aes = SymmetricAlgorithm.Create())
			{
				aes.Mode = CipherMode.CBC;
				aes.Key = cipherKey;
				aes.IV = iv;
				aes.Padding = PaddingMode.PKCS7;
				var encryptor = aes.CreateEncryptor();

				using(var mStream = new MemoryStream())
					using(var cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
					{
						cStream.Write(paddedPlaintext, 0, paddedPlaintext.Length);
						cStream.FlushFinalBlock();
						result = mStream.ToArray();
					}
			}

			return result;
		}

		private class NullDecryptionCallback : IDecryprionCallback
		{
			public void HandlePlaintext(byte[] plaintext)
			{
			}
		}
	}
}

