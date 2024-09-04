using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Events;
using Oculus.Voice;

public class AudioCaptureTool : MonoBehaviour
{
    public Sprite iconSprite;               // Sprite to be used for the icon, assigned via the inspector
    public Transform targetPosition;        // The target position for the icon to move to
    public Transform startingPosition;      // The starting position for the icon, set via the Inspector
    public Vector3 targetScale = new Vector3(0.05f, 0.05f, 0.05f); // The final scale of the icon
    public float tweenTime = 1f;            // Duration for the initial tween animation
    public OVRInput.Button stopRecordingButton; // Button to stop recording, set via Inspector
    public float doublePressTime = 0.5f;    // Time allowed between double presses

    private GameObject icon;                // The Icon GameObject
    private Vector3 initialScale;           // Initial scale of the icon
    private bool isRecording = false;
    private float lastPressTime = 0f;
    private bool isBobbing = false;         // State to track if the icon is currently bobbing

    // Reference to AppVoiceExperience for voice interaction
    [SerializeField] private AppVoiceExperience appVoiceExperience;

    private void Awake()
    {
        // Check if AppVoiceExperience is set in the Inspector
        if (appVoiceExperience == null)
        {
            Debug.LogError("AppVoiceExperience component not found. Please assign it in the Inspector.");
            return;
        }

        // Ensure the AppVoiceExperience is properly initialized
        appVoiceExperience.OnInitialized += () => Debug.Log("AppVoiceExperience initialized successfully.");

        // Subscribe to voice events
        appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener(OnMicLevelChanged);
        appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
        appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnStartListening);

        // Setup the icon GameObject
        if (icon == null)
        {
            icon = new GameObject("Icon");
            icon.transform.SetParent(transform, false); // Parent to the AudioCaptureTool GameObject
            SpriteRenderer iconSpriteRenderer = icon.AddComponent<SpriteRenderer>(); // Use SpriteRenderer for 3D sprite
            iconSpriteRenderer.sprite = iconSprite; // Assign the sprite from the inspector
        }

        // Set initial state
        ResetTool();
    }

    private void ResetTool()
    {
        // Ensure the tool is hidden and reset all UI elements to their initial state
        icon.transform.localScale = Vector3.zero;
        icon.transform.position = startingPosition != null ? startingPosition.position : Vector3.zero;
        initialScale = targetScale;
        LeanTween.cancel(icon); // Cancel any ongoing tweens to avoid conflicts
        gameObject.SetActive(false); // Initially set inactive
        isBobbing = false;
    }

    public void ActivateTool()
    {
        ResetTool();               // Reset UI elements before activating
        gameObject.SetActive(true); // Enable the GameObject in the scene
        ShowVisuals();              // Start the visual animations
    }

    public void DeactivateTool()
    {
        StopListening();          // Stop listening if active
        ResetTool();              // Reset tool visuals and state
    }

    public void ShowVisuals()
    {
        // Tween scale and position to the target position
        LeanTween.scale(icon, targetScale, tweenTime).setEase(LeanTweenType.easeOutBack).setOnComplete(StartListening);
        LeanTween.move(icon, targetPosition.position, tweenTime).setEase(LeanTweenType.easeOutQuad);
    }

    private void StartListening()
    {
        // Use AppVoiceExperience to start listening
        if (appVoiceExperience != null && !isRecording)
        {
            appVoiceExperience.Activate(); // Begin voice capture
            isRecording = true;
        }
    }

    private void OnMicLevelChanged(float micLevel)
    {
        // Trigger bobbing if mic level indicates audio input is detected
        if (micLevel > 0.1f && !isBobbing) // Adjust the threshold as necessary
        {
            StartBobbingAnimation();
        }
    }

    private void StartBobbingAnimation()
    {
        if (!isBobbing)
        {
            isBobbing = true;
            // Bobbing animation between 0.1f and 0.2f on the Y axis
            float bobRange = Random.Range(0.1f, 0.2f); // Random range for slight variation
            LeanTween.moveLocalY(icon, icon.transform.localPosition.y + bobRange, 0.5f)
                .setEase(LeanTweenType.easeInOutSine).setLoopPingPong();
        }
    }

    private void OnStoppedListening()
    {
        // Stop bobbing when listening stops and reset icon to target position
        isBobbing = false;
        LeanTween.cancel(icon); // Stop the bobbing animation
        LeanTween.move(icon, targetPosition.position, 0.5f).setEase(LeanTweenType.easeOutQuad); // Return smoothly to target position
    }

    private void OnStartListening()
    {
        // Reset bobbing state when listening starts
        isBobbing = false;
        LeanTween.cancel(icon); // Stop any previous animations
        LeanTween.move(icon, targetPosition.position, 0.1f).setEase(LeanTweenType.easeOutQuad); // Ensure it's at target position
    }

    private void StopListening()
    {
        if (appVoiceExperience != null && isRecording)
        {
            appVoiceExperience.Deactivate(); // Stop voice capture
            isRecording = false;
            OnStoppedListening(); // Reset the position
        }
    }

    private void TweenToInitialState()
    {
        // Tween icon back to its initial position and scale
        LeanTween.scale(icon, Vector3.zero, tweenTime).setEase(LeanTweenType.easeInBack);
        LeanTween.move(icon, startingPosition != null ? startingPosition.position : Vector3.zero, tweenTime).setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => gameObject.SetActive(false)); // Deactivate after returning to start
    }

    private void Update()
    {
        // Detect double press of the defined button to stop recording
        if (OVRInput.GetDown(stopRecordingButton))
        {
            if (Time.time - lastPressTime < doublePressTime)
            {
                StopListening(); // Stop the recording and animations
            }
            lastPressTime = Time.time;
        }
    }

    // Callback method when voice input is processed
    private void OnWitResponse(string response)
    {
        // Handle the string response from the voice input
        Debug.Log("Voice input received: " + response);
    }
}
