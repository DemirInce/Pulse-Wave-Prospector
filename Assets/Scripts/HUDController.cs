using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public ShipController player;

    public GameObject healthBar;
    public GameObject ammoBar;
    public GameObject fuelBar;

    private Slider healthSlider;
    private Slider ammoSlider;
    private Slider fuelSlider;

    void Start()
    {
        healthSlider = healthBar.GetComponentInChildren<Slider>();
        ammoSlider = ammoBar.GetComponentInChildren<Slider>();
        fuelSlider = fuelBar.GetComponentInChildren<Slider>();
    }

    void Update()
    {
        healthSlider.value = (player.health / player.maxHealth);
        
        Transform playerRoot = player.transform.root;
        Weapon gun = playerRoot.GetComponentInChildren<Weapon>();
        
        int ammo_value = (int)(((float)gun.ammo / gun.MaxAmmo) * 17);
        if (ammo_value == 0 && gun.ammo != 0) ammo_value = 1;
        ammoSlider.value = ammo_value;

        fuelSlider.value = player.fuel;
    }
}
