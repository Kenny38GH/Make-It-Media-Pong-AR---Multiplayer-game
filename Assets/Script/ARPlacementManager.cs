using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class ARPlacementManager : MonoBehaviour
{
    ARRaycastManager m_ARRaycastManager;
    static List<ARRaycastHit> raycast_Hits = new List<ARRaycastHit>();
    public Camera aRCamera;
    public GameObject GameEnvironement;
    // Start is called before the first frame update

    private void Awake()
    {
        m_ARRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 centerOfScreen = new Vector2(Screen.width/2, Screen.height /2);
        Ray ray = aRCamera.ScreenPointToRay(centerOfScreen);

        if(m_ARRaycastManager.Raycast(ray, raycast_Hits,TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycast_Hits[0].pose;
            Vector3 positionToBePlaced = hitPose.position;
            GameEnvironement.transform.position = positionToBePlaced;
            GameEnvironement.transform.rotation = new Quaternion(GameEnvironement.transform.rotation.x,aRCamera.transform.rotation.y,GameEnvironement.transform.rotation.z,aRCamera.transform.rotation.w);
        }
    }
}
