using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Linq;

namespace Axolotl.State
{
	// Complete 

	public class SessionRecord
	{
		const int ARCHIVED_STATES_MAX_LENGTH = 40;

		public SessionState SessionState { get; private set; }

		private LinkedList<SessionState> _previousStates = new LinkedList<SessionState> ();

		public bool IsFresh { get; private set; }

		public SessionRecord ()
		{
			SessionState = new SessionState();

			IsFresh = true;
		}

		public SessionRecord (byte[] serialized)
		{
			RecordStructure record;

			using (var stream = new MemoryStream()) {
				stream.Write (serialized, 0, serialized.Length);
				record = Serializer.Deserialize<RecordStructure> (stream);
			}

			SessionState = new SessionState (record.CurrentSession);
			IsFresh = false;

			foreach (var previousStructure in record.PreviousSessions) {
				_previousStates.AddLast(new SessionState(previousStructure));
			}
		}

		public bool HasSessionState(int version, byte[] aliceBaseKey)
		{
			if (SessionState.Version == version &&
				Array.Equals (aliceBaseKey, SessionState.AliceBaseKey)) {
				return true;
			}

			foreach (var state in _previousStates) {
				if (state.Version == version &&
				    Array.Equals (aliceBaseKey, state.AliceBaseKey)) {
					return true;
				}
			}

			return false;
		}

		public void ArchiveCurrentState()
		{
			PromoteState (new SessionState ());
		}

		public void PromoteState (SessionState promotedState)
		{
			_previousStates.AddFirst (SessionState);
			SessionState = promotedState;

			if (_previousStates.Count > ARCHIVED_STATES_MAX_LENGTH) {
				_previousStates.RemoveLast ();
			}
		}

		public void SetState (SessionState sessionState)
		{
			SessionState = sessionState;
		}

		public byte[] Serialize()
		{
			var previousStructures = new LinkedList<SessionStructure> ();

			foreach (var previousState in _previousStates) {
				previousStructures.AddLast (previousState.Structure);
			}

			var record = new RecordStructure { 
				CurrentSession = SessionState.Structure,
				PreviousSessions = previousStructures.ToList()
			};

			byte[] serialized;
			using (var stream = new MemoryStream()) {
				Serializer.Serialize<RecordStructure> (stream, record);
				serialized = stream.GetBuffer ();
			}
			return serialized;
		}
	}
}

