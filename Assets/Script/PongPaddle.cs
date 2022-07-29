using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PongPaddle : MonoBehaviourPun
{
    public float speed = 10f;
    public PhotonView view;
    private Touch touch;
    private float minY;
    private float maxY;
    
    

    void Start()
    {
        float scaleX = GameObject.Find("Goal2").transform.localScale.x*0.08f;
        float scaleY = Math.Abs(GameObject.Find("Top").transform.position.y - GameObject.Find("Bottom").transform.position.y)*1.6f;

        transform.localScale = new Vector3(scaleX * transform.localScale.x , scaleY * transform.localScale.y, GameObject.Find("Top").transform.localScale.z * transform.localScale.z);
        minY = GameObject.Find("PosMin").transform.position.y;
        maxY = GameObject.Find("PosMax").transform.position.y;

        // GameObject.Find("PosTest").GetComponent<Text>().text = GameObject.Find("GameManager").GetComponent<PhotonView>().ToString();
        // GameObject.Find("PosTest").GetComponent<Text>().text += "\n";
        // GameObject.Find("PosTest").GetComponent<Text>().text += view.ToString();

        // Debug.Log(view + " " + GameObject.Find("GameManager").GetComponent<PhotonView>());

    }

    void Update()
    {
        if(view.IsMine)
       {
            float moveY = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            float newY = transform.position.y + moveY;
            if (newY > maxY) newY = maxY;
            else if (newY < minY) newY = minY;

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                if(touch.phase == TouchPhase.Moved)
                {
                    newY = transform.position.y + touch.deltaPosition.y  * Time.deltaTime /50f;
                    if (newY > maxY) newY = maxY;
                    else if (newY < minY) newY = minY;

                    transform.position = new Vector3(transform.position.x,newY,transform.position.z);
                }
            }
        }
    }
}
