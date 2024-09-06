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

    private GameObject currentPreview;
    private int selectedPrefabIndex = -1; // Indicates no prefab is selected initially

    private void Start()
    {
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
        int buttonsPerRow = 4;
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
    /// Activates the spawner tool.
    /// </summary>
    public void ActivateTool()
    {
        // Ensure the tool is only activated when a prefab is selected
        if (selectedPrefabIndex != -1)
        {
            gameObject.SetActive(true);
            currentPreview.SetActive(true); // Enable the preview only when a prefab is selected
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
