using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutSaucePack : MonoBehaviour, IInteractable
{
    [Header("Audio Settings")]
    public AudioClip putInSound;
    private AudioSource audioSource;

    [Header("Text Settings")]
    public GameObject putInText;

    [Header("Layer Settings")]
    private int interactableLayer;
    private int interactableOutlinedLayer;

    public GameObject currentWater;
    [SerializeField] private Color targetWaterColor;
    [SerializeField] private float colorLerpTime = 0.3f;

    private Collider col;
    private MeshRenderer meshRenderer;

    public GameManager.HandRigTypes HandRigType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Awake()
    {
        col = GetComponent<Collider>();
        meshRenderer = col.GetComponent<MeshRenderer>();

        audioSource = GetComponent<AudioSource>();

        interactableLayer = LayerMask.NameToLayer("Interactable");
        interactableOutlinedLayer = LayerMask.NameToLayer("InteractableOutlined");

    }

    public void OnFocus()
    {
        putInText.SetActive(true);
        gameObject.layer = interactableOutlinedLayer;
    }

    public void OnInteract()
    {
        col.enabled = false;
        meshRenderer.enabled = false;
        GameManager.Instance.ResetPlayerGrabAndInteract();

        Material currentWaterMat = currentWater.GetComponent<MeshRenderer>().material;

        StartCoroutine(LerpColor(currentWaterMat));

        audioSource.PlayOneShot(putInSound);

        Invoke("TurnOffObject", putInSound.length);

    }

    public void OnLoseFocus()
    {
        putInText.SetActive(false);
        gameObject.layer = interactableLayer;
    }

    private void TurnOffObject()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator LerpColor(Material mat)
    {
        Color startColor = mat.color;

        float elapsedTime = 0f;

        while (elapsedTime < colorLerpTime)
        {
            mat.color = Color.Lerp(startColor, targetWaterColor, elapsedTime/colorLerpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mat.color = targetWaterColor;
    }
}
