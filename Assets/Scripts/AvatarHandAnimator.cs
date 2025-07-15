using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AvatarHandAnimator : MonoBehaviour
{
    public string jsonFilePath = "Assets/json_hand/video_hand_to_json.json";
    public Transform rightHandBone; // Assign in Inspector
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
        if (frame.landmarks != null && frame.landmarks.Count > 0 && rightHandBone != null)
        {
            var lm = frame.landmarks[0]; // Use the first landmark for demo
            rightHandBone.position = new Vector3(lm.x * 10 - 5, lm.y * 10 - 5, -lm.z * 10);
        }
    }
}
