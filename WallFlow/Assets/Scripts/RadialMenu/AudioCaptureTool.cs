using UnityEngine;
using UnityEngine.UI;
using Meta.WitAi; // Assuming this namespace for Meta Voice SDK

public class AudioCaptureTool : MonoBehaviour
{
    public Sprite iconSprite;               // Sprite to be used for the icon, assigned via the inspector
    public Transform targetPosition;        // The target position for the icon to move to
    public Transform startingPosition;      // The starting position for the icon, set via the Inspector
    public Vector3 targetScale = new Vector3(1, 1, 1); // The final scale of the icon
    public float tweenTime = 1f;            // Duration for the initial tween animation
    public OVRInput.Button stopRecordingButton; // Button to stop recording, set via Inspector
    public float doublePressTime = 0.5f;    // Time allowed between double presses

    private GameObject icon;                // The Icon GameObject
    private Vector3 initialScale;           // Initial scale of the icon

    private bool isRecording = false;
    private float lastPressTime = 0f;

    // META Voice SDK variables
    private Wit wit; // Reference to Wit component (assumes Wit.ai for voice recognition)

    private void Awake()
    {
        // Find the Wit component in the scene (make sure it's set up)
        wit = FindObjectOfType<Wit>();
        if (wit == null)
        {
            Debug.LogError("Wit component not found in the scene. Please add and configure Wit.");
        }

        // Find or create the icon GameObject and set it up
        if (icon == null)
        {
            icon = new GameObject("Icon");
            icon.transform.SetParent(transform, false); // Parent to the AudioCaptureTool GameObject
            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = iconSprite;
        }

        // Set initial state
        ResetTool();
    }

    private void ResetTool()
    {
        // Ensure the tool is hidden and reset to its initial state
        icon.transform.localScale = Vector3.zero;
        icon.transform.position = startingPosition != null ? startingPosition.position : Vector3.zero;
        initialScale = targetScale;
        gameObject.SetActive(false); // Initially set inactive
    }

    public void ActivateTool()
    {
        gameObject.SetActive(true); // Activate the tool
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
        // Start listening using the Meta Voice SDK (Wit.ai)
        if (wit != null)
        {
            wit.Activate(); // Begin voice capture
            isRecording = true;
            StartBobbingAnimation(); // Start bobbing animation to indicate listening
        }
    }

    private void StartBobbingAnimation()
    {
        // Bobbing animation to indicate active listening
        LeanTween.moveLocalY(icon, icon.transform.localPosition.y + 0.1f, 0.2f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong();
    }

    private void StopListening()
    {
        if (wit != null && isRecording)
        {
            wit.Deactivate(); // Stop voice capture
            isRecording = false;
            LeanTween.cancel(icon); // Stop the bobbing animation
            TweenToInitialState(); // Tween icon back to initial position and scale
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
