using Fusion.XR.Shared.Grabbing;
using Oculus.Interaction;
using Oculus.Interaction.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static Oculus.Interaction.Demo.WaterSpray;
public class SnapObject : MonoBehaviour
{
    public GameObject visualObject;
    public Material defaultMaterial;   // Ĭ�ϲ���
    public Material silhouetteMaterial; // Silhouette����
    private List<Renderer> renderers;
    private GrabbableEditor grabbable;
    private bool isGrabbed = false;
    private SnappingPoint snappingPoint;
    
    void Start()
    {
        renderers = new List<Renderer>();

        // ��ȡ��ǰ���������������Renderer���
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Debug.Log("Renderers are " + renderers.Count);
        foreach (Renderer rend in allRenderers)
        {
            renderers.Add(rend);
        }

        grabbable = GetComponent <GrabbableEditor>();

        // ����ץȡ���ͷ��¼�
        //grabInteractable.selectEntered.AddListener(OnGrab);
        //grabInteractable.selectExited.AddListener(OnRelease);
    }

    // ����ֲ�������ײ����ʱ����ΪSilhouette����
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("things enter the Snap object collider");
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Hands entered");
            ChangeMaterials(silhouetteMaterial); // ��ʾSilhouetteЧ��
        }
        else if (other.CompareTag("SnappingPoint"))
        {
            Debug.Log("Snapping point entered");
            snappingPoint = other.GetComponent<SnappingPoint>();
        }
    }

    // ���ֲ��뿪��ײ����ʱ�ָ�Ĭ�ϲ���
    void OnTriggerExit(Collider other)
    {
        Debug.Log("things exit the Snap object collider");
        if (other.CompareTag("Hand") && renderers != null)
        {
            ChangeMaterials(defaultMaterial);
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
    private void ChangeMaterials(Material newMaterial)
    {
        foreach (Renderer rend in renderers)
        {
            rend.material = newMaterial;
        }
    }
}
