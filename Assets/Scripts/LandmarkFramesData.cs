using System.Collections.Generic;

[System.Serializable]
public class Frame
{
    public List<Landmark> landmarks;
    public float timestamp;
}

[System.Serializable]
public class LandmarkFramesData
{
    public List<Frame> frames;
    public string handType;
    public int totalFrames;
} 