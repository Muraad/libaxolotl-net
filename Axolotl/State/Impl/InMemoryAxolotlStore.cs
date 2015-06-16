using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class InMemoryAxolotlStore : IAxolotlStore
	{
        private InMemoryPreKeyStore       _preKeyStore       = new InMemoryPreKeyStore();
        private InMemorySessionStore      _sessionStore      = new InMemorySessionStore();
        private InMemorySignedPreKeyStore _signedPreKeyStore = new InMemorySignedPreKeyStore();

        private InMemoryIdentityKeyStore  _identityKeyStore;

        public InMemoryAxolotlStore(IdentityKeyPair identityKeyPair, UInt32 registrationId) 
        {
          _identityKeyStore = new InMemoryIdentityKeyStore(identityKeyPair, registrationId);
        }

        public IdentityKeyPair GetIdentityKeyPair() 
        {
          return _identityKeyStore.GetIdentityKeyPair();
        }

        public UInt32 GetLocalRegistrationId() 
        {
          return _identityKeyStore.GetLocalRegistrationId();
        }

        public void SaveIdentity(string name, IdentityKey identityKey) 
        {
          _identityKeyStore.SaveIdentity(name, identityKey);
        }

        public bool IsTrustedIdentity(string name, IdentityKey identityKey) 
        {
          return _identityKeyStore.IsTrustedIdentity(name, identityKey);
        }

        public PreKeyRecord LoadPreKey(UInt32 preKeyId)
        {
            try 
            {
              return _preKeyStore.LoadPreKey(preKeyId);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("wtf", e);
            }
        }

        public void StorePreKey(UInt32 preKeyId, PreKeyRecord record) 
        {
          _preKeyStore.StorePreKey(preKeyId, record);
        }

        public bool ContainsPreKey(UInt32 preKeyId) 
        {
          return _preKeyStore.ContainsPreKey(preKeyId);
        }

        public void RemovePreKey(UInt32 preKeyId) 
        {
          _preKeyStore.RemovePreKey(preKeyId);
        }

        public SessionRecord LoadSession(AxolotlAddress address) 
        {
          return _sessionStore.LoadSession(address);
        }

        public List<UInt32> GetSubDeviceSessions(string name) 
        {
          return _sessionStore.GetSubDeviceSessions(name);
        }

        public void StoreSession(AxolotlAddress address, SessionRecord record) 
        {
          _sessionStore.StoreSession(address, record);
        }

        public bool ContainsSession(AxolotlAddress address) 
        {
          return _sessionStore.ContainsSession(address);
        }

        public void DeleteSession(AxolotlAddress address) 
        {
          _sessionStore.DeleteSession(address);
        }

        public void DeleteAllSessions(string name) 
        {
          _sessionStore.DeleteAllSessions(name);
        }

        public SignedPreKeyRecord LoadSignedPreKey(UInt32 signedPreKeyId) 
        {
          try {
              return _signedPreKeyStore.LoadSignedPreKey(signedPreKeyId);
          }
          catch (Exception e)
          {
            throw new InvalidOperationException("wtf", e);
          }
        }

        public List<SignedPreKeyRecord> LoadSignedPreKeys() 
        {
          return _signedPreKeyStore.LoadSignedPreKeys();
        }

        public void StoreSignedPreKey(UInt32 signedPreKeyId, SignedPreKeyRecord record) 
        {
          _signedPreKeyStore.StoreSignedPreKey(signedPreKeyId, record);
        }

        public bool ContainsSignedPreKey(UInt32 signedPreKeyId) 
        {
          return _signedPreKeyStore.ContainsSignedPreKey(signedPreKeyId);
        }

        public void RemoveSignedPreKey(UInt32 signedPreKeyId) 
        {
          _signedPreKeyStore.RemoveSignedPreKey(signedPreKeyId);
        }
	}
}

