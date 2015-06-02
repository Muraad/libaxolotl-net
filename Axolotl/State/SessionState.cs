using System;
using Axolotl.Ratchet;
using Axolotl.KDF;
using Axolotl.ECC;
using Axolotl.SessionStructure;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class SessionState
	{
		private UInt32 _sessionVersion;
		private IdentityKey _remoteIdentityKey;
		private IdentityKey _localIdentityKey;

		public UInt32 SessionVersion { 
			get {
				var sVersion = Structure.SessionVersion;
				return sVersion == 0 ?  2 : sVersion;
			} 
			set {
				_sessionVersion = value;
			} 
		}

		public IdentityKey RemoteIdentityKey { 
			get {
				//TODO: check it right and add exceptions
				if (Structure.RemoteIdentityPublic == null)
					return null;
				return new IdentityKey (Structure.RemoteIdentityPublic, 0);
			}
			set {
				_remoteIdentityKey = value;
			}
		}


		public IdentityKey LocalIdentityKey { 
			get {
				return new IdentityKey (Structure.LocalIdentityPublic, 0);
			} 
			set {
				_localIdentityKey = value;
			}
		}

		public byte[] AliceBaseKey { get; set; }

		public SessionStructure Structure { get; private set; }

		public UInt32 PreviousCounter {
			get { return Structure.PreviousCounter; }
			set { Structure.PreviousCounter = (UInt32)value; }
		}

		public RootKey RKey {
			get { return new RootKey (HKDF.CreateFor ((int)SessionVersion), Structure.RootKey); }
			set { Structure.RootKey = value.Key; }
		}

		public ECPublicKey SenderRatchetKey {
			get {
				return Curve.DecodePoint (Structure.SenderChain.SenderRatchetKey, 0);
			}
		}

		public ECKeyPair SenderRatchetKeyPair {
			get {
				var pKey = SenderRatchetKey;
				var sKey = Curve.DecodePrivatePoint (Structure.SenderChain.SenderRatchetKeyPrivate);
				return new ECKeyPair (sKey, pKey);
			}
		}

		public ChainKey SenderChainKey {
			get {
				var chainKeyStructure = Structure.SenderChain.chainKey;
				return new ChainKey (HKDF.CreateFor ((int)SessionVersion),
				                    chainKeyStructure.key,
				                    (int)chainKeyStructure.index);
			}
			set {
				var chainKey = new Chain.ChainKey { 
					key = value.Key,
					index = (UInt32)value.Index
				};
				Structure.SenderChain.chainKey = chainKey;
			}
		}

		public SessionState ()
		{
			Structure = new SessionStructure ();
		}

		public SessionState (SessionStructure sessionStructure)
		{
			Structure = sessionStructure;
		}

		public SessionState (SessionState copy)
		{
			Structure = copy.Structure;
		}

		public bool HasReceiverChain(ECPublicKey senderEphemeral)
		{
			return GetReceiverChain (senderEphemeral) != null;
		}

		public bool HasSenderChain()
		{
			return Structure.SenderChain != null;
		}

		private Tuple<Chain, int> GetReceiverChain (ECPublicKey senderEphemeral)
		{
			List<Chain> ReceiverChains = Structure.ReceiverChains;
			int index = 0;

			foreach (var chain in ReceiverChains) {
				// TODO: add exceptions
				ECPublicKey chainSenderRatchetKey = Curve.DecodePoint (chain.SenderRatchetKey, 0);

				if (chainSenderRatchetKey.Equals (senderEphemeral))
					return new Tuple<Chain, int> (chain, index);

				index++;
			}

			return null;
		}

		public ChainKey GetReceiverChainKey(ECPublicKey senderEphemeral)
		{
			Tuple<Chain, int> ReceiverChainAndIndex = GetReceiverChain (senderEphemeral);
			var ReceiverChain = ReceiverChainAndIndex.Item1;

			if (ReceiverChain == null) {
				return null;
			} else {
				return new ChainKey (HKDF.CreateFor ((int)SessionVersion),
				                    ReceiverChain.chainKey.key,
				                    (int)ReceiverChain.chainKey.index);
			}
		}

		public void AddReceiverChain(ECPublicKey senderRatchetKey, ChainKey chainKey)
		{
			var chainKeyStructure = new Chain.ChainKey {
				key = chainKey.Key,
				index = (UInt32)chainKey.Index
			};

			var chain = new Chain {
				chainKey = chainKeyStructure,
				SenderRatchetKey =  senderRatchetKey.Serialize()
			};

			Structure.ReceiverChains.Add (chain);

			if (Structure.ReceiverChains.Count > 5) {
				Structure.ReceiverChains.RemoveAt (0);
			}
		}

		public void SetSenderChain(ECKeyPair senderRatchetKeyPair, ChainKey chainKey)
		{
			var chainKeyStructure = new Chain.ChainKey {
				key = chainKey.Key,
				index = (UInt32)chainKey.Index
			};

			var senderChain = new Chain { 
				SenderRatchetKey = senderRatchetKeyPair.PublicKey.Serialize(),
				SenderRatchetKeyPrivate = senderRatchetKeyPair.PrivateKey.Serialize(),
				chainKey = chainKeyStructure
			};

			Structure.SenderChain = senderChain;
		}

		public bool HasMessageKeys(ECPublicKey senderEphemeral, int counter) 
		{
			var chainAndIndex = GetReceiverChain(senderEphemeral);
			var chain = chainAndIndex.Item1;

			if (chain == null) {
				return false;
			}

			List<Chain.MessageKey> messageKeyList = chain.messageKeys;

			foreach (var mkey in messageKeyList) {
				if (mkey.index == counter)
					return true;
			}

			return false;
		}

		//public MessageKeys RemoveMessageKeys(ECPublicKey senderEphemeral, int counter) 
		//{
		//	var chainAndIndex = GetReceiverChain(senderEphemeral);
		//	Chain chain = chainAndIndex.Item1;
		//
		//	if (chain == null) {
		//		return null;
		//	}
		//
		//	var messageKeyList = new LinkedList<Chain.MessageKey> (chain.messageKeys);
		//	var mKeyEnumerator = messageKeyList.GetEnumerator ();
			//Iterator<Chain.MessageKey> messageKeyIterator = messageKeyList.iterator();
			//MessageKeys result = null;

			//while (mKeyEnumerator.hasNext()) {
			//	Chain.MessageKey messageKey = messageKeyIterator.next();
			//
			//	if (messageKey.getIndex() == counter) {
			//		result = new MessageKeys(new SecretKeySpec(messageKey.getCipherKey().toByteArray(), "AES"),
			//		                         new SecretKeySpec(messageKey.getMacKey().toByteArray(), "HmacSHA256"),
			//		                         new IvParameterSpec(messageKey.getIv().toByteArray()),
			//		                         messageKey.getIndex());
			//
			//		messageKeyIterator.remove();
			//		break;
			//	}
			//}

			//Chain updatedChain = chain.toBuilder().clearMessageKeys()
			//	.addAllMessageKeys(messageKeyList)
			//		.build();
			//
			//this.sessionStructure = this.sessionStructure.toBuilder()
			//	.setReceiverChains(chainAndIndex.second(), updatedChain)
			//		.build();

			//return result;
	//	}
	}
}

