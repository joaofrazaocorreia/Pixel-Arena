using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private Spawner spawner;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject waitingScreen;
    [SerializeField] private GameObject endBG;
    [SerializeField] private Button[] abilityButtons;
    [SerializeField] private float matchTimeLimit = 600;
    [SerializeField] private TextMeshProUGUI timerDisplay;
    
    private PlayerTower localPlayer;
    private bool win = false;
    private float timer = 0;

    public void Quit()
    {
        Application.Quit();
    }

    public void Win()
    {
        if (!loseScreen.activeSelf)
        {
            endBG.SetActive(true);
            loseScreen.SetActive(false);
            winScreen.SetActive(true);

            Time.timeScale = 0;
            StopServerRpc();
        }
    }

    public void Lose()
    {
        if (!winScreen.activeSelf)
        {
            endBG.SetActive(true);
            winScreen.SetActive(false);
            loseScreen.SetActive(true);

            Time.timeScale = 0;
            StopServerRpc();
        }
    }

    void Start()
    {
        var players = FindObjectsOfType<PlayerTower>();
        timer = matchTimeLimit;

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
        var players = FindObjectsOfType<PlayerTower>();

        if(NetworkManager.Singleton.IsServer)
        {
            waitingScreen.SetActive(false);

            if(players.Count() >= 2)
            {
                timer -= Time.deltaTime;
                UpdateTimerText(timer);
                UpdatetimerTextClientRpc(timer);
            }
        }


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
        UpdateAbilityIcon(abilityButtons[3], 3);
        UpdateAbilityIcon(abilityButtons[4], 3);

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
                    if (!p.IsLocalPlayer && !p.isDead)
                    {
                        if (timer <= 0 && localPlayer.Health >= p.Health)
                            win = true;
                        
                        else if (timer <= 0 && localPlayer.Health < p.Health)
                        {
                            win = false;
                            Lose();
                            break;
                        }

                        else
                        {
                            win = false;
                            break;
                        }
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
        if (NetworkManager.Singleton.IsClient)
        {
            float mana = localPlayer.mana;

            if (mana < treshold)
                b.GetComponent<Image>().color = Color.gray;
            else
                b.GetComponent<Image>().color = Color.white;
        }
    }

    private void UpdateTimerText(float timer)
    {
        string text = "";

        if (timer > 0)
        {
            float minutes = Mathf.Floor(timer / 60);
            float seconds = Mathf.Floor(timer % 60);
            float milliseconds = Mathf.Floor((timer % 60 - Mathf.Floor(timer % 60)) * 100);

            if (minutes < 10) text += "0";
            if (minutes <= 0) text += "0:";
            else text += $"{minutes}:";

            if (seconds < 10) text += "0";
            if (seconds <= 0) text += "0:";
            else text += $"{seconds}:";

            if (milliseconds < 10) text += "0";
            if (milliseconds <= 0) text += "0";
            else text += $"{milliseconds}";
        }

        else
            text = "00:00:00";

        timerDisplay.text = text;
        this.timer = timer;
    }

    [ClientRpc]
    private void UpdatetimerTextClientRpc(float timer)
    {
        if(!winScreen.activeSelf && !loseScreen.activeSelf)
            UpdateTimerText(timer);
    }

    [ServerRpc]
    private void StopServerRpc()
    {
        Destroy(FindObjectOfType<Spawner>().gameObject);
        foreach (Enemy e in FindObjectsOfType<Enemy>())
            Destroy(e.gameObject);
        
        
        Time.timeScale = 0;
    }
}
