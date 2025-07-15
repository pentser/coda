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
public class BlendShapesData
{
    public float eyeBlinkLeft;
    public float eyeBlinkRight;
    public float jawOpen;
    public float mouthSmile;
    public float browUp;
    public float cheekPuff;
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
public class ArmData
{
    public Vector3Data position;
    public QuaternionData rotation;
}

[System.Serializable]
public class ArmPoseData
{
    public ArmData shoulder;
    public ArmData elbow;
    public ArmData wrist;
    public HandData hand;
}

[System.Serializable]
public class LegData
{
    public ArmData hip;
    public ArmData knee;
    public ArmData ankle;
}

[System.Serializable]
public class HeadData
{
    public Vector3Data position;
    public QuaternionData rotation;
    public BlendShapesData blendShapes;
}

[System.Serializable]
public class BodyPoseData
{
    public Vector3Data position;
    public QuaternionData rotation;
}

[System.Serializable]
public class TorsoData
{
    public Vector3Data position;
    public QuaternionData rotation;
}

[System.Serializable]
public class EnhancedFrame
{
    public float timestamp;
    public BodyPoseData bodyPose;
    public HeadData head;
    public ArmPoseData leftArm;
    public ArmPoseData rightArm;
    public LegData leftLeg;
    public LegData rightLeg;
    public TorsoData torso;
}

[System.Serializable]
public class AnimationCurveData
{
    public float[] x;
    public float[] y;
    public float[] z;
    public float[] w;
}

[System.Serializable]
public class ArmSwingData
{
    public float amplitude;
    public float frequency;
    public float phase;
}

[System.Serializable]
public class WalkingCycleData
{
    public float[] hipRotation;
    public float[] kneePosition;
    public float[] anklePosition;
}

[System.Serializable]
public class WalkingCycle
{
    public WalkingCycleData leftLeg;
    public WalkingCycleData rightLeg;
}

[System.Serializable]
public class AnimationCurvesData
{
    public AnimationCurveData headRotation;
    public ArmSwingData leftArmSwing;
    public ArmSwingData rightArmSwing;
    public WalkingCycle walkingCycle;
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
public class EnhancedMovementData
{
    public MetadataData metadata;
    public List<EnhancedFrame> frames;
    public AnimationCurvesData animationCurves;
    public int totalFrames;
    public float duration;
    public int frameRate;
    public string handType;
    public TrackingQualityData trackingQuality;
} 