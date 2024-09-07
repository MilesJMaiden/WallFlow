using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using OpenAI;

public class GPTManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher responseMatcher;
    [SerializeField] private TextMeshProUGUI transcriptionText;

    [Header("Prefab Settings")]
    [SerializeField] private GameObject chatGPTPrefab; // The UI prefab to instantiate
    [SerializeField] private Camera mainCamera; // Reference to the player's main camera (headset)

    [Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<string> completeTranscription;

    private bool _voiceCommandReady = false;
    private bool isFirstDeactivationDone = false;

    private void Start()
    {
        if (!isFirstDeactivationDone)
        {
            isFirstDeactivationDone = true;
        }
    }

    private void Update()
    {
        // Optional: Reactivate voice recognition on specific conditions or keys
        ReactivateVoice();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReactivateVoice();
        }
    }

    private void Awake()
    {
        // Setup voice events listeners
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        // Setup wake word detection event
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        // Activate voice recognition on app start
        appVoiceExperience.Activate();
    }

    private void OnDestroy()
    {
        // Cleanup listeners
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
    /// Re-activate voice recognition.
    /// </summary>
    public void ReactivateVoice()
    {
        appVoiceExperience.Activate();
        Debug.Log("Voice recognition activated.");
    }

    /// <summary>
    /// Called when a wake word is detected.
    /// </summary>
    private void WakeWordDetected(string[] arg0)
    {
        _voiceCommandReady = true;
        wakeWordDetected?.Invoke();
    }

    /// <summary>
    /// Handles partial transcription during voice recognition.
    /// </summary>
    private void OnPartialTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        transcriptionText.text = transcription;
    }

    /// <summary>
    /// Handles full transcription and sends text to ChatGPT.
    /// </summary>
    private void OnFullTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        _voiceCommandReady = false;
        completeTranscription?.Invoke(transcription);

        // Spawn the prefab and pass transcription to ChatGPTHandler
        SpawnPrefabAndSetTranscription(transcription);

        // Disable the manager after processing
        gameObject.SetActive(false);
        Debug.Log("GPTManager: GameObject has been deactivated.");
    }

    private void SpawnPrefabAndSetTranscription(string transcription)
    {
        // Instantiate the prefab in front of the player's head
        GameObject chatGPTUI = Instantiate(chatGPTPrefab);
        PositionPrefabInFrontOfPlayer(chatGPTUI);

        // Access the ChatGPTHandler from the prefab
        ChatGPTHandler chatGPTHandler = chatGPTUI.GetComponent<ChatGPTHandler>();
        if (chatGPTHandler != null)
        {
            chatGPTHandler.SetTextAndRequestResponse(transcription);
        }
        else
        {
            Debug.LogWarning("ChatGPTHandler component not found on the instantiated prefab.");
        }
    }

    private void PositionPrefabInFrontOfPlayer(GameObject prefab)
    {
        // Ensure mainCamera is assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Fallback to main camera
            if (mainCamera == null)
            {
                Debug.LogError("Main camera is not assigned and could not be found.");
                return;
            }
        }

        // Position 1.5 meters in front of the player's head
        Vector3 forwardPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;

        // Set the prefab position
        prefab.transform.position = forwardPosition;

        // Rotate the prefab to face the player
        prefab.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
    }
}
