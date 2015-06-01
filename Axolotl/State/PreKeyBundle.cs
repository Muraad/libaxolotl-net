using System;
using Axolotl.ECC;

namespace Axolotl.State
{
	public class PreKeyBundle
	{
		public int RegistrationID { get; private set; }
		public int DeviceID { get; private set; }
		public int PreKeyID { get; private set; }
		public ECPublicKey PreKeyPublic { get; private set; }
		public int SignedPreKeyID { get; private set; }
		public ECPublicKey SignedPreKeyPublic { get; private set; }
		public byte[] SignedPreKeySignature { get; private set; }
		public IdentityKey IdentityKey;

		public PreKeyBundle (int registrationId, int deviceId, int preKeyId, ECPublicKey preKeyPublic,
		                     int signedPreKeyId, ECPublicKey signedPreKeyPublic, byte[] signedPreKeySignature,
		                     IdentityKey identityKey)
		{
			RegistrationID = registrationId;
			DeviceID = deviceId;
			PreKeyID = preKeyId;
			PreKeyPublic = preKeyPublic;
			SignedPreKeyID = signedPreKeyId;
			SignedPreKeyPublic = signedPreKeyPublic;
			SignedPreKeySignature = signedPreKeySignature;
			IdentityKey = identityKey;
		}


	}
}

