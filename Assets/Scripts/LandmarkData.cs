using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Landmark
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class LandmarkData
{
    public List<Landmark> hand_landmarks;
    public List<Landmark> face_landmarks;
}

