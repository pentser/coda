using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleFileBrowser;

namespace HardCoded.VRigUnity {
    public class DirectAvatarPlayback : MonoBehaviour {
        [Header("Direct Avatar Animation")]
        [SerializeField] private string captureFileName = "test_5frames.json";
        [SerializeField] private float playbackSpeed = 1.0f;
        [SerializeField] private bool autoFindAvatar = true;
        [SerializeField] private Animator targetAnimator;
        
        private CaptureAvatarData loadedData;
        private bool isPlaying = false;
        private bool isPaused = false;
        private int currentFrameIndex = 0;
        private float playbackStartTime;
        private Coroutine playbackCoroutine;
        
        // Bone mapping for direct animation
        private Dictionary<HumanBodyBones, Transform> boneMap = new Dictionary<HumanBodyBones, Transform>();
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        
        // VRigUnity components that interfere with direct bone animation
        private VRMAnimator vrmAnimator;
        private RigAnimator rigAnimator;
        private bool originalVRMAnimatorEnabled;
        private bool originalRigAnimatorEnabled;
        
        void Start() {
            LoadCaptureFile();
            FindAndSetupAvatar();
        }
        
        void Update() {
            HandleInput();
        }
        
        private void HandleInput() {
            if (Input.GetKeyDown(KeyCode.P)) {
                if (isPlaying && !isPaused) {
                    PausePlayback();
                } else if (isPaused) {
                    ResumePlayback();
                } else {
                    StartPlayback();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.O)) {
                StopPlayback();
            }
            
            if (Input.GetKeyDown(KeyCode.L)) {
                LoadNewFile();
            }
        }
        
        private void FindAndSetupAvatar() {
            if (targetAnimator == null && autoFindAvatar) {
                // Try to find DebugModel or any avatar with Animator
                GameObject debugModel = GameObject.Find("DebugModel");
                if (debugModel != null) {
                    targetAnimator = debugModel.GetComponent<Animator>();
                    Debug.Log($"[DirectPlayback] Found DebugModel: {debugModel.name}");
                }
                
                if (targetAnimator == null) {
                    // Look for any Animator in the scene
                    targetAnimator = FindObjectOfType<Animator>();
                    if (targetAnimator != null) {
                        Debug.Log($"[DirectPlayback] Found Animator on: {targetAnimator.name}");
                    }
                }
            }
            
            if (targetAnimator != null) {
                SetupBoneMapping();
                // Store initial transform
                initialPosition = targetAnimator.transform.position;
                initialRotation = targetAnimator.transform.rotation;
                
                // Find and store VRigUnity components that might interfere
                vrmAnimator = targetAnimator.GetComponent<VRMAnimator>();
                rigAnimator = targetAnimator.GetComponent<RigAnimator>();
                
                if (vrmAnimator != null) {
                    Debug.Log($"[DirectPlayback] Found VRMAnimator - will disable during playback");
                }
                if (rigAnimator != null) {
                    Debug.Log($"[DirectPlayback] Found RigAnimator - will disable during playback");
                }
                
                Debug.Log($"[DirectPlayback] Avatar setup complete. Found {boneMap.Count} bones.");
            } else {
                Debug.LogError("[DirectPlayback] No avatar with Animator found! Make sure your avatar is in the scene.");
            }
        }
        
        private void SetupBoneMapping() {
            boneMap.Clear();
            
            // Map all important bones
            var bonesToMap = new HumanBodyBones[] {
                HumanBodyBones.Hips,
                HumanBodyBones.Spine,
                HumanBodyBones.UpperChest,
                HumanBodyBones.Neck,
                HumanBodyBones.Head,
                
                // Arms
                HumanBodyBones.LeftShoulder,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LeftHand,
                HumanBodyBones.RightShoulder,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.RightHand,
                
                // Legs
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.RightFoot
            };
            
            foreach (var bone in bonesToMap) {
                Transform boneTransform = targetAnimator.GetBoneTransform(bone);
                if (boneTransform != null) {
                    boneMap[bone] = boneTransform;
                    Debug.Log($"[DirectPlayback] Mapped bone: {bone} -> {boneTransform.name}");
                } else {
                    Debug.LogWarning($"[DirectPlayback] Bone not found: {bone}");
                }
            }
        }
        
        private void LoadCaptureFile() {
            string filePath = Path.Combine(Application.dataPath, "captureAvatar", captureFileName);
            if (File.Exists(filePath)) {
                try {
                    string jsonContent = File.ReadAllText(filePath);
                    loadedData = JsonUtility.FromJson<CaptureAvatarData>(jsonContent);
                    Debug.Log($"[DirectPlayback] Loaded {loadedData.frameCount} frames from {captureFileName}");
                } catch (Exception e) {
                    Debug.LogError($"[DirectPlayback] Failed to load capture file: {e.Message}");
                }
            } else {
                Debug.LogError($"[DirectPlayback] Capture file not found: {filePath}");
            }
        }
        
        private void LoadNewFile() {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("JSON", ".json"));
            FileBrowser.SetDefaultFilter(".json");
            
            FileBrowser.ShowLoadDialog((paths) => {
                if (paths.Length > 0) {
                    LoadFileFromPath(paths[0]);
                }
            }, null, FileBrowser.PickMode.Files, false, null, null, "Load Avatar Capture", "Load");
        }
        
        private void LoadFileFromPath(string filePath) {
            try {
                string jsonContent = File.ReadAllText(filePath);
                loadedData = JsonUtility.FromJson<CaptureAvatarData>(jsonContent);
                Debug.Log($"[DirectPlayback] Loaded {loadedData.frameCount} frames from {Path.GetFileName(filePath)}");
                StopPlayback();
            } catch (Exception e) {
                Debug.LogError($"[DirectPlayback] Failed to load file: {e.Message}");
            }
        }
        
        private void StartPlayback() {
            if (loadedData == null || targetAnimator == null) {
                Debug.LogError("[DirectPlayback] Cannot start playback - missing data or avatar!");
                return;
            }
            
            Debug.Log($"[DirectPlayback] Starting direct bone animation playback - {loadedData.frameCount} frames");
            
            // Disable VRigUnity components that interfere with direct bone animation
            DisableVRigComponents();
            
            isPlaying = true;
            isPaused = false;
            currentFrameIndex = 0;
            playbackStartTime = Time.time;
            
            if (playbackCoroutine != null) {
                StopCoroutine(playbackCoroutine);
            }
            
            playbackCoroutine = StartCoroutine(DirectPlaybackCoroutine());
        }
        
        private void PausePlayback() {
            isPaused = true;
            Debug.Log("[DirectPlayback] Playback paused");
        }
        
        private void ResumePlayback() {
            isPaused = false;
            float frameInterval = loadedData.frameCount > 1 ? loadedData.totalDuration / (loadedData.frameCount - 1) : 0f;
            playbackStartTime = Time.time - (currentFrameIndex * frameInterval);
            Debug.Log("[DirectPlayback] Playback resumed");
        }
        
        private void StopPlayback() {
            isPlaying = false;
            isPaused = false;
            
            if (playbackCoroutine != null) {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }
            
            // Re-enable VRigUnity components
            RestoreVRigComponents();
            
            // Reset avatar to initial position
            if (targetAnimator != null) {
                targetAnimator.transform.position = initialPosition;
                targetAnimator.transform.rotation = initialRotation;
            }
            
            Debug.Log("[DirectPlayback] Playback stopped");
        }
        
        private IEnumerator DirectPlaybackCoroutine() {
            while (isPlaying && currentFrameIndex < loadedData.frames.Count) {
                if (isPaused) {
                    yield return null;
                    continue;
                }
                
                var frame = loadedData.frames[currentFrameIndex];
                float targetTime = (Time.time - playbackStartTime) * playbackSpeed;
                
                // Wait until it's time for this frame
                if (frame.pose.timestamp > targetTime) {
                    yield return null;
                    continue;
                }
                
                // Apply the frame data directly to bones
                ApplyFrameDirectly(frame);
                
                currentFrameIndex++;
                
                // Debug every 60 frames
                if (currentFrameIndex % 60 == 0) {
                    Debug.Log($"[DirectPlayback] Frame {currentFrameIndex}/{loadedData.frameCount} - Direct bone animation");
                }
                
                yield return null;
            }
            
            Debug.Log("[DirectPlayback] Playback completed!");
            isPlaying = false;
        }
        
        private void ApplyFrameDirectly(SerializableAvatarFrame frame) {
            var pose = frame.pose;
            
            Debug.Log($"[DirectPlayback] Applying frame - Neck: {pose.neckRotation}, Chest: {pose.chestRotation}, Hips: {pose.hipsRotation}");
            
            // Apply rotations directly to bones
            ApplyBoneRotation(HumanBodyBones.Neck, pose.neckRotation);
            ApplyBoneRotation(HumanBodyBones.Spine, pose.chestRotation);
            ApplyBoneRotation(HumanBodyBones.Hips, pose.hipsRotation);
            
            // Apply hip position (root motion)
            if (boneMap.ContainsKey(HumanBodyBones.Hips)) {
                var hipsTransform = boneMap[HumanBodyBones.Hips];
                hipsTransform.localPosition = pose.hipsPosition;
            }
            
            // Apply arm rotations based on positions (convert positions to rotations)
            ApplyArmFromPositions(pose);
            
            // Apply leg rotations
            ApplyBoneRotation(HumanBodyBones.RightUpperLeg, pose.rightUpperLegRotation);
            ApplyBoneRotation(HumanBodyBones.RightLowerLeg, pose.rightLowerLegRotation);
            ApplyBoneRotation(HumanBodyBones.LeftUpperLeg, pose.leftUpperLegRotation);
            ApplyBoneRotation(HumanBodyBones.LeftLowerLeg, pose.leftLowerLegRotation);
            
            // Apply hand data
            if (frame.rightHand != null) {
                ApplyHandDirectly(frame.rightHand, true);
            }
            if (frame.leftHand != null) {
                ApplyHandDirectly(frame.leftHand, false);
            }
        }
        
        private void ApplyBoneRotation(HumanBodyBones bone, Vector3 eulerRotation) {
            if (boneMap.ContainsKey(bone) && boneMap[bone] != null) {
                var oldRotation = boneMap[bone].localRotation;
                boneMap[bone].localRotation = Quaternion.Euler(eulerRotation);
                Debug.Log($"[DirectPlayback] Applied {bone}: {eulerRotation} to {boneMap[bone].name}");
            } else {
                Debug.LogWarning($"[DirectPlayback] Bone {bone} not found or null in bone map");
            }
        }
        
        private void ApplyArmFromPositions(SerializablePoseFrame pose) {
            // Simple arm IK - point arms toward hand positions
            if (boneMap.ContainsKey(HumanBodyBones.RightUpperArm) && boneMap.ContainsKey(HumanBodyBones.RightHand)) {
                var shoulderPos = boneMap[HumanBodyBones.RightShoulder]?.position ?? boneMap[HumanBodyBones.RightUpperArm].position;
                var handTargetPos = shoulderPos + pose.rightHandPosition;
                var direction = (handTargetPos - shoulderPos).normalized;
                
                if (direction.magnitude > 0.1f) {
                    var targetRotation = Quaternion.LookRotation(direction);
                    boneMap[HumanBodyBones.RightUpperArm].rotation = targetRotation;
                }
            }
            
            if (boneMap.ContainsKey(HumanBodyBones.LeftUpperArm) && boneMap.ContainsKey(HumanBodyBones.LeftHand)) {
                var shoulderPos = boneMap[HumanBodyBones.LeftShoulder]?.position ?? boneMap[HumanBodyBones.LeftUpperArm].position;
                var handTargetPos = shoulderPos + pose.leftHandPosition;
                var direction = (handTargetPos - shoulderPos).normalized;
                
                if (direction.magnitude > 0.1f) {
                    var targetRotation = Quaternion.LookRotation(direction);
                    boneMap[HumanBodyBones.LeftUpperArm].rotation = targetRotation;
                }
            }
        }
        
        private void ApplyHandDirectly(SerializableHandFrame hand, bool isRightHand) {
            var wristBone = isRightHand ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            ApplyBoneRotation(wristBone, hand.wristRotation);
            
            // Note: Finger bones are not part of HumanBodyBones, so we'd need to find them manually
            // For now, just apply wrist rotation
        }
        
        private void DisableVRigComponents() {
            if (vrmAnimator != null) {
                originalVRMAnimatorEnabled = vrmAnimator.enabled;
                vrmAnimator.enabled = false;
                Debug.Log("[DirectPlayback] Disabled VRMAnimator for direct bone control");
            }
            
            if (rigAnimator != null) {
                originalRigAnimatorEnabled = rigAnimator.enabled;
                rigAnimator.enabled = false;
                Debug.Log("[DirectPlayback] Disabled RigAnimator for direct bone control");
            }
        }
        
        private void RestoreVRigComponents() {
            if (vrmAnimator != null) {
                vrmAnimator.enabled = originalVRMAnimatorEnabled;
                Debug.Log("[DirectPlayback] Restored VRMAnimator");
            }
            
            if (rigAnimator != null) {
                rigAnimator.enabled = originalRigAnimatorEnabled;
                Debug.Log("[DirectPlayback] Restored RigAnimator");
            }
        }
        
        void OnGUI() {
            if (loadedData == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 100));
            
            string status;
            if (isPlaying && !isPaused) {
                float progress = (float)currentFrameIndex / loadedData.frameCount;
                status = $"PLAYING {progress:P0} - Frame {currentFrameIndex}/{loadedData.frameCount}";
            } else if (isPaused) {
                status = $"PAUSED - Frame {currentFrameIndex}/{loadedData.frameCount}";
            } else {
                status = $"READY - {loadedData.frameCount} frames loaded";
            }
            
            GUILayout.Label($"Direct Avatar Playback: {status}");
            GUILayout.Label("P = Play/Pause | O = Stop | L = Load File");
            GUILayout.Label($"Avatar: {(targetAnimator ? targetAnimator.name : "None")} | Bones: {boneMap.Count}");
            
            GUILayout.EndArea();
        }
    }
} 