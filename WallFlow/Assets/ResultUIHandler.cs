using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUIHandler : MonoBehaviour
{
    [Header("UI Components")]
    public Image aiResultImage; // Assign this in the prefab inspector
    public TMP_InputField inputField; // Assign this in the prefab inspector
    public GameObject loadingLabel; // Assign this in the prefab inspector
    public TextMeshProUGUI transcriptionText; // Add this field and assign it in the inspector
}
