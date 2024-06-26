using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpawnTimerDisplay : NetworkBehaviour
{
    [SerializeField] float maxTime = 10.0f;
    [SerializeField] GameObject timerDisplay;
    [SerializeField] private Image timerFill;

    
    private NetworkVariable<float>  timer = new(0);

    void Start()
    {
        if(NetworkManager.Singleton.IsServer)
            timer.Value = 0;
        
        timer.OnValueChanged += RunDisplay;
    }

    void Update()
    {
        if(timerDisplay.transform.rotation != Quaternion.identity)
            timerDisplay.transform.rotation = Quaternion.identity;
    }

    void RunDisplay(float oldValue, float newValue)
    {
        float p = Mathf.Clamp01(newValue / maxTime);

        if (timerFill)
        {
            timerFill.transform.localScale = new Vector3(p, 1.0f, 1.0f);
        }

        if (timerDisplay != null)
        {
            if(p <= 0.0f)
                timerFill.gameObject.SetActive(false);
        }
    }

    public void UpdateTimer(float timerValue)
    {
        float oldValue = timer.Value;
        timer.Value = Mathf.Clamp(timerValue, 0, maxTime);

        RunDisplay(oldValue, timer.Value);
    }
}
