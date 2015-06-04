using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	// Complete
	public interface ISessionStore
	{
		SessionRecord LoadSession(AxolotlAddress address);

		List<int> GetSubDeviceSessions(string name);

		bool ContainsSession(AxolotlAddress address);

		void DeleteSession(AxolotlAddress address);

		void DeleteAllSessions(string name);

		void StoreSession(AxolotlAddress address, SessionRecord record);
	}
}

