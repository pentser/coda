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
    public VRMBlendShapeProxy blendShapeProxy;
    
    [Header("Body Bones")]
    public Transform hipsBone;
    public Transform spineBone;
    public Transform chestBone;
    public Transform neckBone;
    public Transform headBone;
    
    [Header("Arm Bones")]
    public Transform leftShoulderBone;
    public Transform leftElbowBone;
    public Transform leftWristBone;
    public Transform leftHandBone;
    public Transform rightShoulderBone;
    public Transform rightElbowBone;
    public Transform rightWristBone;
    public Transform rightHandBone;
    
    [Header("Leg Bones")]
    public Transform leftHipBone;
    public Transform leftKneeBone;
    public Transform leftAnkleBone;
    public Transform rightHipBone;
    public Transform rightKneeBone;
    public Transform rightAnkleBone;

    [Header("Finger Bones")]
    public Transform[] leftFingerBones;
    public Transform[] rightFingerBones;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool applySmoothing = true;
    public float smoothingFactor = 0.1f;

    private EnhancedMovementData movementData;
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;
    
    // Previous frame data for smoothing
    private Vector3 previousBodyPosition;
    private Quaternion previousBodyRotation;
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
            movementData = JsonUtility.FromJson<EnhancedMovementData>(json);
            
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

        if (blendShapeProxy == null)
        {
            blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
        }

        // Auto-assign bones if not set
        if (avatarAnimator != null)
        {
            if (hipsBone == null) hipsBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Hips);
            if (spineBone == null) spineBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Spine);
            if (chestBone == null) chestBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Chest);
            if (neckBone == null) neckBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Neck);
            if (headBone == null) headBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Head);
            
            if (leftShoulderBone == null) leftShoulderBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            if (leftElbowBone == null) leftElbowBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (leftWristBone == null) leftWristBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            if (rightShoulderBone == null) rightShoulderBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (rightElbowBone == null) rightElbowBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if (rightWristBone == null) rightWristBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            
            if (leftHipBone == null) leftHipBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            if (leftKneeBone == null) leftKneeBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            if (leftAnkleBone == null) leftAnkleBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            if (rightHipBone == null) rightHipBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if (rightKneeBone == null) rightKneeBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            if (rightAnkleBone == null) rightAnkleBone = avatarAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
    }

    void ApplyFrameToAvatar(EnhancedFrame frame)
    {
        if (frame == null) return;

        // Apply body pose
        ApplyBodyPose(frame.bodyPose);
        
        // Apply head and facial expressions
        ApplyHeadPose(frame.head);
        
        // Apply arm poses
        ApplyArmPose(frame.leftArm, true);
        ApplyArmPose(frame.rightArm, false);
        
        // Apply leg poses
        ApplyLegPose(frame.leftLeg, true);
        ApplyLegPose(frame.rightLeg, false);
        
        // Apply torso
        ApplyTorsoPose(frame.torso);
    }

    void ApplyBodyPose(BodyPoseData bodyPose)
    {
        if (bodyPose == null || hipsBone == null) return;

        Vector3 targetPosition = new Vector3(bodyPose.position.x, bodyPose.position.y, bodyPose.position.z);
        Quaternion targetRotation = new Quaternion(bodyPose.rotation.x, bodyPose.rotation.y, bodyPose.rotation.z, bodyPose.rotation.w);

        if (applySmoothing)
        {
            targetPosition = Vector3.Lerp(previousBodyPosition, targetPosition, smoothingFactor);
            targetRotation = Quaternion.Slerp(previousBodyRotation, targetRotation, smoothingFactor);
        }

        hipsBone.position = targetPosition;
        hipsBone.rotation = targetRotation;

        previousBodyPosition = targetPosition;
        previousBodyRotation = targetRotation;
    }

    void ApplyHeadPose(HeadData head)
    {
        if (head == null) return;

        // Apply head position and rotation
        if (headBone != null)
        {
            Vector3 targetPosition = new Vector3(head.position.x, head.position.y, head.position.z);
            Quaternion targetRotation = new Quaternion(head.rotation.x, head.rotation.y, head.rotation.z, head.rotation.w);

            if (applySmoothing)
            {
                targetPosition = Vector3.Lerp(GetPreviousPosition(headBone), targetPosition, smoothingFactor);
                targetRotation = Quaternion.Slerp(GetPreviousRotation(headBone), targetRotation, smoothingFactor);
            }

            headBone.position = targetPosition;
            headBone.rotation = targetRotation;

            SetPreviousTransform(headBone, targetPosition, targetRotation);
        }

        // Apply facial blend shapes
        if (blendShapeProxy != null && head.blendShapes != null)
        {
            var blendShapes = head.blendShapes;
            
            blendShapeProxy.SetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), blendShapes.eyeBlinkLeft);
            blendShapeProxy.SetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), blendShapes.eyeBlinkRight);
            blendShapeProxy.SetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), blendShapes.jawOpen);
            blendShapeProxy.SetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Smile), blendShapes.mouthSmile);
            blendShapeProxy.SetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.BrowUp_L), blendShapes.browUp);
            blendShapeProxy.SetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.CheekPuff), blendShapes.cheekPuff);
        }
    }

    void ApplyArmPose(ArmPoseData armPose, bool isLeft)
    {
        if (armPose == null) return;

        Transform shoulderBone = isLeft ? leftShoulderBone : rightShoulderBone;
        Transform elbowBone = isLeft ? leftElbowBone : rightElbowBone;
        Transform wristBone = isLeft ? leftWristBone : rightWristBone;
        Transform handBone = isLeft ? leftHandBone : rightHandBone;

        // Apply shoulder
        if (shoulderBone != null && armPose.shoulder != null)
        {
            ApplyBoneTransform(shoulderBone, armPose.shoulder);
        }

        // Apply elbow
        if (elbowBone != null && armPose.elbow != null)
        {
            ApplyBoneTransform(elbowBone, armPose.elbow);
        }

        // Apply wrist
        if (wristBone != null && armPose.wrist != null)
        {
            ApplyBoneTransform(wristBone, armPose.wrist);
        }

        // Apply hand
        if (handBone != null && armPose.hand != null)
        {
            ApplyBoneTransform(handBone, armPose.hand);
            
            // Apply finger positions if available
            if (armPose.hand.fingers != null)
            {
                ApplyFingerPositions(armPose.hand.fingers, isLeft);
            }
        }
    }

    void ApplyLegPose(LegData legPose, bool isLeft)
    {
        if (legPose == null) return;

        Transform hipBone = isLeft ? leftHipBone : rightHipBone;
        Transform kneeBone = isLeft ? leftKneeBone : rightKneeBone;
        Transform ankleBone = isLeft ? leftAnkleBone : rightAnkleBone;

        // Apply hip
        if (hipBone != null && legPose.hip != null)
        {
            ApplyBoneTransform(hipBone, legPose.hip);
        }

        // Apply knee
        if (kneeBone != null && legPose.knee != null)
        {
            ApplyBoneTransform(kneeBone, legPose.knee);
        }

        // Apply ankle
        if (ankleBone != null && legPose.ankle != null)
        {
            ApplyBoneTransform(ankleBone, legPose.ankle);
        }
    }

    void ApplyTorsoPose(TorsoData torso)
    {
        if (torso == null || spineBone == null) return;

        Vector3 targetPosition = new Vector3(torso.position.x, torso.position.y, torso.position.z);
        Quaternion targetRotation = new Quaternion(torso.rotation.x, torso.rotation.y, torso.rotation.z, torso.rotation.w);

        if (applySmoothing)
        {
            targetPosition = Vector3.Lerp(GetPreviousPosition(spineBone), targetPosition, smoothingFactor);
            targetRotation = Quaternion.Slerp(GetPreviousRotation(spineBone), targetRotation, smoothingFactor);
        }

        spineBone.position = targetPosition;
        spineBone.rotation = targetRotation;

        SetPreviousTransform(spineBone, targetPosition, targetRotation);
    }

    void ApplyBoneTransform(Transform bone, ArmData armData)
    {
        if (bone == null || armData == null) return;

        Vector3 targetPosition = new Vector3(armData.position.x, armData.position.y, armData.position.z);
        Quaternion targetRotation = new Quaternion(armData.rotation.x, armData.rotation.y, armData.rotation.z, armData.rotation.w);

        if (applySmoothing)
        {
            targetPosition = Vector3.Lerp(GetPreviousPosition(bone), targetPosition, smoothingFactor);
            targetRotation = Quaternion.Slerp(GetPreviousRotation(bone), targetRotation, smoothingFactor);
        }

        bone.position = targetPosition;
        bone.rotation = targetRotation;

        SetPreviousTransform(bone, targetPosition, targetRotation);
    }

    void ApplyFingerPositions(FingerData fingerData, bool isLeft)
    {
        Transform[] fingerBones = isLeft ? leftFingerBones : rightFingerBones;
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
            Debug.Log("Enhanced Avatar Animation Started");
        }
    }

    public void StopAnimation()
    {
        isPlaying = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Enhanced Avatar Animation Stopped");
        }
    }

    public void PauseAnimation()
    {
        isPlaying = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Enhanced Avatar Animation Paused");
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