using UnityEngine;
using HardCoded.VRigUnity;

public class SimpleAvatarTest : MonoBehaviour {
    private Animator targetAnimator;
    private Transform neckBone;
    private float time = 0f;
    private VRMAnimator vrmAnimator;
    private RigAnimator rigAnimator;
    
    void Start() {
        // Find the DebugModel
        GameObject debugModel = GameObject.Find("DebugModel");
        if (debugModel != null) {
            targetAnimator = debugModel.GetComponent<Animator>();
            if (targetAnimator != null) {
                neckBone = targetAnimator.GetBoneTransform(HumanBodyBones.Neck);
                if (neckBone != null) {
                    Debug.Log($"[SimpleTest] Found neck bone: {neckBone.name}");
                    
                    // Disable VRigUnity components that interfere
                    vrmAnimator = debugModel.GetComponent<VRMAnimator>();
                    rigAnimator = debugModel.GetComponent<RigAnimator>();
                    
                    if (vrmAnimator != null) {
                        vrmAnimator.enabled = false;
                        Debug.Log("[SimpleTest] Disabled VRMAnimator");
                    }
                    if (rigAnimator != null) {
                        rigAnimator.enabled = false;
                        Debug.Log("[SimpleTest] Disabled RigAnimator");
                    }
                } else {
                    Debug.LogError("[SimpleTest] Neck bone not found!");
                }
            } else {
                Debug.LogError("[SimpleTest] No Animator found on DebugModel!");
            }
        } else {
            Debug.LogError("[SimpleTest] DebugModel not found!");
        }
    }
    
    void Update() {
        if (neckBone != null) {
            time += Time.deltaTime;
            // Simple neck rotation animation
            float rotationX = Mathf.Sin(time) * 20f; // -20 to +20 degrees
            neckBone.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        
        // Press T to test manual neck animation
        if (Input.GetKeyDown(KeyCode.T)) {
            TestNeckAnimation();
        }
    }
    
    void TestNeckAnimation() {
        if (neckBone != null) {
            Debug.Log("[SimpleTest] Testing neck animation...");
            neckBone.localRotation = Quaternion.Euler(30f, 0, 0);
        }
    }
    
    void OnDestroy() {
        // Re-enable components when script is destroyed
        if (vrmAnimator != null) {
            vrmAnimator.enabled = true;
        }
        if (rigAnimator != null) {
            rigAnimator.enabled = true;
        }
    }
} 