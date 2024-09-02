using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBall : MonoBehaviour
{
    public GameObject ball;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            GameObject spawnedBall = Instantiate(ball, transform.position, Quaternion.identity);
            Rigidbody spawnedBallRB = spawnedBall.GetComponent<Rigidbody>();
            spawnedBallRB.velocity = transform.forward * speed;
        }
    }
}
