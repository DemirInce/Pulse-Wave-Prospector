using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PulseScan : MonoBehaviour
{
    private PlayerControls inputActions;
    public MiniMapRenderer minimap;
    public Heatmap heatmap;
    public ParticleSystem scanEffect;

    public float cooldownTime = 7f;
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

    void OnDisable()
    {
        inputActions.Player.Disable();
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

            var gm = GameManager.Instance;
            heatmap.GenerateAndDisplayHeatmap(gm.Sector, gm.currArea);
        }
    }
}