using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro; // TextMeshPro 네임스페이스 추가

namespace OpenAI
{
    public class DallE : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField; // 기존 InputField를 TMP_InputField로 변경
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private GameObject loadingLabel;

        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            button.onClick.AddListener(SendImageRequest);
        }

        private void Update()
        {
            // I 키 입력을 감지하여 버튼 클릭 이벤트 발생
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("I key pressed! Simulating button click.");
                button.onClick.Invoke();  // 버튼의 클릭 이벤트 호출
            }
        }

        public void SetInputFieldText(string transcription)
        {
            // VoiceManager에서 받은 텍스트를 TMP_InputField에 설정
            inputField.text = transcription;
            Debug.Log("DallE: Text received from VoiceManager: " + transcription);

            // 이미지를 생성하기 위한 요청 시작
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
                Prompt = inputField.text, // TMP_InputField의 텍스트 사용
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
    }
}
