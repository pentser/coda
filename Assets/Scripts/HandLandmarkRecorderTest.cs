using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mediapipe;
using Mediapipe.Unity;

namespace HardCoded.VRigUnity
{
    public class HandLandmarkRecorderTest : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool enableTestMode = true;
        public KeyCode testKey = KeyCode.T;
        
        private HandLandmarkRecorder recorder;
        private HolisticGraph holisticGraph;
        
        void Start()
        {
            if (!enableTestMode) return;
            
            Debug.Log("=== HandLandmarkRecorder Test Started ===");
            
            // Find the HandLandmarkRecorder component
            recorder = FindObjectOfType<HandLandmarkRecorder>();
            if (recorder == null)
            {
                Debug.LogError("HandLandmarkRecorderTest: No HandLandmarkRecorder found in scene!");
                return;
            }
            Debug.Log("HandLandmarkRecorderTest: Found HandLandmarkRecorder component");
            
            // Find the HolisticGraph component
            holisticGraph = FindObjectOfType<HolisticGraph>();
            if (holisticGraph == null)
            {
                Debug.LogError("HandLandmarkRecorderTest: No HolisticGraph found in scene!");
                return;
            }
            Debug.Log("HandLandmarkRecorderTest: Found HolisticGraph component");
            
            // Test folder creation
            TestFolderCreation();
            
            // Test manual save function
            StartCoroutine(TestManualSave());
        }
        
        void Update()
        {
            if (!enableTestMode) return;
            
            // Test S key functionality
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("HandLandmarkRecorderTest: S key pressed - testing save function");
                TestSaveFunction();
            }
            
            // Test with T key for manual testing
            if (Input.GetKeyDown(testKey))
            {
                Debug.Log("HandLandmarkRecorderTest: T key pressed - running comprehensive test");
                StartCoroutine(RunComprehensiveTest());
            }
        }
        
        private void TestFolderCreation()
        {
            Debug.Log("HandLandmarkRecorderTest: Testing folder creation...");
            
            string folderPath = Path.Combine(Application.dataPath, "json_hand");
            Debug.Log($"HandLandmarkRecorderTest: Target folder path: {folderPath}");
            
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Debug.Log($"HandLandmarkRecorderTest: Successfully created folder: {folderPath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"HandLandmarkRecorderTest: Failed to create folder: {e.Message}");
                }
            }
            else
            {
                Debug.Log($"HandLandmarkRecorderTest: Folder already exists: {folderPath}");
            }
        }
        
        private void TestSaveFunction()
        {
            if (recorder == null)
            {
                Debug.LogError("HandLandmarkRecorderTest: Recorder is null!");
                return;
            }
            
            Debug.Log("HandLandmarkRecorderTest: Calling SaveToJson() method...");
            
            // Use reflection to access private methods for testing
            var saveMethod = typeof(HandLandmarkRecorder).GetMethod("SaveToJson", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (saveMethod != null)
            {
                try
                {
                    saveMethod.Invoke(recorder, null);
                    Debug.Log("HandLandmarkRecorderTest: SaveToJson() method called successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"HandLandmarkRecorderTest: Error calling SaveToJson(): {e.Message}");
                }
            }
            else
            {
                Debug.LogError("HandLandmarkRecorderTest: SaveToJson method not found!");
            }
        }
        
        private System.Collections.IEnumerator TestManualSave()
        {
            yield return new WaitForSeconds(2f);
            
            Debug.Log("HandLandmarkRecorderTest: Testing manual save after 2 seconds...");
            TestSaveFunction();
        }
        
        private System.Collections.IEnumerator RunComprehensiveTest()
        {
            Debug.Log("=== HandLandmarkRecorderTest: Running Comprehensive Test ===");
            
            // Test 1: Check if recorder is recording
            Debug.Log($"HandLandmarkRecorderTest: Is recording: {IsRecording()}");
            
            // Test 2: Check frame counts
            Debug.Log($"HandLandmarkRecorderTest: Left hand frames: {GetLeftHandFrameCount()}");
            Debug.Log($"HandLandmarkRecorderTest: Right hand frames: {GetRightHandFrameCount()}");
            
            // Test 3: Check if HolisticGraph is running
            Debug.Log($"HandLandmarkRecorderTest: HolisticGraph is running: {IsHolisticGraphRunning()}");
            
            // Test 4: Check if events are subscribed
            Debug.Log($"HandLandmarkRecorderTest: Events subscribed: {AreEventsSubscribed()}");
            
            // Test 5: Create test data and save
            yield return StartCoroutine(CreateTestDataAndSave());
            
            Debug.Log("=== HandLandmarkRecorderTest: Comprehensive Test Complete ===");
        }
        
        private bool IsRecording()
        {
            if (recorder == null) return false;
            
            var isRecordingField = typeof(HandLandmarkRecorder).GetField("isRecording", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (isRecordingField != null)
            {
                return (bool)isRecordingField.GetValue(recorder);
            }
            
            return false;
        }
        
        private int GetLeftHandFrameCount()
        {
            if (recorder == null) return 0;
            
            var leftHandFramesField = typeof(HandLandmarkRecorder).GetField("leftHandFrames", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (leftHandFramesField != null)
            {
                var leftHandFrames = leftHandFramesField.GetValue(recorder) as List<HandLandmarkFrame>;
                return leftHandFrames?.Count ?? 0;
            }
            
            return 0;
        }
        
        private int GetRightHandFrameCount()
        {
            if (recorder == null) return 0;
            
            var rightHandFramesField = typeof(HandLandmarkRecorder).GetField("rightHandFrames", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (rightHandFramesField != null)
            {
                var rightHandFrames = rightHandFramesField.GetValue(recorder) as List<HandLandmarkFrame>;
                return rightHandFrames?.Count ?? 0;
            }
            
            return 0;
        }
        
        private bool IsHolisticGraphRunning()
        {
            if (holisticGraph == null) return false;
            
            var isRunningField = typeof(HolisticGraph).GetField("_isRunning", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (isRunningField != null)
            {
                return (bool)isRunningField.GetValue(holisticGraph);
            }
            
            return false;
        }
        
        private bool AreEventsSubscribed()
        {
            if (recorder == null) return false;
            
            var leftHandSubscribedField = typeof(HandLandmarkRecorder).GetField("leftHandSubscribed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rightHandSubscribedField = typeof(HandLandmarkRecorder).GetField("rightHandSubscribed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            bool leftSubscribed = false;
            bool rightSubscribed = false;
            
            if (leftHandSubscribedField != null)
            {
                leftSubscribed = (bool)leftHandSubscribedField.GetValue(recorder);
            }
            
            if (rightHandSubscribedField != null)
            {
                rightSubscribed = (bool)rightHandSubscribedField.GetValue(recorder);
            }
            
            return leftSubscribed || rightSubscribed;
        }
        
        private System.Collections.IEnumerator CreateTestDataAndSave()
        {
            Debug.Log("HandLandmarkRecorderTest: Creating test data...");
            
            // Create some test hand landmark data
            var testFrame = new HandLandmarkFrame
            {
                landmarks = new HandLandmark[]
                {
                    new HandLandmark { x = 0.1f, y = 0.2f, z = 0.3f },
                    new HandLandmark { x = 0.4f, y = 0.5f, z = 0.6f },
                    new HandLandmark { x = 0.7f, y = 0.8f, z = 0.9f }
                },
                timestamp = Time.time
            };
            
            // Add test data to the recorder
            var leftHandFramesField = typeof(HandLandmarkRecorder).GetField("leftHandFrames", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (leftHandFramesField != null)
            {
                var leftHandFrames = leftHandFramesField.GetValue(recorder) as List<HandLandmarkFrame>;
                if (leftHandFrames != null)
                {
                    leftHandFrames.Add(testFrame);
                    leftHandFrames.Add(testFrame); // Add a second frame
                    Debug.Log("HandLandmarkRecorderTest: Added test data to left hand frames");
                }
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Try to save the test data
            Debug.Log("HandLandmarkRecorderTest: Attempting to save test data...");
            TestSaveFunction();
            
            yield return new WaitForSeconds(1f);
            
            // Check if files were created
            string folderPath = Path.Combine(Application.dataPath, "json_hand");
            string[] files = Directory.GetFiles(folderPath, "*.json");
            Debug.Log($"HandLandmarkRecorderTest: Found {files.Length} JSON files in folder:");
            foreach (string file in files)
            {
                Debug.Log($"HandLandmarkRecorderTest: - {Path.GetFileName(file)}");
            }
        }
    }
} 