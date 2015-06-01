using System;
using Axolotl.Util;
using System.Security.Cryptography;
using Mono.Math;

namespace Axolotl.ECC
{
	public class DjbECPublicKey : ECPublicKey
	{
		public byte[] PublicKey { get; private set; }

		public DjbECPublicKey (byte[] publicKey)
		{
			PublicKey = publicKey;
		}

		public override byte[] Serialize ()
		{
			byte[] type = { Curve.DJB_TYPE };
			return ByteUtil.Combine(type, PublicKey);
		}

		public override int GetKeyType ()
		{
			return Curve.DJB_TYPE;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType () != typeof(DjbECPublicKey))
				return false;

			var that = (DjbECPublicKey)obj;
			return Array.Equals (PublicKey, that.PublicKey);
		}

		// TODO: implement GetHashCode and comparators
		//@Override
		//	public int hashCode() {
		//	return Arrays.hashCode(publicKey);
		//}

		public override int CompareTo(ECPublicKey another)
		{
			return (int)new BigInteger (PublicKey).Compare (new BigInteger (((DjbECPublicKey)another).PublicKey));
		}
	}
}

