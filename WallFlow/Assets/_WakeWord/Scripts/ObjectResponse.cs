using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectResponse : MonoBehaviour
{
    [SerializeField] List<GameObject> shapes; // 활성화할 여러 오브젝트 리스트
    [SerializeField] GameObject teddyBear; // Teddy Bear 오브젝트 참조

    public void SetTeddyBear(string[] response)
    {
        if (response.Length > 0)
        {
            // 음성 명령에서 'Go fish'가 있는지 확인
            if (response[0].ToLower() == "go" && response[1].ToLower() == "fish")
            {
                foreach (var shape in shapes)
                {
                    shape.SetActive(true); // 모든 shape 오브젝트 활성화
                }
            }

            // 'Teddy Bear' 단어를 감지하면 Teddy Bear 오브젝트 활성화
            if (IsTeddyBearMentioned(response))
            {
                teddyBear.SetActive(true); // Teddy Bear 오브젝트 활성화
                Debug.Log("Teddy Bear is activated!");
            }
        }
    }

    // 음성 명령에 'Teddy Bear'가 있는지 확인하는 함수
    private bool IsTeddyBearMentioned(string[] response)
    {
        foreach (var word in response)
        {
            // 단어가 'teddy' 또는 'bear'인 경우에 활성화
            if (word.ToLower().Contains("teddy") || word.ToLower().Contains("bear"))
            {
                return true;
            }
        }
        return false;
    }
}
