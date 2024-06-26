using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Spawner spawner;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject waitingScreen;
    [SerializeField] private GameObject endBG;
    [SerializeField] private GameObject abilityButton0;
    [SerializeField] private GameObject abilityButton1;
    [SerializeField] private GameObject abilityButton2;
    
    private PlayerTower localPlayer;
    private bool win = false;

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

        abilityButton0.GetComponent<Button>().onClick.AddListener(() => spawner.IncreasePlayerSpawnCount(localPlayer));
        abilityButton1.GetComponent<Button>().onClick.AddListener(() => localPlayer.Upgrade("attack"));
        abilityButton2.GetComponent<Button>().onClick.AddListener(() =>  localPlayer.Upgrade("speed"));
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
