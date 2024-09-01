using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColourFromList : MonoBehaviour
{
    public List<Color> colours;

    public void SetColour(int i)
    {
        GetComponent<Renderer>().material.color = colours[i];
    }
}
