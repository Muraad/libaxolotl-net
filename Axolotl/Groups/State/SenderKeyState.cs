using System;
using System.Collections.Generic;
using Axolotl.State;
using Axolotl.ECC;
using Axolotl.Groups.Ratchet;
using System.Linq;

namespace Axolotl.Groups.State
{
	public class SenderKeyState
	{
		public SenderKeyStateStructure Structure { get; private set; }

		public uint KeyId {
			get { return Structure.SenderKeyId; }
		}

		public SenderChainKey SenderChainKey {
			get {
				return new SenderChainKey ((int)Structure.senderChainKey.Iteration,
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

		public SenderKeyState (int id, int iteration, byte[] chainKey, ECPublicKey signatureKey)
			: this(id, iteration, chainKey, signatureKey, null)
		{
		}

		public SenderKeyState (int id, int iteration, byte[] chainKey, ECKeyPair signatureKey)
			: this(id, iteration, chainKey, signatureKey.PublicKey, signatureKey.PrivateKey)
		{
		}

		private SenderKeyState(int id, int iteration, byte[] chainKey,
		                       ECPublicKey signatureKeyPublic,
		                       ECPrivateKey signatureKeyPrivate = null)
		{
			var senderChainKeyStructure = new SenderKeyStateStructure.SenderChainKey {
				Iteration = (uint)iteration,
				Seed = chainKey
			};

			var signingKeyStructure = new SenderKeyStateStructure.SenderSigningKey {
				PublicKey = signatureKeyPublic.Serialize()
			};

			if (signatureKeyPrivate != null) {
				signingKeyStructure.PrivateKey = signatureKeyPrivate.Serialize ();
			}

			Structure = new SenderKeyStateStructure {
				SenderKeyId = (uint)id,
				senderChainKey = senderChainKeyStructure,
				senderSigningKey = signingKeyStructure
			};
		}

		public bool HasSenderMessageKey(int iteration)
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

		public SenderMessageKey RemoveSenderMessageKey(int iteration)
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
				return new SenderMessageKey ((int)result.Iteration, result.Seed);
			} else {
				return null;
			}
		}
	}
}

