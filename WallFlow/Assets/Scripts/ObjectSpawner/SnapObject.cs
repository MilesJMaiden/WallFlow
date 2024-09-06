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
    public Material defaultMaterial;   // 默认材质
    public Material silhouetteMaterial; // Silhouette材质
    private List<Renderer> renderers;
    private GrabbableEditor grabbable;
    private bool isGrabbed = false;
    private SnappingPoint snappingPoint;
    
    void Start()
    {
        renderers = new List<Renderer>();

        // 获取当前对象及所有子物体的Renderer组件
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Debug.Log("Renderers are " + renderers.Count);
        foreach (Renderer rend in allRenderers)
        {
            renderers.Add(rend);
        }

        grabbable = GetComponent <GrabbableEditor>();

        // 监听抓取和释放事件
        //grabInteractable.selectEntered.AddListener(OnGrab);
        //grabInteractable.selectExited.AddListener(OnRelease);
    }

    // 检测手部进入碰撞区域时更换为Silhouette材质
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("things enter the Snap object collider");
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Hands entered");
            ChangeMaterials(silhouetteMaterial); // 显示Silhouette效果
        }
        else if (other.CompareTag("SnappingPoint"))
        {
            Debug.Log("Snapping point entered");
            snappingPoint = other.GetComponent<SnappingPoint>();
        }
    }

    // 当手部离开碰撞区域时恢复默认材质
    void OnTriggerExit(Collider other)
    {
        Debug.Log("things exit the Snap object collider");
        if (other.CompareTag("Hand") && renderers != null)
        {
            ChangeMaterials(defaultMaterial);
        }
    }


    // 当对象被抓取时
    public void GrabObject()
    {
        isGrabbed = true;
    }

    // 当对象被松开时
    public void ReleaseObject()
    {
        isGrabbed = false;

        if (snappingPoint != null)
        {
            // 如果在Snapping Point范围内，执行吸附逻辑
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
