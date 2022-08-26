using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;


namespace KoboldKare {
    [CreateAssetMenu(fileName = "NewGameEventPhotonView", menuName = "Data/GameEvent: PhotonView", order = 2)]
    public class GameEventPhotonView : GameEvent<PhotonView> {}
}