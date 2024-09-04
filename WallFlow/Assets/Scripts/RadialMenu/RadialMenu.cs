using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RadialMenu : MonoBehaviour
{
    public OVRInput.Button spawnRadialMenuButton;

    [Range(2, 6)]
    public int segmentTotal;
    public GameObject radialSegment;
    public Transform radialSegmentCanvas;
    public float angleBetweenSegments = 5f;
    public Transform handTransform;

    public UnityEvent<int> onSegmentSelected;

    public Sprite[] segmentIcons; // Array to hold the sprites for each segment

    private List<GameObject> spawnedSegments = new List<GameObject>();
    private int currentSelectedRadialSegment = -1;

    [SerializeField]
    private GameObject audioCaptureToolPrefab; // Prefab reference for AudioCaptureTool

    [SerializeField]
    private AudioCaptureTool audioCaptureTool; // Reference to AudioCaptureTool in the scene

    // Keyboard keys for testing
    public KeyCode testActivateKey = KeyCode.T; // Key to activate the AudioCaptureTool
    public KeyCode testDeactivateKey = KeyCode.Y; // Key to deactivate the AudioCaptureTool

    void Update()
    {
        if (OVRInput.GetDown(spawnRadialMenuButton))
        {
            SpawnRadialMenu();
        }

        if (OVRInput.Get(spawnRadialMenuButton))
        {
            GetSelectedRadialSegment();
        }

        if (OVRInput.GetUp(spawnRadialMenuButton))
        {
            HideAndTriggerSelected();
        }

        // Check for test activation and deactivation keys
        if (Input.GetKeyDown(testActivateKey))
        {
            ActivateAudioCaptureTool();
        }

        if (Input.GetKeyDown(testDeactivateKey))
        {
            DeactivateAudioCaptureTool();
        }
    }

    public void HideAndTriggerSelected()
    {
        onSegmentSelected.Invoke(currentSelectedRadialSegment);
        radialSegmentCanvas.gameObject.SetActive(false);

        // Execute logic based on the selected segment
        switch (currentSelectedRadialSegment)
        {
            case 0:
                ExecuteAudioCaptureTool();
                break;
            case 1:
                ExecuteTool2();
                break;
            case 2:
                ExecuteTool3();
                break;
            case 3:
                ExecuteTool4();
                break;
            // Add more cases if you have more segments
            default:
                Debug.LogWarning("Unknown segment selected");
                break;
        }
    }

    public void GetSelectedRadialSegment()
    {
        Vector3 centerToHand = handTransform.position - radialSegmentCanvas.position;
        Vector3 centerToHandProjected = Vector3.ProjectOnPlane(centerToHand, radialSegmentCanvas.forward);

        float angle = Vector3.SignedAngle(radialSegmentCanvas.up, centerToHandProjected, -radialSegmentCanvas.forward);

        if (angle < 0)
        {
            angle += 360;
        }

        currentSelectedRadialSegment = (int)(angle * segmentTotal / 360);

        for (int i = 0; i < spawnedSegments.Count; i++)
        {
            if (i == currentSelectedRadialSegment)
            {
                spawnedSegments[i].GetComponent<Image>().color = Color.yellow;
                spawnedSegments[i].transform.localScale = 1.1f * Vector3.one;
            }
            else
            {
                spawnedSegments[i].GetComponent<Image>().color = Color.white;
                spawnedSegments[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void SpawnRadialMenu()
    {
        radialSegmentCanvas.gameObject.SetActive(true);
        radialSegmentCanvas.position = handTransform.position;
        radialSegmentCanvas.rotation = handTransform.rotation;

        foreach (var item in spawnedSegments)
        {
            Destroy(item);
        }

        spawnedSegments.Clear();

        for (int i = 0; i < segmentTotal; i++)
        {
            float angle = -i * 360 / segmentTotal - angleBetweenSegments / 2;
            Vector3 radialSegmentEulerAngle = new Vector3(0, 0, angle);

            GameObject spawnedRadialSegment = Instantiate(radialSegment, radialSegmentCanvas);
            spawnedRadialSegment.transform.position = radialSegmentCanvas.position;
            spawnedRadialSegment.transform.localEulerAngles = radialSegmentEulerAngle;

            spawnedRadialSegment.GetComponent<Image>().fillAmount = (1 / (float)segmentTotal) - (angleBetweenSegments / 360);

            // Create and position the icon image in the center of the segment
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(spawnedRadialSegment.transform, false); // Set parent with 'false' to maintain local positioning
            Image iconImage = icon.AddComponent<Image>();

            // Assign the sprite from the array based on the segment index
            if (segmentIcons != null && i < segmentIcons.Length)
            {
                iconImage.sprite = segmentIcons[i]; // Use the sprite from the array
            }

            iconImage.rectTransform.sizeDelta = new Vector2(50, 50); // Set the size of the icon
            iconImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Anchor to the center
            iconImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); // Anchor to the center
            iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f); // Pivot in the center

            // Set the icon position to (45, 67.5) for all segments
            iconImage.rectTransform.anchoredPosition = new Vector2(45, 67.5f);

            // Scale the icon to 0.5 on x, y, and z axes
            icon.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Invert the icon's rotation based on the parent's Z rotation
            icon.transform.localRotation = Quaternion.Euler(0, 0, -spawnedRadialSegment.transform.localEulerAngles.z);

            // Set the icon's color to black
            iconImage.color = Color.black;

            spawnedSegments.Add(spawnedRadialSegment);
        }
    }

    // Method to activate the AudioCaptureTool
    private void ExecuteAudioCaptureTool()
    {
        if (audioCaptureTool != null)
        {
            audioCaptureTool.ActivateTool(); // Activate the AudioCaptureTool
            Debug.Log("Audio Capture Tool activated");
        }
        else
        {
            Debug.LogError("AudioCaptureTool reference is not set in the Inspector.");
        }
    }

    // Method for keyboard activation of the AudioCaptureTool
    private void ActivateAudioCaptureTool()
    {
        if (audioCaptureTool != null)
        {
            audioCaptureTool.gameObject.SetActive(true); // Enable the GameObject
            audioCaptureTool.ActivateTool(); // Start the UI effects
            Debug.Log("Audio Capture Tool activated via keyboard.");
        }
        else
        {
            Debug.LogError("AudioCaptureTool reference is not set in the Inspector.");
        }
    }

    // Method for keyboard deactivation of the AudioCaptureTool
    private void DeactivateAudioCaptureTool()
    {
        if (audioCaptureTool != null && audioCaptureTool.gameObject.activeSelf)
        {
            audioCaptureTool.DeactivateTool(); // Reset and hide the tool
            Debug.Log("Audio Capture Tool deactivated via keyboard.");
        }
    }

    private void ExecuteTool2()
    {
        // Logic for Tool 2
        Debug.Log("Tool 2 activated");
    }

    private void ExecuteTool3()
    {
        // Logic for Tool 3
        Debug.Log("Tool 3 activated");
    }

    private void ExecuteTool4()
    {
        // Logic for Tool 4
        Debug.Log("Tool 4 activated");
    }

    // Add more methods if you have more segments
}
