using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;

namespace OpenAI
{
    public class DallE : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private GameObject loadingLabel;
        [SerializeField] private GameObject imagePrefab; // 이미지 복사본을 넣을 프리팹

        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            button.onClick.AddListener(SendImageRequest);
        }

        public void SetInputFieldText(string transcription)
        {
            inputField.text = transcription;
            Debug.Log("DallE: Text received from VoiceManager: " + transcription);

            SendImageRequest();
        }

        private async void SendImageRequest()
        {
            image.sprite = null;
            button.enabled = false;
            inputField.enabled = false;
            loadingLabel.SetActive(true);

            var response = await openai.CreateImage(new CreateImageRequest
            {
                Prompt = inputField.text,
                Size = ImageSize.Size256
            });

            if (response.Data != null && response.Data.Count > 0)
            {
                using (var request = new UnityWebRequest(response.Data[0].Url))
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    request.SendWebRequest();

                    while (!request.isDone) await Task.Yield();

                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(request.downloadHandler.data);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, 256, 256), Vector2.zero, 1f);
                    image.sprite = sprite;

                    // 이미지 생성이 완료되면 새로운 이미지 오브젝트를 생성
                    CreateImageCopy(sprite);
                }
            }
            else
            {
                Debug.LogWarning("No image was created from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
            loadingLabel.SetActive(false);
        }

        // 이미지를 복사하여 새로운 오브젝트를 생성하는 메서드
        private void CreateImageCopy(Sprite originalSprite)
        {
            // 이미지가 복사될 새로운 오브젝트 생성
            GameObject newImageObject = Instantiate(imagePrefab);

            // 새 오브젝트의 이미지 컴포넌트에 복사된 스프라이트 할당
            Image newImageComponent = newImageObject.GetComponent<Image>();
            if (newImageComponent != null)
            {
                newImageComponent.sprite = originalSprite;
                Debug.Log("New image object created with copied sprite.");
            }
            else
            {
                Debug.LogError("The imagePrefab does not have an Image component.");
            }

            // 새 이미지 오브젝트의 위치나 기타 설정을 추가할 수 있음
            newImageObject.transform.SetParent(transform.parent); // 부모를 설정하거나
            newImageObject.transform.position += new Vector3(300, 0, 0); // 원래 이미지 옆에 위치시킴
        }
    }
}
