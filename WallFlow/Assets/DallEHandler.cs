using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

namespace OpenAI
{
    public class DallEHandler : MonoBehaviour
    {
        #region Inspector Variables

        [Tooltip("Image component where the generated image will be displayed.")]
        [SerializeField] private Image image;

        [Tooltip("UI label to show loading indicator while waiting for image generation.")]
        [SerializeField] private GameObject loadingLabel;

        #endregion

        #region Private Variables

        private OpenAIApi openai = new OpenAIApi();

        #endregion

        #region Unity Callbacks

        /// <summary>
        /// Initializes the DallEHandler by ensuring the loading label is hidden.
        /// </summary>
        private void Start()
        {
            loadingLabel.SetActive(false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the process of generating an image from the transcription text.
        /// </summary>
        /// <param name="transcription">The transcription text received from voice input.</param>
        /// <param name="onComplete">Callback to invoke when the image is loaded.</param>
        public void GenerateImageFromText(string transcription, Action onComplete)
        {
            Debug.Log("DallEHandler: Text received: " + transcription);

            ShowLoading();
            SendImageRequest(transcription, onComplete);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the loading label and disables the input field.
        /// </summary>
        private void ShowLoading()
        {
            loadingLabel.SetActive(true);
        }

        /// <summary>
        /// Hides the loading label.
        /// </summary>
        private void HideLoading()
        {
            loadingLabel.SetActive(false);
        }

        /// <summary>
        /// Sends an image generation request to the OpenAI API.
        /// </summary>
        private async void SendImageRequest(string transcription, Action onComplete)
        {
            image.sprite = null;
            Debug.Log("Sending image request to OpenAI...");

            try
            {
                var response = await openai.CreateImage(new CreateImageRequest
                {
                    Prompt = transcription,
                    Size = ImageSize.Size256
                });

                if (response.Data != null && response.Data.Count > 0)
                {
                    using (var request = UnityWebRequestTexture.GetTexture(response.Data[0].Url))
                    {
                        request.SendWebRequest();
                        while (!request.isDone) await Task.Yield();

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(request);
                            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                            image.sprite = sprite;
                            Debug.Log("Image successfully assigned.");
                        }
                        else
                        {
                            Debug.LogError("Failed to download image: " + request.error);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No image was created from this prompt.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error generating image: " + ex.Message);
            }
            finally
            {
                HideLoading();
                onComplete?.Invoke(); // Invoke the callback after image loading is complete
            }
        }

        #endregion
    }
}
