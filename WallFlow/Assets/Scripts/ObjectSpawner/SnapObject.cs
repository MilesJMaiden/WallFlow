using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapObject : MonoBehaviour
{
    private Renderer objectRenderer;
    public Material defaultMaterial;   // Ĭ�ϲ���
    public Material silhouetteMaterial; // Silhouette����
    private bool isGrabbed = false;
    private SnappingPoint snappingPoint;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    // ����ֲ�������ײ����ʱ����ΪSilhouette����
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            objectRenderer.material = silhouetteMaterial; // ��ʾSilhouetteЧ��
        }
        else if (other.CompareTag("SnappingPoint"))
        {
            snappingPoint = other.GetComponent<SnappingPoint>();
        }
    }

    // ���ֲ��뿪��ײ����ʱ�ָ�Ĭ�ϲ���
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            objectRenderer.material = defaultMaterial;
        }
    }

    // ������ץȡʱ
    public void GrabObject()
    {
        isGrabbed = true;
    }

    // �������ɿ�ʱ
    public void ReleaseObject()
    {
        isGrabbed = false;

        if (snappingPoint != null)
        {
            // �����Snapping Point��Χ�ڣ�ִ�������߼�
            snappingPoint.SnapObject(gameObject);
        }
    }
}
