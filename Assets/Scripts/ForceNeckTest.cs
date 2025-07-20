using UnityEngine;
using HardCoded.VRigUnity;

public class ForceNeckTest : MonoBehaviour {
    private Transform neckBone;
    private bool testActive = false;
    
    void Start() {
        // Find ALL GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"[ForceNeckTest] Found {allObjects.Length} GameObjects in scene");
        
        foreach (var obj in allObjects) {
            if (obj.name.Contains("Debug") || obj.name.Contains("Model") || obj.name.Contains("Avatar")) {
                Debug.Log($"[ForceNeckTest] Found object: {obj.name} at {obj.transform.position}");
                
                var animator = obj.GetComponent<Animator>();
                if (animator != null) {
                    Debug.Log($"[ForceNeckTest] Object {obj.name} HAS Animator!");
                    
                    // Try to get neck bone
                    var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                    if (neck != null) {
                        neckBone = neck;
                        Debug.Log($"[ForceNeckTest] SUCCESS! Found neck bone: {neck.name} on {obj.name}");
                        
                        // Disable interfering components
                        var vrm = obj.GetComponent<VRMAnimator>();
                        var rig = obj.GetComponent<RigAnimator>();
                        if (vrm != null) {
                            vrm.enabled = false;
                            Debug.Log("[ForceNeckTest] Disabled VRMAnimator");
                        }
                        if (rig != null) {
                            rig.enabled = false;
                            Debug.Log("[ForceNeckTest] Disabled RigAnimator");
                        }
                        
                        testActive = true;
                        break;
                    } else {
                        Debug.Log($"[ForceNeckTest] Object {obj.name} animator has NO neck bone");
                    }
                } else {
                    Debug.Log($"[ForceNeckTest] Object {obj.name} has NO Animator");
                }
            }
        }
        
        if (!testActive) {
            Debug.LogError("[ForceNeckTest] FAILED to find any avatar with neck bone!");
        }
    }
    
    void Update() {
        if (testActive && neckBone != null) {
            // Force neck to specific rotation every frame
            neckBone.localRotation = Quaternion.Euler(45f, 0, 0);
            
            // Log every 60 frames
            if (Time.frameCount % 60 == 0) {
                Debug.Log($"[ForceNeckTest] Frame {Time.frameCount}: Setting neck to 45 degrees. Current rotation: {neckBone.localRotation.eulerAngles}");
                Debug.Log($"[ForceNeckTest] Neck position: {neckBone.position}, Parent: {neckBone.parent?.name}");
            }
        }
        
        // Press F to toggle test
        if (Input.GetKeyDown(KeyCode.F)) {
            testActive = !testActive;
            Debug.Log($"[ForceNeckTest] Test {(testActive ? "ENABLED" : "DISABLED")}");
        }
        
        // Press G to try different rotation
        if (Input.GetKeyDown(KeyCode.G) && neckBone != null) {
            var randomRotation = new Vector3(
                Random.Range(-45f, 45f),
                Random.Range(-45f, 45f),
                Random.Range(-45f, 45f)
            );
            neckBone.localRotation = Quaternion.Euler(randomRotation);
            Debug.Log($"[ForceNeckTest] Applied random rotation: {randomRotation}");
        }
    }
    
    void OnGUI() {
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 100, 400, 20), $"ForceNeckTest: {(testActive ? "ACTIVE" : "INACTIVE")}");
        if (neckBone != null) {
            GUI.Label(new Rect(10, 120, 400, 20), $"Neck: {neckBone.name} - Rotation: {neckBone.localRotation.eulerAngles}");
        }
        GUI.Label(new Rect(10, 140, 400, 20), "Press F to toggle, G for random rotation");
        GUI.color = Color.white;
    }
} 