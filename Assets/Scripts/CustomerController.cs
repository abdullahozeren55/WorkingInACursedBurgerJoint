using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DialogueSpeaker))]
// [RequireComponent(typeof(Animator))] <-- Bunu kaldýrdým, child'da olabilir.
public class CustomerController : MonoBehaviour, ICustomer, IInteractable
{
    [Header("Runtime Data")]
    [SerializeField] private CustomerProfile currentProfile;
    [SerializeField] private OrderData currentOrder;

    [Header("Settings")]
    [SerializeField] private Transform trayHoldPoint;
    [SerializeField] private float headLookWeight = 1f;

    // State Machine
    public CustomerState CurrentState { get; private set; }

    // IInteractable Props
    public PlayerManager.HandRigTypes HandRigType { get => handRigType; set => handRigType = value; }
    [SerializeField] private PlayerManager.HandRigTypes handRigType = PlayerManager.HandRigTypes.Nothing;

    public bool OutlineShouldBeRed { get => outlineShouldBeRed; set => outlineShouldBeRed = value; }
    private bool outlineShouldBeRed;

    public bool CanInteract { get => canInteract; set => canInteract = value; }
    private bool canInteract;

    public string FocusTextKey { get => focusTextKey; set => focusTextKey = value; }
    [SerializeField] private string focusTextKey;

    // YENÝ: Seçilen diyaloðu hafýzada tutmak için
    [SerializeField] private DialogueData assignedDialogue;

    // YENÝ: Kafa takibi yumuþaklýðý için
    private float currentLookWeight = 0f;

    // Components
    private NavMeshAgent agent;
    private Animator anim;
    private DialogueSpeaker dialogueSpeaker;
    private SkinnedMeshRenderer meshRenderer;

    // Hedefler
    private Transform counterPoint;
    private DiningTable assignedTable;
    private Transform mySeatPoint;
    private Transform myEntryPoint;
    private Transform myTrayPoint;
    private Transform activeDoorHandle;

    // Layers
    private int interactableLayer;
    private int interactableOutlinedLayer;
    private int interactableOutlinedRedLayer;
    private int uninteractableLayer;

    private bool isLeavingShop = false;
    private Door currentTargetDoor;
    private float doorIKWeight = 0f; // Kapýya uzanma aðýrlýðý
    private bool isReachingForDoor = false; // Þu an uzanmalý mý?

    // --- FIX 1: COROUTINE SPAM ENGELLEYÝCÝ ---
    private bool isInteractingWithDoor = false;

    // YENÝ: IK Hedefini geçici olarak deðiþtirmek için
    private Transform overrideLookTarget;
    // YENÝ: Bakýlan noktanýn anlýk (yumuþatýlmýþ) pozisyonu
    private Vector3 currentSmoothedLookPos;

    private float handIKWeight = 0f; // Ellerin tepsiye yapýþma kuvveti (0 = Serbest, 1 = Yapýþýk)
    private Tray carriedTray; // Þu an taþýdýðýmýz tepsi referansý

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // --- FIX 2: ANIMATOR BULMA ---
        // Animator genelde child (mesh) objesindedir. GetComponent root'ta bulamazsa child'a bakar.
        anim = GetComponentInChildren<Animator>();

        dialogueSpeaker = GetComponent<DialogueSpeaker>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");
        interactableOutlinedRedLayer = LayerMask.NameToLayer("InteractableOutlinedRed");
        uninteractableLayer = LayerMask.NameToLayer("Uninteractable");

        CanInteract = false;
    }

    public void Initialize(CustomerProfile profile)
    {
        currentProfile = profile;

        if (meshRenderer && profile.SkinMaterial)
            meshRenderer.material = profile.SkinMaterial;

        agent.speed = profile.WalkSpeed;
        isLeavingShop = false;
        isInteractingWithDoor = false; // Reset

        // 1. Rastgele bir sipariþ senaryosu seç
        var randomSelection = profile.PossibleOrders[Random.Range(0, profile.PossibleOrders.Count)];
        currentOrder = randomSelection.Order;

        // 2. Glitch Zarý At (%0 ile %100 arasý)
        float roll = Random.Range(0f, 100f);

        // Þartlar: Zar tuttuysa VE Glitch diyaloðu boþ deðilse
        bool isGlitched = (roll < randomSelection.GlitchChance) && (randomSelection.GlitchDialogue != null);

        if (isGlitched)
        {
            assignedDialogue = randomSelection.GlitchDialogue;
            // Ýstersen burada müþteriye "IsGlitched = true" gibi bir flag atayýp
            // yürürken garip sesler çýkarmasýný vs. saðlayabilirsin.
        }
        else
        {
            assignedDialogue = randomSelection.NormalDialogue;
        }

        counterPoint = WorldManager.Instance.GetCounterPosition();

        var entryInfo = WorldManager.Instance.GetRandomEntryDoor();
        currentTargetDoor = entryInfo.targetDoor;
        agent.SetDestination(entryInfo.interactionPoint.position);

        GoToState(CustomerState.ApproachingDoor);
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case CustomerState.ApproachingDoor:
                // --- FIX 1: SPAM ENGELÝ ---
                // Eðer zaten kapýyla uðraþýyorsak tekrar coroutine baþlatma!
                if (!isInteractingWithDoor && !agent.pathPending && agent.remainingDistance < 0.2f)
                {
                    StartCoroutine(DoorInteractionRoutine());
                }
                break;

            case CustomerState.Entering:
                // Kapýdan geçti, kasaya gidiyor

                // Datadan gelen deðeri kullanýyoruz
                float stopDist = currentProfile != null ? currentProfile.ArrivalDistance : 0.5f;

                // KONTROL GÜNCELLENDÝ:
                // 1. Yol hesaplamasý bitmiþ mi?
                // 2. Kalan mesafe, belirlediðimiz eþikten (stopDist) küçük mü?
                if (!agent.pathPending && agent.remainingDistance <= stopDist)
                {
                    // Ekstra Güvenlik: Yol gerçekten bitti mi veya gidilemez mi oldu?
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f || agent.remainingDistance < 0.1f)
                    {
                        // Hiçbir þey yapma, NavMesh bazen sapýtýr.
                    }

                    // Gelmiþ sayýyoruz
                    RotateBodyToCounter();
                    GoToState(CustomerState.AtCounter);
                }
                break;

            case CustomerState.AtCounter:
                break;

            case CustomerState.Ordering:
                break;

            case CustomerState.WaitingForFood:
                break;

            case CustomerState.MovingToSeat:
                if (mySeatPoint == null && assignedTable != null)
                {
                    // Fonksiyon artýk trayPoint'i de veriyor
                    mySeatPoint = assignedTable.GetSeatForCustomer(this, out myEntryPoint, out myTrayPoint);
                }

                if (mySeatPoint != null)
                {
                    // --- DEÐÝÞÝKLÝK BURADA ---
                    // Artýk oturma noktasýna deðil, GÝRÝÞ noktasýna yürüyoruz.
                    // Eðer myEntryPoint null gelirse (Unity'de atamayý unutursan) mySeatPoint'e gider.
                    agent.SetDestination(myEntryPoint != null ? myEntryPoint.position : mySeatPoint.position);

                    // Vardýk mý?
                    if (!agent.pathPending && agent.remainingDistance < 0.2f)
                    {
                        // Ekstra kontrol: Hýz gerçekten düþtü mü?
                        if (agent.velocity.sqrMagnitude < 0.1f)
                        {
                            GoToState(CustomerState.Eating);
                        }
                    }
                }
                break;

            case CustomerState.Eating:
                // Test rutini (EatingRoutine) dýþarýdan veya buradan tetiklenebilir.
                break;

            case CustomerState.Leaving:
                if (!agent.pathPending && agent.remainingDistance < 1f)
                {
                    gameObject.SetActive(false);
                }
                break;
        }
    }

    // --- KAFA TAKÝBÝ (HEAD TRACKING) ---
    private void OnAnimatorIK(int layerIndex)
    {
        bool shouldLook = (CurrentState == CustomerState.AtCounter ||
                           CurrentState == CustomerState.Ordering ||
                           CurrentState == CustomerState.WaitingForFood);

        if (shouldLook)
        {
            Transform target = null;

            // 1. Hedef Belirleme
            if (overrideLookTarget != null)
                target = overrideLookTarget;
            else if (PlayerManager.Instance != null)
                target = PlayerManager.Instance.GetHeadTransform();

            // 2. Aðýrlýk Yumuþatma (Gözler ne kadar aktif?)
            float targetWeight = (target != null) ? headLookWeight : 0f;
            currentLookWeight = Mathf.Lerp(currentLookWeight, targetWeight, Time.deltaTime * 5f);

            // 3. POZÝSYON YUMUÞATMA (Asýl Olay Burasý)
            if (target != null)
            {
                // Eðer ilk kez bakýyorsak (sýfýr noktasýndaysa) direkt ata ki kafasý yerden kalkmasýn
                if (currentSmoothedLookPos == Vector3.zero)
                    currentSmoothedLookPos = target.position;

                // Hedef pozisyona doðru yavaþça kay (Saniyede 5 birim hýzla)
                // Bu sayede Player'dan Kameraya geçerken kafa "kayarak" döner.
                currentSmoothedLookPos = Vector3.Lerp(currentSmoothedLookPos, target.position, Time.deltaTime * 5f);
            }

            // 4. IK Uygulama
            anim.SetLookAtWeight(currentLookWeight, 0.2f, 1f, 0.5f, 0.7f);

            // Eðer hedef null ise (bakmýyorsa) eski pozisyonda kalsýn, weight 0 olacaðý için sorun olmaz.
            // Ama hedef varsa yumuþatýlmýþ pozisyonu kullan.
            if (target != null)
            {
                anim.SetLookAtPosition(currentSmoothedLookPos);
            }
            else
            {
                // Hedef yoksa kafanýn önünü hedef al ki kilitlenip kalmasýn (Opsiyonel)
                // anim.SetLookAtPosition(transform.position + transform.forward);
            }
        }
        else
        {
            // Bakma modu kapalýysa aðýrlýðý düþür
            currentLookWeight = Mathf.Lerp(currentLookWeight, 0f, Time.deltaTime * 5f);
            anim.SetLookAtWeight(currentLookWeight);
        }

        // ---------------------------------------------------------
        // 2. EL TAKÝBÝ (Hand IK) - YENÝ KISIM
        // ---------------------------------------------------------

        // Eðer 'MovingToSeat' durumundaysak ve elimizde tepsi varsa IK aðýrlýðýný artýr
        bool isCarryingTray = (carriedTray != null);

        // Aðýrlýðý yumuþakça deðiþtir (Lerp)
        float targetHandWeight = isCarryingTray ? 1f : 0f;
        handIKWeight = Mathf.Lerp(handIKWeight, targetHandWeight, Time.deltaTime * 5f);

        if (handIKWeight > 0.01f && carriedTray != null)
        {
            // SOL EL
            if (carriedTray.LeftHandGrip != null)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKWeight);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKWeight);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, carriedTray.LeftHandGrip.position);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, carriedTray.LeftHandGrip.rotation);
            }

            // SAÐ EL
            if (carriedTray.RightHandGrip != null)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKWeight);
                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKWeight);
                anim.SetIKPosition(AvatarIKGoal.RightHand, carriedTray.RightHandGrip.position);
                anim.SetIKRotation(AvatarIKGoal.RightHand, carriedTray.RightHandGrip.rotation);
            }

            // --- GÖVDE DÜZELTMESÝ (Opsiyonel) ---
            // Tepsi taþýrken hafif öne eðilmesini engellemek veya kollarýn yetiþmesi için
            // Body pozisyonuyla oynanabilir ama þimdilik ellere odaklanalým.
        }
        else
        {
            // Kapý IK Mantýðý
            float targetDoorWeight = isReachingForDoor ? 1f : 0f;
            doorIKWeight = Mathf.Lerp(doorIKWeight, targetDoorWeight, Time.deltaTime * 8f);

            // DEÐÝÞÝKLÝK: 'currentTargetDoor.handlePoint' yerine 'activeDoorHandle' kullanýyoruz
            if (doorIKWeight > 0.01f && activeDoorHandle != null)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, doorIKWeight);
                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, doorIKWeight * 0.5f);

                anim.SetIKPosition(AvatarIKGoal.RightHand, activeDoorHandle.position);
                anim.SetIKRotation(AvatarIKGoal.RightHand, activeDoorHandle.rotation);
            }
        }
    }

    // --- YENÝ HELPER METODLAR (DialogueManager Çaðýracak) ---
    public void SetLookTarget(Transform target)
    {
        overrideLookTarget = target;
    }

    public void ClearLookTarget()
    {
        overrideLookTarget = null;
    }

    private void RotateBodyToCounter()
    {
        Transform faceTarget = WorldManager.Instance.GetCounterFacePoint();
        if (faceTarget != null)
        {
            agent.updateRotation = false;
            transform.DORotateQuaternion(faceTarget.rotation, 0.5f).SetEase(Ease.OutSine);
        }
    }

    // --- ETKÝLEÞÝM (DÝYALOG BAÞLATMA) ---
    public void OnInteract()
    {
        if (!CanInteract) return;

        // Sadece kasada bekliyorsa konuþabilir
        if (CurrentState == CustomerState.AtCounter)
        {
            // State deðiþtir (Tekrar týklanmasýn ve Idle oynamaya devam etsin)
            GoToState(CustomerState.Ordering);

            if (assignedDialogue != null)
            {
                // Diyaloðu baþlat ve BÝTÝNCE ne yapacaðýný söyle (Callback)
                DialogueManager.Instance.StartDialogue(assignedDialogue, OnOrderingFinished);
            }
            else
            {
                Debug.LogWarning("Müþterinin diyaloðu yok! Direkt sipariþ vermiþ sayýyorum.");
                OnOrderingFinished();
            }
        }
    }

    // Callback Fonksiyonu: Diyalog bitince burasý çalýþacak
    private void OnOrderingFinished()
    {
        // Ekrana sipariþi yazdýrabilirsin (UI Manager'a haber ver)
        // UIManager.Instance.ShowOrderTicket(currentOrder);

        // Sipariþ beklemeye geç
        GoToState(CustomerState.WaitingForFood);
    }

    // --- YENÝ YARDIMCI METOT ---
    private DialogueData GetRandomFeedbackDialogue(List<CustomerProfile.InteractionDialogue> options)
    {
        if (options == null || options.Count == 0) return null;

        // 1. Listeden Rastgele Bir Seçenek Al
        var selection = options[Random.Range(0, options.Count)];

        // 2. Glitch Zarý At
        float roll = Random.Range(0f, 100f);

        // Þartlar: Zar tuttuysa VE Glitch diyaloðu tanýmlýysa
        if (roll < selection.GlitchChance && selection.GlitchDialogue != null)
        {
            return selection.GlitchDialogue;
        }

        // Yoksa normali döndür
        return selection.NormalDialogue;
    }

    // --- GÜNCELLENMÝÞ TryReceiveTray (Doðru Sipariþ) ---
    public bool TryReceiveTray(Tray tray)
    {
        if (CurrentState != CustomerState.WaitingForFood) return false;

        bool isOrderCorrect = OrderValidator.Validate(tray, currentOrder);

        if (isOrderCorrect)
        {
            GrabTray(tray); // Tepsi yapýþýr, IK devreye girer.

            // YENÝ SEÇÝM SÝSTEMÝ:
            DialogueData selectedDialogue = GetRandomFeedbackDialogue(currentProfile.PossibleCorrectOrderDialogues);

            if (selectedDialogue != null)
            {
                // Diyalog bitince masaya git (Callback)
                DialogueManager.Instance.StartDialogue(selectedDialogue, HandleFinishDialogue);
            }
            else
            {
                // Diyalog yoksa direkt git
                HandleFinishDialogue();
            }

            return true;
        }
        else
        {
            // Yanlýþ sipariþ ise sessizce reddet (Manager konuþacak)
            return false;
        }
    }

    private void GrabTray(Tray tray)
    {
        carriedTray = tray; // IK için referansý al

        // --- DEÐÝÞÝKLÝK BURADA ---
        // Eski manuel kapatma kodlarýný sil (trayRb.isKinematic = true, trayCol.enabled = false vs.)
        // Yerine tek satýrla her þeyi kapat:
        tray.SetPhysicsState(false);
        // -------------------------

        // Tepsiyi 'trayHoldPoint'e baðla
        tray.transform.SetParent(trayHoldPoint != null ? trayHoldPoint : transform);

        // Pozisyonu sýfýrla (Böylece HoldPoint nerede duruyorsa oraya ýþýnlanýr)
        tray.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack);

        // Rotasyonu sýfýrla (HoldPoint nasýl dönükse öyle döner)
        tray.transform.DOLocalRotate(Vector3.zero, 0.5f);
    }

    public void HandleFinishDialogue()
    {
        if (CurrentState == CustomerState.Ordering)
        {
            GoToState(CustomerState.WaitingForFood);
        }
        else if (CurrentState == CustomerState.WaitingForFood)
        {
            GoToState(CustomerState.MovingToSeat);
        }
        else if (CurrentState == CustomerState.Eating)
        {
            GoToState(CustomerState.Leaving);
        }
    }

    private void GoToState(CustomerState newState)
    {
        // Eski state'ten çýkarken temizlik yapalým
        if (CurrentState == CustomerState.AtCounter ||
            CurrentState == CustomerState.Ordering ||
            CurrentState == CustomerState.WaitingForFood)
        {
            // Eðer kasadan ayrýlýyorsak (Leaving veya MovingToSeat) kaydý sil.
            // Ama Ordering -> WaitingForFood geçiþinde silme, çünkü hala kasada.
            if (newState != CustomerState.Ordering && newState != CustomerState.WaitingForFood && newState != CustomerState.AtCounter)
            {
                CustomerManager.Instance.UnregisterCustomerAtCounter(this);
            }
        }

        CurrentState = newState;

        switch (newState)
        {
            case CustomerState.ApproachingDoor:
                agent.isStopped = false;
                agent.updateRotation = true;
                anim.SetBool("walk", true); // Artýk kapýya giderken yürüyecek!
                break;

            case CustomerState.Leaving:
                // Önce NavMesh'i tekrar açabilmek için objeyi NavMesh üstüne ýþýnla/yaklaþtýr
                // Çünkü animasyonla kaymýþ olabilir.

                agent.enabled = true; // Tekrar aç
                agent.isStopped = false;
                agent.updateRotation = true;
                anim.SetBool("walk", true);
                anim.SetBool("Sitting", false);
                break;

            case CustomerState.Entering:
                agent.isStopped = false;
                agent.updateRotation = true;
                anim.SetBool("walk", true);

                // --- FIX: NULL CHECK VE GÜVENLÝK ---
                if (counterPoint == null)
                {
                    // Eðer deðiþken boþsa, WorldManager'dan tekrar çekmeyi dene
                    if (WorldManager.Instance != null)
                        counterPoint = WorldManager.Instance.GetCounterPosition();
                }

                if (counterPoint != null)
                {
                    agent.SetDestination(counterPoint.position);
                }
                else
                {
                    // Buraya düþüyorsa WorldManager sahneye koyulmamýþ veya Instance oluþmamýþ demektir.
                    Debug.LogError($"KRÝTÝK HATA: '{gameObject.name}' kasaya gidemiyor çünkü CounterPoint YOK! WorldManager sahnede mi?");
                }
                break;

            case CustomerState.AtCounter:
                CanInteract = true;
                ChangeLayer(interactableLayer);

                // --- BURAYI GÜÇLENDÝRDÝK ---
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // Fiziði tamamen öldür
                agent.ResetPath(); // Hedefi unut ki tekrar yürümeye kalkmasýn

                anim.SetBool("walk", false);

                // --- YENÝ: Kendimizi kaydettiriyoruz ---
                CustomerManager.Instance.RegisterCustomerAtCounter(this);

                // ---------------------------
                break;

            case CustomerState.Ordering:
                CanInteract = false;
                ChangeLayer(uninteractableLayer);
                break;

            case CustomerState.WaitingForFood:
                CanInteract = false;
                ChangeLayer(uninteractableLayer);
                // Not: Burada Unregister YAPMIYORUZ çünkü hala kasada bekliyor.
                break;

            case CustomerState.MovingToSeat:
                // Kasa ile iþimiz bitti, yukarýdaki 'if' bloðu zaten Unregister yapacak.
                agent.enabled = true; // Yürürken emin olalým
                agent.isStopped = false;
                agent.updateRotation = true;
                anim.SetBool("walk", true);
                break;

            case CustomerState.Eating:
                // 1. NAVMESH'Ý DEVREDEN ÇIKAR (Kritik!)
                // Agent açýk kalýrsa karakteri itmeye çalýþýr veya kaydýrýr.
                agent.isStopped = true;
                agent.enabled = false; // Tamamen kapatýyoruz ki transform kontrolü bize geçsin.

                anim.SetBool("walk", false);
                anim.SetBool("sit", true);

                // 2. OTURMA POZÝSYONUNU HESAPLA
                if (mySeatPoint != null)
                {
                    // A. Tepsiyi Masaya Býrak (Görsel olarak)
                    // (Bunu istersen ayrý bir animasyon event ile de yapabilirsin ama þimdilik buraya koyalým)
                    if (carriedTray != null)
                    {
                        PlaceTrayOnTable();
                    }

                    // B. Karakteri Yerleþtir (Mýknatýs)
                    SnapToSeat();
                }
                break;
        }
    }

    // --- YENÝ METOT: Tepsiyi Masaya Koyma ---
    private void PlaceTrayOnTable()
    {
        if (carriedTray == null) return;

        carriedTray.transform.SetParent(null);

        // ESKÝ HESAPLAMAYI SÝL:
        // Vector3 tablePos = mySeatPoint.position + ... (Bunu sil)

        // YENÝ HESAPLAMA:
        Vector3 targetPos;
        Quaternion targetRot;

        if (myTrayPoint != null)
        {
            targetPos = myTrayPoint.position;
            targetRot = myTrayPoint.rotation;
        }
        else
        {
            // Fallback (Eðer editörde unuttuysan)
            targetPos = transform.position + transform.forward * 0.5f + Vector3.up * 0.8f;
            targetRot = Quaternion.identity;
        }

        // Tepsiyi hedefe yolla
        carriedTray.transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutQuad);

        // Tepsiyi masanýn açýsýna göre döndür (Random hafif yamukluk da katabiliriz)
        carriedTray.transform.DORotateQuaternion(targetRot, 0.5f);

        carriedTray = null;
    }

    // --- YENÝ METOT: Koltuða Oturtma ---
    private void SnapToSeat()
    {
        // 1. Hedef Pozisyon: Koltuk Pozisyonu + (Koltuk Yönüne Göre Offset)
        // TransformDirection kullanýyoruz ki koltuk dönerse offset de dönsün.
        Vector3 finalPos = mySeatPoint.position +
                           mySeatPoint.TransformDirection(currentProfile.SitPositionOffset);

        // 2. Hedef Rotasyon: Koltuk Rotasyonu * Offset Rotasyonu
        Quaternion finalRot = mySeatPoint.rotation * Quaternion.Euler(currentProfile.SitRotationOffset);

        // 3. DOTween ile Yumuþak Geçiþ (0.5 saniyede yerine otursun)
        transform.DOMove(finalPos, 0.5f).SetEase(Ease.OutBack); // Hafif yaylanarak otursun
        transform.DORotateQuaternion(finalRot, 0.5f).SetEase(Ease.OutQuad);
    }

    public void OnFocus()
    {
        if (!CanInteract) return;
        ChangeLayer(OutlineShouldBeRed ? interactableOutlinedRedLayer : interactableOutlinedLayer);
    }

    public void OnLoseFocus()
    {
        if (!CanInteract) return;
        ChangeLayer(interactableLayer);
    }

    public void OutlineChangeCheck()
    {
        if (meshRenderer.gameObject.layer == interactableOutlinedLayer && OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedRedLayer);
        else if (meshRenderer.gameObject.layer == interactableOutlinedRedLayer && !OutlineShouldBeRed)
            ChangeLayer(interactableOutlinedLayer);
    }

    public void ChangeLayer(int layer)
    {
        gameObject.layer = layer;
        if (meshRenderer) meshRenderer.gameObject.layer = layer;
    }

    public void AssignTable(DiningTable table)
    {
        assignedTable = table;
    }

    public void OnScareEvent()
    {
    }

    public IEnumerator EatingRoutine()
    {
        yield return new WaitForSeconds(5f);
        StartLeaving();
    }

    public void StartLeaving()
    {
        if (assignedTable != null) assignedTable.ReleaseTable();

        isLeavingShop = true;

        var exitInfo = WorldManager.Instance.GetRandomExitDoor();
        currentTargetDoor = exitInfo.targetDoor;

        agent.SetDestination(exitInfo.interactionPoint.position);

        anim.SetBool("sit", false);
        // agent.isStopped ve animasyon ayarý GoToState içinde yapýlýyor

        GoToState(CustomerState.ApproachingDoor);
    }

    private IEnumerator DoorInteractionRoutine()
    {
        isInteractingWithDoor = true;
        agent.isStopped = true;
        anim.SetBool("walk", false);

        if (currentTargetDoor != null && !currentTargetDoor.isOpened)
        {
            // 1. Hangi kolu tutacaðýmýzý hesapla ve kaydet
            activeDoorHandle = currentTargetDoor.GetBestHandle(transform.position);

            // 2. Kapýya (daha doðrusu tutacaðýmýz kola) dön
            // Kapýnýn merkezine deðil, tutacaðýmýz kola dönersek daha doðal olur.
            if (activeDoorHandle != null)
            {
                Vector3 lookTarget = activeDoorHandle.position;
                lookTarget.y = transform.position.y; // Kafayý yukarý dikmesin, gövde dönsün
                transform.DOLookAt(lookTarget, 0.3f);
            }

            // 3. Elini Uzat
            isReachingForDoor = true;
            yield return new WaitForSeconds(0.4f);

            // 4. Kapýyý Aç
            currentTargetDoor.OpenByNPC();
            yield return new WaitForSeconds(0.2f);

            // 5. Elini Ýndir
            isReachingForDoor = false;
        }

        agent.isStopped = false;

        if (isLeavingShop)
        {
            // Çýkýyoruz -> Despawn Noktasýna git
            Transform despawnPoint = WorldManager.Instance.GetSpawnPosition(); // User isteði: Spawn ve Despawn ayný
            if (despawnPoint != null)
                agent.SetDestination(despawnPoint.position);

            GoToState(CustomerState.Leaving);
        }
        else
        {
            // Giriyoruz -> Kasaya git
            // Counter point zaten null check ile GoToState içinde kullanýlýyor
            GoToState(CustomerState.Entering);
        }

        isInteractingWithDoor = false; // KÝLÝDÝ AÇ (Gerçi state deðiþtiði için gerek kalmaz ama temizlik)
    }

    // --- GÜNCELLENMÝÞ OnWrongOrderReceived (Yanlýþ Sipariþ) ---
    // Bu fonksiyonu CustomerManager çaðýrýyor.
    public void OnWrongOrderReceived()
    {
        // YENÝ SEÇÝM SÝSTEMÝ:
        DialogueData selectedDialogue = GetRandomFeedbackDialogue(currentProfile.PossibleWrongOrderDialogues);

        // Eðer zaten konuþuyorsa (bir önceki cümlesi bitmediyse) kesip yenisini mi söylesin?
        // DialogueManager.StartDialogue zaten öncekini keser.
        if (selectedDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(selectedDialogue);
        }
    }

    // --- YENÝ: Boþ Tepsi Reaksiyonu (Manager çaðýracak) ---
    public void OnEmptyTrayReceived()
    {
        DialogueData feedback = GetRandomFeedbackDialogue(currentProfile.PossibleEmptyTrayDialogues);

        if (feedback != null)
        {
            DialogueManager.Instance.StartDialogue(feedback);
        }
        else
        {
            // Eðer boþ tepsi diyaloðu yazmadýysan varsayýlan olarak yanlýþ sipariþ desin
            OnWrongOrderReceived();
        }
    }

    // --- YENÝ: Býçaklanma Reaksiyonu (Knife çaðýracak) ---
    public void OnHitByKnife()
    {
        // 1. Diyalog Baþlat
        DialogueData feedback = GetRandomFeedbackDialogue(currentProfile.PossibleKnifeHitDialogues);
        if (feedback != null)
        {
            DialogueManager.Instance.StartDialogue(feedback);
        }

        // 2. (Opsiyonel) Efekt/Animasyon
        // Buraya "Pain" animasyonu, kan efekti veya iliþki düþmesi eklenebilir.
        // anim.SetTrigger("Hit"); 

        Debug.Log($"{name}: BIÇAKLANDIM!");
    }
}