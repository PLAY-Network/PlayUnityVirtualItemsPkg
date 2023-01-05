using Firebase;
using Firebase.Functions;
using RGN.Modules.VirtualItems;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    public class AddVirtualItemEditorWindow : EditorWindow
    {
        private FirebaseApp _firebaseApp;

        private VirtualItem _newVirtualItem;
        private bool _asyncOperationIsRunning;

        [MenuItem("ReadyGamesNetwork/Window/Add Virtual Item")]
        private static void Init()
        {
            AddVirtualItemEditorWindow window = (AddVirtualItemEditorWindow)EditorWindow.GetWindow(typeof(AddVirtualItemEditorWindow));
            window.Show();
        }

        private void OnGUI()
        {
            if (_newVirtualItem == null)
            {
                _newVirtualItem = new VirtualItem();
            }
            GUILayout.Label("New Virtual Item Fields:", EditorStyles.boldLabel);
            _newVirtualItem.name = EditorGUILayout.TextField("Name", _newVirtualItem.name);
            _newVirtualItem.description = EditorGUILayout.TextField("Description", _newVirtualItem.description);
            //_newVirtualItem.properties = EditorGUILayout.TextField("Properties", _newVirtualItem.properties);
            bool guiEnabledState = GUI.enabled;
            GUI.enabled = CheckRequiredFields() && !_asyncOperationIsRunning;
            if (GUILayout.Button("Create Virtual Item in Database"))
            {
                Debug.LogWarning("Create Request for virtual item: " + _newVirtualItem.ToString());
                CreateVirtualItemAsync();
            }
            GUI.enabled = guiEnabledState;
        }

        private bool CheckRequiredFields()
        {
            return
                !string.IsNullOrWhiteSpace(_newVirtualItem.name) &&
                !string.IsNullOrWhiteSpace(_newVirtualItem.description);
        }
        private async void CreateVirtualItemAsync()
        {
            if (_firebaseApp == null)
            {
                var appOptions = new AppOptions()
                {//TODO: refactor this out. Use the ApplicationStore scriptable object
                    ApiKey = Environment.GetEnvironmentVariable("READY_EDITOR_STAGING_API_KEY"),
                    AppId = Environment.GetEnvironmentVariable("READY_EDITOR_STAGING_API_ID"),
                    ProjectId = Environment.GetEnvironmentVariable("READY_EDITOR_STAGING_PROJECT_ID")
                };
                _firebaseApp = FirebaseApp.Create(appOptions, "Secondary");
            }
            HttpsCallableReference function = FirebaseFunctions.GetInstance(_firebaseApp).GetHttpsCallable("virtualItems-add");
            var request = new Dictionary<string, object>()
            {
                {"name", _newVirtualItem.name },
                {"description", _newVirtualItem.description },
                {"properties", _newVirtualItem.properties }
            };
            try
            {
                _asyncOperationIsRunning = true;
                var result = await function.CallAsync(request);

                string response = (string)((IDictionary<object, object>)result.Data)["response"];
                Debug.Log(response);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _asyncOperationIsRunning = false;
            }
        }
    }
}
