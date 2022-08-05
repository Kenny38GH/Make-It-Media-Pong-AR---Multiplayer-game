using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// Controlleur de la reconnaissance de surface plane

public class ARPlacementAndPlaneDetectionController : MonoBehaviour
{
    ARPlaneManager m_ARPlaneManager;
    ARPlacementManager m_ARPlacementManager;
    public GameObject placeButton,adjustButton,scaleSlider;
    // Start is called before the first frame update
    private void Awake()
    {
        m_ARPlaneManager = GetComponent<ARPlaneManager>();
        m_ARPlacementManager = GetComponent<ARPlacementManager>();
    }

    void Start()
    {
        placeButton.SetActive(true);
        adjustButton.SetActive(false);
        scaleSlider.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DisableARPlacementAndPlaneDetection()
    {
        m_ARPlaneManager.enabled = false;
        m_ARPlacementManager.enabled = false;
        SetAllPlanesActiveOrDeactive(false);
        scaleSlider.SetActive(false);
        placeButton.SetActive(false);
        adjustButton.SetActive(true);

    }
    public void EnableARPlacementAndPlaneDetection()
    {
        m_ARPlaneManager.enabled = true;
        m_ARPlacementManager.enabled = true;
        SetAllPlanesActiveOrDeactive(true);
        scaleSlider.SetActive(true);
        placeButton.SetActive(true);
        adjustButton.SetActive(false);
    }

    private void SetAllPlanesActiveOrDeactive(bool value)
    {
        foreach(var plane in m_ARPlaneManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
    }
}
