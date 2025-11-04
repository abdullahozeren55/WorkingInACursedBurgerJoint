using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class Car : MonoBehaviour
{
    public System.Action<GameObject> OnCarDestroyed;

    [Header("AI Settings")]
    public Transform target;
    public Transform castOrigin;
    public float targetReachDistance = 1f;
    public float stopDistance = 5f;
    public Vector3 boxHalfExtents = new Vector3(1f, 0.5f, 1f);
    public LayerMask obstacleMask;

    [Header("Animation Settings")]
    public float turnThreshold = 2f;   // kaç derece fark olunca dönüyor saysýn
    public float turnCooldown = 0.1f;  // anim spam’ini engeller

    [Header("Material Settings")]
    public MeshRenderer[] renderers; // only the color change ones

    [Header("CarNPC Settings")]
    public GameObject[] carNPCList;

    [Header("Speed Settings")]
    public float speed = 8f;

    [Header("Audio Settings")]
    public AudioClip engineLoopSound;
    public AudioClip waitingLoopSound;
    public AudioClip breakSound;
    public AudioClip honkSound;
    [Space]
    public float engineLoopSoundVolume = 1f;
    public float waitingLoopSoundVolume = 1f;
    public float breakSoundVolume = 1f;
    public float honkSoundVolume = 1f;
    public float honkSoundCooldown = 5f;
    private Coroutine honkingCoroutine;

    private Animator animator;
    private AudioSource audioSource;
    private NavMeshAgent agent;
    private Collider selfCollider;

    private bool isStoppedByObstacle;
    private bool obstacleDetected;
    private float lastTurnTime;
    private int currentIndex;

    private CarManager.CarDestinations destinations;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        selfCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        ChangeCarColor(CarManager.Instance.GetRandomCar0Material());
        DecideCarNPC();
        DecideSpeed();
    }

    private void Update()
    {
        HandleObstacleDetection();
        HandleTurnAnimation();
        HandleReachingTarget();
    }

    public void DecideDestinations(CarManager.CarDestinations dest)
    {
        destinations = dest;

        currentIndex = 0;

        target = destinations.endPoint[currentIndex];

        if (!agent.pathPending)
            agent.SetDestination(target.position);
    }

    private void HandleLoopSound(bool isMoving)
    {
        audioSource.Stop();
        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.volume = isMoving ? engineLoopSoundVolume : waitingLoopSoundVolume;
        audioSource.clip = isMoving ? engineLoopSound : waitingLoopSound;
        audioSource.Play();
    }

    private void DecideSpeed()
    {
        agent.speed = Random.Range(speed * 0.7f, speed * 1.4f);
    }

    private void DecideCarNPC()
    {
        foreach (GameObject driver in carNPCList)
        {
            driver.SetActive(false);
        }

        carNPCList[Random.Range(0, carNPCList.Length)].SetActive(true);
    }

    private void ChangeCarColor(Material mat)
    {
        foreach (Renderer ren in renderers)
        {
            var mats = ren.materials;

            if (mats.Length > 0)
            {
                mats[0] = mat;
                ren.materials = mats;
            }
        }
    }

    private void HandleReachingTarget()
    {
        if (Vector3.Distance(transform.position, target.position) < targetReachDistance)
        {
            if (currentIndex < destinations.endPoint.Length - 1)
            {
                currentIndex++;

                target = destinations.endPoint[currentIndex];

                if (!agent.pathPending)
                    agent.SetDestination(target.position);
            }
            else
            {
                OnCarDestroyed?.Invoke(gameObject);
                Destroy(gameObject);
            }
        }
    }

    private void HandleObstacleDetection()
    {
        Vector3 origin = castOrigin ? castOrigin.position : transform.position + Vector3.up * 0.5f;
        Quaternion rotation = transform.rotation;
        Vector3 direction = transform.forward;

        bool hitForward = Physics.BoxCast(origin, boxHalfExtents, direction, out RaycastHit hit, rotation, stopDistance, obstacleMask, QueryTriggerInteraction.Ignore);

        Collider[] hits = Physics.OverlapBox(origin + direction * stopDistance * 0.5f, boxHalfExtents, rotation, obstacleMask, QueryTriggerInteraction.Ignore);

        bool hitInside = false;
        foreach (var col in hits)
        {
            if (col != selfCollider)
            {
                hitInside = true;
                break;
            }
        }

        obstacleDetected = hitForward || hitInside;

        if (obstacleDetected && !isStoppedByObstacle)
        {
            agent.isStopped = true;
            isStoppedByObstacle = true;

            //Animasyonu durdur
            if (animator != null)
                animator.speed = 0f;

            HandleLoopSound(!isStoppedByObstacle);

            SoundManager.Instance.PlaySoundFX(breakSound, transform, breakSoundVolume, 0.85f, 1.15f);

            honkingCoroutine = StartCoroutine(Honk());
        }
        else if (!obstacleDetected && isStoppedByObstacle)
        {
            agent.isStopped = false;
            isStoppedByObstacle = false;

            //Animasyonu devam ettir
            if (animator != null)
                animator.speed = 1f;

            HandleLoopSound(!isStoppedByObstacle);

            if (honkingCoroutine != null)
            {
                StopCoroutine(honkingCoroutine);
                honkingCoroutine = null;
            }
        }
    }

    private void HandleTurnAnimation()
    {
        if (animator == null || agent.velocity.sqrMagnitude < 0.1f)
        {
            animator?.SetBool("turnLeft", false);
            animator?.SetBool("turnRight", false);
            return;
        }

        // Arabanýn o an baktýðý yön
        Vector3 forward = transform.forward;
        // NavMesh'in gitmeye çalýþtýðý yön
        Vector3 desired = agent.desiredVelocity.normalized;

        // Yön farkýný hesapla
        float angle = Vector3.SignedAngle(forward, desired, Vector3.up);

        if (Time.time - lastTurnTime > turnCooldown)
        {
            if (angle > turnThreshold)
            {
                animator.SetBool("turnRight", true);
                animator.SetBool("turnLeft", false);
            }
            else if (angle < -turnThreshold)
            {
                animator.SetBool("turnLeft", true);
                animator.SetBool("turnRight", false);
            }
            else
            {
                animator.SetBool("turnLeft", false);
                animator.SetBool("turnRight", false);
            }

            lastTurnTime = Time.time;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            obstacleDetected = false;

        Gizmos.color = obstacleDetected ? Color.red : Color.green;
        Vector3 origin = castOrigin ? castOrigin.position : transform.position + Vector3.up * 0.5f;
        Quaternion rotation = transform.rotation;
        Matrix4x4 cubeMatrix = Matrix4x4.TRS(origin + transform.forward * stopDistance * 0.5f, rotation, boxHalfExtents * 2f);
        Gizmos.matrix = cubeMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + transform.forward * stopDistance);
    }

    private IEnumerator Honk()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(honkSoundCooldown * 0.8f, honkSoundCooldown * 1.2f));
            SoundManager.Instance.PlaySoundFX(honkSound, transform, honkSoundVolume, 0.85f, 1.15f);
        }
    }
}
