using System;
using System.Collections.Generic;
using Axolotl.State;
using Axolotl.ECC;
using Axolotl.Groups.Ratchet;
using System.Linq;
using Functional.Maybe;

namespace Axolotl.Groups.State
{
	public class SenderKeyState
	{
		// Completed

		public SenderKeyStateStructure Structure { get; private set; }

		public UInt32 KeyId {
			get { return Structure.SenderKeyId.Value; }
		}

		public SenderChainKey SenderChainKey {
			get {
				return new SenderChainKey (Structure.senderChainKey.Iteration.Value,
				                           Structure.senderChainKey.Seed);
			}
			set {
				var senderCkStruct = new SenderKeyStateStructure.SenderChainKey {
					Iteration = (uint)value.Iteration,
					Seed = value.Seed
				};
				Structure.senderChainKey = senderCkStruct;
			}
		}

		public ECPublicKey SigningKeyPublic {
			get { 
				return Curve.DecodePoint (Structure.senderSigningKey.PublicKey, 0);
			}
		}

		public ECPrivateKey SigningKeyPrivate {
			get {
				return Curve.DecodePrivatePoint (Structure.senderSigningKey.PrivateKey);
			}
		}

		public SenderKeyState (SenderKeyStateStructure structure)
		{
			Structure = structure;
		}

		public SenderKeyState (UInt32 id, UInt32 iteration, byte[] chainKey, ECPublicKey signatureKey)
			: this(id, iteration, chainKey, signatureKey, Maybe<ECPrivateKey>.Nothing)
		{
		}

		public SenderKeyState (UInt32 id, UInt32 iteration, byte[] chainKey, ECKeyPair signatureKey)
			: this(id, iteration, chainKey, signatureKey.PublicKey, signatureKey.PrivateKey.ToMaybe())
		{
		}

		private SenderKeyState(UInt32 id, UInt32 iteration, byte[] chainKey,
		                       ECPublicKey signatureKeyPublic,
		                       Maybe<ECPrivateKey> signatureKeyPrivate)
		{
			var senderChainKeyStructure = new SenderKeyStateStructure.SenderChainKey {
				Iteration = (uint)iteration,
				Seed = chainKey
			};

			var signingKeyStructure = new SenderKeyStateStructure.SenderSigningKey {
				PublicKey = signatureKeyPublic.Serialize()
			};

			signatureKeyPrivate.Do (SKp => {
				signingKeyStructure.PrivateKey = SKp.Serialize();
			});
		 
			Structure = new SenderKeyStateStructure {
				SenderKeyId = (uint)id,
				senderChainKey = senderChainKeyStructure,
				senderSigningKey = signingKeyStructure
			};
		}

		public bool HasSenderMessageKey(UInt32 iteration)
		{
			return Structure.senderMessageKeys.Any (x => x.Iteration == iteration);
		}

		public void AddSenderMessageKey(SenderMessageKey senderMessageKey) 
		{
			var senderMessageKeyStructure = new SenderKeyStateStructure.SenderMessageKey {
				Iteration = (uint)senderMessageKey.Iteration,
				Seed = senderMessageKey.Seed
			};

			Structure.senderMessageKeys.Add (senderMessageKeyStructure);
		}

		public SenderMessageKey RemoveSenderMessageKey(UInt32 iteration)
		{
			var keys = new List<SenderKeyStateStructure.SenderMessageKey>(Structure.senderMessageKeys);

			SenderKeyStateStructure.SenderMessageKey result = null;

			//TODO: replace with LINQ
			foreach (var key in keys) {
				if (key.Iteration == iteration) {
					result = key;
					keys.Remove (key);
					break;
				}
			}

			if (result != null) {
				Structure.senderMessageKeys = keys;
				return new SenderMessageKey (result.Iteration.Value, result.Seed);
			} else {
				return null;
			}
		}
	}
}

