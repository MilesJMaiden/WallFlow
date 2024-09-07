using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;


public class VoiceActivator : MonoBehaviour
{
    
    [SerializeField] private GameObject voiceManagerObject;

    private bool isFirstDeactivationDone = false; 

    private void Start()
    {
        
        if (!isFirstDeactivationDone)
        {
            DeactivateVoice();
            isFirstDeactivationDone = true; 
        }
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Space key pressed in VoiceActivator!");


            if (!voiceManagerObject.activeInHierarchy)
            {
                voiceManagerObject.SetActive(true);
                Debug.Log("VoiceActivator: VoiceManager object reactivated.");


                VoiceManager voiceManager = voiceManagerObject.GetComponent<VoiceManager>();
                if (voiceManager != null)
                {
                    voiceManager.ReactivateVoice();
                    Debug.Log("VoiceActivator: appVoiceExperience reactivated.");
                }
            }
        }

    }

    
    private void DeactivateVoice()
    {
        if (voiceManagerObject != null)
        {
            voiceManagerObject.SetActive(false); 
            Debug.Log("VoiceActivator: VoiceManager object has been deactivated on game start.");
        }
        else
        {
            Debug.LogError("VoiceActivator: VoiceManager object reference is missing!");
        }
    }
}