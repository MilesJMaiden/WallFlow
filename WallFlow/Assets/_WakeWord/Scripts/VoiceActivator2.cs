using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;


public class VoiceActivator2 : MonoBehaviour
{
    // VoiceManager�� ���� ����
    [SerializeField] private GameObject voiceManagerObject;

    private bool isFirstDeactivationDone = false; // �� �� ���� ��Ȱ��ȭ�� �ϱ� ���� �÷���

    private void Start()
    {
        // ������ ���۵Ǹ� �� �� �� VoiceManager ������Ʈ�� ��Ȱ��ȭ
        if (!isFirstDeactivationDone)
        {
            DeactivateVoice();
            isFirstDeactivationDone = true; // ���Ŀ��� �� �κ��� �ٽ� ������� �ʵ��� �÷��� ����
        }
    }

    private void Update()
    {


        // �����̽��� �Է��� ����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed in VoiceActivator!");

            // VoiceManager ������Ʈ�� ��Ȱ��ȭ ���¶�� �ٽ� Ȱ��ȭ
            if (!voiceManagerObject.activeInHierarchy)
            {
                voiceManagerObject.SetActive(true);
                Debug.Log("VoiceActivator: VoiceManager object reactivated.");

                // VoiceManager�� appVoiceExperience Ȱ��ȭ �Լ� ȣ��
                VoiceManager2 voiceManager2 = voiceManagerObject.GetComponent<VoiceManager2>();
                if (voiceManager2 != null)
                {
                    voiceManager2.ReactivateVoice();
                    Debug.Log("VoiceActivator: appVoiceExperience reactivated.");
                }
            }
        }


    }

    // VoiceManager ������Ʈ�� ��Ȱ��ȭ�ϴ� �Լ�
    private void DeactivateVoice()
    {
        if (voiceManagerObject != null)
        {
            voiceManagerObject.SetActive(false); // VoiceManager ������Ʈ ��Ȱ��ȭ
            Debug.Log("VoiceActivator: VoiceManager object has been deactivated on game start.");
        }
        else
        {
            Debug.LogError("VoiceActivator: VoiceManager object reference is missing!");
        }
    }
}