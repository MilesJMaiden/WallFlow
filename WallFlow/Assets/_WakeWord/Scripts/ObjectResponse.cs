using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectResponse : MonoBehaviour
{
    [SerializeField] List<GameObject> shapes; // Ȱ��ȭ�� ���� ������Ʈ ����Ʈ
    [SerializeField] GameObject teddyBear; // Teddy Bear ������Ʈ ����

    public void SetTeddyBear(string[] response)
    {
        if (response.Length > 0)
        {
            // ���� ��ɿ��� 'Go fish'�� �ִ��� Ȯ��
            if (response[0].ToLower() == "go" && response[1].ToLower() == "fish")
            {
                foreach (var shape in shapes)
                {
                    shape.SetActive(true); // ��� shape ������Ʈ Ȱ��ȭ
                }
            }

            // 'Teddy Bear' �ܾ �����ϸ� Teddy Bear ������Ʈ Ȱ��ȭ
            if (IsTeddyBearMentioned(response))
            {
                teddyBear.SetActive(true); // Teddy Bear ������Ʈ Ȱ��ȭ
                Debug.Log("Teddy Bear is activated!");
            }
        }
    }

    // ���� ��ɿ� 'Teddy Bear'�� �ִ��� Ȯ���ϴ� �Լ�
    private bool IsTeddyBearMentioned(string[] response)
    {
        foreach (var word in response)
        {
            // �ܾ 'teddy' �Ǵ� 'bear'�� ��쿡 Ȱ��ȭ
            if (word.ToLower().Contains("teddy") || word.ToLower().Contains("bear"))
            {
                return true;
            }
        }
        return false;
    }
}
