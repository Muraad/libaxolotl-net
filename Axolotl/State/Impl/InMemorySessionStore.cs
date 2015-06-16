using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Axolotl.State
{
	public class InMemorySessionStore : ISessionStore
	{
		Dictionary<AxolotlAddress, byte[]> _sessions;

		public InMemorySessionStore ()
		{
			_sessions = new Dictionary<AxolotlAddress, byte[]> ();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public SessionRecord LoadSession(AxolotlAddress axolotlAddress)
		{
			// TODO: add checks
			if (ContainsSession (axolotlAddress)) {
				return new SessionRecord (_sessions [axolotlAddress]);
			} else {
				return new SessionRecord ();
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public List<UInt32> GetSubDeviceSessions(string name)
		{
			var deviceIds = new List<UInt32>();

			foreach (var key in _sessions.Keys) {
				if (key.Name.Equals(name) &&
					key.DeviceID != 1)
				{
					deviceIds.Add (key.DeviceID);
				}
			}

			return deviceIds;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool ContainsSession(AxolotlAddress axolotlAddress)
		{
			return _sessions.ContainsKey (axolotlAddress);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void StoreSession(AxolotlAddress address, SessionRecord record) {
			_sessions.Add(address, record.Serialize());
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void DeleteSession(AxolotlAddress address) {
			_sessions.Remove(address);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void DeleteAllSessions(string name) {
			foreach(var key in _sessions.Keys)
			{
				if(key.Name.Equals(name))
					_sessions.Remove(key);
			}
		}
	}
}

