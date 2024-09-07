using UnityEngine;
using UnityEngine.Events;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using OpenAI;

[RequireComponent(typeof(AppVoiceExperience))]
public class DalleManager : MonoBehaviour
{
    #region Inspector Variables

    [Header("Wit Configuration")]
    [Tooltip("The voice experience component for handling voice commands.")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;

    [Tooltip("Response matcher for detecting specific voice commands.")]
    [SerializeField] private WitResponseMatcher responseMatcher;

    [Header("Prefab Settings")]
    [Tooltip("The UI prefab to instantiate for displaying the result.")]
    [SerializeField] private GameObject resultPrefab;

    [Tooltip("Reference to the player's main camera, typically the VR headset.")]
    [SerializeField] private Camera mainCamera;

    [Header("Voice Events")]
    [Tooltip("Event triggered when the wake word is detected.")]
    [SerializeField] private UnityEvent wakeWordDetected;

    [Tooltip("Event triggered when the transcription is completed.")]
    [SerializeField] private UnityEvent<string> completeTranscription;

    #endregion

    #region Private Variables

    private bool _voiceCommandReady;
    private bool isFirstDeactivationDone = false;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Initializes necessary components on Awake.
    /// </summary>
    private void Awake()
    {
        // Listen for voice events
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        // Access private event field from WitResponseMatcher
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        // Activate voice input
        appVoiceExperience.Activate();
    }

    /// <summary>
    /// Handles destruction and removes listeners when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.RemoveListener(WakeWordDetected);
        }
    }

    /// <summary>
    /// Runs logic that should happen at the start of the application.
    /// </summary>
    private void Start()
    {
        if (!isFirstDeactivationDone)
        {
            isFirstDeactivationDone = true;
        }
    }

    /// <summary>
    /// Updates the object each frame to check for manual voice reactivation.
    /// </summary>
    private void Update()
    {
        ReactivateVoice();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReactivateVoice();
        }
    }

    #endregion

    #region Voice Command Handlers

    /// <summary>
    /// Reactivates the voice input to await new commands.
    /// </summary>
    public void ReactivateVoice() => appVoiceExperience.Activate();

    /// <summary>
    /// Called when the wake word is detected.
    /// </summary>
    private void WakeWordDetected(string[] args)
    {
        _voiceCommandReady = true;
        wakeWordDetected?.Invoke();
    }

    /// <summary>
    /// Handles partial transcription from voice input (not used currently).
    /// </summary>
    private void OnPartialTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
    }

    /// <summary>
    /// Handles full transcription and spawns the result prefab with the transcription text.
    /// </summary>
    private void OnFullTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        _voiceCommandReady = false;
        completeTranscription?.Invoke(transcription);

        SpawnPrefabAndSetTranscription(transcription);
    }

    #endregion

    #region Prefab Management

    /// <summary>
    /// Spawns the result prefab and sets the transcription on the DallEHandler.
    /// </summary>
    private void SpawnPrefabAndSetTranscription(string transcription)
    {
        GameObject resultUI = Instantiate(resultPrefab);
        PositionPrefabInFrontOfPlayer(resultUI);

        DallEHandler dallEHandler = resultUI.GetComponent<DallEHandler>();
        if (dallEHandler != null)
        {
            dallEHandler.GenerateImageFromText(transcription, OnImageGenerated);
        }
        else
        {
            Debug.LogWarning("DallEHandler component not found on the instantiated prefab.");
        }
    }

    /// <summary>
    /// Callback invoked when the image is successfully generated and loaded.
    /// </summary>
    private void OnImageGenerated()
    {
        // Disable the DalleManager only after the image is loaded and displayed
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Positions the result prefab in front of the player.
    /// </summary>
    private void PositionPrefabInFrontOfPlayer(GameObject prefab)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 forwardPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        prefab.transform.position = forwardPosition;

        Vector3 lookDirection = mainCamera.transform.forward;
        lookDirection.y = 0; // Ensure it only rotates horizontally
        prefab.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    #endregion
}
