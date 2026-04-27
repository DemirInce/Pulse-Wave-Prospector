using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Camera mainCamera;

    private ShipController ship;

    [Header("Combat")]
    public Weapon gun;

    public float deadZone = 0.2f;

    void Awake()
    {
        inputActions = new PlayerControls();
        mainCamera = Camera.main;
        ship = GetComponent<ShipController>();
    }

    void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Attack.performed += _ => gun.SetFiring(true);
        inputActions.Player.Attack.canceled += _ => gun.SetFiring(false);
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        HandleMovement();
        FaceMouse();

        ship.Tick();
    }

    void HandleMovement()
    {
        ship.ApplyThrust(moveInput.y);
    }

    void FaceMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;

            if (direction.magnitude < deadZone)
                return;

            ship.RotateTowards(direction);
        }
    }
}