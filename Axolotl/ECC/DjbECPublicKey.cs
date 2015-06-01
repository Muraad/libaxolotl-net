using System;
using Axolotl.Util;
using System.Security.Cryptography;

namespace Axolotl.ECC
{
	public class DjbECPublicKey : ECPublicKey
	{
		private byte[] _publicKey;

		public DjbECPublicKey (byte[] publicKey)
		{
			_publicKey = publicKey;
		}

		public override byte[] Serialize ()
		{
			byte[] type = { Curve.DJB_TYPE };
			return ByteUtil.Combine(type, _publicKey);
		}

		public override int GetType ()
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
			return Array.Equals (_publicKey, that._publicKey);
		}

		// TODO: implement GetHashCode and comparators
		//@Override
		//	public int hashCode() {
		//	return Arrays.hashCode(publicKey);
		//}

		//@Override
		//	public int compareTo(ECPublicKey another) {
		//	return new BigInteger(publicKey).compareTo(new BigInteger(((DjbECPublicKey)another).publicKey));
		//}

		//public byte[] getPublicKey() {
		//	return publicKey;
		//}
	}
}

