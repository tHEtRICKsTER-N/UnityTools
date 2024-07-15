//It lists down all the scenes in your project.
//The list is updated as soon as a scene is deleted or created.
//You can Locate, Open or Delete the Scene
//You can also add scenes into favourites

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneGridEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Vector2 favouritesScrollPosition;
    private string sceneSearchFilter = "";
    private List<string> favouriteScenes = new List<string>();

    [MenuItem("Window/Scene Quick Access")]
    private static void ShowWindow()
    {
        GetWindow<SceneGridEditorWindow>("Scene Quick Access");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Quick Access", EditorStyles.boldLabel);

        EditorGUILayout.Space(10, true);

        sceneSearchFilter = EditorGUILayout.TextField("Search", sceneSearchFilter);

        EditorGUILayout.Space(10, true);

        // Favourites subwindow
        GUILayout.Label("Favourites", EditorStyles.boldLabel);
        favouritesScrollPosition = EditorGUILayout.BeginScrollView(favouritesScrollPosition, GUILayout.Height(225));
        GUILayout.BeginHorizontal();
        if (favouriteScenes.Count == 0)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Click on Favourite to add a scene", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        else
        {
            foreach (var scene in favouriteScenes)
            {
                GUILayout.BeginVertical();
                // Display scene icon
                Texture2D sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
                if (GUILayout.Button(sceneIcon, GUILayout.Width(150), GUILayout.Height(150)))
                {
                    EditorSceneManager.OpenScene(scene);
                }
                // Display scene name
                GUILayout.Label(Path.GetFileNameWithoutExtension(scene), GUILayout.Width(150));
                // Button to remove the scene from favourites
                if (GUILayout.Button("Remove", GUILayout.Width(150)))
                {
                    favouriteScenes.Remove(scene);
                    break; // Exit the loop to avoid modifying the collection while iterating
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();


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

            // Button to add the scene to favourites
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Favourite", GUILayout.Width(80)))
            {
                if (!favouriteScenes.Contains(scene))
                {
                    favouriteScenes.Add(scene);
                }
            }

            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}
