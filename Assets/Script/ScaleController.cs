using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

// Controlleur pour le scaler du terrain
public class ScaleController : MonoBehaviour
{

    ARSessionOrigin m_ArSessionOrigin;
    public Slider scaleSlider;

    private void Awake()
    {
        m_ArSessionOrigin = GetComponent<ARSessionOrigin>();
    }
    // Start is called before the first frame update
    void Start()
    {
        scaleSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public float getScaleValue()
    {
        return scaleSlider.value;
    }

    public void OnSliderValueChanged(float value)
    {
        if(scaleSlider != null)
        {
            m_ArSessionOrigin.transform.localScale = Vector3.one / value;
        }

    }
}
