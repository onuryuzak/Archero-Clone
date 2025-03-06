using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using System.IO;

/// <summary>
/// Editor tool for automatically creating and setting up a player character with all required components
/// </summary>
public class PlayerCreator : EditorWindow
{
    private GameObject playerPrefab;
    private GameObject weaponPrefab;
    private GameObject projectilePrefab;
    private GameObject joystickPrefab;
    
    [MenuItem("Tools/Player System/Create Player")]
    public static void ShowWindow()
    {
        GetWindow<PlayerCreator>("Player Creator");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Player Creator Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool will create a fully set up player for your Archero clone. " +
            "You can optionally assign prefabs, or leave them empty to create new ones.", MessageType.Info);
        EditorGUILayout.Space();
        
        playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab (Optional)", 
            playerPrefab, typeof(GameObject), false);
            
        weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab (Optional)", 
            weaponPrefab, typeof(GameObject), false);
            
        projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab (Optional)", 
            projectilePrefab, typeof(GameObject), false);
            
        joystickPrefab = (GameObject)EditorGUILayout.ObjectField("Joystick Prefab (Optional)", 
            joystickPrefab, typeof(GameObject), false);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Player"))
        {
            CreatePlayer();
        }
    }
    
    private void CreatePlayer()
    {
        // Create prefabs directory if it doesn't exist
        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
            AssetDatabase.Refresh();
        }
        
        // Create player GameObject
        GameObject player = playerPrefab != null ? 
            Instantiate(playerPrefab) : new GameObject("Player");
        player.tag = "Player";
        
        // Layer kontrolü ve güvenli atama
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0 && playerLayer <= 31) {
            player.layer = playerLayer;
        } else {
            // "Player" layer tanımlı değilse varsayılan layer kullanılır
            Debug.LogWarning("\"Player\" layer not found. Using \"Default\" layer instead. Consider adding a \"Player\" layer in Project Settings.");
            player.layer = LayerMask.NameToLayer("Default");
        }
        
        // Set up necessary components
        SetupPlayerComponents(player);
        
        // Create and set up the weapon
        // GameObject weapon = SetupWeapon(player);
        
        // Create and set up the joystick if needed
        SetupJoystick();
        
        // Create a prefab from the player GameObject
        string prefabPath = "Assets/Prefabs/Player.prefab";
        bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
        
        if (prefabExists)
        {
            // If a prefab already exists, update it
            GameObject existingPrefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            Debug.Log("Updated existing player prefab at: " + prefabPath);
            Selection.activeObject = existingPrefab;
        }
        else
        {
            // Otherwise create a new prefab
            GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            Debug.Log("Created new player prefab at: " + prefabPath);
            Selection.activeObject = newPrefab;
        }
        
        // Destroy the temporary instance
        DestroyImmediate(player);
    }
    
    private void SetupPlayerComponents(GameObject player)
    {
        // Add a capsule collider if it doesn't exist
        if (player.GetComponent<CapsuleCollider>() == null)
        {
            CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
        }
        
        // Add a rigidbody if it doesn't exist
        if (player.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 1f;
            rb.drag = 5f; // Add some drag for smoother movement
        }
        
        // Add a PlayerController if it doesn't exist
        PlayerController controller = player.GetComponent<PlayerController>() ?? 
            player.AddComponent<PlayerController>();
        
        // Set speed and other properties using serialized object because they are private SerializeField properties
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("moveSpeed").floatValue = 5f;
        serializedController.FindProperty("rotationSpeed").floatValue = 10f;
        serializedController.ApplyModifiedProperties();
        
        // Add PlayerSkillSystem if it doesn't exist
        PlayerSkillSystem skillSystem = player.GetComponent<PlayerSkillSystem>() ?? 
            player.AddComponent<PlayerSkillSystem>();
            
        // Load skill assets from Resources folder
        if (Directory.Exists("Assets/Resources/Skills"))
        {
            string[] skillPaths = Directory.GetFiles("Assets/Resources/Skills", "*.asset");
            SkillData[] skills = new SkillData[skillPaths.Length];
            
            for (int i = 0; i < skillPaths.Length; i++)
            {
                string path = skillPaths[i].Replace("\\", "/").Replace("Assets/Resources/", "");
                path = path.Substring(0, path.Length - 6); // Remove .asset extension
                SkillData skill = Resources.Load<SkillData>(path);
                if (skill != null)
                {
                    skills[i] = skill;
                }
            }
            
            // Use reflection to set the skills array in the PlayerSkillSystem
            var field = typeof(PlayerSkillSystem).GetField("availableSkills", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                field.SetValue(skillSystem, skills);
                Debug.Log($"Added {skills.Length} skills to player");
            }
        }
        else
        {
            Debug.LogWarning("Skills folder not found. You may need to create skills first.");
        }
        
        // If the player doesn't have a visual representation yet, add a simple one
        if (player.transform.childCount == 0)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "PlayerVisual";
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = new Vector3(0, 1f, 0);
            visual.transform.localScale = new Vector3(1f, 1f, 1f);
            
            // Remove the collider from the visual since the player has its own collider
            DestroyImmediate(visual.GetComponent<Collider>());
            
            // Add a material with a distinct color
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.2f, 0.6f, 1f); // Blue color
                renderer.material = material;
                
                // Save the material as an asset
                string materialPath = "Assets/Materials";
                if (!Directory.Exists(materialPath))
                {
                    Directory.CreateDirectory(materialPath);
                    AssetDatabase.Refresh();
                }
                
                AssetDatabase.CreateAsset(material, "Assets/Materials/PlayerMaterial.mat");
            }
        }
    }
    
    // private GameObject SetupWeapon(GameObject player)
    // {
    //     // Find or create a weapon container
    //     Transform weaponHolder = player.transform.Find("WeaponHolder");
    //     if (weaponHolder == null)
    //     {
    //         GameObject holder = new GameObject("WeaponHolder");
    //         holder.transform.SetParent(player.transform);
    //         holder.transform.localPosition = new Vector3(0, 1.5f, 0.3f);
    //         weaponHolder = holder.transform;
    //     }
    //     
    //     // Create or find the weapon
    //     GameObject weapon = weaponPrefab != null ? 
    //         Instantiate(weaponPrefab) : new GameObject("Weapon");
    //     weapon.transform.SetParent(weaponHolder);
    //     weapon.transform.localPosition = Vector3.zero;
    //     weapon.transform.localRotation = Quaternion.identity;
    //     
    //     // Add WeaponController if it doesn't exist
    //     WeaponController weaponController = weapon.GetComponent<WeaponController>() ?? 
    //         weapon.AddComponent<WeaponController>();
    //     
    //     // Set projectile if provided
    //     if (projectilePrefab != null)
    //     {
    //         weaponController._projectilePrefab = projectilePrefab;
    //     }
    //     else
    //     {
    //         // Create a simple projectile if none is provided
    //         GameObject projectile = CreateDefaultProjectile();
    //         
    //         // Save projectile as prefab
    //         string prefabPath = "Assets/Prefabs/Projectile.prefab";
    //         bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
    //         
    //         if (prefabExists)
    //         {
    //             // If a prefab already exists, update it
    //             GameObject existingPrefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
    //             weaponController._projectilePrefab = existingPrefab;
    //         }
    //         else
    //         {
    //             // Otherwise create a new prefab
    //             GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
    //             weaponController._projectilePrefab = newPrefab;
    //         }
    //         
    //         // Destroy the temporary instance
    //         DestroyImmediate(projectile);
    //     }
    //     
    //     // Set up references
    //     PlayerSkillSystem skillSystem = player.GetComponent<PlayerSkillSystem>();
    //     if (skillSystem != null)
    //     {
    //         var field = typeof(PlayerSkillSystem).GetField("weaponController", 
    //             System.Reflection.BindingFlags.Instance | 
    //             System.Reflection.BindingFlags.NonPublic);
    //             
    //         if (field != null)
    //         {
    //             field.SetValue(skillSystem, weaponController);
    //         }
    //     }
    //     
    //     // Give the weapon a visual representation
    //     if (weapon.transform.childCount == 0)
    //     {
    //         GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //         visual.name = "WeaponVisual";
    //         visual.transform.SetParent(weapon.transform);
    //         visual.transform.localPosition = Vector3.zero;
    //         visual.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
    //         
    //         // Remove the collider
    //         DestroyImmediate(visual.GetComponent<Collider>());
    //         
    //         // Add a material with a distinct color
    //         Renderer renderer = visual.GetComponent<Renderer>();
    //         if (renderer != null)
    //         {
    //             Material material = new Material(Shader.Find("Standard"));
    //             material.color = new Color(0.8f, 0.2f, 0.2f); // Red color
    //             renderer.material = material;
    //             
    //             // Save the material as an asset
    //             string materialPath = "Assets/Materials";
    //             if (!Directory.Exists(materialPath))
    //             {
    //                 Directory.CreateDirectory(materialPath);
    //                 AssetDatabase.Refresh();
    //             }
    //             
    //             AssetDatabase.CreateAsset(material, "Assets/Materials/WeaponMaterial.mat");
    //         }
    //     }
    //     
    //     return weapon;
    // }
    
    private GameObject CreateDefaultProjectile()
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // Add Projectile component
        Projectile projectileComponent = projectile.AddComponent<Projectile>();
        
        // Add Rigidbody with error handling
        Rigidbody rb = null;
        try {
            // Önce var olup olmadığını kontrol et
            rb = projectile.GetComponent<Rigidbody>();
            
            // Eğer yoksa, eklemeyi dene
            if (rb == null) {
                // Prefab modu kontrolü
                bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(projectile) || PrefabUtility.IsPartOfPrefabInstance(projectile);
                
                if (isPrefab) {
                    Debug.LogWarning("Cannot add components to prefabs directly in editor scripts. Consider making a copy first.");
                    // Prefab'ı düzeltmek için daha uygun bir yöntem kullanın
                    GameObject tempObj = PrefabUtility.InstantiatePrefab(projectile) as GameObject;
                    if (tempObj != null) {
                        rb = tempObj.AddComponent<Rigidbody>();
                        PrefabUtility.ApplyPrefabInstance(tempObj, InteractionMode.AutomatedAction);
                        GameObject.DestroyImmediate(tempObj);
                    }
                } else {
                    rb = projectile.AddComponent<Rigidbody>();
                }
            }
            
            // Rigidbody varsa özelliklerini ayarla
            if (rb != null) {
                rb.useGravity = false;
                rb.isKinematic = true; // We'll handle movement in the script
            } else {
                Debug.LogError("Could not add or access Rigidbody component on projectile. Movement will not work correctly.");
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"Failed to add or configure Rigidbody: {e.Message}");
        }
        
        // Set the layer
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        if (projectileLayer >= 0 && projectileLayer <= 31) {
            projectile.layer = projectileLayer;
        } else {
            // "Projectile" layer tanımlı değilse varsayılan layer kullanılır
            Debug.LogWarning("\"Projectile\" layer not found. Using \"Default\" layer instead. Consider adding a \"Projectile\" layer in Project Settings.");
            projectile.layer = LayerMask.NameToLayer("Default");
        }
        
        // Add material
        Renderer renderer = projectile.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(1f, 0.9f, 0.2f); // Yellow color
            renderer.material = material;
            
            // Save the material as an asset
            string materialPath = "Assets/Materials";
            if (!Directory.Exists(materialPath))
            {
                Directory.CreateDirectory(materialPath);
                AssetDatabase.Refresh();
            }
            
            AssetDatabase.CreateAsset(material, "Assets/Materials/ProjectileMaterial.mat");
        }
        
        return projectile;
    }
    
    private void SetupJoystick()
    {
        // Only create joystick if a prefab is provided
        if (joystickPrefab == null) return;
        
        // Check if UI Canvas exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Create Canvas
            GameObject canvasObject = new GameObject("UICanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add CanvasScaler
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            
            // Add GraphicRaycaster
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Instantiate joystick as a child of the canvas
        GameObject joystick = Instantiate(joystickPrefab, canvas.transform);
        joystick.name = "Joystick";
        
        // Position the joystick in the bottom-left corner
        RectTransform joystickRect = joystick.GetComponent<RectTransform>();
        if (joystickRect != null)
        {
            joystickRect.anchorMin = new Vector2(0, 0);
            joystickRect.anchorMax = new Vector2(0, 0);
            joystickRect.pivot = new Vector2(0.5f, 0.5f);
            joystickRect.anchoredPosition = new Vector2(200, 200);
        }
        
        // Find the PlayerController in the scene
        PlayerController controller = FindObjectOfType<PlayerController>();
        if (controller != null)
        {
            // Manually set up the reference if joystick has a Joystick component
            Joystick joystickComponent = joystick.GetComponent<Joystick>();
            if (joystickComponent != null)
            {
                var field = typeof(PlayerController).GetField("joystick", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic);
                    
                if (field != null)
                {
                    field.SetValue(controller, joystickComponent);
                    Debug.Log("Joystick reference set in PlayerController");
                }
            }
        }
    }
} 