using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;


public class VoiceActivator2 : MonoBehaviour
{
    // VoiceManager에 대한 참조
    [SerializeField] private GameObject voiceManagerObject;

    private bool isFirstDeactivationDone = false; // 딱 한 번만 비활성화를 하기 위한 플래그

    private void Start()
    {
        // 게임이 시작되면 딱 한 번 VoiceManager 오브젝트를 비활성화
        if (!isFirstDeactivationDone)
        {
            DeactivateVoice();
            isFirstDeactivationDone = true; // 이후에는 이 부분이 다시 실행되지 않도록 플래그 설정
        }
    }

    private void Update()
    {


        // 스페이스바 입력을 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed in VoiceActivator!");

            // VoiceManager 오브젝트가 비활성화 상태라면 다시 활성화
            if (!voiceManagerObject.activeInHierarchy)
            {
                voiceManagerObject.SetActive(true);
                Debug.Log("VoiceActivator: VoiceManager object reactivated.");

                // VoiceManager의 appVoiceExperience 활성화 함수 호출
                VoiceManager2 voiceManager2 = voiceManagerObject.GetComponent<VoiceManager2>();
                if (voiceManager2 != null)
                {
                    voiceManager2.ReactivateVoice();
                    Debug.Log("VoiceActivator: appVoiceExperience reactivated.");
                }
            }
        }


    }

    // VoiceManager 오브젝트를 비활성화하는 함수
    private void DeactivateVoice()
    {
        if (voiceManagerObject != null)
        {
            voiceManagerObject.SetActive(false); // VoiceManager 오브젝트 비활성화
            Debug.Log("VoiceActivator: VoiceManager object has been deactivated on game start.");
        }
        else
        {
            Debug.LogError("VoiceActivator: VoiceManager object reference is missing!");
        }
    }
}