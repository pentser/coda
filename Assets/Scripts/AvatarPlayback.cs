using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleFileBrowser;

namespace HardCoded.VRigUnity {
    public class AvatarPlayback : MonoBehaviour {
        [Header("Playback Settings")]
        [SerializeField] private bool autoLoadFile = true;
        [SerializeField] private string defaultFileName = "avatar_capture_20250717_184050.json";
        [SerializeField] private bool showPlaybackStatus = true;
        [SerializeField] private float playbackSpeed = 1.0f;

        private CaptureAvatarData loadedData;
        private bool isPlaying = false;
        private bool isPaused = false;
        private float playbackStartTime;
        private int currentFrameIndex = 0;
        private HolisticSolution holisticSolution;
        private Coroutine playbackCoroutine;

        private void Start() {
            holisticSolution = SolutionUtils.GetSolution();
            if (holisticSolution == null) {
                Debug.LogError("[AvatarPlayback] Could not find HolisticSolution in scene. Make sure the GameObject has the 'Solution' tag.");
                return;
            }

            if (autoLoadFile) {
                LoadDefaultFile();
            }
        }

        private void Update() {
            HandleInput();
        }

        private void HandleInput() {
            // Play/Pause/Stop playback with P key
            if (Input.GetKeyDown(KeyCode.P)) {
                if (loadedData == null) {
                    Debug.LogWarning("[AvatarPlayback] No capture data loaded. Load a file first with L key.");
                    return;
                }

                if (!isPlaying) {
                    StartPlayback();
                } else if (isPaused) {
                    ResumePlayback();
                } else {
                    PausePlayback();
                }
            }

            // Stop playback with O key
            if (Input.GetKeyDown(KeyCode.O)) {
                StopPlayback();
            }

            // Load file with L key
            if (Input.GetKeyDown(KeyCode.L)) {
                LoadCaptureFile();
            }

            // Speed controls
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus)) {
                playbackSpeed = Mathf.Min(playbackSpeed + 0.1f, 3.0f);
                Debug.Log($"[AvatarPlayback] Playback speed: {playbackSpeed:F1}x");
            }

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) {
                playbackSpeed = Mathf.Max(playbackSpeed - 0.1f, 0.1f);
                Debug.Log($"[AvatarPlayback] Playback speed: {playbackSpeed:F1}x");
            }
        }

        private void LoadDefaultFile() {
            string filePath = Path.Combine(Application.dataPath, "captureAvatar", defaultFileName);
            if (File.Exists(filePath)) {
                LoadFile(filePath);
            } else {
                Debug.LogWarning($"[AvatarPlayback] Default file not found: {filePath}");
            }
        }

        private void LoadCaptureFile() {
            string initialPath = Path.Combine(Application.dataPath, "captureAvatar");
            
            FileBrowser.SetFilters(true, new FileBrowser.Filter("JSON Files", ".json"));
            FileBrowser.SetDefaultFilter(".json");
            FileBrowser.ShowLoadDialog(
                (paths) => { LoadFile(paths[0]); },
                () => { Debug.Log("[AvatarPlayback] File loading cancelled."); },
                FileBrowser.PickMode.Files,
                false,
                initialPath,
                null,
                "Load Avatar Capture File",
                "Load"
            );
        }

        private void LoadFile(string filePath) {
            try {
                string jsonContent = File.ReadAllText(filePath);
                loadedData = JsonUtility.FromJson<CaptureAvatarData>(jsonContent);
                
                Debug.Log($"[AvatarPlayback] Successfully loaded capture data from: {Path.GetFileName(filePath)}");
                Debug.Log($"[AvatarPlayback] Frames: {loadedData.frameCount}, Duration: {loadedData.totalDuration:F2}s");
                Debug.Log($"[AvatarPlayback] Press P to play, O to stop, L to load another file");
                
                // Check if avatar model is available
                if (holisticSolution != null && holisticSolution.Model != null) {
                    Debug.Log($"[AvatarPlayback] Avatar model found: {holisticSolution.Model.VrmModel?.name}");
                    Debug.Log($"[AvatarPlayback] Model is prepared: {holisticSolution.Model.IsPrepared}");
                    Debug.Log($"[AvatarPlayback] Model is visible: {holisticSolution.Model.IsVisible}");
                } else {
                    Debug.LogWarning("[AvatarPlayback] No avatar model found in HolisticSolution!");
                }
                
                // Reset playback state
                StopPlayback();
            }
            catch (Exception e) {
                Debug.LogError($"[AvatarPlayback] Failed to load file: {e.Message}");
                Debug.LogError($"[AvatarPlayback] File path: {filePath}");
            }
        }

        private void StartPlayback() {
            if (loadedData == null || holisticSolution == null) return;

            Debug.Log($"[AvatarPlayback] Starting playback of {loadedData.frameCount} frames");
            
            // Ensure hands are tracked during playback
            holisticSolution.TrackLeftHand = true;
            holisticSolution.TrackRightHand = true;
            
            // Ensure the model is visible and bone settings are enabled
            if (holisticSolution.Model != null) {
                holisticSolution.Model.IsVisible = true;
                Debug.Log("[AvatarPlayback] Avatar model visibility enabled");
                Debug.Log($"[AvatarPlayback] VRM Model active: {holisticSolution.Model.VrmModel.activeInHierarchy}");
                Debug.Log($"[AvatarPlayback] Solution IsPaused: {holisticSolution.IsPaused}");
                
                // Check VRM model details
                var vrmModel = holisticSolution.Model.VrmModel;
                if (vrmModel != null) {
                    Debug.Log($"[AvatarPlayback] VRM Model name: {vrmModel.name}");
                    var animator = vrmModel.GetComponent<Animator>();
                    Debug.Log($"[AvatarPlayback] VRM Animator present: {animator != null}");
                    if (animator != null) {
                        Debug.Log($"[AvatarPlayback] Animator enabled: {animator.enabled}");
                        Debug.Log($"[AvatarPlayback] Avatar root bone: {animator.GetBoneTransform(HumanBodyBones.Hips)?.name}");
                    }
                } else {
                    Debug.LogError("[AvatarPlayback] VRM Model is NULL!");
                }
                
                // Check if SceneModel is properly connected
                var sceneModel = holisticSolution.Model;
                Debug.Log($"[AvatarPlayback] SceneModel exists: {sceneModel != null}");
                if (sceneModel != null) {
                    Debug.Log($"[AvatarPlayback] SceneModel.IsVisible: {sceneModel.IsVisible}");
                    Debug.Log($"[AvatarPlayback] SceneModel type: {sceneModel.GetType().Name}");
                }
            } else {
                Debug.LogError("[AvatarPlayback] CRITICAL: holisticSolution.Model is NULL! Avatar cannot move without a model.");
                
                // Try to find DebugModel manually in the scene
                var debugModel = GameObject.Find("DebugModel");
                if (debugModel != null) {
                    Debug.Log($"[AvatarPlayback] Found DebugModel in scene at: {debugModel.transform.position}");
                    Debug.Log($"[AvatarPlayback] DebugModel active: {debugModel.activeSelf}");
                } else {
                    Debug.LogError("[AvatarPlayback] DebugModel not found in scene! Make sure your avatar is loaded.");
                }
                
                // Log current bone settings
                Debug.Log($"[AvatarPlayback] Bone Settings - Neck: {BoneSettings.Get(BoneSettings.NECK)}, " +
                         $"Chest: {BoneSettings.Get(BoneSettings.CHEST)}, " +
                         $"Hips: {BoneSettings.Get(BoneSettings.HIPS)}, " +
                         $"Left Arm: {BoneSettings.Get(BoneSettings.LEFT_ARM)}, " +
                         $"Right Arm: {BoneSettings.Get(BoneSettings.RIGHT_ARM)}, " +
                         $"Left Hand: {BoneSettings.Get(BoneSettings.LEFT_WRIST)}, " +
                         $"Right Hand: {BoneSettings.Get(BoneSettings.RIGHT_WRIST)}");
                
                // Check if bone settings are mostly disabled
                bool anyBonesEnabled = BoneSettings.Get(BoneSettings.NECK) || 
                                     BoneSettings.Get(BoneSettings.CHEST) || 
                                     BoneSettings.Get(BoneSettings.HIPS) ||
                                     BoneSettings.Get(BoneSettings.LEFT_ARM) ||
                                     BoneSettings.Get(BoneSettings.RIGHT_ARM);
                
                if (!anyBonesEnabled) {
                    Debug.LogWarning("[AvatarPlayback] WARNING: Most bone settings appear to be disabled! " +
                                   "Avatar may not move. Check your bone settings in the UI.");
                    
                    // Temporary fix: Force enable key bone settings for playback
                    Debug.Log("[AvatarPlayback] Force enabling bone settings for playback...");
                    BoneSettings.Set(BoneSettings.NECK, true);
                    BoneSettings.Set(BoneSettings.CHEST, true);
                    BoneSettings.Set(BoneSettings.HIPS, true);
                    BoneSettings.Set(BoneSettings.LEFT_ARM, true);
                    BoneSettings.Set(BoneSettings.RIGHT_ARM, true);
                    BoneSettings.Set(BoneSettings.LEFT_WRIST, true);
                    BoneSettings.Set(BoneSettings.RIGHT_WRIST, true);
                }
            }
            
            isPlaying = true;
            isPaused = false;
            currentFrameIndex = 0;
            playbackStartTime = Time.time;
            
            if (playbackCoroutine != null) {
                StopCoroutine(playbackCoroutine);
            }
            
            playbackCoroutine = StartCoroutine(PlaybackCoroutine());
        }

        private void PausePlayback() {
            isPaused = true;
            Debug.Log("[AvatarPlayback] Playback paused. Press P to resume.");
        }

        private void ResumePlayback() {
            isPaused = false;
            // Adjust start time to account for pause duration
            playbackStartTime = Time.time - (currentFrameIndex > 0 ? loadedData.frames[currentFrameIndex].pose.timestamp / playbackSpeed : 0);
            Debug.Log("[AvatarPlayback] Playback resumed.");
        }

        private void StopPlayback() {
            isPlaying = false;
            isPaused = false;
            currentFrameIndex = 0;
            
            if (playbackCoroutine != null) {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }
            
            Debug.Log("[AvatarPlayback] Playback stopped.");
        }

        private IEnumerator PlaybackCoroutine() {
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

                // Apply the frame data to the holistic solution
                float currentTime = holisticSolution.TimeNow;
                ApplyFrameData(frame, currentTime);
                
                // Force update and animate the model
                holisticSolution.UpdateModel();
                holisticSolution.AnimateModel();
                
                // Debug: Check if transforms are actually changing
                if (currentFrameIndex % 60 == 0 && holisticSolution.Model?.VrmModel != null) {
                    var animator = holisticSolution.Model.VrmModel.GetComponent<Animator>();
                    if (animator != null) {
                        var neckTransform = animator.GetBoneTransform(HumanBodyBones.Neck);
                        var chestTransform = animator.GetBoneTransform(HumanBodyBones.Chest);
                        if (neckTransform != null && chestTransform != null) {
                            Debug.Log($"[AvatarPlayback] Frame {currentFrameIndex} - " +
                                     $"Neck transform: {neckTransform.rotation.eulerAngles}, " +
                                     $"Chest transform: {chestTransform.rotation.eulerAngles}");
                        }
                    }
                }
                
                // Debug info every 30 frames
                if (currentFrameIndex % 30 == 0) {
                    Debug.Log($"[AvatarPlayback] Frame {currentFrameIndex}: Applying pose data - " +
                             $"Chest rotation: {frame.pose.chestRotation}, " +
                             $"Right hand pos: {frame.pose.rightHandPosition}");
                    Debug.Log($"[AvatarPlayback] Current Pose values - " +
                             $"Chest: {holisticSolution.Pose.Chest.Current.eulerAngles}, " +
                             $"Neck: {holisticSolution.Pose.Neck.Current.eulerAngles}");
                }
                
                // Force check if model is still active and not paused
                if (!holisticSolution.Model.VrmModel.activeInHierarchy) {
                    Debug.LogWarning("[AvatarPlayback] VRM Model became inactive during playback!");
                }
                if (holisticSolution.IsPaused) {
                    Debug.LogWarning("[AvatarPlayback] Solution became paused during playback!");
                }
                
                currentFrameIndex++;
                
                // Small delay to prevent overwhelming the system
                yield return null;
            }

            // Playback finished
            if (currentFrameIndex >= loadedData.frames.Count) {
                Debug.Log("[AvatarPlayback] Playback completed.");
                isPlaying = false;
            }
        }

        private void ApplyFrameData(SerializableAvatarFrame frame, float time) {
            // Apply pose data
            ApplyPoseData(frame.pose, time);
            
            // Apply hand data
            ApplyHandData(frame.rightHand, holisticSolution.RightHand, time);
            ApplyHandData(frame.leftHand, holisticSolution.LeftHand, time);
            
            // Apply face data
            ApplyFaceData(frame.face, time);
        }

        private void ApplyPoseData(SerializablePoseFrame poseFrame, float time) {
            var pose = holisticSolution.Pose;
            
            // Debug first frame
            if (currentFrameIndex == 0) {
                Debug.Log($"[AvatarPlayback] First frame pose data - " +
                         $"Neck: {poseFrame.neckRotation}, " +
                         $"Chest: {poseFrame.chestRotation}, " +
                         $"Time: {time}");
            }
            
            // Debug every 60 frames to check if values are being applied
            if (currentFrameIndex % 60 == 0) {
                Debug.Log($"[AvatarPlayback] Frame {currentFrameIndex} - Before: Chest={pose.Chest.Current.eulerAngles}");
            }
            
            // Apply rotations
            pose.Neck.Add(Quaternion.Euler(poseFrame.neckRotation), time);
            pose.Chest.Add(Quaternion.Euler(poseFrame.chestRotation), time);
            pose.Hips.Add(Quaternion.Euler(poseFrame.hipsRotation), time);
            
            // Apply positions
            pose.HipsPosition.Add(poseFrame.hipsPosition, time);
            pose.RightShoulder.Add(poseFrame.rightShoulderPosition, time);
            pose.RightElbow.Add(poseFrame.rightElbowPosition, time);
            pose.RightHand.Add(poseFrame.rightHandPosition, time);
            pose.LeftShoulder.Add(poseFrame.leftShoulderPosition, time);
            pose.LeftElbow.Add(poseFrame.leftElbowPosition, time);
            pose.LeftHand.Add(poseFrame.leftHandPosition, time);
            
            // Apply leg rotations
            if (Settings.UseLegRotation) {
                pose.RightUpperLeg.Add(Quaternion.Euler(poseFrame.rightUpperLegRotation), time);
                pose.RightLowerLeg.Add(Quaternion.Euler(poseFrame.rightLowerLegRotation), time);
                pose.LeftUpperLeg.Add(Quaternion.Euler(poseFrame.leftUpperLegRotation), time);
                pose.LeftLowerLeg.Add(Quaternion.Euler(poseFrame.leftLowerLegRotation), time);
            }
            
            // Debug every 60 frames to check if values were actually applied
            if (currentFrameIndex % 60 == 0) {
                Debug.Log($"[AvatarPlayback] Frame {currentFrameIndex} - After: Chest={pose.Chest.Current.eulerAngles}, " +
                         $"Input was: {poseFrame.chestRotation}");
            }
        }

        private void ApplyHandData(SerializableHandFrame handFrame, HandValues handValues, float time) {
            // Apply wrist rotation
            handValues.Wrist.Add(Quaternion.Euler(handFrame.wristRotation), time);
            
            // Apply finger rotations
            handValues.IndexPip.Add(Quaternion.Euler(handFrame.indexPipRotation), time);
            handValues.IndexDip.Add(Quaternion.Euler(handFrame.indexDipRotation), time);
            handValues.IndexTip.Add(Quaternion.Euler(handFrame.indexTipRotation), time);
            
            handValues.MiddlePip.Add(Quaternion.Euler(handFrame.middlePipRotation), time);
            handValues.MiddleDip.Add(Quaternion.Euler(handFrame.middleDipRotation), time);
            handValues.MiddleTip.Add(Quaternion.Euler(handFrame.middleTipRotation), time);
            
            handValues.RingPip.Add(Quaternion.Euler(handFrame.ringPipRotation), time);
            handValues.RingDip.Add(Quaternion.Euler(handFrame.ringDipRotation), time);
            handValues.RingTip.Add(Quaternion.Euler(handFrame.ringTipRotation), time);
            
            handValues.PinkyPip.Add(Quaternion.Euler(handFrame.pinkyPipRotation), time);
            handValues.PinkyDip.Add(Quaternion.Euler(handFrame.pinkyDipRotation), time);
            handValues.PinkyTip.Add(Quaternion.Euler(handFrame.pinkyTipRotation), time);
            
            handValues.ThumbPip.Add(Quaternion.Euler(handFrame.thumbPipRotation), time);
            handValues.ThumbDip.Add(Quaternion.Euler(handFrame.thumbDipRotation), time);
            handValues.ThumbTip.Add(Quaternion.Euler(handFrame.thumbTipRotation), time);
        }

        private void ApplyFaceData(SerializableFaceFrame faceFrame, float time) {
            var face = holisticSolution.Face;
            
            // Set mouth open value
            face.mouthOpen = faceFrame.mouthOpen;
            
            // Set eye iris positions
            face.lEyeIris.Add(faceFrame.leftEyeIris);
            face.rEyeIris.Add(faceFrame.rightEyeIris);
            
            // Set eye open values
            face.lEyeOpen.Add(faceFrame.leftEyeOpen);
            face.rEyeOpen.Add(faceFrame.rightEyeOpen);
        }

        private void OnGUI() {
            if (!showPlaybackStatus) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;

            string status;
            if (loadedData == null) {
                style.normal.textColor = Color.yellow;
                status = "No capture data loaded. Press L to load file.";
            } else if (isPlaying && !isPaused) {
                style.normal.textColor = Color.green;
                float progress = currentFrameIndex / (float)loadedData.frameCount * 100f;
                status = $"PLAYING... Frame {currentFrameIndex}/{loadedData.frameCount} ({progress:F1}%) Speed: {playbackSpeed:F1}x";
            } else if (isPaused) {
                style.normal.textColor = new Color(1.0f, 0.5f, 0.0f); // Orange color
                status = $"PAUSED - Frame {currentFrameIndex}/{loadedData.frameCount}";
            } else {
                style.normal.textColor = Color.white;
                status = $"Ready to play {loadedData.frameCount} frames ({loadedData.totalDuration:F2}s)";
            }

            GUI.Label(new Rect(10, 70, 600, 30), status, style);

            // Show controls
            style.fontSize = 12;
            style.normal.textColor = Color.gray;
            GUI.Label(new Rect(10, 100, 800, 20), "P = Play/Pause | O = Stop | L = Load File | +/- = Speed Control", style);
            
            if (loadedData != null) {
                GUI.Label(new Rect(10, 120, 600, 20), $"Loaded: {loadedData.captureDate} | {loadedData.frameCount} frames", style);
            }
        }
    }
} 