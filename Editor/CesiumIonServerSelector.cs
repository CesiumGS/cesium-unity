using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CesiumForUnity
{
    public class CesiumIonServerSelector : IDisposable
    {
        private EditorWindow _parent;
        private AdvancedDropdownState _dropDownState = new AdvancedDropdownState();

        public CesiumIonServerSelector(EditorWindow parent)
        {
            this._parent = parent;
            CesiumIonServerManager.instance.CurrentChanged += OnCurrentChanged;
        }

        public void Dispose()
        {
            CesiumIonServerManager.instance.CurrentChanged -= OnCurrentChanged;
        }

        private void OnCurrentChanged(CesiumIonServerManager manager)
        {
            this._parent.Repaint();
        }

        public void OnGUI()
        {
            CesiumIonServer server = CesiumIonServerManager.instance.current;
            GUIContent content = new GUIContent(GetLabelFromCesiumIonServer(server), "The current Cesium ion server");
            Rect rect = EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.DropdownButton(content, FocusType.Keyboard))
            {
                var serverDropDown = new ServerDropDown(this._dropDownState, rect);
                serverDropDown.Show(rect);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static string GetLabelFromCesiumIonServer(CesiumIonServer server)
        {
            if (server == null)
                return "Error: No Cesium ion server configured.";

            CesiumIonSession session = CesiumIonServerManager.instance.GetSession(server);

            string profileName = session.GetProfileUsername();

            string prefix = "";
            string suffix = "";

            if (session.IsConnecting() || session.IsResuming())
                suffix = " (connecting...)";
            else if (session.IsLoadingProfile())
                suffix = " (loading profile...)";
            else if (session.IsConnected() && session.IsProfileLoaded())
                prefix = profileName + " @ ";
            else
                suffix = " (not connected)";

            return prefix + server.name + suffix;
        }

        private class ServerDropDownItem : AdvancedDropdownItem
        {
            public CesiumIonServer server;

            public ServerDropDownItem(CesiumIonServer server)
                : base(GetLabelFromCesiumIonServer(server))
            {
                this.server = server;
            }
        }

        private class ServerDropDown : AdvancedDropdown
        {
            public ServerDropDown(AdvancedDropdownState state, Rect rect)
                : base(state)
            {
                int itemCount = Math.Min(10, CesiumIonServerManager.instance.servers.Count);
                this.minimumSize = new Vector2(50.0f, (itemCount + 3) * EditorGUIUtility.singleLineHeight);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                AdvancedDropdownItem root = new AdvancedDropdownItem("Cesium ion Servers");

                foreach (CesiumIonServer server in CesiumIonServerManager.instance.servers)
                {
                    root.AddChild(new ServerDropDownItem(server));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                CesiumIonServerManager.instance.current = ((ServerDropDownItem)item).server;
            }
        }
    }
}
