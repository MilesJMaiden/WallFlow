using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardChildrenManager : MonoBehaviour
{
    // Public GameObject field to assign in the Inspector
    public GameObject targetObject;

    // This method will delete all children of the assigned target object
    public void DeleteChildren()
    {
        // Check if the target object is assigned
        if (targetObject != null)
        {
            // Loop through each child of the target GameObject
            foreach (Transform child in targetObject.transform)
            {
                // Destroy each child GameObject
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("Target object is not assigned.");
        }
    }
}
