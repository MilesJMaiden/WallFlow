using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingPoint : MonoBehaviour
{
    public Material silhouetteMaterial; // Silhouette����
    public AudioClip attachSound;       // ������Ч
    private GameObject silhouetteObject; // ��������
    private AudioSource audioSource;     // ��Ƶ���
    private Transform snapPoint;         // �������λ�ú���ת

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = attachSound;
        snapPoint = transform; // �������λ�ú���ת�ǵ�ǰ�����λ��
    }

    // ��������봥������ʱ
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SnapObject"))
        {
            if (silhouetteObject == null)
            {
                // ʵ������������
                silhouetteObject = Instantiate(other.gameObject, snapPoint.position, snapPoint.rotation);
                Renderer silhouetteRenderer = silhouetteObject.GetComponent<Renderer>();
                silhouetteRenderer.material = silhouetteMaterial;
                Destroy(silhouetteObject.GetComponent<SnapObject>()); // �Ƴ�ʵ�ʵĽ����ű�����ֹ���ɱ�ץȡ
            }
        }
    }

    // ������ʧȥץȡʱ��������������
    public void SnapObject(GameObject snapObject)
    {
        Debug.Log(snapObject.name + "has been snapped");
        snapObject.transform.position = snapPoint.position;
        snapObject.transform.rotation = snapPoint.rotation;
        snapObject.GetComponent<Rigidbody>().isKinematic = true; // ��������
        audioSource.Play(); // ���Ÿ�����Ч
    }
}
