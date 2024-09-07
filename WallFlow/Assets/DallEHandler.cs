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
                using (var request = UnityWebRequestTexture.GetTexture(response.Data[0].Url))
                {
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    request.SendWebRequest();

                    // Correct way to await the completion of UnityWebRequest
                    while (!request.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1f);
                        image.sprite = sprite;
                        Debug.Log("Image successfully assigned to the prefab.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to download image: " + request.error);
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
