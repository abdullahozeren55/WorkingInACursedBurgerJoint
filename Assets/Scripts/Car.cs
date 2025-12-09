using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class Car : MonoBehaviour
{
    public System.Action<GameObject> OnCarDestroyed;

    public bool IsTurning { get; private set; }

    [Header("AI Settings")]
    public Transform target;
    public Transform castOrigin;
    public float targetReachDistance = 2f;
    public float stopDistance = 6f; // Engel algýlama mesafesi
    public Vector3 boxHalfExtents = new Vector3(1f, 1f, 2f);
    public LayerMask obstacleMask;

    [Header("Physics Settings")]
    public float acceleration = 5f; // Hýzlanma ivmesi
    public float deceleration = 10f; // Frenleme gücü (Yüksek olursa sert durur, düþük olursa kayarak durur)

    [Header("Animation Settings")]
    public float turnThreshold = 5f;
    public float turnCooldown = 0.15f;

    [Header("Material Settings")]
    public MeshRenderer[] renderers;

    [Header("CarNPC Settings")]
    public GameObject[] carNPCList;

    [Header("Speed Settings")]
    public float maxSpeed = 8f; // Speed deðiþkeninin adýný maxSpeed yaptým karýþmasýn diye
    private float currentTargetSpeed; // O an gitmek istediðimiz hýz
    private float originalMaxSpeed;

    [Header("Audio Settings")]
    public AudioClip engineLoopSound;
    public AudioClip waitingLoopSound;
    public AudioClip breakSound;
    public AudioClip honkSound;
    public AudioClip carHitSound;
    [Space]
    public float engineLoopSoundVolume = 0.6f;
    public float waitingLoopSoundVolume = 0.3f;
    public float breakSoundVolume = 0.7f;
    public float honkSoundVolume = 0.8f;
    public float carHitSoundVolume = 1f;
    public float honkSoundCooldown = 5f;
    private Coroutine honkingCoroutine;

    [Header("Knockback Player Settings")]
    public bool CanKnockback = true;
    public float knockbackMinSpeedThreshold = 2f; // Bu hýzýn altýndaysa vurmaz (Kalkýþta korur)
    public float knockbackMultiplier = 5f;
    public float airMultiplier = 1.5f;
    public float horizontalSpread = 1f;

    // --- OPTÝMÝZASYON DEÐÝÞKENLERÝ ---
    private Animator animator;
    private AudioSource audioSource;
    private NavMeshAgent agent;
    private Collider selfCollider;

    private MaterialPropertyBlock propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_MaskColor");

    private static readonly int TurnLeftId = Animator.StringToHash("turnLeft");
    private static readonly int TurnRightId = Animator.StringToHash("turnRight");

    private readonly Collider[] hitBuffer = new Collider[10];

    private bool isBraking; // Fren yapýyor mu?
    private float lastTurnTime;
    private int currentIndex;
    private CarManager.CarDestinations destinations;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        selfCollider = GetComponent<Collider>();
        propBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        if (CarManager.Instance != null)
        {
            ApplyCarColor(CarManager.Instance.GetRandomCarColor());
        }

        HandleLoopSound(true);
        DecideCarNPC();
        DecideSpeed();

        isBraking = false;

        // Baþlangýçta navmesh'in durdurulmadýðýndan emin ol
        agent.isStopped = false;
        currentTargetSpeed = originalMaxSpeed;
    }

    private void Update()
    {
        HandleObstacleDetection();
        HandleMovementPhysics(); // Yeni fizik (yumuþak duruþ) fonksiyonu
        HandleTurnAnimation();
        HandleReachingTarget();
    }

    public void DecideDestinations(CarManager.CarDestinations dest)
    {
        destinations = dest;
        currentIndex = 0;

        if (destinations.endPoint != null && destinations.endPoint.Length > 0)
        {
            target = destinations.endPoint[currentIndex];
            if (agent.isOnNavMesh)
                agent.SetDestination(target.position);
        }
    }

    private void ApplyCarColor(Color color)
    {
        foreach (Renderer ren in renderers)
        {
            ren.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorId, color);
            ren.SetPropertyBlock(propBlock);
        }
    }

    private void HandleObstacleDetection()
    {
        Vector3 origin = castOrigin ? castOrigin.position : transform.position + Vector3.up * 1f;
        Vector3 direction = transform.forward;
        Vector3 center = origin + direction * stopDistance * 0.5f;

        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            boxHalfExtents,
            hitBuffer,
            transform.rotation,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        Car foundCar = null;
        bool foundDanger = false; // Player, Customer, Duvar vs.

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = hitBuffer[i];
            if (col == selfCollider) continue;

            // Önce: Bu bir araba mý?
            Car carScript = null;
            if (col.CompareTag("Car"))
            {
                carScript = col.GetComponent<Car>();
                if (carScript == null) carScript = col.GetComponentInParent<Car>();
            }

            if (carScript != null)
            {
                // Araba bulduk ama döngüyü kýrma! Belki daha yakýnda Player vardýr.
                // Sadece referansý tut.
                foundCar = carScript;
            }
            else
            {
                // Araba deðilse (Player, Customer, Wall...)
                // KESÝN TEHLÝKE VAR DEMEKTÝR.
                foundDanger = true;
                break; // Tehlike bulduysak daha fazla aramaya gerek yok, direkt frene asýlacaðýz.
            }
        }

        // --- KARAR ANI (ÖNCELÝK SIRALAMASI) ---

        // 1. ÖNCELÝK: Tehlike (Player/Wall/Customer) varsa -> DUR
        if (foundDanger)
        {
            if (!isBraking) StartBraking();
            currentTargetSpeed = 0f;
            return;
        }

        // 2. ÖNCELÝK: Araba varsa
        if (foundCar != null)
        {
            // YENÝ EKLENEN KISIM:
            // Eðer öndeki araç dönüyorsa (IsTurning == true), hýzýna uyma, DÝREKT DUR.
            // Böylece virajda içinden geçmeyiz.
            if (foundCar.IsTurning)
            {
                if (!isBraking) StartBraking();
                currentTargetSpeed = 0f;
            }
            else
            {
                // Dönmüyorsa (Düz gidiyorsa) takibe devam
                isBraking = false;
                currentTargetSpeed = foundCar.agent.velocity.magnitude;
            }
            return;
        }

        // 3. ÖNCELÝK: Yol boþ -> GAZLA
        if (isBraking) StopBraking();
        currentTargetSpeed = originalMaxSpeed;
    }

    // YENÝ: Hýzlanma ve Yavaþlama Mantýðý (Lerp ile yumuþak geçiþ)
    private void HandleMovementPhysics()
    {
        // Anlýk hýzý hedefe doðru yumuþat
        float lerpRate = (currentTargetSpeed < agent.speed) ? deceleration : acceleration;

        agent.speed = Mathf.MoveTowards(agent.speed, currentTargetSpeed, Time.deltaTime * lerpRate);

        // Animasyon hýzýný tekerlek hýzýna eþitle
        if (animator != null)
            animator.speed = agent.speed / originalMaxSpeed;

        // NavMesh optimizasyonu: Hýz çok düþükse tamamen durdur ki iþlemci yorulmasýn
        if (agent.speed < 0.1f && currentTargetSpeed == 0f)
        {
            agent.velocity = Vector3.zero; // Kaymayý önle
            agent.isStopped = true;
            HandleLoopSound(false);
        }
        else
        {
            agent.isStopped = false;
            HandleLoopSound(true);
        }
    }

    private void StartBraking()
    {
        isBraking = true;
        // isStopped = true YAPMIYORUZ. Sadece hedef hýzý 0 yapýyoruz, HandleMovementPhysics yavaþlatacak.

        if (SoundManager.Instance && agent.velocity.sqrMagnitude > 2f) // Sadece hýzlýysak fren sesi çal
            SoundManager.Instance.PlaySoundFX(breakSound, transform, breakSoundVolume);

        if (honkingCoroutine == null) honkingCoroutine = StartCoroutine(Honk());
    }

    private void StopBraking()
    {
        isBraking = false;

        if (honkingCoroutine != null)
        {
            StopCoroutine(honkingCoroutine);
            honkingCoroutine = null;
        }
    }

    private void HandleTurnAnimation()
    {
        if (animator == null) return;

        // Araç çok yavaþsa dönüyor sayýlmaz, düz duruyordur
        if (agent.velocity.sqrMagnitude < 0.5f)
        {
            IsTurning = false; // <-- EKLENDÝ
            animator.SetBool(TurnLeftId, false);
            animator.SetBool(TurnRightId, false);
            return;
        }

        if (Time.time - lastTurnTime < turnCooldown) return;

        Vector3 forward = transform.forward;
        Vector3 steeringTargetDir = (agent.steeringTarget - transform.position).normalized;
        if (Vector3.Distance(agent.steeringTarget, transform.position) < 1f)
            steeringTargetDir = agent.desiredVelocity.normalized;

        float angle = Vector3.SignedAngle(forward, steeringTargetDir, Vector3.up);

        if (angle > turnThreshold)
        {
            IsTurning = true; // <-- EKLENDÝ (Saða dönüyor)
            animator.SetBool(TurnRightId, true);
            animator.SetBool(TurnLeftId, false);
        }
        else if (angle < -turnThreshold)
        {
            IsTurning = true; // <-- EKLENDÝ (Sola dönüyor)
            animator.SetBool(TurnLeftId, true);
            animator.SetBool(TurnRightId, false);
        }
        else
        {
            IsTurning = false; // <-- EKLENDÝ (Düz gidiyor)
            animator.SetBool(TurnLeftId, false);
            animator.SetBool(TurnRightId, false);
        }

        lastTurnTime = Time.time;
    }

    private void HandleReachingTarget()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + targetReachDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                GoToNextPoint();
            }
        }
    }

    private void GoToNextPoint()
    {
        if (currentIndex < destinations.endPoint.Length - 1)
        {
            currentIndex++;
            target = destinations.endPoint[currentIndex];
            agent.SetDestination(target.position);
        }
        else
        {
            OnCarDestroyed?.Invoke(gameObject);
            Destroy(gameObject);
        }
    }

    private void HandleLoopSound(bool isMoving)
    {
        if (audioSource == null) return;
        if (isMoving && audioSource.clip == engineLoopSound && audioSource.isPlaying) return;
        if (!isMoving && audioSource.clip == waitingLoopSound && audioSource.isPlaying) return;

        audioSource.Stop();
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = isMoving ? engineLoopSoundVolume : waitingLoopSoundVolume;
        audioSource.clip = isMoving ? engineLoopSound : waitingLoopSound;
        audioSource.Play();
    }

    private void DecideCarNPC()
    {
        for (int i = 0; i < carNPCList.Length; i++)
        {
            if (carNPCList[i] != null) carNPCList[i].SetActive(false);
        }
        if (carNPCList.Length > 0)
            carNPCList[Random.Range(0, carNPCList.Length)].SetActive(true);
    }

    private void DecideSpeed()
    {
        maxSpeed = Random.Range(maxSpeed * 0.8f, maxSpeed * 1.2f);
        originalMaxSpeed = maxSpeed;
        agent.speed = maxSpeed; // Baþlangýçta hýzlanmýþ gelsin
    }

    // --- YENÝ TRIGGER MANTIÐI (HIZ TABANLI) ---
    private void OnTriggerEnter(Collider other)
    {
        if (CanKnockback && other.CompareTag("Player"))
        {
            if (agent.velocity.magnitude < knockbackMinSpeedThreshold) return;

            // --- YENÝ HESAPLAMA ---

            // -1 (Sol) ile 1 (Sað) arasýnda rastgele bir deðer üret
            float randomSide = Random.Range(-1f, 1f);

            // Yön Vektörü: Ýleri + Yukarý + (Rastgele Sað/Sol)
            // Hepsini toplayýp normalize ediyoruz ki yön deðiþse de güç (forcePower) sabit kalsýn.
            Vector3 forceDir = (transform.forward + (Vector3.up * airMultiplier) + (transform.right * randomSide * horizontalSpread)).normalized;

            // ----------------------

            float forcePower = originalMaxSpeed * knockbackMultiplier;

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.ApplyKnockback(forceDir, forcePower);

                // Eðer ses manager varsa
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySoundFX(carHitSound, other.transform, carHitSoundVolume);

                // Görsel Efekt
                if (CameraManager.Instance != null)
                    CameraManager.Instance.PlayCarHitEffects(randomSide);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isBraking ? Color.red : Color.green; // Fren durumuna göre renk
        Vector3 origin = castOrigin ? castOrigin.position : transform.position + Vector3.up * 1f;
        Quaternion rotation = transform.rotation;
        Matrix4x4 cubeMatrix = Matrix4x4.TRS(origin + transform.forward * stopDistance * 0.5f, rotation, boxHalfExtents * 2f);
        Gizmos.matrix = cubeMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private IEnumerator Honk()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(honkSoundCooldown * 0.5f, honkSoundCooldown * 1.5f));
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySoundFX(honkSound, transform, honkSoundVolume);
        }
    }
}