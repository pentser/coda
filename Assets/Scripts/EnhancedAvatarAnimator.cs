using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRM;

public class EnhancedAvatarAnimator : MonoBehaviour
{
    [Header("JSON Configuration")]
    public string jsonFilePath = "Assets/json2movment.json";
    public float playbackSpeed = 1.0f;
    public bool loopAnimation = true;
    public bool autoPlay = true;

    [Header("Avatar References")]
    public Animator avatarAnimator;
    
    [Header("Hand Bones")]
    public Transform leftHandBone;
    public Transform rightHandBone;

    [Header("Finger Bones")]
    public Transform[] leftFingerBones;
    public Transform[] rightFingerBones;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool applySmoothing = true;
    public float smoothingFactor = 0.1f;

    private SimpleMovementData movementData;
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;
    
    // Previous frame data for smoothing
    private Dictionary<Transform, Vector3> previousPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> previousRotations = new Dictionary<Transform, Quaternion>();

    void Start()
    {
        LoadMovementData();
        SetupAvatarReferences();
        
        if (autoPlay)
        {
            PlayAnimation();
        }
    }

    void Update()
    {
        if (!isPlaying || movementData == null || movementData.frames.Count == 0)
            return;

        timer += Time.deltaTime * playbackSpeed;
        float frameTime = 1f / movementData.frameRate;

        if (timer >= frameTime)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % movementData.frames.Count;
            
            if (!loopAnimation && currentFrame == 0)
            {
                StopAnimation();
                return;
            }
            
            ApplyFrameToAvatar(movementData.frames[currentFrame]);
        }
    }

    void LoadMovementData()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError("JSON file not found: " + jsonFilePath);
            return;
        }

        try
        {
            string json = File.ReadAllText(jsonFilePath);
            movementData = JsonUtility.FromJson<SimpleMovementData>(json);
            
            if (showDebugInfo)
            {
                Debug.Log($"Loaded movement data: {movementData.totalFrames} frames, {movementData.frameRate} FPS, Duration: {movementData.duration}s");
                Debug.Log($"Hand type: {movementData.handType}, Confidence: {movementData.trackingQuality.confidence}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse JSON: " + e.Message);
        }
    }

    void SetupAvatarReferences()
    {
        if (avatarAnimator == null)
        {
            avatarAnimator = GetComponent<Animator>();
        }

        // Auto-assign hand bones if not set
        if (avatarAnimator != null)
        {
            if (leftHandBone == null) leftHandBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (rightHandBone == null) rightHandBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        }
    }

    void ApplyFrameToAvatar(SimpleFrame frame)
    {
        if (frame == null) return;

        // Apply left hand
        if (frame.leftHand != null)
        {
            ApplyHandPose(frame.leftHand, true);
        }
        
        // Apply right hand
        if (frame.rightHand != null)
        {
            ApplyHandPose(frame.rightHand, false);
        }
    }

    void ApplyHandPose(HandData handData, bool isLeft)
    {
        if (handData == null) return;

        Transform handBone = isLeft ? leftHandBone : rightHandBone;
        Transform[] fingerBones = isLeft ? leftFingerBones : rightFingerBones;

        // Apply hand position and rotation
        if (handBone != null)
        {
            Vector3 targetPosition = new Vector3(handData.position.x, handData.position.y, handData.position.z);
            Quaternion targetRotation = new Quaternion(handData.rotation.x, handData.rotation.y, handData.rotation.z, handData.rotation.w);

            if (applySmoothing)
            {
                targetPosition = Vector3.Lerp(GetPreviousPosition(handBone), targetPosition, smoothingFactor);
                targetRotation = Quaternion.Slerp(GetPreviousRotation(handBone), targetRotation, smoothingFactor);
            }

            handBone.position = targetPosition;
            handBone.rotation = targetRotation;

            SetPreviousTransform(handBone, targetPosition, targetRotation);
        }

        // Apply finger positions if available
        if (handData.fingers != null && fingerBones != null && fingerBones.Length > 0)
        {
            ApplyFingerPositions(handData.fingers, fingerBones);
        }
    }

    void ApplyFingerPositions(FingerData fingerData, Transform[] fingerBones)
    {
        if (fingerBones == null || fingerBones.Length == 0) return;

        // Apply thumb positions
        if (fingerData.thumb != null && fingerData.thumb.Length > 0 && fingerBones.Length > 0)
        {
            ApplyFingerJoint(fingerBones[0], fingerData.thumb[0]);
        }

        // Apply index finger positions
        if (fingerData.index != null && fingerData.index.Length > 0 && fingerBones.Length > 1)
        {
            ApplyFingerJoint(fingerBones[1], fingerData.index[0]);
        }

        // Apply middle finger positions
        if (fingerData.middle != null && fingerData.middle.Length > 0 && fingerBones.Length > 2)
        {
            ApplyFingerJoint(fingerBones[2], fingerData.middle[0]);
        }

        // Apply ring finger positions
        if (fingerData.ring != null && fingerData.ring.Length > 0 && fingerBones.Length > 3)
        {
            ApplyFingerJoint(fingerBones[3], fingerData.ring[0]);
        }

        // Apply pinky finger positions
        if (fingerData.pinky != null && fingerData.pinky.Length > 0 && fingerBones.Length > 4)
        {
            ApplyFingerJoint(fingerBones[4], fingerData.pinky[0]);
        }
    }

    void ApplyFingerJoint(Transform fingerBone, Vector3Data position)
    {
        if (fingerBone == null || position == null) return;

        Vector3 targetPosition = new Vector3(position.x, position.y, position.z);
        
        if (applySmoothing)
        {
            targetPosition = Vector3.Lerp(GetPreviousPosition(fingerBone), targetPosition, smoothingFactor);
        }

        fingerBone.position = targetPosition;
        SetPreviousTransform(fingerBone, targetPosition, fingerBone.rotation);
    }

    // Smoothing helper methods
    Vector3 GetPreviousPosition(Transform bone)
    {
        return previousPositions.ContainsKey(bone) ? previousPositions[bone] : bone.position;
    }

    Quaternion GetPreviousRotation(Transform bone)
    {
        return previousRotations.ContainsKey(bone) ? previousRotations[bone] : bone.rotation;
    }

    void SetPreviousTransform(Transform bone, Vector3 position, Quaternion rotation)
    {
        previousPositions[bone] = position;
        previousRotations[bone] = rotation;
    }

    // Public control methods
    public void PlayAnimation()
    {
        isPlaying = true;
        currentFrame = 0;
        timer = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log("Hand Animation Started");
        }
    }

    public void StopAnimation()
    {
        isPlaying = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Hand Animation Stopped");
        }
    }

    public void PauseAnimation()
    {
        isPlaying = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Hand Animation Paused");
        }
    }

    public void SetFrame(int frameIndex)
    {
        if (movementData != null && frameIndex >= 0 && frameIndex < movementData.frames.Count)
        {
            currentFrame = frameIndex;
            ApplyFrameToAvatar(movementData.frames[currentFrame]);
        }
    }

    public float GetProgress()
    {
        if (movementData == null || movementData.frames.Count == 0)
            return 0f;
        
        return (float)currentFrame / movementData.frames.Count;
    }

    public void SetProgress(float progress)
    {
        if (movementData != null && movementData.frames.Count > 0)
        {
            int frameIndex = Mathf.Clamp(Mathf.RoundToInt(progress * movementData.frames.Count), 0, movementData.frames.Count - 1);
            SetFrame(frameIndex);
        }
    }
} 