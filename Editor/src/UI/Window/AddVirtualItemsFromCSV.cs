#if UNITY_EDITOR && READY_DEVELOPMENT
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace RGN.MyEditor
{
    public class AddVirtualItemsFromCSV : EditorWindow
    {
        private string _appPackageName = string.Empty;
        private string _virtualItemName = string.Empty;
        private bool _addBlockchainStub = false;
        private string _filePath = string.Empty;

        [MenuItem(UsefulMenuItems.READY_MENU + "Window/Import Virtual Items From CSV")]
        public static void ShowWindow()
        {
            GetWindow<AddVirtualItemsFromCSV>("Import Virtual Items");
        }

        private void OnGUI()
        {
            GUILayout.Label("Import Virtual Items From CSV", EditorStyles.boldLabel);

            _appPackageName = EditorGUILayout.TextField("App Package Name", _appPackageName);
            _virtualItemName = EditorGUILayout.TextField("Virtual Item Name", _virtualItemName);
            _addBlockchainStub = EditorGUILayout.Toggle("Add Blockchain Stub", _addBlockchainStub);

            if (GUILayout.Button("Select CSV File"))
            {
                _filePath = EditorUtility.OpenFilePanelWithFilters(
                    "Select CSV File",
                    Application.dataPath,
                    new string[] { "CSV File", "csv" });
                if (string.IsNullOrWhiteSpace(_virtualItemName))
                {
                    _virtualItemName = Path.GetFileNameWithoutExtension(_filePath);
                }
            }

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                GUILayout.Label("Selected File: " + _filePath);
            }

            if (GUILayout.Button("Import"))
            {
                CreateVirtualItemsFromCSVAsync();
            }
        }

        private async void CreateVirtualItemsFromCSVAsync()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                Debug.LogError("Please select a CSV file");
                return;
            }
            if (string.IsNullOrWhiteSpace(_virtualItemName))
            {
                Debug.LogError("The virtual item name can not be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(_appPackageName))
            {
                Debug.LogError("The app id (app package name) can not be empty");
                return;
            }
            string content = File.ReadAllText(_filePath);
            IRGNRolesCore core = RGNCore.I as IRGNRolesCore;
            var function = core.ReadyMasterFunction.GetHttpsRequest("virtualItemsV2-addFromCSV");

            Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"appPackageName", _appPackageName },
                {"virtualItemName", _virtualItemName },
                {"addBlockchainStub", _addBlockchainStub },
                {"csvFileString", content }
            };

            await function.CallAsync(parameters);
        }
    }
}
#endif
