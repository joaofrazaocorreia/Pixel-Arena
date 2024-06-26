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
    [SerializeField] private Button[] abilityButtons;
    
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

        abilityButtons[0].onClick.AddListener(() => spawner.IncreasePlayerWeakSpawnCount(localPlayer));
        abilityButtons[1].onClick.AddListener(() => spawner.IncreasePlayerRegularSpawnCount(localPlayer));
        abilityButtons[2].onClick.AddListener(() => spawner.IncreasePlayerTankSpawnCount(localPlayer));
        abilityButtons[3].onClick.AddListener(() => localPlayer.Upgrade("attack"));
        abilityButtons[4].onClick.AddListener(() =>  localPlayer.Upgrade("speed"));
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

        UpdateAbilityIcon(abilityButtons[0], 1);
        UpdateAbilityIcon(abilityButtons[1], 3);
        UpdateAbilityIcon(abilityButtons[2], 5);
        UpdateAbilityIcon(abilityButtons[3], 4);
        UpdateAbilityIcon(abilityButtons[4], 4);

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

        else if (localPlayer != null && players.Count() < 2)
        {
            if(!waitingScreen.activeSelf)
                Win();
        }
    }

    private void UpdateAbilityIcon(Button b, int treshold)
    {
        float mana = localPlayer.mana;

        if (mana < treshold)
            b.GetComponent<Image>().color = Color.gray;
        else
            b.GetComponent<Image>().color = Color.white;
    }
}
