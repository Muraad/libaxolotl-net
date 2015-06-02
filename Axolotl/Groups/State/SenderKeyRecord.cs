using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Axolotl.State;
using Axolotl.ECC;
using System.Linq;

namespace Axolotl.Groups.State
{
	public class SenderKeyRecord
	{
		private List<SenderKeyState> _senderKeyStates = new List<SenderKeyState>();


		public bool IsEmpty {
			get { return _senderKeyStates.Count == 0; }
		}

		public SenderKeyRecord () {}

		public SenderKeyRecord (byte[] serialized)
		{
			using (var stream = new MemoryStream()) {
				stream.Write (serialized, 0, serialized.Length);
				var stateStructure = Serializer.Deserialize<SenderKeyRecordStructure> (stream);
				foreach (var structure in stateStructure.senderKeyStates) {
					_senderKeyStates.Add (new SenderKeyState (structure));
				}
			}
		}

		public SenderKeyState GetSenderKeyState()
		{
			if (!IsEmpty) {
				return _senderKeyStates [0];
			} else {
				throw new Exception ("wtf");
			}
		}

		public SenderKeyState GetSenderKeyState(int keyId)
		{
			foreach (var state in _senderKeyStates) {
				if (state.KeyId == keyId) return state;
			}

			throw new KeyNotFoundException ();
		}

		public void AddSenderKeyState(int id, int iteration, byte[] chainKey, ECPublicKey signatureKey) 
		{
			_senderKeyStates.Add(new SenderKeyState(id, iteration, chainKey, signatureKey));
		}

		public void SetSenderKeyState(int id, int iteration, byte[] chainKey, ECKeyPair signatureKey) {
			_senderKeyStates.Clear();
			_senderKeyStates.Add(new SenderKeyState(id, iteration, chainKey, signatureKey));
		}

		public byte[] Serialize() 
		{
			using (var stream = new MemoryStream()) {

				var record = new SenderKeyRecordStructure {
					senderKeyStates = _senderKeyStates.Select(x => new SenderKeyStateStructure {
						// UNDONE
					}).ToList()
				};

				Serializer.Serialize<SenderKeyRecordStructure>(stream, record);

				return stream.GetBuffer();
			}
		}
	}
}

