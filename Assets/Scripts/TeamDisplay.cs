using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TeamDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    teamText;

    PlayerTower localPlayer;

    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            teamText.transform.parent.gameObject.SetActive(false);
        }

        if (localPlayer == null)
        {
            var players = FindObjectsOfType<PlayerTower>();
            
            foreach(PlayerTower p in players)
            {
                if (p.IsLocalPlayer)
                {
                    localPlayer = p;
                    break;
                }
            }
        }

        if (localPlayer != null)
        {
            if (localPlayer.faction == Faction.Red)
            {
                teamText.text = "Red Team";
                teamText.color = Color.red;
            }

            else if (localPlayer.faction == Faction.Blue)
            {
                teamText.text = "Blue Team";
                teamText.color = Color.blue;
            }
        }
    }
}
