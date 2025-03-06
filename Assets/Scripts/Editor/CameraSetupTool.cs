using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool for setting up an Archero-style camera
/// </summary>
public class CameraSetupTool : EditorWindow
{
    private enum CameraStyle
    {
        ArcheroStandard,
        CloserToPlayer,
        TopDown,
        WiderAngle
    }
    
    private CameraStyle cameraStyle = CameraStyle.ArcheroStandard;
    private GameObject targetObject;
    private bool useBoundaries = false;
    private float minX = -50f;
    private float maxX = 50f;
    private float minZ = -50f;
    private float maxZ = 50f;
    
    [MenuItem("Tools/Camera/Setup Archero Camera")]
    public static void ShowWindow()
    {
        GetWindow<CameraSetupTool>("Camera Setup");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Archero-Style Camera Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Target selection
        targetObject = EditorGUILayout.ObjectField("Target to Follow", targetObject, typeof(GameObject), true) as GameObject;
        
        // Camera style selection
        cameraStyle = (CameraStyle)EditorGUILayout.EnumPopup("Camera Style", cameraStyle);
        
        EditorGUILayout.Space(5);
        
        // Boundaries
        useBoundaries = EditorGUILayout.Toggle("Use Boundaries", useBoundaries);
        
        if (useBoundaries)
        {
            EditorGUI.indentLevel++;
            minX = EditorGUILayout.FloatField("Min X", minX);
            maxX = EditorGUILayout.FloatField("Max X", maxX);
            minZ = EditorGUILayout.FloatField("Min Z", minZ);
            maxZ = EditorGUILayout.FloatField("Max Z", maxZ);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // Create button
        if (GUILayout.Button("Create Archero Camera"))
        {
            CreateCamera();
        }
        
        EditorGUILayout.Space(5);
        
        // Setup existing camera button
        if (GUILayout.Button("Setup Existing Main Camera"))
        {
            SetupExistingCamera();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void CreateCamera()
    {
        // Check if Main Camera already exists
        Camera existingCamera = Camera.main;
        GameObject cameraObj;
        
        if (existingCamera != null)
        {
            if (!EditorUtility.DisplayDialog("Camera Already Exists", 
                "A Main Camera already exists in the scene. Do you want to replace its settings?", 
                "Yes, Update It", "Cancel"))
            {
                return;
            }
            
            cameraObj = existingCamera.gameObject;
        }
        else
        {
            // Create new camera
            cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
        }
        
        // Setup the camera
        SetupCameraObject(cameraObj);
        
        // Select the camera in the hierarchy
        Selection.activeGameObject = cameraObj;
        
        Debug.Log("Archero camera created successfully!");
    }
    
    private void SetupExistingCamera()
    {
        Camera existingCamera = Camera.main;
        
        if (existingCamera == null)
        {
            EditorUtility.DisplayDialog("No Main Camera", 
                "No Main Camera found in the scene. Please create a new camera instead.", 
                "OK");
            return;
        }
        
        SetupCameraObject(existingCamera.gameObject);
        Selection.activeGameObject = existingCamera.gameObject;
        
        Debug.Log("Existing main camera set up as Archero camera!");
    }
    
    private void SetupCameraObject(GameObject cameraObj)
    {
        // Make sure the camera has the required components
        Camera camera = cameraObj.GetComponent<Camera>() ?? cameraObj.AddComponent<Camera>();
        CameraController controller = cameraObj.GetComponent<CameraController>() ?? cameraObj.AddComponent<CameraController>();
        
        // Setup camera basic properties
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 1000f;
        
        // Get serialized object to modify CameraController properties
        SerializedObject serializedController = new SerializedObject(controller);
        
        // Set the target if provided
        if (targetObject != null)
        {
            serializedController.FindProperty("target").objectReferenceValue = targetObject.transform;
        }
        
        // Configure camera style based on selection
        Vector3 offset = Vector3.zero;
        float smoothSpeed = 5f;
        
        switch (cameraStyle)
        {
            case CameraStyle.ArcheroStandard:
                offset = new Vector3(0, 15, -8);
                smoothSpeed = 5f;
                break;
                
            case CameraStyle.CloserToPlayer:
                offset = new Vector3(0, 10, -6);
                smoothSpeed = 6f;
                break;
                
            case CameraStyle.TopDown:
                offset = new Vector3(0, 20, -0.1f);
                smoothSpeed = 4f;
                break;
                
            case CameraStyle.WiderAngle:
                offset = new Vector3(0, 18, -12);
                smoothSpeed = 4.5f;
                camera.fieldOfView = 70f;
                break;
        }
        
        serializedController.FindProperty("offset").vector3Value = offset;
        serializedController.FindProperty("smoothSpeed").floatValue = smoothSpeed;
        
        // Set boundaries if enabled
        serializedController.FindProperty("useBoundaries").boolValue = useBoundaries;
        if (useBoundaries)
        {
            serializedController.FindProperty("minX").floatValue = minX;
            serializedController.FindProperty("maxX").floatValue = maxX;
            serializedController.FindProperty("minZ").floatValue = minZ;
            serializedController.FindProperty("maxZ").floatValue = maxZ;
        }
        
        // Position the camera immediately to the appropriate position if target exists
        if (targetObject != null)
        {
            cameraObj.transform.position = targetObject.transform.position + offset;
            cameraObj.transform.LookAt(targetObject.transform);
        }
        else
        {
            // If no target, position at origin with offset
            cameraObj.transform.position = offset;
            cameraObj.transform.rotation = Quaternion.Euler(60, 0, 0);
        }
        
        // Apply all serialized property changes
        serializedController.ApplyModifiedProperties();
    }
} 