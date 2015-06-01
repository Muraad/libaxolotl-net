using System;
using Axolotl.ECC;

namespace Axolotl.State
{
	public class StorageProtos {

		public class IdentityKeyPairStructure
		{
			public object PublicKey {
				get;
				set;
			}

			public static IdentityKeyPairStructure ParseFrom (byte[] serialized)
			{
				throw new NotImplementedException ();
			}
		}

	}
}

