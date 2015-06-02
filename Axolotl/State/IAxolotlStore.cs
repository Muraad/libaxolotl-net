using System;

namespace Axolotl.State
{
	public interface IAxolotlStore : IIdentityKeyStore, IPreKeyStore, ISessionStore, ISignedPreKeyStore
	{
	}
}

