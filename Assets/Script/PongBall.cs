using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

// Behaviour of the Ball and Scoring

public class PongBall : MonoBehaviour
{
    public float speed = 0.0f;
    private Rigidbody rb;
    private PhotonView view;
    private string playerOne;
    private string playerTwo;

    private void Awake()
    {
        playerOne = GameObject.Find("GameManager").GetComponent<gameScriptV2>().TxtPlayerOne.text;
        playerTwo = GameObject.Find("GameManager").GetComponent<gameScriptV2>().TxtPlayerTwo.text;
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
    }
    public void Start()
    {
        Launch();
        //Text testPos = GameObject.Find("PosTest").GetComponent<Text>();
        //testPos.text = "Speed: " + speed + "\n" + transform.position;
    }

    private void Launch()
    {
        float sx = Random.Range(0,2) == 0 ? -1 : 1;
        float sy = Random.Range(0,2) == 0 ? -1 : 1;
        rb.velocity = new Vector3(sx* speed, sy * speed, 0);
    }

    private void checkPlayers()
    {
        playerOne = GameObject.Find("GameManager").GetComponent<gameScriptV2>().TxtPlayerOne.text;
        playerTwo = GameObject.Find("GameManager").GetComponent<gameScriptV2>().TxtPlayerTwo.text;
    }

    private Player FindPlayer(string player){
        return GameObject.Find("GameManager").GetComponent<gameScriptV2>().PlayerInPhotonPlayerList(player);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            checkPlayers();
            if(collision.gameObject.tag == "Goal1" && view.IsMine)
            {
                // Le Player 2 marque un point
                FindPlayer(playerTwo).SetScore(FindPlayer(playerTwo).GetScore()+1);

                view.RPC("UpdateScoreForAllPlayers",RpcTarget.All);
                PhotonNetwork.Destroy(view);
                GameObject.Find("GameManager").GetComponent<gameScriptV2>().StartLaunchBall();
                view.RPC("UpdateLaunchBallText",RpcTarget.Others);

            }
            if(collision.gameObject.tag == "Goal2" && view.IsMine)
            {
                // Le Player 1 marque un point
                FindPlayer(playerOne).SetScore(FindPlayer(playerOne).GetScore()+1);

                view.RPC("UpdateScoreForAllPlayers",RpcTarget.All);
                PhotonNetwork.Destroy(view);
                GameObject.Find("GameManager").GetComponent<gameScriptV2>().StartLaunchBall();
                view.RPC("UpdateLaunchBallText",RpcTarget.Others);
            }

        }

    } 

    [PunRPC]
    void UpdateScoreForAllPlayers()
    {
        GameObject.Find("GameManager").GetComponent<gameScriptV2>().UpdateScoreOfPlayers();
    }
    [PunRPC]
    void UpdateLaunchBallText()
    {
        GameObject.Find("GameManager").GetComponent<gameScriptV2>().StartLaunchBallText();
    }
}
