using UnityEngine;

public class JumpCarController : MonoBehaviour
{
    public enum Axis
    {
        posX,
        negX,
        posZ,
        negZ
    }
    public float particleTriggerDistance = 25f;
    public Transform jumpPoint;

    public int jumpCost = 4;

    public Axis axis;

    private Transform player;
    private ShipController ship;
    private ParticleSystem ps;

    private PlayerControls inputActions;

    private MiniMapRenderer miniMap;

    void Awake()
    {
        inputActions = new PlayerControls();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        ship = player.GetComponentInParent<ShipController>();
        ps = GetComponentInChildren<ParticleSystem>();
        miniMap = FindAnyObjectByType<MiniMapRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 pos = transform.position;

        if (axis == Axis.posX || axis == Axis.negX)
            pos.z = player.position.z;
        else if (axis == Axis.posZ || axis == Axis.negZ)
            pos.x = player.position.x;

        transform.position = pos;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= particleTriggerDistance)
        {
            if (CanJump())
            {
                if (!ps.isPlaying) ps.Play();

                if (inputActions.Player.Interact.WasPressedThisFrame())
                {
                    JumpPlayer();
                }
            }
        }
        else
        {
            if (ps.isPlaying) ps.Stop();
        }
    }

    private void JumpPlayer()
    {
        ship.velocity = Vector3.zero;
        CharacterController cc = player.GetComponentInParent<CharacterController>();
        cc.enabled = false;
        player.position = jumpPoint.position;;
        cc.enabled = true;

        ship.fuel -= jumpCost;

        var gm = GameManager.Instance;
        int curr_x = gm.currArea.x_cord;
        int curr_y = gm.currArea.y_cord;
        if (axis == Axis.posX)
        {
            gm.ActivateOnly(curr_x+1, curr_y);
        } else if (axis == Axis.negX)
        {
            gm.ActivateOnly(curr_x-1, curr_y);
        } else if (axis == Axis.posZ)
        {
            gm.ActivateOnly(curr_x, curr_y+1);
        } else if (axis == Axis.negZ)
        {
            gm.ActivateOnly(curr_x, curr_y-1);
        }
    }

    private bool CanJump()
    {
        if (ship.fuel < jumpCost) return false;

        var gm = GameManager.Instance;
        if (axis == Axis.posX)
        {
            int curr_x = gm.currArea.x_cord;
            return curr_x < 3;
        } else if (axis == Axis.negX)
        {
            int curr_x = gm.currArea.x_cord;
            return curr_x > 0;
        } else if (axis == Axis.posZ)
        {
            int curr_y = gm.currArea.y_cord;
            return curr_y < 3;
        } else if (axis == Axis.negZ)
        {
            int curr_y = gm.currArea.y_cord;
            return curr_y > 0;
        } else return false;
    }
}