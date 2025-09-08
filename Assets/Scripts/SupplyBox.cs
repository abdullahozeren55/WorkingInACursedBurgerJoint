using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cookable;

public class SupplyBox : MonoBehaviour, IGrabable
{
    public Transform LeftHandFixPoint { get => leftHandFixPoint; set => leftHandFixPoint = value; }
    [SerializeField] private Transform leftHandFixPoint;
    public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }
    private bool isGrabbed;

    public GameManager.GrabTypes GrabType { get => grabType; set => grabType = value; }
    [SerializeField] private GameManager.GrabTypes grabType;

    public float HandLerp { get => handLerp; set => handLerp = value; }
    [SerializeField] private float handLerp;

    public SupplyBoxData data;

    [SerializeField] private GameObject grabText;
    [SerializeField] private GameObject dropText;
    [Space]
    

    [Header("Supply Settings")]
    [SerializeField] private GameObject supplyToSpawn;
    [SerializeField] private Transform spawnAreaCenter;
    [SerializeField] private bool shouldRandomRotate;

    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;

    private int grabableLayer;
    private int grabableOutlinedLayer;
    private int ungrabableLayer;

    private bool isJustThrowed;

    private float audioLastPlayedTime;
    private float distance;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        grabableLayer = LayerMask.NameToLayer("Grabable");
        grabableOutlinedLayer = LayerMask.NameToLayer("GrabableOutlined");
        ungrabableLayer = LayerMask.NameToLayer("Ungrabable");

        IsGrabbed = false;

        isJustThrowed = false;

        audioLastPlayedTime = 0f;
    }

    public void OnGrab(Transform grabPoint)
    {
        gameObject.layer = ungrabableLayer;

        PlayAudioWithRandomPitch(0);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        IsGrabbed = true;

        HandleText(true);

        transform.SetParent(grabPoint);
        transform.position = grabPoint.position;
        transform.localPosition = data.grabPositionOffset;
        transform.localRotation = Quaternion.Euler(data.grabRotationOffset);
    }
    public void OnFocus()
    {
        HandleText(true);
        gameObject.layer = grabableOutlinedLayer;
    }
    public void OnLoseFocus()
    {
        HandleText(false);
        gameObject.layer = grabableLayer;
    }

    public void OnDrop(Vector3 direction, float force)
    {
        gameObject.layer = grabableLayer;

        IsGrabbed = false;

        transform.SetParent(null);

        rb.useGravity = true;

        rb.AddForce(direction * force / data.weight, ForceMode.Impulse);
    }

    public void OnThrow(Vector3 direction, float force)
    {
        gameObject.layer = grabableLayer;

        IsGrabbed = false;

        transform.SetParent(null);

        rb.useGravity = true;

        rb.AddForce(direction * force / data.weight, ForceMode.Impulse);

        isJustThrowed = true;
    }

    public void OpenBox()
    {
        gameObject.layer = ungrabableLayer;

        GameObject instantiatedObject = Instantiate(supplyToSpawn, spawnAreaCenter.position, Quaternion.identity);

        // Traverse all the children of the instantiated object
        foreach (Transform child in instantiatedObject.transform)
        {
            if (shouldRandomRotate)
                child.localRotation = Random.rotation;
            // Check if the child has a Rigidbody component
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Set useGravity to true for this Rigidbody
                rb.useGravity = true;
            }
        }

        Instantiate(data.destroyParticle, transform.position, Quaternion.Euler(transform.rotation.x - 90f, transform.rotation.y + 90f, transform.rotation.z + 90f));

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        HandleText(false);
        GameManager.Instance.ResetPlayerGrab(this);
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
            grabText.SetActive(false);
            dropText.SetActive(false);
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
        if (!IsGrabbed && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Door")))
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
}
