using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cookable;

public class Noodle : MonoBehaviour, IGrabable
{
    public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }
    private bool isGrabbed;

    public GameManager.GrabTypes GrabType { get => grabType; set => grabType = value; }
    [SerializeField] private GameManager.GrabTypes grabType;

    public float HandLerp { get => handLerp; set => handLerp = value; }
    [SerializeField] private float handLerp;

    public bool IsGettingPutOnTrash { get => isGettingPutOnTrash; set => isGettingPutOnTrash = value; }
    private bool isGettingPutOnTrash;

    public bool IsGettingPutOnContainer { get => isGettingPutOnContainer; set => isGettingPutOnContainer = value; }
    private bool isGettingPutOnContainer;

    public bool IsGettingPutOnHologram { get => isGettingPutOnHologram; set => isGettingPutOnHologram = value; }
    private bool isGettingPutOnHologram;

    public NoodleData data;

    [SerializeField] private GameObject[] childObjects;
    [SerializeField] private GameObject hologramPart;
    [SerializeField] private GameObject hologramStorePart;
    [SerializeField] private GameObject grabText;
    [SerializeField] private GameObject dropText;
    [SerializeField] private OutSaucePack outSaucePack;
    [SerializeField] private GameObject saucePack;
    [SerializeField] private GameObject water;
    [SerializeField] private Kettle kettle;
    [Space]
    [SerializeField] private Vector3 grabPositionOffset;
    [SerializeField] private Vector3 grabRotationOffset;

    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;
    private Collider[] childColliders;
    private Renderer hologramRenderer;
    private Renderer hologramStoreRenderer;
    private Animator anim;

    private int grabableLayer;
    private int grabableOutlinedLayer;
    private int ungrabableLayer;
    private int onTrashLayer;
    private int onContainerLayer;

    private int interactableLayer;

    private Vector3 trashPos;
    private Vector3 containerPos;
    private Vector3 hologramPos;
    private Quaternion hologramRotation;

    private bool isJustThrowed;

    private Coroutine putOnTrashCoroutine;
    private Coroutine putOnContainerCoroutine;
    private Coroutine putOnHologramCoroutine;

    private float audioLastPlayedTime;
    private float distance;

    private Quaternion targetRotation;
    private float rotationDistance;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        hologramRenderer = hologramPart.GetComponent<Renderer>();
        hologramStoreRenderer = hologramStorePart.GetComponent<Renderer>();
        anim = GetComponent<Animator>();

        foreach (Material material in hologramRenderer.materials)
        {
            Color color = material.color;

            color.a = 0f;

            material.color = color;
        }

        foreach (Material material in hologramStoreRenderer.materials)
        {
            Color color = material.color;

            color.a = 0f;

            material.color = color;
        }

        childColliders = new Collider[childObjects.Length];

        for (int i = 0; i < childObjects.Length; i++)
        {
            childColliders[i] = childObjects[i].GetComponent<Collider>();
        }

        grabableLayer = LayerMask.NameToLayer("Grabable");
        grabableOutlinedLayer = LayerMask.NameToLayer("GrabableOutlined");
        ungrabableLayer = LayerMask.NameToLayer("Ungrabable");
        onTrashLayer = LayerMask.NameToLayer("OnTrash");
        onContainerLayer = LayerMask.NameToLayer("OnContainer");
        interactableLayer = LayerMask.NameToLayer("Interactable");

        IsGrabbed = false;
        IsGettingPutOnTrash = false;
        IsGettingPutOnContainer = false;
        IsGettingPutOnHologram = false;

        isJustThrowed = false;

        audioLastPlayedTime = 0f;
    }

    public void PutOnTrash(Vector3 trashPos, Transform parentTrash)
    {

        IsGettingPutOnTrash = true;
        gameObject.layer = onTrashLayer;
        ChangeChildLayers(onTrashLayer);

        PlayAudioWithRandomPitch(1);

        IsGrabbed = false;
        HandleText(false);

        transform.parent = parentTrash;

        this.trashPos = trashPos;

        putOnTrashCoroutine = StartCoroutine(PutOnTrash());
    }

    public void PutOnContainer(Vector3 containerPos, Transform parentContainer)
    {

        IsGettingPutOnContainer = true;
        gameObject.layer = onContainerLayer;
        ChangeChildLayers(onContainerLayer);

        PlayAudioWithRandomPitch(1);

        IsGrabbed = false;
        HandleText(false);

        transform.parent = parentContainer;

        this.containerPos = containerPos;

        putOnContainerCoroutine = StartCoroutine(PutOnContainer());
    }

    public void PutOnHologram(Vector3 hologramPos, Quaternion hologramRotation, bool isStoreHologram)
    {
        IsGettingPutOnHologram = true;

        audioSource.enabled = false;

        gameObject.layer = ungrabableLayer;
        ChangeChildLayers(ungrabableLayer);

        IsGrabbed = false;
        HandleText(false);

        if (isStoreHologram)
            hologramStorePart.SetActive(false);
        else
            hologramPart.SetActive(false);

        this.hologramPos = hologramPos;
        this.hologramRotation = hologramRotation;

        putOnHologramCoroutine = StartCoroutine(PutOnHologram(isStoreHologram));
    }

    public void OnGrab(Transform grabPoint)
    {
        gameObject.layer = ungrabableLayer;

        audioSource.enabled = true;

        PlayAudioWithRandomPitch(0);

        targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        ChangeChildLayers(ungrabableLayer);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        foreach (Material material in hologramRenderer.materials)
        {
            Color color = material.color;

            color.a = 40f/255f;

            material.color = color;
        }

        foreach (Material material in hologramStoreRenderer.materials)
        {
            Color color = material.color;

            color.a = 40f / 255f;

            material.color = color;
        }

        IsGrabbed = true;

        HandleText(true);

        transform.SetParent(grabPoint);
        transform.position = grabPoint.position;
        transform.localPosition = grabPositionOffset;
        transform.localRotation = Quaternion.Euler(grabRotationOffset);
    }
    public void OnFocus()
    {
        if (!isGettingPutOnTrash && !IsGettingPutOnContainer && !IsGettingPutOnHologram)
        {
            HandleText(true);
            gameObject.layer = grabableOutlinedLayer;

            ChangeChildLayers(grabableOutlinedLayer);
        }

    }
    public void OnLoseFocus()
    {
        if (!isGettingPutOnTrash && !IsGettingPutOnContainer && !IsGettingPutOnHologram)
        {
            rb.isKinematic = false;

            HandleText(false);
            gameObject.layer = grabableLayer;

            ChangeChildLayers(ungrabableLayer);
        }

    }

    public void OnDrop(Vector3 direction, float force)
    {
        IsGrabbed = false;

        transform.SetParent(null);

        foreach (Material material in hologramRenderer.materials)
        {
            Color color = material.color;

            color.a = 0f;

            material.color = color;
        }

        foreach (Material material in hologramStoreRenderer.materials)
        {
            Color color = material.color;

            color.a = 0f;

            material.color = color;
        }

        rb.useGravity = true;

        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void OnThrow(Vector3 direction, float force)
    {
        IsGrabbed = false;

        transform.SetParent(null);

        foreach (Material material in hologramRenderer.materials)
        {
            Color color = material.color;

            color.a = 0f;

            material.color = color;
        }

        foreach (Material material in hologramStoreRenderer.materials)
        {
            Color color = material.color;

            color.a = 0f;

            material.color = color;
        }

        rb.useGravity = true;

        rb.AddForce(direction * force, ForceMode.Impulse);

        isJustThrowed = true;
    }

    private void HandleText(bool isFocused)
    {
        if (isFocused)
        {
            grabText.SetActive(!IsGrabbed);
            dropText.SetActive(IsGrabbed);
        }
        else
        {
            if (grabText.activeSelf) grabText.SetActive(false);
            if (dropText.activeSelf) dropText.SetActive(false);
        }
    }

    private void ChangeChildLayers(int layer)
    {
        for (int i = 0; i < childObjects.Length; i++)
        {
            childObjects[i].layer = layer;
        }
    }

    private void PlayAudioWithRandomPitch(int index)
    {
        audioLastPlayedTime = Time.time;
        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.PlayOneShot(data.audioClips[index]);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsGrabbed && !IsGettingPutOnTrash && !IsGettingPutOnContainer && !IsGettingPutOnHologram && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Door") || collision.gameObject.CompareTag("Customer")))
        {
            if (isJustThrowed)
            {

                PlayAudioWithRandomPitch(2);

                isJustThrowed = false;
            }
            else if (Time.time > audioLastPlayedTime + 0.1f)
            {
                PlayAudioWithRandomPitch(1);
            }

        }


    }

    private IEnumerator PutOnTrash()
    {
        rb.isKinematic = true;

        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Random.rotation;

        float timeElapsed = 0f;
        float rate = 0f;

        while (timeElapsed < data.timeToPutOnTray)
        {
            rate = timeElapsed / data.timeToPutOnTray;

            transform.position = Vector3.Lerp(startPos, trashPos, rate);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rate);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = trashPos;
        transform.rotation = targetRotation;

        rb.isKinematic = false;
        rb.useGravity = true;

        putOnTrashCoroutine = null;
    }

    private IEnumerator PutOnContainer()
    {
        rb.isKinematic = true;

        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Random.rotation;

        float timeElapsed = 0f;
        float rate = 0f;

        while (timeElapsed < data.timeToPutOnTray)
        {

            rate = timeElapsed / data.timeToPutOnTray;

            transform.position = Vector3.Lerp(startPos, containerPos, rate);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rate);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = containerPos;
        transform.rotation = targetRotation;

        rb.isKinematic = false;
        rb.useGravity = true;

        putOnContainerCoroutine = null;
    }

    private IEnumerator PutOnHologram(bool isStoreHologram)
    {
        rb.isKinematic = true;

        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;

        float timeElapsed = 0f;
        float rate = 0f;

        while (timeElapsed < data.timeToPutOnTray)
        {

            rate = timeElapsed / data.timeToPutOnTray;

            transform.localPosition = Vector3.Lerp(startPos, hologramPos, rate);
            transform.localRotation = Quaternion.Slerp(startRotation, hologramRotation, rate);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = hologramPos;
        transform.localRotation = hologramRotation;

        if (!isStoreHologram)
        {
            col.enabled = false;
            saucePack.GetComponent<Collider>().enabled = true;

            outSaucePack.currentWater = water;
            kettle.currentNoodle = gameObject.GetComponent<NoodleInteractable>();

            gameObject.layer = interactableLayer;
            ChangeChildLayers(interactableLayer);
        } 

        IsGettingPutOnHologram = false;

        putOnHologramCoroutine = null;
    }
}
