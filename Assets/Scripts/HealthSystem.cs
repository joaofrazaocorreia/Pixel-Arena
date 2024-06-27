using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class HealthSystem : NetworkBehaviour
{
    [SerializeField] Faction        _faction;
    [SerializeField] float           maxHealth = 100.0f;
    [SerializeField] GameObject      healthDisplay;
    [SerializeField] Image           fill;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Color           flashColor = Color.white;


    private NetworkVariable<float>  health = new(100);
    public float Health => health.Value;
    private Flasher                 flasher;

    public bool isDead => health.Value <= 0.0f;
    public Faction faction => _faction;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    void Start()
    {
        flasher = GetComponent<Flasher>();

        if(NetworkManager.Singleton.IsServer)
            health.Value = maxHealth;
        
        health.OnValueChanged += RunDisplay;

        if (healthText)
            healthText.text = $"{health.Value} / {maxHealth}";
    }

    void Update()
    {
        if(healthDisplay.transform.rotation != Quaternion.identity)
            healthDisplay.transform.rotation = Quaternion.identity;
    }

    void RunDisplay(float oldValue, float newValue)
    {
        if (flasher) flasher.Flash(flashColor, 0.2f);

        float p = Mathf.Clamp01(newValue / maxHealth);

        if (fill)
        {
            fill.transform.localScale = new Vector3(p, 1.0f, 1.0f);
        }

        if (healthDisplay != null)
        {
            if(p <= 0.0f)
                fill.gameObject.SetActive(false);
                
            if (healthText)
                healthText.text = $"{health.Value} / {maxHealth}";
        }
    }

    public void DealDamage(float damage)
    {
        float oldValue = health.Value;
        health.Value = Mathf.Clamp(health.Value - damage, 0, maxHealth);

        RunDisplay(oldValue, health.Value);

        if (isDead && onDeath != null)
            onDeath();
    }
}
