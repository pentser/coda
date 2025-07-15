using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonLandmarkVisualizer : MonoBehaviour
{
    public string jsonFilePath = "Assets/json_hand/video_hand_to_json.json"; // Update this path as needed
    public GameObject handLandmarkPrefab;
    public int frameToShow = 0; // You can change this in the Inspector to show a different frame

    private List<GameObject> handLandmarks = new List<GameObject>();

    void Start()
    {
        LoadAndApplyLandmarks();
    }

    public void LoadAndApplyLandmarks()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError("JSON file not found: " + jsonFilePath);
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        LandmarkFramesData data = JsonUtility.FromJson<LandmarkFramesData>(json);

        if (data.frames == null || data.frames.Count == 0)
        {
            Debug.LogError("No frames found in JSON!");
            return;
        }

        // Clamp frameToShow to valid range
        int frameIndex = Mathf.Clamp(frameToShow, 0, data.frames.Count - 1);
        var frame = data.frames[frameIndex];

        // Visualize hand landmarks for the selected frame
        for (int i = 0; i < frame.landmarks.Count; i++)
        {
            if (handLandmarks.Count <= i)
                handLandmarks.Add(Instantiate(handLandmarkPrefab));

            var lm = frame.landmarks[i];
            handLandmarks[i].SetActive(true);
            handLandmarks[i].transform.position = new Vector3(lm.x * 10 - 5, lm.y * 10 - 5, -lm.z * 10);
        }
        for (int i = frame.landmarks.Count; i < handLandmarks.Count; i++)
            handLandmarks[i].SetActive(false);
    }
}
