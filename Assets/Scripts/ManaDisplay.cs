using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    manaCountText;
    [SerializeField] private Image              manaBarFill;

    Wyzard localPlayer;

    void Start()
    {

    }

    void Update()
    {
        if (localPlayer == null)
        {
            var players = FindObjectsOfType<Wyzard>();
            
            foreach(Wyzard p in players)
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
            manaCountText.text = $"{localPlayer.mana}";

            float p = (float)localPlayer.mana / (float)localPlayer.maxMana;
            manaBarFill.transform.localScale = new Vector3(p, 1.0f, 1.0f);
        }
    }
}
