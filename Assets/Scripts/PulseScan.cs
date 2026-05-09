using UnityEngine;
using UnityEngine.UI;

public class PulseScan : MonoBehaviour
{
    private PlayerControls inputActions;
    public MiniMapRenderer minimap;
    public ParticleSystem scanEffect;

    public float cooldownTime = 5f;
    private float cooldownTimer;

    public GameObject energyBar;
    private Slider cooldownSlider;

    void Awake()
    {
        inputActions = new PlayerControls();
        cooldownSlider = energyBar.GetComponentInChildren<Slider>();
        cooldownTimer = cooldownTime;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void Update()
    {
        HandleCooldown();

        if (inputActions.Player.Scan.WasPressedThisFrame())
            Scan();

        minimap.UpdateScan(transform.position);
    }

    void HandleCooldown()
    {
        if (cooldownTimer < cooldownTime)
        {
            cooldownTimer += Time.deltaTime;
        }

        cooldownTimer = Mathf.Min(cooldownTimer, cooldownTime);

        if (cooldownSlider != null)
        {
            cooldownSlider.value = cooldownTimer / cooldownTime;
        }
    }

    void Scan()
    {
        if (cooldownTimer < cooldownTime)
            return;

        ShipController player = GetComponentInParent<ShipController>();

        if (player.fuel >= 1)
        {
            minimap.DrawScan();
            scanEffect.Play();
            player.fuel -= 1;
            cooldownTimer = 0f;
        }
    }
}