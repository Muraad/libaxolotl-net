using System;
using Axolotl.ECC;

namespace Axolotl.State
{
	public class PreKeyBundle
	{
		// Complete

		public UInt32 RegistrationID { get; private set; }
		public UInt32 DeviceID { get; private set; }
		public UInt32 PreKeyId { get; private set; }
		public ECPublicKey PreKeyPublic { get; private set; }
		public UInt32 SignedPreKeyID { get; private set; }
		public ECPublicKey SignedPreKeyPublic { get; private set; }
		public byte[] SignedPreKeySignature { get; private set; }
		public IdentityKey IdentityKey;

		public PreKeyBundle (UInt32 registrationId, UInt32 deviceId, UInt32 preKeyId, ECPublicKey preKeyPublic,
		                     UInt32 signedPreKeyId, ECPublicKey signedPreKeyPublic, byte[] signedPreKeySignature,
		                     IdentityKey identityKey)
		{
			RegistrationID = registrationId;
			DeviceID = deviceId;
			PreKeyId = preKeyId;
			PreKeyPublic = preKeyPublic;
			SignedPreKeyID = signedPreKeyId;
			SignedPreKeyPublic = signedPreKeyPublic;
			SignedPreKeySignature = signedPreKeySignature;
			IdentityKey = identityKey;
		}

		public ECPublicKey GetSignedPreKey()
		{
			return SignedPreKeyPublic;
		}

		public ECPublicKey GetPreKey()
		{
			return PreKeyPublic;
		}
	}
}

