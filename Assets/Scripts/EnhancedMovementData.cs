using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class QuaternionData
{
    public float x;
    public float y;
    public float z;
    public float w;
}

[System.Serializable]
public class FingerData
{
    public Vector3Data[] thumb;
    public Vector3Data[] index;
    public Vector3Data[] middle;
    public Vector3Data[] ring;
    public Vector3Data[] pinky;
}

[System.Serializable]
public class HandData
{
    public Vector3Data position;
    public QuaternionData rotation;
    public FingerData fingers;
}

[System.Serializable]
public class SimpleFrame
{
    public float timestamp;
    public HandData leftHand;
    public HandData rightHand;
}

[System.Serializable]
public class TrackingQualityData
{
    public float confidence;
    public bool occlusion;
    public bool smoothing;
}

[System.Serializable]
public class MetadataData
{
    public string version;
    public string description;
    public string created;
    public int frameRate;
}

[System.Serializable]
public class SimpleMovementData
{
    public MetadataData metadata;
    public List<SimpleFrame> frames;
    public int totalFrames;
    public float duration;
    public int frameRate;
    public string handType;
    public TrackingQualityData trackingQuality;
} 