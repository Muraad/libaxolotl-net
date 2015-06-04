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

		public List<SessionState> PreviousStates { get; private set; }

		public bool IsFresh { get; private set; }

		public SessionRecord()
		{

			PreviousStates = new List<SessionState>();

			SessionState = new SessionState();

			IsFresh = true;
		}

		public SessionRecord(byte[] serialized)
		{
			PreviousStates = new List<SessionState>();

			RecordStructure record;

			using(var stream = new MemoryStream())
			{
				stream.Write(serialized, 0, serialized.Length);
				record = Serializer.Deserialize<RecordStructure>(stream);
			}

			SessionState = new SessionState(record.CurrentSession);
			IsFresh = false;

			foreach(var previousStructure in record.PreviousSessions)
			{
				PreviousStates.Add(new SessionState(previousStructure));
			}
		}

		public bool HasSessionState(UInt32 version, byte[] aliceBaseKey)
		{
			if(SessionState.SessionVersion == version &&
			   Array.Equals(aliceBaseKey, SessionState.AliceBaseKey))
			{
				return true;
			}

			foreach(var state in PreviousStates)
			{
				if(state.SessionVersion == version &&
				   Array.Equals(aliceBaseKey, state.AliceBaseKey))
				{
					return true;
				}
			}

			return false;
		}

		public void ArchiveCurrentState()
		{
			PromoteState(new SessionState());
		}

		public void PromoteState(SessionState promotedState)
		{
			PreviousStates.Insert(0, SessionState);
			SessionState = promotedState;

			if(PreviousStates.Count > ARCHIVED_STATES_MAX_LENGTH)
			{
				PreviousStates.RemoveAt(PreviousStates.Count - 1);
			}
		}

		public void SetState(SessionState sessionState)
		{
			SessionState = sessionState;
		}

		public byte[] Serialize()
		{
			var previousStructures = new LinkedList<SessionStructure>();

			foreach(var previousState in PreviousStates)
			{
				previousStructures.AddLast(previousState.Structure);
			}

			var record = new RecordStructure { 
				CurrentSession = SessionState.Structure,
				PreviousSessions = previousStructures.ToList()
			};

			byte[] serialized;
			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<RecordStructure>(stream, record);
				serialized = stream.ToArray();
			}
			return serialized;
		}
	}
}

