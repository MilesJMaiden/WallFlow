using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using OpenAI;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class DalleManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher responseMatcher;
    [SerializeField] private TextMeshProUGUI transcriptionText;

    [Header("Prefab Settings")]
    [SerializeField] private GameObject resultPrefab; // The UI prefab to instantiate
    [SerializeField] private Camera mainCamera; // Reference to the player's main camera (headset)

    [Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<string> completeTranscription;

    private bool _voiceCommandReady;
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
        ReactivateVoice();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReactivateVoice();
        }
    }

    private void Awake()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        appVoiceExperience.Activate();
    }

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

    public void ReactivateVoice() => appVoiceExperience.Activate();

    private void WakeWordDetected(string[] arg0)
    {
        _voiceCommandReady = true;
        wakeWordDetected?.Invoke();
    }

    private void OnPartialTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        transcriptionText.text = transcription;
    }

    private void OnFullTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        _voiceCommandReady = false;
        completeTranscription?.Invoke(transcription);

        // Spawn the prefab and pass transcription to DallE script
        SpawnPrefabAndSetTranscription(transcription);
    }

    private void SpawnPrefabAndSetTranscription(string transcription)
    {
        // Instantiate the prefab in front of the player's head
        GameObject resultUI = Instantiate(resultPrefab);
        PositionPrefabInFrontOfPlayer(resultUI);

        // Access the DallE handler from the prefab
        DallEHandler dallEHandler = resultUI.GetComponent<DallEHandler>();
        if (dallEHandler != null)
        {
            dallEHandler.SetInputFieldText(transcription);
        }
        else
        {
            Debug.LogWarning("DallEHandler component not found on the instantiated prefab.");
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
