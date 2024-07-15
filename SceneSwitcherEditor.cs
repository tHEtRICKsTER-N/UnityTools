// To add the scene in the list, just Add your scene to the Build Settings first !

using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using System.Linq;

public class SceneGridEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string sceneSearchFilter = "";

    [MenuItem("Window/Scene Quick Access")]
    private static void ShowWindow()
    {
        GetWindow<SceneGridEditorWindow>("Scene Quick Access");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Quick Access", EditorStyles.boldLabel);

        sceneSearchFilter = EditorGUILayout.TextField("Search", sceneSearchFilter);

        // Get all scenes in the build settings
        var scenes = AssetDatabase.FindAssets("t:Scene")
      .Select(AssetDatabase.GUIDToAssetPath)
      .Where(scenePath => string.IsNullOrEmpty(sceneSearchFilter) || Path.GetFileNameWithoutExtension(scenePath).ToLower().Contains(sceneSearchFilter.ToLower()))
      .OrderBy(scenePath => scenePath);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var scene in scenes)
        {
            GUILayout.BeginHorizontal();

            // Extract scene name from the path
            string sceneName = Path.GetFileNameWithoutExtension(scene);

            // Display custom scene icon if available, otherwise use default scene icon
            Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;

            // Display scene icon (without functionality to open the scene)
            GUILayout.Label(sceneIcon, GUILayout.Width(50), GUILayout.Height(50));

            // Display scene name
            GUILayout.Label(sceneName, GUILayout.ExpandWidth(true));

            // Button to locate scene in the Project tab
            if (GUILayout.Button("Locate", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene));
            }

            // Button to directly open the scene
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                EditorSceneManager.OpenScene(scene);
            }

            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}
