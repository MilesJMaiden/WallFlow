using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using OpenAI;

/// <summary>
/// Manages the integration of voice commands into text using Oculus Voice and Wit.ai services.
/// Controls activation and deactivation of the voice manager and handles wake words and transcriptions.
/// </summary>
public class VoiceToTextManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher responseMatcher;
    [SerializeField] private TextMeshProUGUI transcriptionText;

    [Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<string> completeTranscription;

    private bool _voiceCommandReady = false; // ���� �ν� �غ� ����

    private void Awake()
    {
        // ���� �ν� �̺�Ʈ ������ ����
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        // ����ũ ���� ���� �̺�Ʈ ����
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        // �� ���� �� ���� �ν� ��Ȱ��ȭ
        appVoiceExperience.Activate();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ������ ����
        appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.RemoveListener(WakeWordDetected);
        }
    }

    private void Update()
    {
        // �ʿ�� �ٸ� ������Ʈ ���� �߰� ����
    }

    /// <summary>
    /// ����ũ ���尡 �����Ǿ��� �� ���� �ν��� Ȱ��ȭ�մϴ�.
    /// </summary>
    private void WakeWordDetected(string[] arg0)
    {
        _voiceCommandReady = true; // ���� �ν� �غ� ���� ����
        wakeWordDetected?.Invoke(); // ����ũ ���� ���� �̺�Ʈ ȣ��
        ReactivateVoice(); // ���� �ν� Ȱ��ȭ
    }

    /// <summary>
    /// ���� �ν��� ��Ȱ��ȭ�ϴ� �޼���.
    /// </summary>
    public void ReactivateVoice()
    {
        if (_voiceCommandReady) // ���� �ν��� �غ�Ǿ��� ���� Ȱ��ȭ
        {
            appVoiceExperience.Activate(); // ���� �ν� ����
            Debug.Log("Voice recognition activated.");
        }
    }

    /// <summary>
    /// ���� �ν� �� �κ��� �ؽ�Ʈ�� �����Ǿ��� �� ȣ��˴ϴ�.
    /// </summary>
    private void OnPartialTranscription(string transcription)
    {
        if (_voiceCommandReady) // ���� �ν� �غ� ������ ���� �ؽ�Ʈ ó��
        {
            transcriptionText.text = transcription; // UI�� �κ� �ؽ�Ʈ ������Ʈ
            Debug.Log("Partial transcription: " + transcription);
        }
    }

    /// <summary>
    /// ���� �ν��� �Ϸ�Ǿ��� �� ȣ��Ǵ� �޼���.
    /// </summary>
    private void OnFullTranscription(string transcription)
    {
        if (_voiceCommandReady) // ���� �ν� �غ� ������ ���� �Ϸ� ó��
        {
            _voiceCommandReady = false; // ���� �ν� �Ϸ� �� �غ� ���� ����
            completeTranscription?.Invoke(transcription); // ��ü �ؽ�Ʈ ó��
            Debug.Log("Full transcription: " + transcription);

            // ���� �ν� ��Ȱ��ȭ (�߰����� ó���� ������ �ٽ� ��� ���·�)
            appVoiceExperience.Deactivate();
        }
    }
}
