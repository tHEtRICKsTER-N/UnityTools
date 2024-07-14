using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;

public class SceneGridEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("Window/Scene Grid")]
    private static void ShowWindow()
    {
        GetWindow<SceneGridEditorWindow>("Scene Grid");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Grid", EditorStyles.boldLabel);

        // Get all scenes in the build settings
        var scenes = EditorBuildSettings.scenes;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var scene in scenes)
        {
            GUILayout.BeginHorizontal();

            // Extract scene name from the path
            string sceneName = Path.GetFileNameWithoutExtension(scene.path);

            // Display custom scene icon if available, otherwise use default scene icon
            Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;

            // Display scene icon (without functionality to open the scene)
            GUILayout.Label(sceneIcon, GUILayout.Width(50), GUILayout.Height(50));

            // Display scene name
            GUILayout.Label(sceneName, GUILayout.ExpandWidth(true));

            // Button to locate scene in the Project tab
            if (GUILayout.Button("Locate", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
            }

            // Button to directly open the scene
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                EditorSceneManager.OpenScene(scene.path);
            }

            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}
