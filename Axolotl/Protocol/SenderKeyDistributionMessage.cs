using System;
using Axolotl.Groups.State;

namespace Axolotl.Protocol
{
	public class SenderKeyDistributionMessage
	{
		// UNDONE

		public int Id {
			get;
			set;
		}

		public int Iteration {
			get;
			set;
		}

		public byte[] ChainKey {
			get;
			set;
		}

		public Axolotl.ECC.ECPublicKey SignatureKey {
			get;
			set;
		}

		public SenderKeyDistributionMessage (uint keyId, int iteration, byte[] seed, Axolotl.ECC.ECPublicKey signingKeyPublic)
		{
			throw new NotImplementedException ();
		}
	}
}

