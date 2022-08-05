using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

// Création de Room sur le serveur Photon / Pouvoir les rejoindres et les afficher sur le menu

public class PUN : MonoBehaviourPunCallbacks,IMatchmakingCallbacks
{
    public GameObject canvas,button;
    public Text ListRoomText, PunInfoTxt;
    public InputField IfPseudo, IfRoom;
    List<Text> RoomListText;
    List<GameObject> ButtunList;
    
    #region UNITY METHODS
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        PunInfoTxt.text = PhotonNetwork.NetworkClientState.ToString();
    }

    void CreateAccessToRoom(Vector3 posText, Vector3 posButton, RoomInfo room)
    {
        Vector3 posT = new Vector3(82.25f,-96.75f,0f);
        Text tempTextBox = Instantiate(ListRoomText, posText, Quaternion.identity) as Text;
        tempTextBox.transform.SetParent(canvas.transform, false);
        int nbPlayer = room.PlayerCount;
        int roomMaxPlayer = room.MaxPlayers;
        if(nbPlayer != 0)
        {
            tempTextBox.text = room.Name + " [" + nbPlayer + "/" + roomMaxPlayer + "] \n";
            RoomListText.Add(tempTextBox);
            GameObject newButton = Instantiate(button,posButton,Quaternion.identity) as GameObject;
            newButton.transform.SetParent(canvas.transform, false);
            ButtunList.Add(newButton);
            newButton.transform.localScale = new Vector3(2,2,2);
            Text buttonText = Instantiate(ListRoomText,new Vector3(213,-222,0),Quaternion.identity) as Text;
            buttonText.transform.SetParent(newButton.transform, false);
            buttonText.text = "+";
            buttonText.transform.localScale = new Vector3(0.7f,0.7f,0.7f);
            newButton.GetComponent<Button>().onClick.AddListener(delegate { RejoindreRoom(room.Name);});
        }
    }

    void DeleteObject()
    {
        if(RoomListText != null)
        {
            foreach(Text el in RoomListText)
            {
                Destroy(el);
            }
            foreach(GameObject el in ButtunList)
            {
                Destroy(el);
            }  
        }
    }

    #endregion
    #region BUTTON METHODS
    public void OnPlayButtonClicked()
    {
        JoindreRoom();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region PHOTON METHODS

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        RoomListText = new List<Text>();   
        ButtunList = new List<GameObject>(); 
        IfPseudo.text = "Player" + Random.Range(1,400);
        IfRoom.text = "Room1";
    }
    public override void OnJoinedRoom(){
        PhotonNetwork.LoadLevel("GameScene");
    }

    public void JoindreRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = IfPseudo.text;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        EnterRoomParams enterRoomParams = new EnterRoomParams();
        PhotonNetwork.JoinOrCreateRoom(IfRoom.text,roomOptions,TypedLobby.Default);
    }
    public void RejoindreRoom(string roomName)
    {
        PhotonNetwork.LocalPlayer.NickName = IfPseudo.text;
        PhotonNetwork.JoinRoom(roomName);
    }


    public override void OnRoomListUpdate(List< RoomInfo > roomList)
    {
        DeleteObject();
        Vector3 pos = new Vector3(500f,0.0f,0);
        Vector3 posB = new Vector3(800f,305.0f,0);
        Vector3 vecAdd = new Vector3(0.0f,30.0f,0f);
        foreach(RoomInfo room in roomList){
            CreateAccessToRoom(pos,posB,room);
            pos += vecAdd;
            posB += vecAdd;
        }
    }

    #endregion

}
