using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    [SerializeField] GameObject     winScreen;
    [SerializeField] GameObject     loseScreen;
    [SerializeField] GameObject     waitingScreen;
    [SerializeField] GameObject     endBG;
    [SerializeField] private GameObject buffButton1;
    [SerializeField] private GameObject buffButton2;
    
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

    void Start()
    {
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

        buffButton1.GetComponent<Button>().onClick.AddListener(() => localPlayer.Upgrade("attack"));
        buffButton2.GetComponent<Button>().onClick.AddListener(() =>  localPlayer.Upgrade("speed"));
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
