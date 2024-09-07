using UnityEngine;
using UnityEngine.UI;
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

    [Header("ChatGPT Reference")]
    [SerializeField] private ChatGPT chatGPT;

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
        // Reactivate voice recognition every frame for testing purposes (optional depending on logic).
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

        // Send transcription to ChatGPT for processing
        if (chatGPT != null)
        {
            chatGPT.SetInputFieldText(transcription);
        }
        else
        {
            Debug.LogWarning("ChatGPT reference is null. SetInputFieldText() not called.");
        }

        // Deactivate voice recognition after processing
        // DeactivateVoiceObject();
    }

    /// <summary>
    /// Deactivates the voice recognition object.
    /// </summary>
    private void DeactivateVoiceObject()
    {
        gameObject.SetActive(false);
        Debug.Log("VoiceManager: GameObject has been deactivated.");
    }
}
