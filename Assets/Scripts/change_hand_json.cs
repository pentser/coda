using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AvatarHandAnimator : MonoBehaviour
{
    public string jsonFilePath = "Assets/json_hand/video_hand_to_json.json";
    
    // Left Hand Bones
    public Transform leftHandBone;
    public Transform leftThumbTip;
    public Transform leftIndexTip;
    public Transform leftMiddleTip;
    public Transform leftRingTip;
    public Transform leftPinkyTip;
    
    // Right Hand Bones
    public Transform rightHandBone;
    public Transform rightThumbTip;
    public Transform rightIndexTip;
    public Transform rightMiddleTip;
    public Transform rightRingTip;
    public Transform rightPinkyTip;
    
    public float playbackSpeed = 1.0f;

    private LandmarkFramesData landmarkData;
    private int currentFrame = 0;
    private float timer = 0f;

    void Start()
    {
        LoadLandmarkData();
    }

    void Update()
    {
        if (landmarkData == null || landmarkData.frames.Count == 0)
            return;

        timer += Time.deltaTime * playbackSpeed;

        if (timer >= 0.1f) // 10 FPS playback
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % landmarkData.frames.Count;
            ApplyLandmarkToAvatar(landmarkData.frames[currentFrame]);
        }
    }

    void LoadLandmarkData()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError("JSON file not found: " + jsonFilePath);
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        landmarkData = JsonUtility.FromJson<LandmarkFramesData>(json);
    }

    void ApplyLandmarkToAvatar(Frame frame)
    {
        if (frame.landmarks == null || frame.landmarks.Count == 0)
            return;

        // Assuming your JSON has 21 landmarks per hand (MediaPipe standard)
        // First 21 landmarks = right hand, next 21 = left hand
        if (frame.landmarks.Count >= 42) // Both hands
        {
            // Right Hand (landmarks 0-20)
            ApplyHandLandmarks(frame.landmarks.GetRange(0, 21), rightHandBone, 
                rightThumbTip, rightIndexTip, rightMiddleTip, rightRingTip, rightPinkyTip);
            
            // Left Hand (landmarks 21-41)
            ApplyHandLandmarks(frame.landmarks.GetRange(21, 21), leftHandBone, 
                leftThumbTip, leftIndexTip, leftMiddleTip, leftRingTip, leftPinkyTip);
        }
        else if (frame.landmarks.Count >= 21) // Single hand
        {
            // Apply to right hand by default
            ApplyHandLandmarks(frame.landmarks, rightHandBone, 
                rightThumbTip, rightIndexTip, rightMiddleTip, rightRingTip, rightPinkyTip);
        }
    }

    void ApplyHandLandmarks(List<Landmark> landmarks, Transform handBone, 
        Transform thumbTip, Transform indexTip, Transform middleTip, Transform ringTip, Transform pinkyTip)
    {
        if (handBone != null && landmarks.Count >= 21)
        {
            // Move hand bone to wrist position (landmark 0)
            var wrist = landmarks[0];
            handBone.position = new Vector3(wrist.x * 10 - 5, wrist.y * 10 - 5, -wrist.z * 10);
            
            // Move finger tips (landmarks 4, 8, 12, 16, 20)
            if (thumbTip != null) thumbTip.position = new Vector3(landmarks[4].x * 10 - 5, landmarks[4].y * 10 - 5, -landmarks[4].z * 10);
            if (indexTip != null) indexTip.position = new Vector3(landmarks[8].x * 10 - 5, landmarks[8].y * 10 - 5, -landmarks[8].z * 10);
            if (middleTip != null) middleTip.position = new Vector3(landmarks[12].x * 10 - 5, landmarks[12].y * 10 - 5, -landmarks[12].z * 10);
            if (ringTip != null) ringTip.position = new Vector3(landmarks[16].x * 10 - 5, landmarks[16].y * 10 - 5, -landmarks[16].z * 10);
            if (pinkyTip != null) pinkyTip.position = new Vector3(landmarks[20].x * 10 - 5, landmarks[20].y * 10 - 5, -landmarks[20].z * 10);
        }
    }
}
