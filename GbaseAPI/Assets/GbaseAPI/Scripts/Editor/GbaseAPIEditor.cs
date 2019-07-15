using UnityEditor;
using UnityEngine;

namespace Gbase
{
    [CustomEditor(typeof(GbaseAPI))]
    public class GbaseAPIEditor : Editor
    {
        private SerializedProperty _enableLogs;
        private SerializedProperty _localhost;
        private SerializedProperty _hmacSecret;
        private SerializedProperty _projectName;
        private SerializedProperty _environment;
        private SerializedProperty _domainName;
        private SerializedProperty _platform;
        private SerializedProperty _version;
        private SerializedProperty _autoReauth;
        
        private void OnEnable()
        {
            _enableLogs = serializedObject.FindProperty("_enableLogs");
            _localhost = serializedObject.FindProperty("_localhost");
            _hmacSecret = serializedObject.FindProperty("_hmacSecret");
            _projectName = serializedObject.FindProperty("_projectName");
            _environment = serializedObject.FindProperty("_environment");
            _domainName = serializedObject.FindProperty("_domainName");
            _platform = serializedObject.FindProperty("_platform");
            _version = serializedObject.FindProperty("_version");
            _autoReauth = serializedObject.FindProperty("AutoReauth");
        }

        public override void OnInspectorGUI()
        {
            GUI.skin.box.stretchWidth = true;

            EditorGUILayout.PropertyField(_enableLogs);
            EditorGUILayout.PropertyField(_localhost);

            if (!_localhost.boolValue)
            {
                EditorGUILayout.PropertyField(_hmacSecret);
                EditorGUILayout.PropertyField(_projectName);
                EditorGUILayout.PropertyField(_environment);
                EditorGUILayout.PropertyField(_domainName);
            }
            EditorGUILayout.PropertyField(_platform);
            EditorGUILayout.PropertyField(_version);
            EditorGUILayout.PropertyField(_autoReauth, new GUIContent("Auto reauth on dead session"));
            GUILayout.Space(5);

            var path = _localhost.boolValue ? "http://localhost:8000/" : "https://{0}-{1}.{2}/";
            path = string.Format(path, _projectName.stringValue, _environment.stringValue, _domainName.stringValue);
            GUILayout.Box("Path:\n" + path);

            if (GUILayout.Button("Test"))
            {
                ((GbaseAPI)target).Test();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}