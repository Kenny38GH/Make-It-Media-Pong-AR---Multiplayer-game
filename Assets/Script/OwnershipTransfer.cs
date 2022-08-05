using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// I'm not using this script anymore
public class OwnershipTransfer : MonoBehaviourPun
{
    private void OnMouseDown() 
    {
        base.photonView.RequestOwnership(); 
    }
}
