using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Mediapipe;
using Mediapipe.Unity;

namespace HardCoded.VRigUnity
{
    [System.Serializable]
    public class HandLandmark
    {
        public float x, y, z;
    }

    [System.Serializable]
    public class HandLandmarkFrame
    {
        public HandLandmark[] landmarks;
        public float timestamp;
    }

    [System.Serializable]
    public class HandLandmarkVideo
    {
        public HandLandmarkFrame[] frames;
        public string handType; // "left" or "right"
        public int totalFrames;
    }

    public class HandLandmarkRecorder : MonoBehaviour
    {
        [Header("Recording Settings")]
        public HolisticGraph holisticGraph;
        public bool recordLeftHand = true;
        public bool recordRightHand = true;
        public bool autoSaveOnQuit = true;
        public bool saveOnKeyPress = true;
        public KeyCode saveKey = KeyCode.S;

        [Header("File Settings")]
        public string fileName = "video_hand_to_json.json";
        public string folderName = "json_hand";

        private List<HandLandmarkFrame> leftHandFrames = new List<HandLandmarkFrame>();
        private List<HandLandmarkFrame> rightHandFrames = new List<HandLandmarkFrame>();
        private bool isRecording = false;
        private float startTime;

        void Start()
        {
            // Find HolisticGraph if not assigned
            if (holisticGraph == null)
            {
                holisticGraph = FindObjectOfType<HolisticGraph>();
            }

            if (holisticGraph != null)
            {
                if (recordLeftHand)
                {
                    holisticGraph.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
                }
                if (recordRightHand)
                {
                    holisticGraph.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
                }
            }
            else
            {
                Debug.LogError("HandLandmarkRecorder: HolisticGraph not found!");
            }

            // Create folder if it doesn't exist
            string folderPath = Path.Combine(Application.dataPath, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        void Update()
        {
            // Start recording on first frame
            if (!isRecording && (holisticGraph != null))
            {
                StartRecordingInternal();
            }

            // Save on key press
            if (saveOnKeyPress && Input.GetKeyDown(saveKey))
            {
                SaveToJson();
            }
        }

        void StartRecordingInternal()
        {
            isRecording = true;
            startTime = Time.time;
            Debug.Log("HandLandmarkRecorder: Started recording hand landmarks");
        }

        void OnLeftHandLandmarksOutput(object sender, OutputEventArgs<NormalizedLandmarkList> e)
        {
            if (e.value == null || !isRecording) return;

            var frame = new HandLandmarkFrame();
            frame.landmarks = e.value.Landmark.Select(l => new HandLandmark 
            { 
                x = l.X, 
                y = l.Y, 
                z = l.Z 
            }).ToArray();
            frame.timestamp = Time.time - startTime;
            
            leftHandFrames.Add(frame);
        }

        void OnRightHandLandmarksOutput(object sender, OutputEventArgs<NormalizedLandmarkList> e)
        {
            if (e.value == null || !isRecording) return;

            var frame = new HandLandmarkFrame();
            frame.landmarks = e.value.Landmark.Select(l => new HandLandmark 
            { 
                x = l.X, 
                y = l.Y, 
                z = l.Z 
            }).ToArray();
            frame.timestamp = Time.time - startTime;
            
            rightHandFrames.Add(frame);
        }

        public void SaveToJson()
        {
            string folderPath = Path.Combine(Application.dataPath, folderName);
            string filePath = Path.Combine(folderPath, fileName);

            // Save left hand data
            if (leftHandFrames.Count > 0)
            {
                var leftHandVideo = new HandLandmarkVideo
                {
                    frames = leftHandFrames.ToArray(),
                    handType = "left",
                    totalFrames = leftHandFrames.Count
                };
                string leftHandJson = JsonUtility.ToJson(leftHandVideo, true);
                string leftHandFilePath = filePath.Replace(".json", "_left.json");
                File.WriteAllText(leftHandFilePath, leftHandJson);
                Debug.Log($"HandLandmarkRecorder: Saved {leftHandFrames.Count} left hand frames to {leftHandFilePath}");
            }

            // Save right hand data
            if (rightHandFrames.Count > 0)
            {
                var rightHandVideo = new HandLandmarkVideo
                {
                    frames = rightHandFrames.ToArray(),
                    handType = "right",
                    totalFrames = rightHandFrames.Count
                };
                string rightHandJson = JsonUtility.ToJson(rightHandVideo, true);
                string rightHandFilePath = filePath.Replace(".json", "_right.json");
                File.WriteAllText(rightHandFilePath, rightHandJson);
                Debug.Log($"HandLandmarkRecorder: Saved {rightHandFrames.Count} right hand frames to {rightHandFilePath}");
            }

            // Save combined data
            if (leftHandFrames.Count > 0 || rightHandFrames.Count > 0)
            {
                var combinedVideo = new HandLandmarkVideo
                {
                    frames = leftHandFrames.Concat(rightHandFrames).ToArray(),
                    handType = "both",
                    totalFrames = leftHandFrames.Count + rightHandFrames.Count
                };
                string combinedJson = JsonUtility.ToJson(combinedVideo, true);
                File.WriteAllText(filePath, combinedJson);
                Debug.Log($"HandLandmarkRecorder: Saved combined data to {filePath}");
            }
        }

        void OnApplicationQuit()
        {
            if (autoSaveOnQuit)
            {
                SaveToJson();
            }
        }

        void OnDestroy()
        {
            // Clean up event listeners
            if (holisticGraph != null)
            {
                if (recordLeftHand)
                {
                    holisticGraph.OnLeftHandLandmarksOutput -= OnLeftHandLandmarksOutput;
                }
                if (recordRightHand)
                {
                    holisticGraph.OnRightHandLandmarksOutput -= OnRightHandLandmarksOutput;
                }
            }
        }

        // Public methods for external control
        public void StartRecording()
        {
            isRecording = true;
            startTime = Time.time;
            leftHandFrames.Clear();
            rightHandFrames.Clear();
            Debug.Log("HandLandmarkRecorder: Started recording hand landmarks");
        }

        public void StopRecording()
        {
            isRecording = false;
            Debug.Log("HandLandmarkRecorder: Stopped recording hand landmarks");
        }

        public void ClearRecordedData()
        {
            leftHandFrames.Clear();
            rightHandFrames.Clear();
            Debug.Log("HandLandmarkRecorder: Cleared recorded data");
        }

        public int GetLeftHandFrameCount()
        {
            return leftHandFrames.Count;
        }

        public int GetRightHandFrameCount()
        {
            return rightHandFrames.Count;
        }
    }
} 