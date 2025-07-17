using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
    [System.Serializable]
    public class SerializablePoseFrame {
        public float timestamp;
        public Vector3 neckRotation;
        public Vector3 chestRotation;
        public Vector3 hipsRotation;
        public Vector3 hipsPosition;
        public Vector3 rightShoulderPosition;
        public Vector3 rightElbowPosition;
        public Vector3 rightHandPosition;
        public Vector3 leftShoulderPosition;
        public Vector3 leftElbowPosition;
        public Vector3 leftHandPosition;
        public Vector3 rightUpperLegRotation;
        public Vector3 rightLowerLegRotation;
        public Vector3 leftUpperLegRotation;
        public Vector3 leftLowerLegRotation;
    }

    [System.Serializable]
    public class SerializableHandFrame {
        public float timestamp;
        public Vector3 wristRotation;
        public Vector3 indexPipRotation;
        public Vector3 indexDipRotation;
        public Vector3 indexTipRotation;
        public Vector3 middlePipRotation;
        public Vector3 middleDipRotation;
        public Vector3 middleTipRotation;
        public Vector3 ringPipRotation;
        public Vector3 ringDipRotation;
        public Vector3 ringTipRotation;
        public Vector3 pinkyPipRotation;
        public Vector3 pinkyDipRotation;
        public Vector3 pinkyTipRotation;
        public Vector3 thumbPipRotation;
        public Vector3 thumbDipRotation;
        public Vector3 thumbTipRotation;
    }

    [System.Serializable]
    public class SerializableFaceFrame {
        public float timestamp;
        public float mouthOpen;
        public Vector2 leftEyeIris;
        public Vector2 rightEyeIris;
        public float leftEyeOpen;
        public float rightEyeOpen;
    }

    [System.Serializable]
    public class SerializableAvatarFrame {
        public SerializablePoseFrame pose;
        public SerializableHandFrame rightHand;
        public SerializableHandFrame leftHand;
        public SerializableFaceFrame face;
    }

    [System.Serializable]
    public class CaptureAvatarData {
        public string captureDate;
        public float captureStartTime;
        public float totalDuration;
        public int frameCount;
        public List<SerializableAvatarFrame> frames;
    }

    public class CaptureAvatar : MonoBehaviour {
        [Header("Recording Settings")]
        [SerializeField] private float captureRate = 30f; // Frames per second
        [SerializeField] private string fileName = "avatar_capture";
        [SerializeField] private bool showRecordingStatus = true;

        private bool isRecording = false;
        private float lastCaptureTime = 0f;
        private float captureStartTime = 0f;
        private List<SerializableAvatarFrame> capturedFrames = new List<SerializableAvatarFrame>();
        private HolisticSolution holisticSolution;

        private void Start() {
            holisticSolution = SolutionUtils.GetSolution();
            if (holisticSolution == null) {
                Debug.LogError("[CaptureAvatar] Could not find HolisticSolution in scene. Make sure the GameObject has the 'Solution' tag.");
            }
        }

        private void Update() {
            HandleInput();
            
            if (isRecording && holisticSolution != null) {
                CaptureFrame();
            }
        }

        private void HandleInput() {
            // Start/Stop recording with R key
            if (Input.GetKeyDown(KeyCode.R)) {
                if (isRecording) {
                    StopRecording();
                } else {
                    StartRecording();
                }
            }

            // Save recording with S key
            if (Input.GetKeyDown(KeyCode.S)) {
                if (capturedFrames.Count > 0) {
                    SaveRecording();
                } else {
                    Debug.LogWarning("[CaptureAvatar] No frames captured. Record some movement first with the R key.");
                }
            }
        }

        private void StartRecording() {
            isRecording = true;
            captureStartTime = Time.time;
            lastCaptureTime = 0f;
            capturedFrames.Clear();
            
            if (showRecordingStatus) {
                Debug.Log("[CaptureAvatar] Started recording avatar movement. Press R again to stop, S to save.");
            }
        }

        private void StopRecording() {
            isRecording = false;
            
            if (showRecordingStatus) {
                float duration = Time.time - captureStartTime;
                Debug.Log($"[CaptureAvatar] Stopped recording. Captured {capturedFrames.Count} frames over {duration:F2} seconds. Press S to save.");
            }
        }

        private void CaptureFrame() {
            float currentTime = Time.time - captureStartTime;
            float frameInterval = 1f / captureRate;
            
            if (currentTime - lastCaptureTime >= frameInterval) {
                SerializableAvatarFrame frame = new SerializableAvatarFrame();
                
                // Capture pose data
                frame.pose = CapturePoseData(currentTime);
                
                // Capture hand data
                frame.rightHand = CaptureHandData(holisticSolution.RightHand, currentTime);
                frame.leftHand = CaptureHandData(holisticSolution.LeftHand, currentTime);
                
                // Capture face data
                frame.face = CaptureFaceData(currentTime);
                
                capturedFrames.Add(frame);
                lastCaptureTime = currentTime;
            }
        }

        private SerializablePoseFrame CapturePoseData(float timestamp) {
            var pose = holisticSolution.Pose;
            return new SerializablePoseFrame {
                timestamp = timestamp,
                neckRotation = QuaternionToEuler(pose.Neck.Current),
                chestRotation = QuaternionToEuler(pose.Chest.Current),
                hipsRotation = QuaternionToEuler(pose.Hips.Current),
                hipsPosition = pose.HipsPosition.Current,
                rightShoulderPosition = pose.RightShoulder.Current,
                rightElbowPosition = pose.RightElbow.Current,
                rightHandPosition = pose.RightHand.Current,
                leftShoulderPosition = pose.LeftShoulder.Current,
                leftElbowPosition = pose.LeftElbow.Current,
                leftHandPosition = pose.LeftHand.Current,
                rightUpperLegRotation = QuaternionToEuler(pose.RightUpperLeg.Current),
                rightLowerLegRotation = QuaternionToEuler(pose.RightLowerLeg.Current),
                leftUpperLegRotation = QuaternionToEuler(pose.LeftUpperLeg.Current),
                leftLowerLegRotation = QuaternionToEuler(pose.LeftLowerLeg.Current)
            };
        }

        private SerializableHandFrame CaptureHandData(HandValues handValues, float timestamp) {
            return new SerializableHandFrame {
                timestamp = timestamp,
                wristRotation = QuaternionToEuler(handValues.Wrist.Current),
                indexPipRotation = QuaternionToEuler(handValues.IndexPip.Current),
                indexDipRotation = QuaternionToEuler(handValues.IndexDip.Current),
                indexTipRotation = QuaternionToEuler(handValues.IndexTip.Current),
                middlePipRotation = QuaternionToEuler(handValues.MiddlePip.Current),
                middleDipRotation = QuaternionToEuler(handValues.MiddleDip.Current),
                middleTipRotation = QuaternionToEuler(handValues.MiddleTip.Current),
                ringPipRotation = QuaternionToEuler(handValues.RingPip.Current),
                ringDipRotation = QuaternionToEuler(handValues.RingDip.Current),
                ringTipRotation = QuaternionToEuler(handValues.RingTip.Current),
                pinkyPipRotation = QuaternionToEuler(handValues.PinkyPip.Current),
                pinkyDipRotation = QuaternionToEuler(handValues.PinkyDip.Current),
                pinkyTipRotation = QuaternionToEuler(handValues.PinkyTip.Current),
                thumbPipRotation = QuaternionToEuler(handValues.ThumbPip.Current),
                thumbDipRotation = QuaternionToEuler(handValues.ThumbDip.Current),
                thumbTipRotation = QuaternionToEuler(handValues.ThumbTip.Current)
            };
        }

        private SerializableFaceFrame CaptureFaceData(float timestamp) {
            var face = holisticSolution.Face;
            return new SerializableFaceFrame {
                timestamp = timestamp,
                mouthOpen = face.mouthOpen,
                leftEyeIris = face.lEyeIris.Average(),
                rightEyeIris = face.rEyeIris.Average(),
                leftEyeOpen = face.lEyeOpen.Average(),
                rightEyeOpen = face.rEyeOpen.Average()
            };
        }

        private Vector3 QuaternionToEuler(Quaternion rotation) {
            return rotation.eulerAngles;
        }

        private void SaveRecording() {
            if (capturedFrames.Count == 0) {
                Debug.LogWarning("[CaptureAvatar] No frames to save.");
                return;
            }

            var captureData = new CaptureAvatarData {
                captureDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                captureStartTime = captureStartTime,
                totalDuration = capturedFrames.Count > 0 ? capturedFrames[capturedFrames.Count - 1].pose.timestamp : 0f,
                frameCount = capturedFrames.Count,
                frames = capturedFrames
            };

            // Create captureAvatar folder if it doesn't exist
            string folderPath = Path.Combine(Application.dataPath, "captureAvatar");
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"[CaptureAvatar] Created folder: {folderPath}");
            }

            string json = JsonUtility.ToJson(captureData, true);
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fullFileName = $"{fileName}_{timestamp}.json";
            string filePath = Path.Combine(folderPath, fullFileName);

            try {
                File.WriteAllText(filePath, json);
                Debug.Log($"[CaptureAvatar] Recording saved successfully! File: {fullFileName}");
                Debug.Log($"[CaptureAvatar] Saved {captureData.frameCount} frames over {captureData.totalDuration:F2} seconds");
                Debug.Log($"[CaptureAvatar] File location: {filePath}");
                
                // Clear the captured data
                capturedFrames.Clear();
            }
            catch (System.Exception e) {
                Debug.LogError($"[CaptureAvatar] Failed to save recording: {e.Message}");
                Debug.LogError($"[CaptureAvatar] Attempted path: {filePath}");
            }
        }

        private void OnGUI() {
            if (!showRecordingStatus) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.normal.textColor = isRecording ? Color.red : Color.white;

            string status = isRecording ? 
                $"RECORDING... ({capturedFrames.Count} frames)" : 
                $"Press R to record, S to save ({capturedFrames.Count} frames captured)";

            GUI.Label(new Rect(10, 10, 400, 30), status, style);

            // Show instructions
            style.fontSize = 12;
            style.normal.textColor = Color.gray;
            GUI.Label(new Rect(10, 40, 500, 20), "R = Start/Stop Recording | S = Save to JSON file", style);
        }
    }
} 