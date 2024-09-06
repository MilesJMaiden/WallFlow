using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingPoint : MonoBehaviour
{
    public Material silhouetteMaterial; // Silhouette材质
    public AudioClip attachSound;       // 附加音效
    private GameObject silhouetteObject; // 轮廓对象
    private AudioSource audioSource;     // 音频组件
    private Transform snapPoint;         // 吸附点的位置和旋转

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = attachSound;
        snapPoint = transform; // 吸附点的位置和旋转是当前对象的位置
    }

    // 当对象进入触发区域时
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SnapObject"))
        {
            if (silhouetteObject == null)
            {
                // 实例化轮廓对象
                silhouetteObject = Instantiate(other.gameObject, snapPoint.position, snapPoint.rotation);
                Renderer silhouetteRenderer = silhouetteObject.GetComponent<Renderer>();
                silhouetteRenderer.material = silhouetteMaterial;
                Destroy(silhouetteObject.GetComponent<SnapObject>()); // 移除实际的交互脚本，防止它可被抓取
            }
        }
    }

    // 当对象失去抓取时，吸附到吸附点
    public void SnapObject(GameObject snapObject)
    {
        Debug.Log(snapObject.name + "has been snapped");
        snapObject.transform.position = snapPoint.position;
        snapObject.transform.rotation = snapPoint.rotation;
        snapObject.GetComponent<Rigidbody>().isKinematic = true; // 锁定对象
        audioSource.Play(); // 播放附加音效
    }
}
