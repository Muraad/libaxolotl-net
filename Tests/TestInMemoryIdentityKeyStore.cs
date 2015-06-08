using NUnit.Framework;
using System;
using Axolotl;
using Axolotl.State;
using Axolotl.ECC;
using Axolotl.Protocol;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Tests
{
	public class TestInMemoryIdentityKeyStore : IIdentityKeyStore
	{
		public void SaveIdentity(string name, IdentityKey identityKey){
			throw new NotImplementedException ();
		}
		public bool IsTrustedIdentity(string name, IdentityKey identityKey){
			throw new NotImplementedException ();
		}
		public IdentityKeyPair GetIdentityKeyPair(){
			throw new NotImplementedException ();
		}
		public UInt32 GetLocalRegistrationId (){
			throw new NotImplementedException ();
		}
	}
}

