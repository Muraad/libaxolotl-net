using System;
using Axolotl.Ratchet;
using Axolotl.KDF;
using Axolotl.ECC;
using Axolotl.SessionStructure;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Functional.Maybe;

namespace Axolotl.State
{
	public class SessionState
	{
		private UInt32 _sessionVersion;
		private IdentityKey _remoteIdentityKey;
		private IdentityKey _localIdentityKey;

		public UInt32 SessionVersion
		{ 
			get
			{
				var sVersion = Structure.SessionVersion.Value;
				return sVersion == 0 ? 2 : sVersion;
			} 
			set
			{
				_sessionVersion = value;
			} 
		}

		public IdentityKey RemoteIdentityKey
		{ 
			get
			{
				//TODO: check it right and add exceptions
				if(Structure.RemoteIdentityPublic == null)
					return null;
				return new IdentityKey(Structure.RemoteIdentityPublic, 0);
			}
			set
			{
				_remoteIdentityKey = value;
			}
		}


		public IdentityKey LocalIdentityKey
		{ 
			get
			{
				return new IdentityKey(Structure.LocalIdentityPublic, 0);
			} 
			set
			{
				_localIdentityKey = value;
			}
		}

		public byte[] AliceBaseKey { get; set; }

		public SessionStructure Structure { get; private set; }

		public UInt32 PreviousCounter
		{
			get { return Structure.PreviousCounter.Value; }
			set { Structure.PreviousCounter = value; }
		}

		public RootKey RootKey
		{
			get { return new RootKey(HKDF.CreateFor(SessionVersion), Structure.RootKey); }
			set { Structure.RootKey = value.Key; }
		}

		public ECPublicKey SenderRatchetKey
		{
			get
			{
				return Curve.DecodePoint(Structure.SenderChain.SenderRatchetKey, 0);
			}
		}

		public ECKeyPair SenderRatchetKeyPair
		{
			get
			{
				var pKey = SenderRatchetKey;
				var sKey = Curve.DecodePrivatePoint(Structure.SenderChain.SenderRatchetKeyPrivate);
				return new ECKeyPair(sKey, pKey);
			}
		}

		public ChainKey SenderChainKey
		{
			get
			{
				var chainKeyStructure = Structure.SenderChain.chainKey;
				return new ChainKey(HKDF.CreateFor(SessionVersion),
					chainKeyStructure.key,
					(int)chainKeyStructure.index);
			}
			set
			{
				var chainKey = new Chain.ChainKey { 
					key = value.Key,
					index = (UInt32)value.Index
				};
				Structure.SenderChain.chainKey = chainKey;
			}
		}

		public UInt32 PendingKeyExchangeSequence
		{
			get { return Structure.PendKeyExchange.Sequence.Value; }
		}

		public ECKeyPair PendingKeyExchangeBaseKey
		{
			get
			{
				var publicKey = Curve.DecodePoint(Structure.PendKeyExchange.LocalBaseKey, 0);
				var privateKey = Curve.DecodePrivatePoint(Structure.PendKeyExchange.LocalBaseKeyPrivate);
				return new ECKeyPair(privateKey, publicKey);
			}
		}

		public ECKeyPair PendingKeyExchangeRatchetKey
		{
			get
			{
				var publicKey = Curve.DecodePoint(Structure.PendKeyExchange.LocalRatchetKey, 0);
				var privateKey = Curve.DecodePrivatePoint(Structure.PendKeyExchange.LocalRatchetKeyPrivate);
				return new ECKeyPair(privateKey, publicKey);
			}
		}

		public IdentityKeyPair PendingKeyExchangeIdentityKey
		{
			get
			{
				var publicKey = new IdentityKey(Structure.PendKeyExchange.LocalIdentityKey, 0);
				var privateKey = Curve.DecodePrivatePoint(Structure.PendKeyExchange.LocalIdentityKeyPrivate);
				return new IdentityKeyPair(publicKey, privateKey);
			}
		}

		public bool HasPendingKeyExchange
		{			
			get
			{
				// TODO: Check
				return Structure.PendKeyExchange != null;
			}
		}

		public bool HasUnacknowledgedPreKeyMessage()
		{
			// TODO: Check
			return Structure.PendPreKey != null;
		}

		public UInt32 RemoteRegistrationId
		{
			// TODO: Check
			get { return Structure.RemoteRegistrationId.Value; }
			set { Structure.RemoteRegistrationId = value; }
		}

		public UInt32 LocalRegistrationId
		{
			get { return Structure.LocalRegistrationId.Value; }
			set { Structure.LocalRegistrationId = value; }
		}

		public SessionState()
		{
			Structure = new SessionStructure();
		}

		public SessionState(SessionStructure sessionStructure)
		{
			Structure = sessionStructure;
		}

		public SessionState(SessionState copy)
		{
			Structure = copy.Structure;
		}

		public bool HasReceiverChain(ECPublicKey senderEphemeral)
		{
			return GetReceiverChain(senderEphemeral) != null;
		}

		public bool HasSenderChain()
		{
			return Structure.SenderChain != null;
		}

		private Tuple<Chain, int> GetReceiverChain(ECPublicKey senderEphemeral)
		{
			List<Chain> ReceiverChains = Structure.ReceiverChains;
			int index = 0;

			foreach(var chain in ReceiverChains)
			{
				// TODO: add exceptions
				ECPublicKey chainSenderRatchetKey = Curve.DecodePoint(chain.SenderRatchetKey, 0);

				if(chainSenderRatchetKey.Equals(senderEphemeral))
					return new Tuple<Chain, int>(chain, index);

				index++;
			}

			return null;
		}

		public ChainKey GetReceiverChainKey(ECPublicKey senderEphemeral)
		{
			Tuple<Chain, int> ReceiverChainAndIndex = GetReceiverChain(senderEphemeral);
			var ReceiverChain = ReceiverChainAndIndex.Item1;

			if(ReceiverChain == null)
			{
				return null;
			}
			else
			{
				return new ChainKey(HKDF.CreateFor(SessionVersion),
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
				SenderRatchetKey = senderRatchetKey.Serialize()
			};

			Structure.ReceiverChains.Add(chain);

			if(Structure.ReceiverChains.Count > 5)
			{
				Structure.ReceiverChains.RemoveAt(0);
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

			if(chain == null)
			{
				return false;
			}

			List<Chain.MessageKey> messageKeyList = chain.messageKeys;

			foreach(var mKey in messageKeyList)
			{
				if(mKey.index == counter)
					return true;
			}

			return false;
		}

		public MessageKeys RemoveMessageKeys(ECPublicKey senderEphemeral, int counter)
		{
			// UNDONE
			throw new NotImplementedException();
		}

		public void SetMessageKeys(ECPublicKey senderEphemeral, MessageKeys messageKeys)
		{

			var chainAndIndex = GetReceiverChain(senderEphemeral);
			var chain = chainAndIndex.Item1;
			var messageKeyStructure = new Chain.MessageKey {
				cipherKey = messageKeys.CipherKey,
				macKey = messageKeys.MacKey,
				index = messageKeys.Counter,
				iv = messageKeys.Iv
			};
			chain.messageKeys.Add(messageKeyStructure);

			// TODO

			//			this.sessionStructure = this.sessionStructure.toBuilder()
			//				.setReceiverChains(chainAndIndex.second(), updatedChain)
			//				.build();
		}

		public void SetReceiverChainKey(ECPublicKey senderEphemeral, ChainKey chainKey)
		{

			var chainAndIndex = GetReceiverChain(senderEphemeral);
			var chain = chainAndIndex.Item1;
			var chainKeyStructure = new Chain.ChainKey {
				key = chainKey.Key,
				index = (UInt32)chainKey.Index
			};
			chain.chainKey = chainKeyStructure;


			// TODO
			//			this.sessionStructure = this.sessionStructure.toBuilder()
			//				.setReceiverChains(chainAndIndex.second(), updatedChain)
			//				.build();
		}

		public void setPendingKeyExchange(int sequence,
		                                  ECKeyPair ourBaseKey,
		                                  ECKeyPair ourRatchetKey,
		                                  IdentityKeyPair ourIdentityKey)
		{
			var structure = new PendingKeyExchange {
				Sequence = (UInt32)sequence,
				LocalBaseKey = ourBaseKey.PublicKey.Serialize(),
				LocalBaseKeyPrivate = ourBaseKey.PrivateKey.Serialize(),
				LocalRatchetKey = ourRatchetKey.PublicKey.Serialize(),
				LocalRatchetKeyPrivate = ourRatchetKey.PrivateKey.Serialize(),
				LocalIdentityKey = ourIdentityKey.PublicKey.Serialize(),
				LocalIdentityKeyPrivate = ourIdentityKey.PrivateKey.Serialize()
			};

			Structure.PendKeyExchange = structure;
		}

		public void SetUnacknowledgedPreKeyMessage(Maybe<UInt32> preKeyId, int signedPreKeyId, ECPublicKey baseKey)
		{
			// TODO: check
			var pending = new PendingPreKey {
				signedPreKeyId = signedPreKeyId,
				baseKey = baseKey.Serialize()
			};
			preKeyId.Do (pKid => pending.preKeyId = pKid);

			Structure.PendPreKey = pending;
		}

		// UNDONE
		public UnacknowledgedPreKeyMessageItems getUnacknowledgedPreKeyMessageItems()
		{
			Maybe<UInt32> preKeyId;

			if(Structure.PendPreKey.preKeyId.HasValue)
			{
				preKeyId = Structure.PendPreKey.preKeyId.ToMaybe ();
			}
			else
			{
				preKeyId = Maybe<UInt32>.Nothing; 
			}

			return new UnacknowledgedPreKeyMessageItems(preKeyId, Structure.PendPreKey.signedPreKeyId.Value, Curve.DecodePoint(Structure.PendPreKey.baseKey, 0));
		}

		public void ClearUnacknowledgedPreKeyMessage()
		{
			// TODO: make normal clearing
			Structure.PendPreKey = null;
		}

		public byte[] Serialize()
		{
			using(var stream = new MemoryStream())
			{
				//TODO: check
				Serializer.Serialize<SessionStructure>(stream, Structure);
				return stream.ToArray ();
			}
		}

		public class UnacknowledgedPreKeyMessageItems
		{
			public Maybe<UInt32> PreKeyId { get; private set; }

			public int SignedPreKeyId { get; private set; }

			public ECPublicKey BaseKey { get; private set; }

			public UnacknowledgedPreKeyMessageItems(Maybe<UInt32> preKeyId,
			                                        int signedPreKeyId,
			                                        ECPublicKey baseKey)
			{
				PreKeyId = preKeyId;
				SignedPreKeyId = signedPreKeyId;
				BaseKey = baseKey;
			}
		}
	}
}

