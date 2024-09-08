using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class ColorTransfer : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private LineDrawing lineDrawing;
    public GameObject drawing;
    public Color storedColor; // Variable to store the color

    [Tooltip("Reference to the LineDrawing object to pass the color to.")]

    private void Awake()
    {
        // Get the SpriteRenderer component attached to this GameObject
        meshRenderer = GetComponent<MeshRenderer>();
        //storedColor = meshRenderer.material.color;

    }

    // OnTriggerEnter is called when another collider enters the trigger attached to this GameObject
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("something goes in!!");
        // Check if the collider has the tag "PenTip"
        if (other.CompareTag("PenTip"))
        {
            Debug.Log("PenTip has entered collider");
            lineDrawing = drawing.GetComponent<LineDrawing>();

            // Pass the stored color to the LineDrawing object
            if (lineDrawing != null)
            {
                storedColor = meshRenderer.material.color;
            }
            else
            {
                Debug.Log("LineDrawing reference is not set in the Inspector.");
            }
        }
    }
}
