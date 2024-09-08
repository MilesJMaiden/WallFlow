using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachObject : MonoBehaviour
{
    [SerializeField]
    private Collider deteCol;

    public Transform targetParent;

    public AudioSource audioSource;

    //public AudioClip acIn;
    //public AudioClip acOut;

    private void Start()
    {
        deteCol = GetComponent<BoxCollider>();
        Debug.Log("You fucking idiot!!");
    }


    private void OnTriggerEnter(Collider other)
    {

        if ( deteCol != null)
            Debug.Log("detectioncollider is on me");
        {
            // Check if the object entering the detection zone has a Rigidbody (optional check)
            if (other != null)
            {
                Debug.Log("some idiot entered the collider!!");

                Rigidbody[] rigidbodies = other.transform.root.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rigidbodies)
                {
                    // Disable physics by setting the Rigidbody to kinematic
                    rb.isKinematic = true;
                    Debug.Log("Disabled Rigidbody for object: " + rb.gameObject.name);
                }

                other.transform.root.SetParent(targetParent);

                //audioSource.PlayOneShot(acIn);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (deteCol != null)
        {
            // Check if the object exiting has a Rigidbody (optional check)
            if (other != null)
            {
                // Unparent the object (make it a root object in the hierarchy)
                other.transform.root.SetParent(null);

                Rigidbody[] rigidbodies = other.transform.root.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rigidbodies)
                {
                    // Disable physics by setting the Rigidbody to kinematic
                    rb.isKinematic = false;
                    Debug.Log("Enabled Rigidbody for object: " + rb.gameObject.name);
                }
                //audioSource.PlayOneShot(acOut);
            }
        }
    }
}
