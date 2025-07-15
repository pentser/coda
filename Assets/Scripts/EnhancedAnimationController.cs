using UnityEngine;
using UnityEngine.UI;

public class EnhancedAnimationController : MonoBehaviour
{
    [Header("Animation Reference")]
    public EnhancedAvatarAnimator animator;

    [Header("UI Controls")]
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Slider progressSlider;
    public Slider speedSlider;
    public Text frameText;
    public Text speedText;
    public Toggle loopToggle;
    public Toggle smoothingToggle;

    [Header("Debug Info")]
    public Text debugInfoText;
    public bool showDebugInfo = true;

    private void Start()
    {
        SetupUI();
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    void SetupUI()
    {
        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayAnimation);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseAnimation);
        
        if (stopButton != null)
            stopButton.onClick.AddListener(StopAnimation);

        // Setup slider listeners
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnProgressChanged);
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
        }

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            speedSlider.minValue = 0.1f;
            speedSlider.maxValue = 3f;
            speedSlider.value = 1f;
        }

        // Setup toggle listeners
        if (loopToggle != null)
            loopToggle.onValueChanged.AddListener(OnLoopToggled);
        
        if (smoothingToggle != null)
            smoothingToggle.onValueChanged.AddListener(OnSmoothingToggled);
    }

    void UpdateUI()
    {
        if (animator == null) return;

        // Update progress slider
        if (progressSlider != null)
        {
            progressSlider.value = animator.GetProgress();
        }

        // Update frame text
        if (frameText != null)
        {
            frameText.text = $"Frame: {animator.GetProgress():P0}";
        }

        // Update speed text
        if (speedText != null && speedSlider != null)
        {
            speedText.text = $"Speed: {speedSlider.value:F1}x";
        }

        // Update debug info
        if (debugInfoText != null && showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }

    void UpdateDebugInfo()
    {
        if (animator == null) return;

        string debugInfo = "Enhanced Animation Debug Info:\n";
        debugInfo += $"• Animation Playing: {animator.GetProgress():P0}\n";
        debugInfo += $"• Playback Speed: {animator.playbackSpeed:F1}x\n";
        debugInfo += $"• Loop Animation: {animator.loopAnimation}\n";
        debugInfo += $"• Apply Smoothing: {animator.applySmoothing}\n";
        debugInfo += $"• Smoothing Factor: {animator.smoothingFactor:F2}\n";
        
        debugInfoText.text = debugInfo;
    }

    // UI Event Handlers
    public void PlayAnimation()
    {
        if (animator != null)
        {
            animator.PlayAnimation();
        }
    }

    public void PauseAnimation()
    {
        if (animator != null)
        {
            animator.PauseAnimation();
        }
    }

    public void StopAnimation()
    {
        if (animator != null)
        {
            animator.StopAnimation();
        }
    }

    public void OnProgressChanged(float value)
    {
        if (animator != null)
        {
            animator.SetProgress(value);
        }
    }

    public void OnSpeedChanged(float value)
    {
        if (animator != null)
        {
            animator.playbackSpeed = value;
        }
    }

    public void OnLoopToggled(bool value)
    {
        if (animator != null)
        {
            animator.loopAnimation = value;
        }
    }

    public void OnSmoothingToggled(bool value)
    {
        if (animator != null)
        {
            animator.applySmoothing = value;
        }
    }

    // Public methods for external control
    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.playbackSpeed = Mathf.Clamp(speed, 0.1f, 3f);
        }
    }

    public void SetSmoothingFactor(float factor)
    {
        if (animator != null)
        {
            animator.smoothingFactor = Mathf.Clamp(factor, 0.01f, 1f);
        }
    }

    public void JumpToFrame(int frameIndex)
    {
        if (animator != null)
        {
            animator.SetFrame(frameIndex);
        }
    }

    public void JumpToProgress(float progress)
    {
        if (animator != null)
        {
            animator.SetProgress(Mathf.Clamp01(progress));
        }
    }
} 