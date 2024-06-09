using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    
    [SerializeField] GameObject     winScreen;
    [SerializeField] GameObject     loseScreen;
    [SerializeField] GameObject     waitingScreen;
    [SerializeField] GameObject     endBG;
    
    PlayerTower localPlayer;
    bool win = false;

    public void Quit()
    {
        Application.Quit();
    }

    public void Win()
    {
        endBG.SetActive(true);
        loseScreen.SetActive(false);
        winScreen.SetActive(true);
        
        Time.timeScale = 0;
    }

    public void Lose()
    {
        endBG.SetActive(true);
        winScreen.SetActive(false);
        loseScreen.SetActive(true);

        Time.timeScale = 0;
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            waitingScreen.SetActive(false);
        }

        var players = FindObjectsOfType<PlayerTower>();

        if (localPlayer == null)
        {
            foreach(PlayerTower p in players)
            {
                if (p.IsLocalPlayer)
                {
                    localPlayer = p;
                    break;
                }
            }
        }

        if (localPlayer != null && players.Count() >= 2)
        {
            if(waitingScreen.activeSelf)
                waitingScreen.SetActive(false);


            if (localPlayer.isDead && !win)
                Lose();
            
            else
            {
                win = true;

                foreach(PlayerTower p in players)
                {
                    if (!p.isDead && !p.IsLocalPlayer)
                    {
                        win = false;
                        break;
                    }
                }

                if (win)
                    Win();
            }
        }
    }
}
