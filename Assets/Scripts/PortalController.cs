using UnityEngine;
using System.Collections;

public class PortalController : MonoBehaviour
{
    public ParticleSystem ring;
    public Renderer portal;

    public float speed = 50f;
    public float startBrightness = 200f;

    private Material glow;
    private bool on = true;
    public float particleTriggerDistance = 10f;
    public Transform player;
    public Transform jumpPoint;
    public ParticleSystem ps;

    public ParticleSystem loadingRing;
    public float triggerDistance = 25f;

    public float countdownTimer = 10f;
    public float timer;

    private float pulseTimer = 0f;
    public float startPulseDelay = 1f;
    public float endPulseDelay = 0.05f;

    public ParticleSystem startRing;

    public bool disabled = false;

    private PlayerControls inputActions;
    void Awake()
    {
        inputActions = new PlayerControls();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Start()
    {
        glow = portal.material;
        glow.SetFloat("Vector1_51108891", startBrightness);
        Shutdown();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerDistance && !on && !disabled)
        {
            timer -= Time.deltaTime;

            float progress = 1f - (timer / countdownTimer);

            float currentDelay = Mathf.Lerp(
                startPulseDelay,
                endPulseDelay,
                progress
            );

            pulseTimer -= Time.deltaTime;

            if (pulseTimer <= 0f)
            {
                loadingRing.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                loadingRing.Play();

                pulseTimer = currentDelay;
            }

            if (timer <= 0)
            {
                loadingRing.Stop();
                startRing.Play();
                Startup();
            }
        }
        else
        {
            timer = countdownTimer;
            pulseTimer = 0f;

            loadingRing.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        distance = Vector3.Distance(jumpPoint.position, player.position);
        if (distance <= particleTriggerDistance && on)
        {
            if (!ps.isPlaying) ps.Play();
            if (inputActions.Player.Interact.WasPressedThisFrame())
            {
                GameManager.Instance.FinishGame();
            }
        }
        else
        {
            if (ps.isPlaying) ps.Stop();
        }

        if (disabled)
        {
            if (ring.isPlaying) ring.Stop();
        } else
        {
            if (!ring.isPlaying) ring.Play();
        }
    }

    public void Shutdown()
    {
        StartCoroutine(FadeOut());
        on = false;
    }

    public void Startup()
    {
        StartCoroutine(FadeIn());
        on = true;
    }

    IEnumerator FadeOut()
    {
        float curr = startBrightness;

        while (curr > 0)
        {
            curr -= speed * Time.deltaTime;

            glow.SetFloat("Vector1_51108891", curr);

            yield return null;
        }
        portal.gameObject.SetActive(false);
    }

    IEnumerator FadeIn()
    {
        float curr = 0;
        portal.gameObject.SetActive(true);

        while (curr < startBrightness)
        {
            curr += speed * Time.deltaTime;

            glow.SetFloat("Vector1_51108891", curr);

            yield return null;
        }
    }
}