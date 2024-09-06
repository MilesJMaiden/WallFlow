using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimplePrefabSpawner : MonoBehaviour
{
    public OVRInput.Button spawnPrefabButton; // Button to spawn the prefab
    public GameObject[] prefabs;              // Array of prefabs to spawn
    public GameObject[] previewPrefabs;       // Array of preview prefabs with semi-transparent materials
    public Sprite[] prefabImages;             // Array of sprites representing each prefab for UI
    public Transform controllerAnchor;        // Reference to the controller anchor for position adjustments

    [SerializeField]
    private Image selectedPrefabImage;        // UI element to display the currently selected prefab image

    private GameObject currentPreview;        // Current active preview instance
    private int selectedPrefabIndex = -1;     // Index of the currently selected prefab, starts at -1 to indicate no selection
    private float previewDistance = 1f;       // Initial distance of the preview from the controller
    private float scaleMultiplier = 1f;       // Scale factor for the preview and instantiated prefabs
    private float scaleStep = 0.1f;           // Incremental step for scaling
    private float positionStep = 0.1f;        // Incremental step for moving preview closer or farther

    void Start()
    {
        // Initialize without any preview until a prefab is selected
        if (previewPrefabs.Length > 0)
        {
            currentPreview = null; // No preview active initially
        }
        UpdatePrefabUI(selectedPrefabIndex);
    }

    void Update()
    {
        if (!gameObject.activeSelf || selectedPrefabIndex < 0) return; // Return early if the tool is not active or no prefab is selected

        // Create a ray from the controller
        Ray ray = new Ray(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),
                          OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Update preview prefab position and rotation to match the raycast hit
            Vector3 targetPosition = hit.point;
            Vector3 offset = controllerAnchor.forward * previewDistance;
            currentPreview.transform.position = targetPosition - offset;
            currentPreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            currentPreview.transform.localScale = Vector3.one * scaleMultiplier; // Apply scaling

            // Show preview while tool is active
            currentPreview.SetActive(true);

            // Spawn the selected prefab when the assigned button is pressed
            if (OVRInput.GetDown(spawnPrefabButton))
            {
                GameObject spawnedPrefab = Instantiate(prefabs[selectedPrefabIndex],
                                                       currentPreview.transform.position,
                                                       currentPreview.transform.rotation);
                spawnedPrefab.transform.localScale = Vector3.one * scaleMultiplier; // Apply the current scale to the instantiated prefab

                // Deactivate the tool after spawning
                DeactivateTool();
            }
        }
        else
        {
            // Hide the preview if no valid hit is detected
            currentPreview.SetActive(false);
        }

        // Handle thumbstick input for adjustments
        HandleThumbstickInput();
    }

    // Method to activate the spawner tool
    public void ActivateTool()
    {
        gameObject.SetActive(true);
        if (currentPreview != null)
        {
            currentPreview.SetActive(true); // Enable the preview if it exists
        }
    }

    // Method to deactivate the spawner tool
    public void DeactivateTool()
    {
        gameObject.SetActive(false);
        if (currentPreview != null)
        {
            currentPreview.SetActive(false); // Disable the preview if it exists
        }
    }

    // Method to set the selected prefab from the UI
    public void SetSelectedPrefab(int index)
    {
        if (index >= 0 && index < prefabs.Length && index < previewPrefabs.Length && index < prefabImages.Length)
        {
            selectedPrefabIndex = index;

            // Destroy the current preview and instantiate the new preview prefab
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }

            // Instantiate the selected preview prefab for visualization
            currentPreview = Instantiate(previewPrefabs[selectedPrefabIndex]);
            currentPreview.SetActive(true); // Show the preview
            currentPreview.transform.localScale = Vector3.one * scaleMultiplier; // Apply the initial scale

            // Activate the spawner tool to start previewing
            ActivateTool();

            // Update UI to reflect the selected prefab
            UpdatePrefabUI(index);
        }
        else
        {
            Debug.LogWarning("Invalid prefab index selected.");
        }
    }

    // Private method to update UI to show the selected prefab
    private void UpdatePrefabUI(int index)
    {
        // Set the selected prefab image in the UI to reflect the current selection
        if (selectedPrefabImage != null && index >= 0 && index < prefabImages.Length)
        {
            selectedPrefabImage.sprite = prefabImages[index];
            selectedPrefabImage.color = Color.white; // Highlight the selected image
        }
        else
        {
            selectedPrefabImage.color = Color.clear; // Clear selection if no valid index
        }
    }

    // Method to handle thumbstick input for position and scale adjustments
    private void HandleThumbstickInput()
    {
        Vector2 thumbstickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick); // Read thumbstick input

        // Adjust distance to raycast hit point with thumbstick up/down
        if (thumbstickInput.y > 0.1f)
        {
            previewDistance = Mathf.Max(0.1f, previewDistance - positionStep); // Move closer to the raycast point
        }
        else if (thumbstickInput.y < -0.1f)
        {
            previewDistance += positionStep; // Move closer to the controller
        }

        // Adjust scale with thumbstick left/right
        if (thumbstickInput.x > 0.1f)
        {
            scaleMultiplier += scaleStep; // Increase scale
        }
        else if (thumbstickInput.x < -0.1f)
        {
            scaleMultiplier = Mathf.Max(0.1f, scaleMultiplier - scaleStep); // Decrease scale
        }
    }
}