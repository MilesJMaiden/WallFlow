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

    private bool _voiceCommandReady = false; // 음성 인식 준비 상태

    private void Awake()
    {
        // 음성 인식 이벤트 리스너 설정
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        // 웨이크 워드 감지 이벤트 설정
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        // 앱 시작 시 음성 인식 비활성화
        appVoiceExperience.Activate();
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 해제
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
        // 필요시 다른 업데이트 로직 추가 가능
    }

    /// <summary>
    /// 웨이크 워드가 감지되었을 때 음성 인식을 활성화합니다.
    /// </summary>
    private void WakeWordDetected(string[] arg0)
    {
        _voiceCommandReady = true; // 음성 인식 준비 상태 설정
        wakeWordDetected?.Invoke(); // 웨이크 워드 감지 이벤트 호출
        ReactivateVoice(); // 음성 인식 활성화
    }

    /// <summary>
    /// 음성 인식을 재활성화하는 메서드.
    /// </summary>
    public void ReactivateVoice()
    {
        if (_voiceCommandReady) // 음성 인식이 준비되었을 때만 활성화
        {
            appVoiceExperience.Activate(); // 음성 인식 시작
            Debug.Log("Voice recognition activated.");
        }
    }

    /// <summary>
    /// 음성 인식 중 부분적 텍스트가 감지되었을 때 호출됩니다.
    /// </summary>
    private void OnPartialTranscription(string transcription)
    {
        if (_voiceCommandReady) // 음성 인식 준비 상태일 때만 텍스트 처리
        {
            transcriptionText.text = transcription; // UI에 부분 텍스트 업데이트
            Debug.Log("Partial transcription: " + transcription);
        }
    }

    /// <summary>
    /// 음성 인식이 완료되었을 때 호출되는 메서드.
    /// </summary>
    private void OnFullTranscription(string transcription)
    {
        if (_voiceCommandReady) // 음성 인식 준비 상태일 때만 완료 처리
        {
            _voiceCommandReady = false; // 음성 인식 완료 후 준비 상태 해제
            completeTranscription?.Invoke(transcription); // 전체 텍스트 처리
            Debug.Log("Full transcription: " + transcription);

            // 음성 인식 비활성화 (추가적인 처리가 없으면 다시 대기 상태로)
            appVoiceExperience.Deactivate();
        }
    }
}
