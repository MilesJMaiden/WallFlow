using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RadialMenu : MonoBehaviour
{
    #region Fields

    [Header("Radial Menu Settings")]
    [Tooltip("Button used to spawn the radial menu.")]
    public OVRInput.Button spawnRadialMenuButton;

    [Tooltip("Number of segments in the radial menu.")]
    [Range(2, 6)]
    public int segmentTotal;

    [Tooltip("Prefab for individual radial segments.")]
    public GameObject radialSegment;

    [Tooltip("Parent canvas for radial segments.")]
    public Transform radialSegmentCanvas;

    [Tooltip("Angle between segments in the radial menu.")]
    public float angleBetweenSegments = 5f;

    [Tooltip("Transform of the hand to position the radial menu.")]
    public Transform handTransform;

    [Tooltip("Event triggered when a segment is selected.")]
    public UnityEvent<int> onSegmentSelected;

    [Tooltip("Icons for each segment of the radial menu.")]
    public Sprite[] segmentIcons;

    private List<GameObject> spawnedSegments = new List<GameObject>();
    private int currentSelectedRadialSegment = -1;

    [Header("Tool References")]
    [Tooltip("Prefab reference for the Audio Capture Tool.")]
    [SerializeField]
    private GameObject audioCaptureToolPrefab;

    [Tooltip("Reference to the Audio Capture Tool in the scene.")]
    [SerializeField]
    private AudioCaptureTool audioCaptureTool;

    [Tooltip("Reference to the Prefab Spawner Tool in the scene.")]
    [SerializeField]
    private SimplePrefabSpawner prefabSpawnerTool;

    #region Testing Keys
    [Header("Testing Keyboard Shortcuts")]
    [Tooltip("Key to activate the Audio Capture Tool.")]
    public KeyCode testActivateAudioCaptureToolKey = KeyCode.T;

    [Tooltip("Key to deactivate the Audio Capture Tool.")]
    public KeyCode testDeactivateAudioCaptureToolKey = KeyCode.Y;

    [Tooltip("Key to activate the Prefab Spawner Tool.")]
    public KeyCode testActivatePrefabSpawnerToolKey = KeyCode.U;

    [Tooltip("Key to deactivate the Prefab Spawner Tool.")]
    public KeyCode testDeactivatePrefabSpawnerToolKey = KeyCode.I;
    #endregion

    #endregion

    #region Unity Methods

    /// <summary>
    /// Unity Update method called once per frame.
    /// Handles input for showing the radial menu and selecting segments.
    /// Also handles test keys for activating/deactivating tools.
    /// </summary>
    void Update()
    {
        HandleRadialMenuInput();
        HandleTestingInput();
    }

    #endregion

    #region Radial Menu Methods

    /// <summary>
    /// Handles the input for the radial menu button press.
    /// </summary>
    private void HandleRadialMenuInput()
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
    }

    /// <summary>
    /// Spawns the radial menu segments around the hand position.
    /// </summary>
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

    /// <summary>
    /// Identifies which radial segment is selected based on hand position.
    /// </summary>
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

    /// <summary>
    /// Hides the radial menu and triggers the selected segment's associated action.
    /// </summary>
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
                ExecutePrefabSpawnerTool();
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

    #endregion

    #region Tool Execution Methods

    /// <summary>
    /// Executes the Audio Capture Tool.
    /// </summary>
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

    /// <summary>
    /// Executes the Prefab Spawner Tool.
    /// </summary>
    private void ExecutePrefabSpawnerTool()
    {
        if (prefabSpawnerTool != null)
        {
            prefabSpawnerTool.ActivateTool(); // Activate the Prefab Spawner Tool
            Debug.Log("Prefab Spawner Tool activated");
        }
        else
        {
            Debug.LogError("PrefabSpawnerTool reference is not set in the Inspector.");
        }
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

    #endregion

    #region Testing Input Methods

    /// <summary>
    /// Handles keyboard input for testing activation and deactivation of tools.
    /// </summary>
    private void HandleTestingInput()
    {
        // Check for test activation and deactivation keys
        if (Input.GetKeyDown(testActivateAudioCaptureToolKey))
        {
            ActivateAudioCaptureTool();
        }

        if (Input.GetKeyDown(testDeactivateAudioCaptureToolKey))
        {
            DeactivateAudioCaptureTool();
        }

        if (Input.GetKeyDown(testActivatePrefabSpawnerToolKey))
        {
            ActivatePrefabSpawnerTool();
        }

        if (Input.GetKeyDown(testDeactivatePrefabSpawnerToolKey))
        {
            DeactivatePrefabSpawnerTool();
        }
    }

    /// <summary>
    /// Method for keyboard activation of the AudioCaptureTool.
    /// </summary>
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

    /// <summary>
    /// Method for keyboard deactivation of the AudioCaptureTool.
    /// </summary>
    private void DeactivateAudioCaptureTool()
    {
        if (audioCaptureTool != null && audioCaptureTool.gameObject.activeSelf)
        {
            audioCaptureTool.DeactivateTool(); // Reset and hide the tool
            Debug.Log("Audio Capture Tool deactivated via keyboard.");
        }
    }

    /// <summary>
    /// Method for keyboard activation of the Prefab Spawner Tool.
    /// </summary>
    private void ActivatePrefabSpawnerTool()
    {
        if (prefabSpawnerTool != null)
        {
            prefabSpawnerTool.ActivateTool(); // Activate the Prefab Spawner Tool
            Debug.Log("Prefab Spawner Tool activated via keyboard.");
        }
        else
        {
            Debug.LogError("PrefabSpawnerTool reference is not set in the Inspector.");
        }
    }

    /// <summary>
    /// Method for keyboard deactivation of the Prefab Spawner Tool.
    /// </summary>
    private void DeactivatePrefabSpawnerTool()
    {
        if (prefabSpawnerTool != null && prefabSpawnerTool.gameObject.activeSelf)
        {
            prefabSpawnerTool.DeactivateTool(); // Reset and hide the tool
            Debug.Log("Prefab Spawner Tool deactivated via keyboard.");
        }
    }

    #endregion
}
