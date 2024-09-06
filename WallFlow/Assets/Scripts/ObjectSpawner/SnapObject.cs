using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapObject : MonoBehaviour
{
    public GameObject visualObject;
    public Material defaultMaterial;   // 默认材质
    public Material silhouetteMaterial; // Silhouette材质
    private Renderer objectRenderer;
    private bool isGrabbed = false;
    private SnappingPoint snappingPoint;

    void Start()
    {
        if (visualObject != null)
        {
            objectRenderer = visualObject.GetComponent<Renderer>();
        }
    }

    // 检测手部进入碰撞区域时更换为Silhouette材质
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            objectRenderer.material = silhouetteMaterial; // 显示Silhouette效果
        }
        else if (other.CompareTag("SnappingPoint"))
        {
            snappingPoint = other.GetComponent<SnappingPoint>();
        }
    }

    // 当手部离开碰撞区域时恢复默认材质
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand") && objectRenderer != null)
        {
            objectRenderer.material = defaultMaterial;
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
}
