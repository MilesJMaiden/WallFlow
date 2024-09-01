using Meta.XR.MRUtilityKit;
using Photon.Voice.PUN.UtilityScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRUKDemo : MonoBehaviour
{
    [SerializeField] private MRUK m_MRUK;

    [SerializeField] private OVRInput.Controller controller;

    [SerializeField] private GameObject ObjectForWallAnchors;
    //[SerializeField] private GameObject ObjectForKeyWallAnchor;

    //[SerializeField] private GameObject ObjectForGroundAnchors;

    private bool sceneLoaded;
    private MRUKRoom currentRoom;
    private List<GameObject> wallAnchorObjectsCreated = new();

    private bool infoAvailable => currentRoom != null && sceneLoaded;

    private void OnEnable()
    {
        m_MRUK.RoomCreatedEvent.AddListener(BindRoomInfo);
    }

    private void OnDisable()
    {
        m_MRUK.RoomCreatedEvent.RemoveListener(BindRoomInfo);
    }

    public void EnableMRUKDemo()
    {
        sceneLoaded = true;
        Debug.Log("EnableMRUKDemo Called");
    }

    private void BindRoomInfo(MRUKRoom room)
    {
        currentRoom = room;
    }

    // Update is called once per frame
    void Update()
    {
        spawneyes();
    }

    public void spawneyes()
    {
        if (wallAnchorObjectsCreated.Count == 0)
        {
            foreach (var wallAnchor in currentRoom.WallAnchors)
            {
                var createdWallObject = Instantiate(ObjectForWallAnchors, Vector3.zero, Quaternion.identity, wallAnchor.transform);
                createdWallObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                wallAnchorObjectsCreated.Add(createdWallObject);
                Debug.Log("Wall object created");
            }
            Debug.Log("Wall Objects added to walls");
        }
        else
        {
            foreach (var wallObject in wallAnchorObjectsCreated)
            {
                Destroy(wallObject);
            }
            wallAnchorObjectsCreated.Clear();
            Debug.Log("Wall objects deleated");
        }
    }
}
