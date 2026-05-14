using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Camera mainCamera;

    [SerializeField] private float interactDistance;
    private TargetController selected = null;
    private Collider lastCollider;

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
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Attack.canceled += OnAttackCanceled;

        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Attack.canceled -= OnAttackCanceled;

        inputActions.Disable();
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        gun.SetFiring(true);
    }

    void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        gun.SetFiring(false);
    }

    void Update()
    {
        HandleMovement();
        FaceMouse();
        TargetInteract();

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

    void TargetInteract()
    {
        if (inputActions.Player.Interact.WasPressedThisFrame() && selected)
        {
            selected.LiftAway();
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactDistance))
        {
            Collider collider = hit.collider;
            if (collider == lastCollider) return;

            TargetController interactable = collider.GetComponentInParent<TargetController>();
            if (interactable && interactable.isActiveAndEnabled)
            {
                selected = interactable;
                interactable.ShowOutline(true);
                lastCollider = collider;
                return;
            }
        }

        if (selected)
        {
            selected.ShowOutline(false);
            selected = null;
            lastCollider = null;
        }   
    }

    public void Shutdown(float speed)
    {
        StartCoroutine(FadeOut(speed));
    }

    public void Startup(float speed)
    {
        StartCoroutine(FadeIn(speed));
    }

    IEnumerator FadeOut(float speed)
    {
        var LOS = GetComponent<LineOfSight>();
        float curr = LOS.viewRadius;

        while (curr > 0)
        {
            curr -= speed * Time.deltaTime;
            LOS.viewRadius = curr;
            yield return null;
        }
    }

    IEnumerator FadeIn(float speed)
    {
        var LOS = GetComponent<LineOfSight>();
        float curr = 0;

        while (curr < 40)
        {
            curr += speed * Time.deltaTime;
            LOS.viewRadius = curr;
            yield return null;
        }
    }

}