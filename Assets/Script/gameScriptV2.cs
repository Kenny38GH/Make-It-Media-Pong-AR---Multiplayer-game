using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

// Script principale de l'application lors d'une partie: Instantiations des joueurs et déroulement de la partie --> Score, Déconnexion, Réajustement du terrain...
public class gameScriptV2 : MonoBehaviourPunCallbacks
{
    public List<string> ListOfPlayers; // Liste des joueurs prêts à jouer
    public Text TxtRoom,TxtPlayerOne,TxtPlayerTwo,TxtScore,ListTxt,PhotonList; // UI Text
    public GameObject PlayerPrefab,BallPrefab; // Prefabs du joueur et de la balle
    public GameObject PosObj,PosObj2,PosBall; // Positions des spawns
    public GameObject GameEnvironementGameObject; // "Plateau du jeu"
    public PhotonView view; // PhotonView
    private bool isRight = false; // Savoir si le joueur est placé à droite ou à gauche
    private GameObject J1GameObject; // GameObject du joueur "local"
    private GameObject J2GameObject; // GameObject de l'autre joueur
    private GameObject BallGameObject; // GameObject de la balle "locale"

    private bool isReady = false;

    // Valeurs utiles pour l'animation d'attente de joueur -->
    private int dotNumber = 1;
    private float nextStatusChange;

    void Awake() 
    {
        J1GameObject = new GameObject();
        J2GameObject = new GameObject();
        BallGameObject = new GameObject();
        view = GetComponent<PhotonView>();
        ListOfPlayers = new List<string>();
    }
    void Start()
    {
        if(!PhotonNetwork.IsMasterClient) // Si je ne suis pas le masterClient je demande a récupérer la Liste des joueurs prêts
        {
             view.RPC("AskListOfPlayer",RpcTarget.MasterClient);  
        }
    }
    void Update()
    {
        //CheckPlayerList();
        UpdateScoreOfPlayers();
        if(ListOfPlayers.Count == 1) // Le joueur est seul donc on attend le prochain
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

        // AFFICHAGE POUR LE DEBUG || A SUPPRIMER
        ListTxt.text = null;
        foreach(string player in ListOfPlayers)
        {
            ListTxt.text += player + "\n";
        }
        
        PhotonList.text = null;
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            PhotonList.text += player + "\n";
        }
        if(J1GameObject != null)
        {
            J1GameObject.transform.rotation = PosObj.transform.rotation;
        }
        if(J2GameObject != null)
        {
            J2GameObject.transform.rotation = PosObj.transform.rotation;
        }
    }
    public void RetourLobby(){
        view.RPC("Disconnection",RpcTarget.Others);
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom(){
        PhotonNetwork.LoadLevel("Menu");
    }


    public void PlaceMyPlayer() // Methode pour placer notre joueur
    {
        if(ListOfPlayers.Count == 0 && !isReady) // Si je suis le premier joueur a me placer
        {
            SpawnPlayerEverywhere(0);
            ListOfPlayers.Add(PhotonNetwork.LocalPlayer.NickName); // On ajoute le player a la liste des joueurs pret et on l'envoie aux autres
            foreach (string player in ListOfPlayers)
            {
                view.RPC("SendListOfPlayer",RpcTarget.OthersBuffered, player);
            }
            isReady = true;
        }
        if(ListOfPlayers.Count == 1 && !isReady) // Si je suis le deuxième joueur a me placer
        {
            J2GameObject.transform.position = PosObj.transform.position;
            isRight = true;
            ListOfPlayers.Add(PhotonNetwork.LocalPlayer.NickName);
            view.RPC("clearList",RpcTarget.OthersBuffered);
            foreach (string player in ListOfPlayers)
            {
                view.RPC("SendListOfPlayer",RpcTarget.OthersBuffered, player);
            }
            SpawnPlayerEverywhere(1);

            view.RPC("LancementDeLaBalle",RpcTarget.All); // On lance la partie
            isReady = true; 
        }
    }

    public void AdjustMyPlayer() // Si le joueur décide de réajuster son terrain
        {
            DespawnPlayer();
            isReady = false;
            foreach(string player in ListOfPlayers) // On supprime le joueur de la Liste des joueurs prêts
            {
                if(player == PhotonNetwork.LocalPlayer.NickName)
                {
                    ListOfPlayers.Remove(player);
                    break;
                }
            }
            view.RPC("clearList",RpcTarget.Others); // On synchronise la liste avec les autres joueurs
            foreach (string player in ListOfPlayers)
            {
                view.RPC("SendListOfPlayer",RpcTarget.Others, player);
            }
            view.RPC("RespawnAfterAdjustmentOfOtherPlayer",RpcTarget.Others);
            PhotonNetwork.LocalPlayer.SetScore(0);
            
        }

        
    void OnPhotonPlayerDisconnected(){ // Si un joueur se déconnecte
        Debug.Log("Player has disconnected - OnPhotonVersion");
        CheckPlayerList();
        isRight = false;
        PhotonNetwork.LocalPlayer.SetScore(0);
        DespawnPlayer();

        SpawnPlayerEverywhere(0);
        UpdateScoreOfPlayers();
    }
  


    public void UpdateScoreOfPlayers(){ // AFFICHAGE DU SCORE ET DES PSEUDOS AU BON ENDROIT
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

    public void CheckPlayerList()
    {
        if(PhotonNetwork.CurrentRoom.Players.Count != ListOfPlayers.Count )
        {
            Debug.Log("CheckList effectif");
            ListOfPlayers = new List<string>();
            ListOfPlayers.Add(PhotonNetwork.PlayerList[0].NickName);
        } 
    }

    public Player PlayerInPhotonPlayerList(string playerNickname)  // Retrouver un joueur grace a son Nickname dans la PhotonPlayerList
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

#region BALL METHODS
    IEnumerator LaunchBall()
    {
        int nb = 3;
            for (int i = 0; i < 3; i++) 
            {
                TxtRoom.text = nb.ToString();
                nb--;
                yield return new WaitForSeconds(1);
            }
        TxtRoom.text = "JOUEZ !";
        if(ListOfPlayers.Count == 2)
        {
             BallGameObject = Instantiate(BallPrefab,PosBall.transform.position,PosObj.transform.rotation);
            //PhotonNetwork.Instantiate(BallPrefab.name,PosBall.transform.position, PosBall.transform.rotation,0); // NEED TO CHANGE THIS INSTANTIATION
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
            TxtRoom.text = "JOUEZ !";
    }

    public void StartLaunchBall()
    {
        StartCoroutine(LaunchBall());
    }

    public void StartLaunchBallText()
    {
        StartCoroutine(LaunchBallOnlyText());
    }

#endregion

#region Spawn et Despawn

    private void SpawnPlayerEverywhere(int pos)
    {
        Vector3 pos_player = new Vector3(0,0,0);
        if(pos == 0)
        {
            pos_player = PosObj.transform.position;
        }
        if(pos == 1)
        {
            pos_player = PosObj2.transform.position;
        }
        J1GameObject = Instantiate(PlayerPrefab,pos_player,PosObj.transform.rotation);
        Debug.Log("Spawn local");
        PhotonView J1playerPhotonView  = J1GameObject.transform.root.gameObject.GetPhotonView();
        
        if (PhotonNetwork.AllocateViewID(J1playerPhotonView))
        {
            view.RPC("SpawnPlayerOnNetwork", RpcTarget.OthersBuffered, pos , J1playerPhotonView.ViewID);
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");
            Destroy(J1GameObject);
        }
        
    }
    private void DespawnPlayer()
    {
        view.RPC("DespawnPlayerOnNetwork", RpcTarget.OthersBuffered);
        Destroy(J1GameObject);
        Destroy(J2GameObject);
    }

#endregion

#region METHODES RPC
    [PunRPC]
    void Respawn()
    {
        if(isReady)
        {
            PhotonNetwork.LocalPlayer.SetScore(0);
            DespawnPlayer();
            isRight = false;
            SpawnPlayerEverywhere(0);
            Debug.Log("Respawn Effectif");
        }
        
    }
    [PunRPC]
    void SpawnPlayerOnNetwork(int pos, int id)
    {
        Vector3 pos_player = new Vector3(0,0,0);
        if(pos == 0)
        {
            pos_player = PosObj.transform.position;
        }
        if(pos == 1)
        {
            pos_player = PosObj2.transform.position;
        }
        J2GameObject = Instantiate(PlayerPrefab,pos_player,PosObj.transform.rotation);
        PhotonView viewJ2 = J2GameObject.transform.root.gameObject.GetPhotonView();
        viewJ2.ViewID = id;
        Debug.Log("SpawnPlayerOnNetwork");
        
    }
    [PunRPC]
    void DespawnPlayerOnNetwork()
    {
        Destroy(J1GameObject);
        Destroy(J2GameObject);
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
    [PunRPC]
    void Disconnection()
    {
        Debug.Log("Player has disconnected");
        CheckPlayerList();
        isRight = false;
        PhotonNetwork.LocalPlayer.SetScore(0);
        DespawnPlayer();
        SpawnPlayerEverywhere(0);
        ListOfPlayers = new List<string>();
        ListOfPlayers.Add(PhotonNetwork.LocalPlayer.NickName);
        UpdateScoreOfPlayers();
    }
    [PunRPC]
    void RespawnAfterAdjustmentOfOtherPlayer()
    {
        DespawnPlayer();
        Debug.Log("Player has to respawn");
        CheckPlayerList();
        isRight = false;
        PhotonNetwork.LocalPlayer.SetScore(0);
        SpawnPlayerEverywhere(0);
        UpdateScoreOfPlayers();
    }
    [PunRPC]
    void LancementDeLaBalle()
    {
        //view.RPC("CountdownBeforeBall",RpcTarget.All);
        StartLaunchBall();
    }
    [PunRPC]
    void CountdownBeforeBall()
    {
        StartLaunchBallText();
    }
#endregion
}
