using System;
using Axolotl.Ratchet;
using Axolotl.KDF;
using Axolotl.ECC;
using Axolotl.SessionStructure;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Functional.Maybe;
using Axolotl.Logging;

namespace Axolotl.State
{
	public class SessionState
	{
		private UInt32 _sessionVersion;

		public IdentityKey GetRemoteIdentityKey()
		{
			try {
				if(Structure.RemoteIdentityPublic == null)
					return null;
				return new IdentityKey(Structure.RemoteIdentityPublic, 0);
			}
			catch(InvalidKeyException e) {
                Logger.w("SessionRecordV2", e);
				return null;
			}
		}

		public void SetRemoteIdentityKey(IdentityKey identityKey)
		{
			Structure.RemoteIdentityPublic = identityKey.Serialize ();
		}

		public IdentityKey GetLocalIdentityKey()
		{
			try {
				return new IdentityKey(Structure.LocalIdentityPublic, 0);
			}
			catch(InvalidKeyException e) {
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		public void SetLocalIdentityKey(IdentityKey identityKey)
		{
			Structure.LocalIdentityPublic = identityKey.Serialize();
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
			get { return new RootKey(HKDF.CreateFor(GetSessionVersion()), Structure.RootKey); }
			set { Structure.RootKey = value.Key; }
		}

		public ECPublicKey SenderRatchetKey
		{
			get
			{
				try
				{
					return Curve.DecodePoint(Structure.SenderChain.SenderRatchetKey, 0);
				}
				catch(InvalidKeyException e)
				{
					throw new InvalidOperationException("Assertin error", e);
				}
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

		public ChainKey GetSenderChainKey()
		{
			var chainKeyStructure = Structure.SenderChain.chainKey;
			var cK = new ChainKey(HKDF.CreateFor(GetSessionVersion()),
			                     chainKeyStructure.key,
			                     chainKeyStructure.index); 
			return cK;
		}

		public void SetSenderChainKey(ChainKey senderChainKey)
		{
			var chainKey = new Chain.ChainKey { 
				key = senderChainKey.Key,
				index = senderChainKey.Index
			};
			Structure.SenderChain.chainKey = chainKey;
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
				// TODO: Check~
				return Structure.PendKeyExchange != null;
			}
		}

		public bool HasUnacknowledgedPreKeyMessage()
		{
			// TODO: Check~
			return Structure.PendPreKey != null;
		}

		public UInt32 RemoteRegistrationId
		{
			// TODO: Check~
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

		public UInt32 GetSessionVersion()
		{
			var sVersion = Structure.SessionVersion.Value;
			return sVersion == 0 ? 2 : sVersion;
		}

		public void SetSessionVersion(UInt32 version)
		{
			Structure.SessionVersion = version;
		}

		public bool HasReceiverChain(ECPublicKey senderEphemeral)
		{
			return GetReceiverChain(senderEphemeral) != null;
		}

		public bool HasSenderChain()
		{
			return Structure.SenderChain != null;
		}

		private Tuple<Chain, UInt32> GetReceiverChain(ECPublicKey senderEphemeral)
		{
			List<Chain> ReceiverChains = Structure.ReceiverChains;
			UInt32 index = 0;

			foreach(var chain in ReceiverChains)
			{
				try
				{
					ECPublicKey chainSenderRatchetKey = Curve.DecodePoint(chain.SenderRatchetKey, 0);

					if(chainSenderRatchetKey.Equals(senderEphemeral))
						return new Tuple<Chain, UInt32>(chain, index);

					index++;
				}
				catch(InvalidKeyException e)
				{
                    Logger.w("SessionRecordV2", e);
				}
			}

			return null;
		}

		public ChainKey GetReceiverChainKey(ECPublicKey senderEphemeral)
		{
			Tuple<Chain, UInt32> ReceiverChainAndIndex = GetReceiverChain(senderEphemeral);
			var ReceiverChain = ReceiverChainAndIndex.Item1;

			if(ReceiverChain == null)
			{
				return null;
			}
			else
			{
				return new ChainKey(HKDF.CreateFor(GetSessionVersion()),
					ReceiverChain.chainKey.key,
					ReceiverChain.chainKey.index);
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

		public bool HasMessageKeys(ECPublicKey senderEphemeral, UInt32 counter)
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

		public MessageKeys RemoveMessageKeys(ECPublicKey senderEphemeral, UInt32 counter)
		{
			Tuple<Chain, UInt32> chainAndIndex = GetReceiverChain(senderEphemeral);
			Chain chain = chainAndIndex.Item1;

			if(chain == null)
			{
				return null;
			}

			MessageKeys result = null;

			// TODO: Check~
			foreach(var mK in chain.messageKeys)
			{
				if(mK.index == counter)
				{
					result = new MessageKeys(mK.cipherKey, mK.macKey, mK.iv, mK.index);
					chain.messageKeys.Remove(mK);
					break;
				}
			}

			return result;
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
		}

		public void SetPendingKeyExchange(UInt32 sequence,
		                                  ECKeyPair ourBaseKey,
		                                  ECKeyPair ourRatchetKey,
		                                  IdentityKeyPair ourIdentityKey)
		{
			var structure = new PendingKeyExchange {
				Sequence = sequence,
				LocalBaseKey = ourBaseKey.PublicKey.Serialize(),
				LocalBaseKeyPrivate = ourBaseKey.PrivateKey.Serialize(),
				LocalRatchetKey = ourRatchetKey.PublicKey.Serialize(),
				LocalRatchetKeyPrivate = ourRatchetKey.PrivateKey.Serialize(),
				LocalIdentityKey = ourIdentityKey.PublicKey.Serialize(),
				LocalIdentityKeyPrivate = ourIdentityKey.PrivateKey.Serialize()
			};

			Structure.PendKeyExchange = structure;
		}

		public void SetUnacknowledgedPreKeyMessage(Maybe<UInt32> preKeyId, UInt32 signedPreKeyId, ECPublicKey baseKey)
		{
			// TODO: check~
			var pending = new PendingPreKey {
				signedPreKeyId = signedPreKeyId,
				baseKey = baseKey.Serialize()
			};
			preKeyId.Do(pKid => pending.preKeyId = pKid);

			Structure.PendPreKey = pending;
		}

		public UnacknowledgedPreKeyMessageItems GetUnacknowledgedPreKeyMessageItems()
		{
			try
			{
				Maybe<UInt32> preKeyId;

				if(Structure.PendPreKey.preKeyId.HasValue)
				{
					preKeyId = Structure.PendPreKey.preKeyId.ToMaybe();
				}
				else
				{
					preKeyId = Maybe<UInt32>.Nothing; 
				}

				return new UnacknowledgedPreKeyMessageItems(preKeyId, Structure.PendPreKey.signedPreKeyId.Value, Curve.DecodePoint(Structure.PendPreKey.baseKey, 0));
			}
			catch(InvalidKeyException e)
			{
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		public void ClearUnacknowledgedPreKeyMessage()
		{
			Structure.PendPreKey = null;
		}

		public byte[] Serialize()
		{
			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<SessionStructure>(stream, Structure);
				return stream.ToArray();
			}
		}

		public class UnacknowledgedPreKeyMessageItems
		{
			public Maybe<UInt32> PreKeyId { get; private set; }

			public UInt32 SignedPreKeyId { get; private set; }

			public ECPublicKey BaseKey { get; private set; }

			public UnacknowledgedPreKeyMessageItems(Maybe<UInt32> preKeyId,
			                                        UInt32 signedPreKeyId,
			                                        ECPublicKey baseKey)
			{
				PreKeyId = preKeyId;
				SignedPreKeyId = signedPreKeyId;
				BaseKey = baseKey;
			}
		}
	}
}

