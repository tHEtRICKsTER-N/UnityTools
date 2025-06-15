using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class MethodInvokerWindow : EditorWindow
{
    private GameObject targetObject;
    private Component targetComponent;
    private string methodName;
    private string[] methodNames = new string[0];
    private int selectedMethodIndex = 0;
    private Component[] components = new Component[0];
    private int selectedComponentIndex = -1;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Method Invoker")]
    public static void ShowWindow()
    {
        GetWindow<MethodInvokerWindow>("Method Invoker");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Header
        EditorGUILayout.Space();
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("Method Invoker", headerStyle);
        EditorGUILayout.Space();

        // Description
        GUIStyle descriptionStyle = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField(
            "A Handy Tool for Invoking Functions at Runtime or Edit Time. " +
            "Just drag the GameObject, choose the function, and hit the button — that’s it! " +
            "Perfect for quick tests and debugging.",
            descriptionStyle
        );

        // Developer Name
        GUIStyle developerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("– Naimish", developerStyle);
        EditorGUILayout.Space(20);

        // Gameobject field
        EditorGUI.BeginChangeCheck();
        targetObject = (GameObject)EditorGUILayout.ObjectField("Gameobject", targetObject, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateComponentAndMethodLists();
        }

        // Script (Component) dropdown
        if (targetObject != null)
        {
            if (EditorApplication.isPlaying)
            {
                UpdateComponentAndMethodLists();
            }

            string[] componentNames = components.Select(c => c != null ? c.GetType().Name : "Null").ToArray();
            EditorGUI.BeginChangeCheck();
            selectedComponentIndex = EditorGUILayout.Popup("Script", selectedComponentIndex, componentNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedComponentIndex >= 0 && selectedComponentIndex < components.Length)
                {
                    targetComponent = components[selectedComponentIndex];
                    UpdateMethodNames();
                }
                else
                {
                    targetComponent = null;
                    methodNames = new string[0];
                    selectedMethodIndex = 0;
                }
            }
        }
        else
        {
            targetComponent = null;
            components = new Component[0];
            methodNames = new string[0];
            selectedComponentIndex = -1;
            selectedMethodIndex = 0;
        }

        // Method dropdown and invoke button
        if (targetComponent != null)
        {
            EditorGUI.BeginChangeCheck();
            selectedMethodIndex = EditorGUILayout.Popup("Method", selectedMethodIndex, methodNames);
            if (EditorGUI.EndChangeCheck() && selectedMethodIndex >= 0 && selectedMethodIndex < methodNames.Length)
            {
                methodName = methodNames[selectedMethodIndex];
            }

            if (GUILayout.Button("Invoke Method"))
            {
                InvokeMethod();
            }
        }

        EditorGUILayout.Space(20);

        // Know the Developer button
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            normal = { textColor = Color.black },
            alignment = TextAnchor.MiddleCenter
        };
        if (GUILayout.Button("Know the Developer", buttonStyle))
        {
            Application.OpenURL("https://naimish.framer.ai"); // Replace with your actual URL
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndScrollView();

        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }

    private void UpdateComponentAndMethodLists()
    {
        if (targetObject == null)
        {
            components = new Component[0];
            selectedComponentIndex = -1;
            targetComponent = null;
            UpdateMethodNames();
            return;
        }

        components = targetObject.GetComponents<Component>().Where(c => c != null).ToArray();
        selectedComponentIndex = targetComponent != null
            ? System.Array.IndexOf(components, targetComponent)
            : -1;

        UpdateMethodNames();
    }

    private void UpdateMethodNames()
    {
        if (targetComponent == null)
        {
            methodNames = new string[0];
            selectedMethodIndex = 0;
            methodName = "";
            return;
        }

        methodNames = targetComponent.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
            .Select(m => m.Name)
            .ToArray();

        selectedMethodIndex = methodNames.Contains(methodName)
            ? System.Array.IndexOf(methodNames, methodName)
            : 0;

        if (methodNames.Length > 0 && selectedMethodIndex < methodNames.Length)
        {
            methodName = methodNames[selectedMethodIndex];
        }
        else
        {
            methodName = "";
        }
    }

    private void InvokeMethod()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("No GameObject assigned.");
            return;
        }

        if (targetComponent == null)
        {
            Debug.LogWarning("No component selected.");
            return;
        }

        if (string.IsNullOrEmpty(methodName))
        {
            Debug.LogWarning("No method selected.");
            return;
        }

        if (!targetObject || !targetComponent)
        {
            Debug.LogWarning("Target GameObject or component has been destroyed.");
            UpdateComponentAndMethodLists();
            return;
        }

        MethodInfo method = targetComponent.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            Debug.LogError($"Method '{methodName}' not found on component {targetComponent.GetType().Name}.");
            return;
        }

        method.Invoke(targetComponent, null);
    }
}