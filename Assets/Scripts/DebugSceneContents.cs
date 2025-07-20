using UnityEngine;

public class DebugSceneContents : MonoBehaviour {
    
    void Start() {
        Debug.Log("=== SCENE ANALYSIS START ===");
        
        // Find all GameObjects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"Total GameObjects in scene: {allObjects.Length}");
        
        // List all root objects
        Debug.Log("=== ROOT OBJECTS ===");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.GetActiveScene().rootCount; i++) {
            var rootObj = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()[i];
            Debug.Log($"Root Object {i}: {rootObj.name}");
        }
        
        // Look for specific objects
        Debug.Log("=== LOOKING FOR AVATAR ===");
        GameObject debugModel = GameObject.Find("DebugModel");
        if (debugModel != null) {
            Debug.Log($"FOUND DebugModel at position: {debugModel.transform.position}");
            Debug.Log($"DebugModel active: {debugModel.activeSelf}");
            
            var animator = debugModel.GetComponent<Animator>();
            if (animator != null) {
                Debug.Log("DebugModel HAS Animator component!");
                Debug.Log($"Animator enabled: {animator.enabled}");
                
                // Try to get a bone
                var neckBone = animator.GetBoneTransform(HumanBodyBones.Neck);
                if (neckBone != null) {
                    Debug.Log($"SUCCESS: Found neck bone '{neckBone.name}'");
                    Debug.Log($"Neck bone position: {neckBone.position}");
                    Debug.Log($"Neck bone rotation: {neckBone.rotation.eulerAngles}");
                } else {
                    Debug.Log("ERROR: Neck bone NOT found in animator");
                }
            } else {
                Debug.Log("ERROR: DebugModel has NO Animator component");
            }
        } else {
            Debug.Log("ERROR: DebugModel NOT found in scene");
        }
        
        // Look for any Animator in scene
        Debug.Log("=== ALL ANIMATORS IN SCENE ===");
        Animator[] animators = FindObjectsOfType<Animator>();
        Debug.Log($"Found {animators.Length} Animator components");
        
        for (int i = 0; i < animators.Length; i++) {
            Debug.Log($"Animator {i}: {animators[i].name} (enabled: {animators[i].enabled})");
        }
        
        Debug.Log("=== SCENE ANALYSIS END ===");
    }
    
    void Update() {
        // Press SPACE to run analysis again
        if (Input.GetKeyDown(KeyCode.Space)) {
            Start();
        }
    }
} 