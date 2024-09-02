using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RadialSelection : MonoBehaviour
{
    public OVRInput.Button spawnRadialMenuButton;

    // Start is called before the first frame update
    [Range(2,6)]
    public int segmentTotal;
    public GameObject radialSegment;
    public Transform radialSegmentCanvas;
    public float angleBetweenSegments = 5f;
    public Transform handTransform;

    public UnityEvent<int> onSegmentSelected;

    private List<GameObject> spawnedSegments = new List<GameObject>();
    private int currentSelectedRadialSegment = -1;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(spawnRadialMenuButton))
        {
            SpawnRadialMenu();
        }

        if(OVRInput.Get(spawnRadialMenuButton))
        {
            GetSelectedRadialSegment();
        }

        if(OVRInput.GetUp(spawnRadialMenuButton))
        {
            HideAndTriggerSelected();
        }
    }

    public void HideAndTriggerSelected()
    {
        onSegmentSelected.Invoke(currentSelectedRadialSegment);
        radialSegmentCanvas.gameObject.SetActive(false);
    }

    public void GetSelectedRadialSegment()
    {
        Vector3 centerToHand = handTransform.position - radialSegmentCanvas.position;
        Vector3 centerToHandProjected = Vector3.ProjectOnPlane(centerToHand, radialSegmentCanvas.forward);

        float angle = Vector3.SignedAngle(radialSegmentCanvas.up, centerToHandProjected, -radialSegmentCanvas.forward);

        if (angle < 0)
        {
            angle += 360;
        }
        
        currentSelectedRadialSegment = (int)angle * segmentTotal / 360;

        for (int i = 0; i < spawnedSegments.Count; i++)
        {
            if(i == currentSelectedRadialSegment)
            {
                spawnedSegments[i].GetComponent<Image>().color = Color.yellow;
                spawnedSegments[i].transform.localScale = 1.1f * Vector3.one;
            } 
            else
            {
                spawnedSegments[i].GetComponent<Image>().color = Color.white;
                spawnedSegments[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void SpawnRadialMenu()
    {
        radialSegmentCanvas.gameObject.SetActive(true);
        radialSegmentCanvas.position = handTransform.position;
        radialSegmentCanvas.rotation = handTransform.rotation;

        foreach (var item in spawnedSegments)
        {
            Destroy(item);
        }

        spawnedSegments.Clear();

        for (int i = 0; i < segmentTotal; i++) 
        {
            float angle = - i * 360 / segmentTotal - angleBetweenSegments / 2;
            Vector3 radialSegmentEulerAngle = new Vector3(0, 0, angle);

            GameObject spawnedRadialSegment = Instantiate(radialSegment, radialSegmentCanvas);
            spawnedRadialSegment.transform.position = radialSegmentCanvas.position;
            spawnedRadialSegment.transform.localEulerAngles = radialSegmentEulerAngle;

            spawnedRadialSegment.GetComponent<Image>().fillAmount = (1 / (float)segmentTotal) - (angleBetweenSegments/360);

            spawnedSegments.Add(spawnedRadialSegment);

        }
    }

}
