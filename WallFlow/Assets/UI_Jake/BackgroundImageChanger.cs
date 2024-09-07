using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageChanger : MonoBehaviour
{
    // The UI panel's Image component (the panel whose image will change)
    public Image panelImage;

    // Array of buttons (in sync with the sprites array)
    public Button[] buttons;

    // Array of background images corresponding to the buttons
    public Sprite[] backgroundImages;

    void Start()
    {
        // Ensure the arrays are in sync and have the same length
        if (buttons.Length != backgroundImages.Length)
        {
            Debug.LogError("The buttons array and backgroundImages array must have the same length.");
            return;
        }

        // Loop through each button and add a listener for the button's click event
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture the index for use in the listener
            buttons[i].onClick.AddListener(() => ChangePanelImage(index));
        }
    }

    // Method to change the panel's background image based on button index
    public void ChangePanelImage(int buttonIndex)
    {
        // Ensure the index is valid
        if (buttonIndex >= 0 && buttonIndex < backgroundImages.Length)
        {
            panelImage.sprite = backgroundImages[buttonIndex];
        }
        else
        {
            Debug.LogWarning("Button index out of bounds!");
        }
    }
}
