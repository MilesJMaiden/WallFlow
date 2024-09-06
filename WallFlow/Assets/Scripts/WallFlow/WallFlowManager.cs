using UnityEngine;
using Meta.XR.MRUtilityKit; // Ensure this namespace is correct based on your project setup
using Oculus.Interaction.Surfaces; // Namespace for the BoundsClipper component
using System.Collections;

public class WallFlowManager : MonoBehaviour
{
    public GameObject wallFlowPrefab; // Reference to the WallFlow prefab
    public float wallPadding = 0.1f;  // Padding for the canvas size relative to the wall
    public float canvasOffset = 0.01f; // Offset distance to move the canvas forward to avoid Z-fighting
    public KeyCode toggleCanvasKey = KeyCode.Q; // Key to toggle canvas active state for testing

    private GameObject wallFlowInstance;   // Instance of the WallFlow canvas
    private MRUKAnchor selectedWall;       // Reference to the selected wall anchor
    private RectTransform canvasRect;      // Reference to the Canvas RectTransform
    private BoundsClipper boundsClipper;   // Reference to the BoundsClipper component on 'Surface'
    private BoxCollider wallFlowCollider;  // Reference to the BoxCollider of the wallFlowPrefab

    void Awake()
    {
        // Start the initialization process with a coroutine to ensure MRUK is ready
        StartCoroutine(InitializeWallFlow());
    }

    private IEnumerator InitializeWallFlow()
    {
        // Wait until MRUK is initialized and ready
        yield return new WaitUntil(() => MRUK.Instance != null && MRUK.Instance.Rooms != null && MRUK.Instance.Rooms.Count > 0);

        // Initialize wallScale with default values
        Vector2 wallScale = Vector2.zero;

        // Try to find the key wall in the scene using MRUK
        selectedWall = MRUK.Instance?.GetCurrentRoom()?.GetKeyWall(out wallScale);

        // If no key wall is found, find the largest wall
        if (selectedWall == null)
        {
            Debug.LogWarning("No key wall found. Searching for the largest wall.");
            selectedWall = FindLargestWall(out wallScale);
        }

        // If a wall is found, proceed with the setup
        if (selectedWall != null)
        {
            // Instantiate the WallFlow prefab and set its parent to this GameObject
            wallFlowInstance = Instantiate(wallFlowPrefab, transform);

            // Find the RectTransform of the Canvas
            canvasRect = wallFlowInstance.GetComponent<RectTransform>();

            // Find the 'Surface' child object and its BoundsClipper component
            var surface = wallFlowInstance.transform.Find("Surface");
            if (surface != null)
            {
                boundsClipper = surface.GetComponent<BoundsClipper>();
            }

            // Find the BoxCollider attached to the wallFlowPrefab
            wallFlowCollider = wallFlowInstance.GetComponent<BoxCollider>();

            if (canvasRect == null || boundsClipper == null || wallFlowCollider == null)
            {
                Debug.LogError("Canvas RectTransform, Surface's BoundsClipper component, or BoxCollider component not found in WallFlow prefab.");
                yield break;
            }

            // Adjust the canvas to match the selected wall's position and size
            AnchorCanvasToWall(wallScale);
        }
        else
        {
            Debug.LogError("No suitable wall found in the current room.");
        }
    }

    private void Update()
    {
        // Toggle the active state of the instantiated canvas with the test key
        if (Input.GetKeyDown(toggleCanvasKey))
        {
            if (wallFlowInstance != null)
            {
                bool isActive = wallFlowInstance.activeSelf;
                wallFlowInstance.SetActive(!isActive);
                Debug.Log($"WallFlow canvas toggled to {(isActive ? "inactive" : "active")}");
            }
        }
    }

    private void AnchorCanvasToWall(Vector2 wallScale)
    {
        // Position the canvas to the selected wall's position and adjust its rotation
        wallFlowInstance.transform.position = selectedWall.GetAnchorCenter() + selectedWall.transform.forward * canvasOffset;
        wallFlowInstance.transform.rotation = selectedWall.transform.rotation;

        // Calculate the scale based on the wall size and apply padding
        Vector2 adjustedScale = new Vector2(
            wallScale.x * (1 - wallPadding),
            wallScale.y * (1 - wallPadding));

        // Apply the adjusted size to the Canvas RectTransform
        canvasRect.sizeDelta = adjustedScale; // Set the size of the Canvas to match the adjusted wall scale

        // Apply the adjusted size to the 'Surface' BoundsClipper component
        boundsClipper.Size = new Vector3(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y, 0.001f); // Match RectTransform and set Z to 0.001

        // Scale the BoxCollider of the wallFlowInstance to match the size of the wallFlowInstance, only adjusting x and y
        if (wallFlowCollider != null)
        {
            Vector3 newColliderSize = new Vector3(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y, wallFlowCollider.size.z); // Keep Z axis unchanged
            wallFlowCollider.size = newColliderSize;
        }
    }

    private MRUKAnchor FindLargestWall(out Vector2 largestWallScale)
    {
        largestWallScale = Vector2.zero;
        MRUKAnchor largestWall = null;
        float maxArea = 0f;

        // Check if MRUK instance and rooms are available
        if (MRUK.Instance == null || MRUK.Instance.Rooms == null)
        {
            Debug.LogError("MRUK instance or rooms are not initialized.");
            return null;
        }

        // Loop through all rooms and their anchors to find walls
        foreach (var room in MRUK.Instance.Rooms)
        {
            if (room.Anchors == null)
            {
                continue; // Skip if no anchors are available in the room
            }

            foreach (var anchor in room.Anchors)
            {
                if (anchor.PlaneRect.HasValue) // Check if the anchor has a defined plane (indicating it's a wall)
                {
                    Vector2 wallSize = anchor.PlaneRect.Value.size;
                    float area = wallSize.x * wallSize.y;

                    if (area > maxArea)
                    {
                        maxArea = area;
                        largestWall = anchor;
                        largestWallScale = wallSize;
                    }
                }
            }
        }

        if (largestWall != null)
        {
            Debug.Log($"Largest wall found with size: {largestWallScale} and area: {maxArea}");
        }
        else
        {
            Debug.LogWarning("No walls found in the current room.");
        }

        return largestWall;
    }
}
