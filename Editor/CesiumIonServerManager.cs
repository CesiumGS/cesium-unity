using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [FilePath("UserSettings/CesiumIonServerManager.asset", FilePathAttribute.Location.ProjectFolder)]
    public class CesiumIonServerManager : ScriptableSingleton<CesiumIonServerManager>
    {
        public event Action<CesiumIonServerManager> CurrentChanged;
        public event Action<CesiumIonServerManager> ServerListChanged;

        public CesiumIonServer current
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
            set
            {
                this._currentCesiumIonServer = value;
                CesiumIonServer.serverForNewObjects = value;
                CurrentChanged?.Invoke(this);
                this.Save(true);
            }
        }

        public CesiumIonSession currentSession
        { 
            get
            {
                return this.GetSession(this.current);
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

        public void ResumeAll()
        {
            foreach (CesiumIonServer server in this.servers)
            {
                CesiumIonSession session = this.GetSession(server);
                if (session != null)
                {
                    session.Resume();
                    session.GetProfileUsername();
                }
            }
        }

        public IReadOnlyList<CesiumIonServer> servers
        {
            get
            {
                this.RefreshServerList();
                return this._servers;
            }
        }

        public void RefreshServerList()
        {
            this._servers = new List<CesiumIonServer>();

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(CesiumIonServer).FullName);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                this._servers.Add(AssetDatabase.LoadAssetAtPath<CesiumIonServer>(path));
            }
            ServerListChanged?.Invoke(this);
        }

        internal string GetUserAccessToken(CesiumIonServer server)
        {
            int index = this._userAccessTokenMap.FindIndex(record => record.server == server);
            return index >= 0 ? this._userAccessTokenMap[index].token : null;
        }

        internal void SetUserAccessToken(CesiumIonServer server, string token)
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

        class RefreshServers : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
            {
                CesiumIonServerManager.instance.ResumeAll();

                // Ensure the `serverForNewObjects` matches the current server. This is
                // essential when the manager is first loaded, and harmless otherwise.
                CesiumIonServer.serverForNewObjects = CesiumIonServerManager.instance.current;
            }
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
        private List<CesiumIonServer> _servers = new List<CesiumIonServer>();
    }
}
