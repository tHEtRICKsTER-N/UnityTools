//It lists down all the scenes in your project.
//The list is updated as soon as a scene is deleted or created.
//You can Locate, Open or Delete the Scene

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
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Locate", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene));
            }

            // Button to directly open the scene
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                EditorSceneManager.OpenScene(scene);
            }

            // Button to delete the scene
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Scene", $"Are you sure you want to delete the scene '{sceneName}'?", "Yes", "No"))
                {
                    AssetDatabase.DeleteAsset(scene);
                    AssetDatabase.Refresh();
                }
            }

            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}
