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
        private bool leftHandSubscribed = false;
        private bool rightHandSubscribed = false;

        void Start()
        {
            // Find HolisticGraph if not assigned
            if (holisticGraph == null)
            {
                holisticGraph = FindObjectOfType<HolisticGraph>();
            }

            if (holisticGraph != null)
            {
                // Subscribe to events after a short delay to ensure HolisticGraph is initialized
                StartCoroutine(SubscribeToEventsWhenReady());
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

        private System.Collections.IEnumerator SubscribeToEventsWhenReady()
        {
            // Wait for Bootstrap to finish initialization
            var bootstrap = SolutionUtils.GetBootstrap();
            if (bootstrap != null)
            {
                yield return new WaitUntil(() => bootstrap.IsFinished);
            }

            // Wait for HolisticGraph to be properly initialized
            if (holisticGraph != null)
            {
                // Wait for the graph to be initialized
                var initRequest = holisticGraph.WaitForInitAsync();
                yield return initRequest;
                
                if (initRequest.isError)
                {
                    Debug.LogError($"HandLandmarkRecorder: Failed to initialize HolisticGraph: {initRequest.error}");
                    yield break;
                }
                
                // Wait a bit more to ensure streams are created
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }

            // Try to subscribe to events
            try
            {
                if (holisticGraph != null)
                {
                    bool leftHandSubscribed = false;
                    bool rightHandSubscribed = false;
                    
                    if (recordLeftHand)
                    {
                        try
                        {
                            holisticGraph.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
                            leftHandSubscribed = true;
                            Debug.Log("HandLandmarkRecorder: Successfully subscribed to left hand events");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"HandLandmarkRecorder: Failed to subscribe to left hand events: {e.Message}");
                        }
                    }
                    
                    if (recordRightHand)
                    {
                        try
                        {
                            holisticGraph.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
                            rightHandSubscribed = true;
                            Debug.Log("HandLandmarkRecorder: Successfully subscribed to right hand events");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"HandLandmarkRecorder: Failed to subscribe to right hand events: {e.Message}");
                        }
                    }
                    
                    if (leftHandSubscribed || rightHandSubscribed)
                    {
                        Debug.Log("HandLandmarkRecorder: Successfully subscribed to HolisticGraph events");
                    }
                }
                else
                {
                    Debug.LogError("HandLandmarkRecorder: HolisticGraph is null after initialization");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HandLandmarkRecorder: Failed to subscribe to events: {e.Message}");
            }
        }

        void Update()
        {
            // Start recording on first frame
            if (!isRecording && (holisticGraph != null))
            {
                StartRecordingInternal();
            }

            // Save on key press - test multiple keys
            if (saveOnKeyPress)
            {
                if (Input.GetKeyDown(saveKey))
                {
                    Debug.Log($"HandLandmarkRecorder: {saveKey} key pressed - saving JSON");
                    SaveToJson();
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    Debug.Log("HandLandmarkRecorder: S key pressed - saving JSON");
                    SaveToJson();
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    Debug.Log("HandLandmarkRecorder: P key pressed - saving JSON");
                    SaveToJson();
                }
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
            
            // Debug: Log every 30 frames (about once per second at 30fps)
            if (leftHandFrames.Count % 30 == 0)
            {
                Debug.Log($"HandLandmarkRecorder: Recorded {leftHandFrames.Count} left hand frames");
            }
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
            
            // Debug: Log every 30 frames (about once per second at 30fps)
            if (rightHandFrames.Count % 30 == 0)
            {
                Debug.Log($"HandLandmarkRecorder: Recorded {rightHandFrames.Count} right hand frames");
            }
        }

        public void SaveToJson()
        {
            Debug.Log("HandLandmarkRecorder: SaveToJson() called");
            Debug.Log($"HandLandmarkRecorder: Left hand frames: {leftHandFrames.Count}");
            Debug.Log($"HandLandmarkRecorder: Right hand frames: {rightHandFrames.Count}");
            
            string folderPath = Path.Combine(Application.dataPath, folderName);
            string filePath = Path.Combine(folderPath, fileName);
            
            Debug.Log($"HandLandmarkRecorder: Folder path: {folderPath}");
            Debug.Log($"HandLandmarkRecorder: File path: {filePath}");

            // Create folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Debug.Log($"HandLandmarkRecorder: Created folder: {folderPath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"HandLandmarkRecorder: Failed to create folder: {e.Message}");
                    return;
                }
            }

            // Save left hand data
            if (leftHandFrames.Count > 0)
            {
                try
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
                catch (System.Exception e)
                {
                    Debug.LogError($"HandLandmarkRecorder: Failed to save left hand data: {e.Message}");
                }
            }
            else
            {
                Debug.Log("HandLandmarkRecorder: No left hand frames to save");
            }

            // Save right hand data
            if (rightHandFrames.Count > 0)
            {
                try
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
                catch (System.Exception e)
                {
                    Debug.LogError($"HandLandmarkRecorder: Failed to save right hand data: {e.Message}");
                }
            }
            else
            {
                Debug.Log("HandLandmarkRecorder: No right hand frames to save");
            }

            // Save combined data
            if (leftHandFrames.Count > 0 || rightHandFrames.Count > 0)
            {
                try
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
                catch (System.Exception e)
                {
                    Debug.LogError($"HandLandmarkRecorder: Failed to save combined data: {e.Message}");
                }
            }
            else
            {
                Debug.Log("HandLandmarkRecorder: No data to save");
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
            try
            {
                if (holisticGraph != null)
                {
                    if (leftHandSubscribed)
                    {
                        try
                        {
                            holisticGraph.OnLeftHandLandmarksOutput -= OnLeftHandLandmarksOutput;
                            Debug.Log("HandLandmarkRecorder: Successfully unsubscribed from left hand events");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"HandLandmarkRecorder: Error unsubscribing from left hand events: {e.Message}");
                        }
                    }
                    
                    if (rightHandSubscribed)
                    {
                        try
                        {
                            holisticGraph.OnRightHandLandmarksOutput -= OnRightHandLandmarksOutput;
                            Debug.Log("HandLandmarkRecorder: Successfully unsubscribed from right hand events");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"HandLandmarkRecorder: Error unsubscribing from right hand events: {e.Message}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"HandLandmarkRecorder: Error during cleanup: {e.Message}");
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