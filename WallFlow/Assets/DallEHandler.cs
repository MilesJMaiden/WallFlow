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
            // Initialization if needed
        }

        public void SetInputFieldText(string transcription)
        {
            inputField.text = transcription;
            Debug.Log("DallEHandler: Text received from VoiceManager: " + transcription);

            SendImageRequest();
        }

        private async void SendImageRequest()
        {
            image.sprite = null;
            inputField.enabled = false;
            loadingLabel.SetActive(true);

            var response = await openai.CreateImage(new CreateImageRequest
            {
                Prompt = inputField.text,
                Size = ImageSize.Size256
            });

            if (response.Data != null && response.Data.Count > 0)
            {
                string imageUrl = response.Data[0].Url;
                Debug.Log($"Image URL received: {imageUrl}");

                using (var request = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    request.SendWebRequest();

                    // Await the request completion
                    while (!request.isDone)
                    {
                        await Task.Yield();
                    }

                    // Check if request succeeded
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Image download successful.");

                        // Get texture from the download handler
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if (texture != null)
                        {
                            Debug.Log("Texture loaded successfully.");

                            // Create sprite from texture
                            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100.0f);
                            if (sprite != null)
                            {
                                image.sprite = sprite;
                                image.preserveAspect = true; // Ensure the aspect ratio is preserved
                                Debug.Log("Image successfully assigned to the prefab.");
                            }
                            else
                            {
                                Debug.LogError("Failed to create sprite from texture.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Texture is null after download.");
                        }
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

            inputField.enabled = true;
            loadingLabel.SetActive(false);
        }
    }
}
