using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using UnityEngine.Networking;
using System.Threading.Tasks;
using OpenAI;

public class DalleManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher responseMatcher;

    [Header("UI Prefab Settings")]
    [SerializeField] private GameObject resultPrefab; // The UI prefab to instantiate
    [SerializeField] private Camera mainCamera; // Reference to the player's main camera (headset)

    [Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<string> completeTranscription;

    private bool _voiceCommandReady;
    private bool isFirstDeactivationDone = false;

    private OpenAIApi openai = new OpenAIApi();

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

        // Since we're passing this to the handler now, the manager does not need to manage this directly.
    }

    private void OnFullTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        _voiceCommandReady = false;
        completeTranscription?.Invoke(transcription);

        SetInputFieldText(transcription);
    }

    public void SetInputFieldText(string transcription)
    {
        // Instantiate the UI prefab in front of the player's head
        GameObject resultUI = Instantiate(resultPrefab);
        PositionPrefabInFrontOfPlayer(resultUI);

        // Access the UI components from the prefab through ResultUIHandler
        ResultUIHandler uiHandler = resultUI.GetComponent<ResultUIHandler>();
        if (uiHandler != null)
        {
            uiHandler.inputField.text = transcription;
            uiHandler.transcriptionText.text = transcription; // Access transcriptionText correctly
            Debug.Log("DalleManager: Text received from VoiceManager: " + transcription);

            // Directly send the image request based on the input field text
            SendImageRequest(uiHandler);
        }
        else
        {
            Debug.LogError("ResultUIHandler not found on the prefab.");
        }
    }

    private async void SendImageRequest(ResultUIHandler uiHandler)
    {
        uiHandler.inputField.enabled = false;
        uiHandler.loadingLabel.SetActive(true);

        var response = await openai.CreateImage(new CreateImageRequest
        {
            Prompt = uiHandler.inputField.text,
            Size = ImageSize.Size256
        });

        if (response.Data != null && response.Data.Count > 0)
        {
            using (var request = new UnityWebRequest(response.Data[0].Url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                request.SendWebRequest();

                while (!request.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(request.downloadHandler.data);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, 256, 256), Vector2.zero, 1f);

                    // Assign the generated image to the AIResult child
                    if (uiHandler.aiResultImage != null)
                    {
                        uiHandler.aiResultImage.sprite = sprite;
                    }
                    else
                    {
                        Debug.LogError("AIResult Image not assigned in ResultUIHandler.");
                    }
                }
                else
                {
                    Debug.LogWarning("Failed to download image: " + request.error);
                }
            }
        }
        else
        {
            Debug.LogWarning("No image was created from this prompt.");
        }

        uiHandler.inputField.enabled = true;
        uiHandler.loadingLabel.SetActive(false);

        // Disable this manager object after the operation
        gameObject.SetActive(false);
        Debug.Log("VoiceManager: GameObject has been deactivated after spawning result UI.");
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
