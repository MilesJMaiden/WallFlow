using System.Collections;
using UnityEngine;

public class CanvasTweener : MonoBehaviour
{
    [Header("Tween Settings")]
    [SerializeField] private Transform targetPosition;      // The target position to move to
    [SerializeField] private Transform startingPosition;    // The starting position
    [SerializeField] private Vector3 targetScale = new Vector3(1f, 1f, 1f); // The final scale when tweened in
    [SerializeField] private float tweenTime = 1f;          // Duration for the tween animation
    [SerializeField] private float activeDuration = 5f;     // Time after which the canvas reverts to its start position and scale

    [Header("Testing Settings")]
    [SerializeField] private KeyCode activationKey = KeyCode.Space; // Key to activate the canvas for testing

    private Vector3 initialScale;                           // Initial scale of the canvas
    private Coroutine deactivationCoroutine;                // Reference to the deactivation coroutine

    private void Awake()
    {
        initialScale = Vector3.zero; // Set initial scale to zero for hidden state
        transform.localScale = initialScale;
        transform.position = startingPosition.position; // Set initial position to the starting position
    }

    private void Update()
    {
        // Check for the activation key press during runtime
        if (Input.GetKeyDown(activationKey))
        {
            ActivateCanvas();
        }
    }

    /// <summary>
    /// Activates the canvas, scales it up, and moves it to the target position.
    /// </summary>
    public void ActivateCanvas()
    {
        // Reset and prepare for tween
        ResetCanvas();

        // Tween to the target position and scale
        LeanTween.scale(gameObject, targetScale, tweenTime).setEase(LeanTweenType.easeOutBack).setOnComplete(StartDeactivationTimer);
        LeanTween.move(gameObject, targetPosition.position, tweenTime).setEase(LeanTweenType.easeOutQuad);
    }

    /// <summary>
    /// Starts the timer to reset the canvas after the specified duration.
    /// </summary>
    private void StartDeactivationTimer()
    {
        // Stop any previous deactivation coroutine and start a new one
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
        }
        deactivationCoroutine = StartCoroutine(ResetAfterTime());
    }

    /// <summary>
    /// Resets the canvas after a set duration by scaling it down and moving it back.
    /// </summary>
    private IEnumerator ResetAfterTime()
    {
        yield return new WaitForSeconds(activeDuration);

        // Tween to scale down and move back to the starting position
        LeanTween.scale(gameObject, Vector3.zero, tweenTime).setEase(LeanTweenType.easeInBack);
        LeanTween.move(gameObject, startingPosition.position, tweenTime).setEase(LeanTweenType.easeInQuad);
    }

    /// <summary>
    /// Resets the canvas to its initial scale and position without deactivating it.
    /// </summary>
    private void ResetCanvas()
    {
        LeanTween.cancel(gameObject); // Cancel any ongoing tweens
        transform.localScale = initialScale; // Reset scale
        transform.position = startingPosition.position; // Reset position
    }

    /// <summary>
    /// Re-activates the canvas animation and restarts the timer.
    /// </summary>
    public void ReactivateCanvas()
    {
        ActivateCanvas();
    }
}
