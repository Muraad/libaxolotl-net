using System;
using Axolotl.Groups.State;
using Axolotl.Groups.Ratchet;

namespace Axolotl.Protocol
{   // UNDONE
	public class SenderKeyMessage
	{
		public byte[] CipherText {
			get;
			set;
		}

		public int Iteration {
			get;
			set;
		}

		public int KeyId {
			get;
			set;
		}

		public SenderKeyMessage (byte[] senderKeyMessageBytes)
		{
			throw new NotImplementedException ();
		}

		public SenderKeyMessage (object par, object par2, byte[] ciphertext, object par3)
		{
			throw new NotImplementedException ();
		}

		public byte[] Serialize ()
		{
			throw new NotImplementedException ();
		}

		public void VerifySignature (object par)
		{
			throw new NotImplementedException ();
		}
	}
}

