using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;

namespace OpenAI
{
    public class DallEHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Image image;
        [SerializeField] private GameObject loadingLabel;

        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            // Ensure loading label is hidden initially
            loadingLabel.SetActive(false);
        }

        public void SetInputFieldText(string transcription)
        {
            inputField.text = transcription;
            Debug.Log("DallEHandler: Text received from VoiceManager: " + transcription);

            // Show loading indicator
            ShowLoading();

            // Initiate the image request
            SendImageRequest();
        }

        private void ShowLoading()
        {
            loadingLabel.SetActive(true);
            inputField.enabled = false;  // Disable the input field during loading
        }

        private void HideLoading()
        {
            loadingLabel.SetActive(false);
            inputField.enabled = true;   // Re-enable the input field after loading
        }

        private async void SendImageRequest()
        {
            image.sprite = null; // Clear any previous image
            Debug.Log("Sending image request to OpenAI...");

            var response = await openai.CreateImage(new CreateImageRequest
            {
                Prompt = inputField.text,
                Size = ImageSize.Size256
            });

            if (response.Data != null && response.Data.Count > 0)
            {
                Debug.Log("Image URL received, starting download...");
                using (var request = UnityWebRequestTexture.GetTexture(response.Data[0].Url))
                {
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    request.SendWebRequest();

                    // Correctly await the UnityWebRequest to complete
                    while (!request.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Image download successful.");
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1f);
                        image.sprite = sprite;
                        Debug.Log("Image successfully assigned to the prefab.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to download image: {request.error}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No image was created from this prompt.");
            }

            // Hide the loading label regardless of success or failure
            HideLoading();
        }
    }
}
