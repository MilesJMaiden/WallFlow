using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimplePrefabSpawner : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Button used to spawn the prefab.")]
    public OVRInput.Button spawnPrefabButton;

    [Header("Prefab Settings")]
    [Tooltip("Array of prefabs to spawn.")]
    public GameObject[] prefabs;

    [Tooltip("Array of preview prefabs with semi-transparent materials.")]
    public GameObject[] previewPrefabs;

    [Header("UI Settings")]
    [Tooltip("Array of images representing each prefab.")]
    public Sprite[] prefabImages;

    [Tooltip("Canvas containing the button panel.")]
    public RectTransform canvas;

    [Tooltip("Panel inside the canvas that stretches to fit the canvas.")]
    public RectTransform buttonPanel;

    [Tooltip("Prefab for the button template used in the UI.")]
    public GameObject buttonPrefab;

    [Tooltip("Spacing between buttons in the grid layout.")]
    public Vector2 buttonSpacing = new Vector2(10f, 10f);

    [Tooltip("Padding applied around the canvas.")]
    public Vector2 canvasPadding = new Vector2(20f, 20f);

    // Public property to access the canvas from other classes
    public RectTransform Canvas => canvas;

    // Public property to access selectedPrefabIndex
    public int SelectedPrefabIndex => selectedPrefabIndex;

    private GameObject currentPreview;
    private int selectedPrefabIndex = -1; // Indicates no prefab is selected initially

    // Reference to the main camera (user's headset)
    public Camera mainCamera;

    private void Start()
    {
        // Get the main camera reference (VR headset)
        mainCamera = Camera.main;
        // Initially, do not instantiate any preview as no prefab is selected
        PopulateButtons();
    }

    private void Update()
    {
        // Ensure the tool is only active when the game object is active and a prefab is selected
        if (!gameObject.activeSelf || selectedPrefabIndex == -1) return;

        // Create a ray from the controller
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward);

        // Raycast to find placement position for the prefab
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Update preview prefab position and rotation to match the raycast hit
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Show preview while tool is active
            currentPreview.SetActive(true);

            // Spawn the selected prefab when the assigned button is pressed
            if (OVRInput.GetDown(spawnPrefabButton))
            {
                GameObject spawnedPrefab = Instantiate(prefabs[selectedPrefabIndex], hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                spawnedPrefab.transform.localScale = currentPreview.transform.localScale; // Match scale of the preview
                DeactivateTool(); // Deactivate the tool after spawning
            }
        }
        else
        {
            // Hide the preview if no valid hit is detected
            currentPreview.SetActive(false);
        }
    }

    /// <summary>
    /// Populates the UI with buttons representing each prefab.
    /// </summary>
    private void PopulateButtons()
    {
        int buttonCount = prefabImages.Length;

        // Set anchors and pivot of the canvas to the top-left
        canvas.anchorMin = new Vector2(0, 1);
        canvas.anchorMax = new Vector2(0, 1);
        canvas.pivot = new Vector2(0, 1);

        float canvasWidth = canvas.rect.width;
        float canvasHeight = canvas.rect.height;

        // Set to 4 buttons per row
        int buttonsPerRow = 2;
        int rows = Mathf.CeilToInt(buttonCount / (float)buttonsPerRow);

        // Calculate button dimensions considering canvas padding and spacing
        float availableWidth = canvasWidth - canvasPadding.x * 2; // Total width minus padding
        float maxButtonWidth = (availableWidth - (buttonsPerRow - 1) * buttonSpacing.x) / buttonsPerRow;
        float maxButtonHeight = maxButtonWidth / 16f * 9f; // Maintain a 16:9 aspect ratio

        // Calculate the total height needed and adjust canvas height if necessary
        float totalButtonHeight = rows * maxButtonHeight + (rows - 1) * buttonSpacing.y;

        if (totalButtonHeight > canvasHeight)
        {
            canvasHeight = totalButtonHeight + canvasPadding.y * 2; // Add padding to the height
            canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasHeight);
        }

        // Calculate starting X and Y positions based on padding
        float startX = canvasPadding.x;
        float startY = -canvasPadding.y; // Start from the top with padding

        for (int i = 0; i < buttonCount; i++)
        {
            GameObject buttonObject = Instantiate(buttonPrefab, buttonPanel);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();

            // Set anchors and pivot of the button to the top-left
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(0, 1);
            buttonRect.pivot = new Vector2(0, 1);

            Button button = buttonObject.GetComponent<Button>();
            Image buttonImage = buttonObject.GetComponent<Image>();

            // Set the button's image
            if (buttonImage != null && i < prefabImages.Length)
            {
                buttonImage.sprite = prefabImages[i];
                buttonImage.preserveAspect = true; // Preserve the image's aspect ratio
            }

            // Position buttons in a grid, starting from the top-left
            int row = i / buttonsPerRow;
            int column = i % buttonsPerRow;

            // Calculate X and Y position
            float xPos = startX + column * (maxButtonWidth + buttonSpacing.x);
            float yPos = startY - row * (maxButtonHeight + buttonSpacing.y);

            // Set button size and position
            buttonRect.sizeDelta = new Vector2(maxButtonWidth, maxButtonHeight);
            buttonRect.anchoredPosition = new Vector2(xPos, yPos);

            // Assign click event
            int index = i; // Capture the current index
            button.onClick.AddListener(() => OnPrefabButtonClicked(index));
        }

        // Adjust the canvas width to fit buttons, considering padding
        float totalWidth = buttonsPerRow * maxButtonWidth + (buttonsPerRow - 1) * buttonSpacing.x;
        if (totalWidth + canvasPadding.x * 2 > canvasWidth)
        {
            canvasWidth = totalWidth + canvasPadding.x * 2;
            canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasWidth);
        }
    }

    /// <summary>
    /// Handles the button click event to select a prefab.
    /// </summary>
    /// <param name="index">Index of the selected prefab.</param>
    private void OnPrefabButtonClicked(int index)
    {
        if (index >= 0 && index < prefabs.Length && index < previewPrefabs.Length)
        {
            selectedPrefabIndex = index;

            // Destroy the current preview if it exists
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }

            // Instantiate the preview prefab and set it to inactive initially
            currentPreview = Instantiate(previewPrefabs[selectedPrefabIndex]);
            currentPreview.SetActive(false);

            // Disable the UI canvas after selecting a prefab
            canvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Invalid prefab index selected.");
        }
    }

    /// <summary>
    /// Activates the spawner tool and positions the canvas in front of the user.
    /// </summary>
    public void ActivateTool()
    {
        gameObject.SetActive(true); // Always activate the spawner GameObject first
        canvas.gameObject.SetActive(true); // Enable the selection UI canvas
        PositionCanvasInFrontOfUser(); // Position the canvas in front of the headset
        Debug.Log("Prefab Spawner Tool activated.");
    }

    /// <summary>
    /// Positions the canvas 1.5 meters in front of the user's headset, aligning the canvas center with the user's forward view.
    /// Moves the canvas up by half of its height to ensure proper alignment with the user's view.
    /// </summary>
    private void PositionCanvasInFrontOfUser()
    {
        if (mainCamera != null)
        {
            // Calculate the position 1.5 meters in front of the camera
            Vector3 forwardPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;

            // Calculate the center of the canvas in world space
            Vector3 canvasCenter = canvas.TransformPoint(new Vector3(canvas.rect.width * 0.5f, -canvas.rect.height * 0.5f, 0));

            // Calculate the offset needed to center the canvas in front of the user
            Vector3 offset = canvas.position - canvasCenter;

            // Set the canvas position to the adjusted forward position with the offset applied
            canvas.position = forwardPosition + offset;

            // Calculate the vertical offset to move the canvas up by half of its height
            float halfCanvasHeightWorld = (canvas.rect.height * 0.5f) * canvas.lossyScale.y; // Adjust for canvas scale

            // Adjust the canvas position upwards by half of its height
            canvas.position += Vector3.up * halfCanvasHeightWorld;

            // Set the rotation so that the canvas faces the user
            canvas.rotation = Quaternion.LookRotation(forwardPosition - mainCamera.transform.position);

            // Adjust the height to match the user's eye level
            canvas.position = new Vector3(canvas.position.x, mainCamera.transform.position.y + halfCanvasHeightWorld, canvas.position.z);
        }
        else
        {
            Debug.LogError("Main camera reference is not set.");
        }
    }

    /// <summary>
    /// Deactivates the spawner tool.
    /// </summary>
    public void DeactivateTool()
    {
        gameObject.SetActive(false);
        if (currentPreview != null)
        {
            currentPreview.SetActive(false); // Disable the preview
        }
        selectedPrefabIndex = -1; // Reset selection
    }
}
