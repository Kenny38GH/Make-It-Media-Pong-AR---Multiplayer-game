using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class SpawnManager : MonoBehaviourPunCallbacks
{

    public GameObject playerPrefabs;
    public GameObject PosObj,PosObj2,PosBall; // Positions des spawns

    public GameObject GameEnvironnementGameobject;

    public enum RaiseEventCodes
    {
        PlayerSpawnEventCode = 0
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

   
    #region Photon Callback Methods
    void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventCodes.PlayerSpawnEventCode)
        {

            object[] data = (object[])photonEvent.CustomData;
            Vector3 receivedPosition = (Vector3)data[0];
            Quaternion receivedRotation = (Quaternion)data[1];

            GameObject player = Instantiate(playerPrefabs, receivedPosition + GameEnvironnementGameobject.transform.position, receivedRotation);
            PhotonView _photonView = player.GetComponent<PhotonView>();
            _photonView.ViewID = (int)data[2];


        }
    }



    public override void OnJoinedRoom()
    {
            Debug.Log("SPAWN");
            SpawnPlayer();
    }
    #endregion


    #region Private Methods
    private void SpawnPlayer()
    {
        Vector3 pos_player = PosObj.transform.position;
        if(PhotonNetwork.CurrentRoom.Players.Count == 2)
        {
             pos_player = PosObj2.transform.position;
        }

            Vector3 instantiatePosition = pos_player;

            GameObject playerGameobject = Instantiate(playerPrefabs,instantiatePosition, Quaternion.identity);

            PhotonView _photonView = playerGameobject.GetComponent<PhotonView>();

            if (PhotonNetwork.AllocateViewID(_photonView))
            {
                object[] data = new object[]
                {

                    playerGameobject.transform.position- GameEnvironnementGameobject.transform.position, playerGameobject.transform.rotation, _photonView.ViewID
                };


                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                   Receivers = ReceiverGroup.Others,
                   CachingOption =EventCaching.AddToRoomCache

                };


                SendOptions sendOptions = new SendOptions
                {
                    Reliability = true
                };

                //Raise Events!
                PhotonNetwork.RaiseEvent((byte)RaiseEventCodes.PlayerSpawnEventCode,data, raiseEventOptions,sendOptions);
            }
            else
            {
                Debug.Log("Failed to allocate a viewID");
                Destroy(playerGameobject);
            }
    }
    #endregion
}