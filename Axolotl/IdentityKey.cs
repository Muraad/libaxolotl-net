using Axolotl.ECC;
using Axolotl.Util;

namespace Axolotl
{
	public class IdentityKey
	{
		public ECPublicKey PublicKey { get; private set; }

		public string FingerPrint
		{
			get
			{ 
				return Hex.ToString(PublicKey.Serialize());
			}
		}

		public IdentityKey(ECPublicKey publicKey)
		{
			PublicKey = publicKey;
		}

		public IdentityKey(byte[] bytes, int offset)
		{
			PublicKey = Curve.DecodePoint(bytes, offset);
		}

		public byte[] Serialize()
		{
			return PublicKey.Serialize();
		}

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			if(obj.GetType() != typeof(IdentityKey))
				return false;

			var res = PublicKey.Equals(((IdentityKey)obj).PublicKey);
			return res;
		}

		public override int GetHashCode()
		{
			return PublicKey.GetHashCode();
		}
	}
}

