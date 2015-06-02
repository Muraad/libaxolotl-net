using System;

namespace Axolotl.State
{
	public interface IIdentityKeyStore
	{
		//Complete

		void SaveIdentity(string name, IdentityKey identityKey);
		bool IsTrustedIdentity(string name, IdentityKey identityKey);
	}
}

