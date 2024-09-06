using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardChildrenManager : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void DeleteChildren()
    {
        // Loop through each child of the current GameObject
        foreach (Transform child in transform)
        {
            // Destroy each child GameObject
            Destroy(child.gameObject);
        }
    }
}
