using UnityEngine;

public class ColorTransfer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private Color storedColor; // Variable to store the color

    [Tooltip("Reference to the LineDrawing object to pass the color to.")]
    [SerializeField] private LineDrawing lineDrawing; // Reference to the LineDrawing object

    private void Awake()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the initial color of the SpriteRenderer
        if (spriteRenderer != null)
        {
            storedColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on this GameObject.");
        }
    }

    // OnTriggerEnter is called when another collider enters the trigger attached to this GameObject
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider has the tag "PenTip"
        if (other.CompareTag("PenTip"))
        {
            Debug.LogError("PenTip has entered collider");

            // Pass the stored color to the LineDrawing object
            if (lineDrawing != null)
            {
                lineDrawing.CurrentColor = storedColor;
                Debug.Log($"Color {storedColor} passed to LineDrawing.");
            }
            else
            {
                Debug.LogError("LineDrawing reference is not set in the Inspector.");
            }
        }
    }
}
