using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

// Old Script not using this one anymore

public class gameScript : MonoBehaviourPunCallbacks
{
    public Text TxtRoom,TxtPlayerOne,TxtPlayerTwo,TxtScore;
    public GameObject PlayerPrefab,BallPrefab;
    private int dotNumber = 1;
    private float nextStatusChange;
    private bool isRight = false;
    public List<string> ListOfPlayers;
    private bool isObs = false;
    public PhotonView view;
    public Text ListTxt;
    public Text PhotonList;
    public GameObject PosObj;
    public GameObject PosObj2;
    public GameObject PosBall;
    public GameObject GameEnvironementGameObject;
    private GameObject playerGameObject;
    private GameObject BallGameObject;
    public enum RaiseEventCodes
    {
        PlayerSpawnEventCode = 0
    }

    void Start()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        playerGameObject = new GameObject();
        view = GetComponent<PhotonView>();
        ListOfPlayers = new List<string>();
        if(PhotonNetwork.CurrentRoom.PlayerCount> 2) // Quand un observateur rejoint la partie
        {
            isObs = true;
            isRight = true;
            TxtRoom.text = "Vous êtes observateur";
        }
    }

    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code == (byte)RaiseEventCodes.PlayerSpawnEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 receivedPosition = (Vector3)data[0];
            Quaternion receivedRotation = (Quaternion)data[1];
            GameObject player = Instantiate(PlayerPrefab,receivedPosition + GameEnvironementGameObject.transform.position,receivedRotation);
            PhotonView photonView = player.GetComponent<PhotonView>();
            photonView.ViewID = (int)data[2];
        }
    }


    public void PlaceNewPlayer()
    
    {
        if(PhotonNetwork.IsMasterClient == false)
        {
             view.RPC("AskListOfPlayer",RpcTarget.MasterClient);  
        }
        else
        {
            Debug.Log("MASTER CLIENT");
            ListOfPlayers.Add(PhotonNetwork.PlayerList[0].NickName);
            Vector3 pos_player = PosObj.transform.position;
            SpawnPlayerEverywhere(pos_player);
            //PhotonNetwork.Instantiate(PlayerPrefab.name,pos_player, PosObj.transform.rotation,0);
        }
        //TxtRoom.text = PhotonNetwork.player.NickName + ", Bienvenu dans " + PhotonNetwork.room.Name;
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient == false)  // Quand deux joueurs ont rejoint la partie
            {
            // view.RPC("AskListOfPlayer",PhotonTargets.MasterClient);
                Vector3 pos_player = PosObj2.transform.position;
                isRight = true;
                SpawnPlayerEverywhere(pos_player);
                //PhotonNetwork.Instantiate(PlayerPrefab.name,pos_player, PosObj2.transform.rotation,0);
                StartCoroutine(LaunchBallOnlyText());
            }
    }

    public void AdjustNewPlayer()
    {
            PhotonNetwork.LocalPlayer.SetScore(0);
            DespawnPlayer();
            ListOfPlayers.RemoveAt(ListOfPlayers.Count-1);
    }

    public void RetourLobby(){
        PhotonNetwork.Disconnect();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom(){
        PhotonNetwork.LoadLevel("Menu");
    }

    public void UpdateListOfPlayers(){
        TxtPlayerOne.text = null;
        TxtPlayerTwo.text = null;
        if(ListOfPlayers.Count == 1 ){
            TxtPlayerOne.text = PhotonNetwork.PlayerList[0].NickName;
            TxtScore.text = "00 - 00";
        }
        if(ListOfPlayers.Count == 2 )
        {
            if(isRight)
            {
                TxtPlayerOne.text = ListOfPlayers[0];
                TxtScore.text = PlayerInPhotonPlayerList(ListOfPlayers[0]).GetScore() + " - " + PlayerInPhotonPlayerList(ListOfPlayers[1]).GetScore();
                TxtPlayerTwo.text = ListOfPlayers[1];
            }
            else
            {
                TxtPlayerOne.text = ListOfPlayers[0];
                TxtScore.text = PlayerInPhotonPlayerList(ListOfPlayers[0]).GetScore() + " - " + PlayerInPhotonPlayerList(ListOfPlayers[1]).GetScore();
                TxtPlayerTwo.text = ListOfPlayers[1];
            }
        }
    }

    void OnPhotonPlayerConnected(){
        if(PhotonNetwork.IsMasterClient && ListOfPlayers.Count < 2)
        {
            if(ListOfPlayers[0] != PhotonNetwork.PlayerList[0].NickName)
            {
                ListOfPlayers.Add(PhotonNetwork.PlayerList[0].NickName);
            }
            else
            {
                ListOfPlayers.Add(PhotonNetwork.PlayerList[1].NickName);
            }
        }
        
        // if (BallPrefab.GetComponent<PhotonView>() != null)
        // {
        //     PhotonNetwork.Destroy(BallPrefab.GetComponent<PhotonView>());
        // }
        if(ListOfPlayers.Count == 2 &&  PhotonNetwork.CurrentRoom.PlayerCount  == 2){

            foreach(string player in ListOfPlayers)
            {
                foreach(Player playerPhoton in  PhotonNetwork.PlayerList)
                {
                    if(player == playerPhoton.NickName)
                    {
                        playerPhoton.SetScore(0);
                    }
                }
            }
        }

        //UpdateListOfPlayers();
    }

    void OnPhotonPlayerDisconnected(){
        if(isObs)
        {
            PhotonNetwork.LeaveRoom();
        }
        
        if(PhotonNetwork.IsMasterClient)
        {
            List<string> NewList = new List<string>();
            for(int i =0; i < ListOfPlayers.Count; i++)
            {
                for(int j =0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
                {
                    if(ListOfPlayers[i] == PhotonNetwork.PlayerList[j].NickName)
                    {
                        NewList.Add(ListOfPlayers[i]);
                    }
                }
            }
            ListOfPlayers = NewList;
            view.RPC("clearList",RpcTarget.Others);
                foreach (string player in ListOfPlayers)
                {
                    view.RPC("SendListOfPlayer",RpcTarget.Others, player);
                }
           
        }
            
        if(ListOfPlayers.Count == 1)
        {
            PhotonNetwork.LocalPlayer.SetScore(0);
            DespawnPlayer();
            Vector3 pos_player = PosObj.transform.position;
            isRight = false;
            SpawnPlayerEverywhere(pos_player);
        }

        if(ListOfPlayers.Count == 2)
        {
            StartCoroutine(LaunchBall());
        }
        
        //UpdateListOfPlayers();
    }

    void Update()
    {
        UpdateListOfPlayers();

        if(ListOfPlayers.Count == 1)
        {
            if (Time.time > nextStatusChange)
            {
                nextStatusChange = Time.time + 0.5f;
                TxtRoom.text = "En Attente d'un 2nd joueur";
                for (int i = 0; i < dotNumber; i++) {
                    TxtRoom.text += ".";
                }
                if (++dotNumber > 3) {
                    dotNumber = 0;
                }
            }    
        }

        ListTxt.text = null;
        //ListTxt.text = ListOfPlayers.Count.ToString() + "\n";

        foreach(string player in ListOfPlayers)
        {
            ListTxt.text += player + "\n";
        }
        
        PhotonList.text = null;
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            PhotonList.text += player + "\n";
        }


    }

    IEnumerator LaunchBall()
    {
        int nb = 3;
            for (int i = 0; i < 3; i++) 
            {
                TxtRoom.text = nb.ToString();
                nb--;
                yield return new WaitForSeconds(1);
            }
        if(isObs)
        {
            TxtRoom.text = "Vous êtes observateur";
        }
        else
        {
            TxtRoom.text = "JOUEZ !";
        }
        if(ListOfPlayers.Count == 2)
        {
            PhotonNetwork.Instantiate(BallPrefab.name,PosBall.transform.position, PosBall.transform.rotation,0);
        }
        
    }

    IEnumerator LaunchBallOnlyText()
    {
        int nb = 3;
            for (int i = 0; i < 3; i++) 
            {
                TxtRoom.text = nb.ToString();
                nb--;
                yield return new WaitForSeconds(1);
            }
        if(isObs)
        {
            TxtRoom.text = "Vous êtes observateur";
        }
        else
        {
            TxtRoom.text = "JOUEZ !";
        }
    }

    public void StartLaunchBall()
    {
        StartCoroutine(LaunchBall());
    }

    public void StartLaunchBallText()
    {
        StartCoroutine(LaunchBallOnlyText());
    }

    public Player PlayerInPhotonPlayerList(string playerNickname)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if(player.NickName == playerNickname)
            { 
                return player;
            }
        }
        return PhotonNetwork.PlayerList[0];
    }

    
    private void SpawnPlayerEverywhere(Vector3 pos_player)
    {   
        playerGameObject = Instantiate(PlayerPrefab,pos_player,PosObj.transform.rotation);
        PhotonView photonView = this.GetComponent<PhotonView>();
        object[] data = new object[]
        {
            playerGameObject.transform.position - GameEnvironementGameObject.transform.position,playerGameObject.transform.rotation, photonView.ViewID
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };
        PhotonNetwork.RaiseEvent((byte)RaiseEventCodes.PlayerSpawnEventCode,data,raiseEventOptions,sendOptions);
        // if(PhotonNetwork.AllocateViewID(photonView))
        // {
            
        // }
        // else
        // {
        //     Debug.Log("Failed to allocate a viewID");
        //     Destroy(playerGameObject);
        // }
        //photonView.RPC("SpawnOnNetwork", RpcTarget.OthersBuffered, playerGameObject.transform.position - GameEnvironementGameObject.transform.position , playerGameObject.transform.rotation, id1);
    }

    private void DespawnPlayer()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        Destroy(this.playerGameObject);
        GameObject playerGameObject = new GameObject();
        PhotonView photonView = this.GetComponent<PhotonView>();
        photonView.RPC("DespawnOnNetwork", RpcTarget.OthersBuffered);
    }

    // private void SpawnBallEverywhere()
    // {
    //     int id2 = PhotonNetwork.AllocateViewID();
    //     BallGameObject = Instantiate(BallPrefab,PosBall.transform.position,PosBall.transform.rotation);
    //     PhotonView photonView = this.GetComponent<PhotonView>();
    //     photonView.RPC("SpawnBallNetwork", RpcTarget.OthersBuffered, BallGameObject.transform.position - GameEnvironementGameObject.transform.position , BallGameObject.transform.rotation, id2);
    // }
   

    [PunRPC]
    void DespawnBallOnNetwork()
    {
        Destroy(this.BallGameObject);
    }

    [PunRPC]
    void DespawnOnNetwork()
    {
        Destroy(this.playerGameObject);
    }



    [PunRPC]
    void SendListOfPlayer(string playerMaster)
    {
        ListOfPlayers.Add(playerMaster);
    }
    [PunRPC]
    void AskListOfPlayer()
    {
        view.RPC("clearList",RpcTarget.Others);
        foreach(string player in ListOfPlayers)
        {
            view.RPC("SendListOfPlayer",RpcTarget.Others, player);
        }
        
    }
    [PunRPC]
    void clearList()
    {
        ListOfPlayers = new List<string>();
    }

}
