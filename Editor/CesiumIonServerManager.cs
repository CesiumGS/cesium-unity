using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [FilePath("UserSettings/CesiumIonServerManager.asset", FilePathAttribute.Location.ProjectFolder)]
    public class CesiumIonServerManager : ScriptableSingleton<CesiumIonServerManager>
    {
        public CesiumIonServer Current
        {
            get
            {
                if (this._currentCesiumIonServer == null)
                {
                    this._currentCesiumIonServer = CesiumIonServer.defaultServer;

                    // For backward compatibility, look for an existing user access token in the EditorPrefs
                    // and move it to the user access token map.
                    if (string.IsNullOrEmpty(this.GetUserAccessToken(this._currentCesiumIonServer)))
                    {
                        const string editorPrefKey = "CesiumUserAccessToken";
                        string userAccessToken = EditorPrefs.GetString(editorPrefKey);
                        if (!string.IsNullOrEmpty(userAccessToken))
                        {
                            this.SetUserAccessToken(this._currentCesiumIonServer, userAccessToken);
                            EditorPrefs.DeleteKey(editorPrefKey);
                        }
                    }

                }
                return this._currentCesiumIonServer;
            }
        }

        public CesiumIonSession CurrentSession
        { 
            get
            {
                return this.GetSession(this.Current);
            }
        }

        public CesiumIonSession GetSession(CesiumIonServer server)
        {
            CesiumIonSession session;
            if (!this._sessions.TryGetValue(server, out session))
            {
                session = new CesiumIonSession(server);
                this._sessions.Add(server, session);
            }
            return session;
        }

        private string GetUserAccessToken(CesiumIonServer server)
        {
            int index = this._userAccessTokenMap.FindIndex(record => record.server == server);
            return index >= 0 ? this._userAccessTokenMap[index].token : null;
        }

        private void SetUserAccessToken(CesiumIonServer server, string token)
        {
            int index = this._userAccessTokenMap.FindIndex(record => record.server == server);
            if (index >= 0)
            {
                this._userAccessTokenMap[index].token = token;
            }
            else
            {
                UserAccessTokenRecord record = new UserAccessTokenRecord();
                record.server = server;
                record.token = token;
                this._userAccessTokenMap.Add(record);
            }

            this.Save(true);
        }

        [Serializable]
        private class UserAccessTokenRecord
        {
            public CesiumIonServer server;
            public string token;
        }

        [SerializeField]
        private List<UserAccessTokenRecord> _userAccessTokenMap = new List<UserAccessTokenRecord>();

        [SerializeField]
        private CesiumIonServer _currentCesiumIonServer;

        private Dictionary<CesiumIonServer, CesiumIonSession> _sessions = new Dictionary<CesiumIonServer, CesiumIonSession>();
    }
}
