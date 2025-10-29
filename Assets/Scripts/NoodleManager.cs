using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoodleManager : MonoBehaviour
{
    public static NoodleManager Instance;

    public enum NoodleStatus
    {
        JustGrabbed,
        OnStoreHologram,
        JustBought,
        ReadyToPrepare,
        LidOpened,
        SaucePackInstantiated,
        OnHouseHologram,
        FilledWithWater,
        SauceAdded,
        WaitingToBeReady,
        Ready,
        Finished
    }
    [Header("Holograms")]
    [SerializeField] private GameObject hologramHouseNoodle;
    [SerializeField] private GameObject hologramKettle;
    [SerializeField] private GameObject hologramSaucePack;

    [Header("GameObjects")]
    [SerializeField] private GameObject kettleGO;

    [Header("Noodle Water Settings")]
    private Vector3 waterStartPos;
    public Vector3 waterEndPos;
    private Vector3 waterStartScale;
    public Vector3 waterEndScale;
    public int requiredDrops = 200;
    private int currentDrops = 0;
    [Space]
    [SerializeField] private Color saucedWaterColor;
    [SerializeField] private float timeToSauceWater = 0.3f;

    [HideInInspector] public GameObject currentNoodleGO;
    [HideInInspector] public GameObject currentSmokeGO;
    [HideInInspector] public GameObject currentWaterGO;
    [HideInInspector] public GameObject currentSaucePackGO;
    [HideInInspector] public NoodleStatus currentNoodleStatus;
    private Noodle currentNoodleScript;
    private SaucePack currentSaucePackScript;

    private bool saucePackIsOnHologram;

    private Material currentWaterMat;
    private Collider currentNoodleCollider;

    private Material hologramHouseNoodleMat;
    private Material hologramSaucePackMat;

    private Color hologramHouseNoodleMatDefaultColor;
    private Color hologramSaucePackMatDefaultColor;

    private int grabableLayer;
    private int grabableOutlinedLayer;
    private int ungrabableLayer;
    private int interactableLayer;

    private void Awake()
    {
        if (Instance == null)
        {
            // If not, set this instance as the singleton
            Instance = this;

            // Optionally, mark GameManager as not destroyed between scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one to enforce the singleton pattern
            Destroy(gameObject);
        }

        grabableLayer = LayerMask.NameToLayer("Grabable");
        grabableOutlinedLayer = LayerMask.NameToLayer("GrabableOutlined");
        ungrabableLayer = LayerMask.NameToLayer("Ungrabable");
        interactableLayer = LayerMask.NameToLayer("Interactable");

        hologramHouseNoodleMat = hologramHouseNoodle.GetComponent<Renderer>().material;
        hologramSaucePackMat = hologramSaucePack.GetComponent<Renderer>().material;

        hologramHouseNoodleMatDefaultColor = hologramHouseNoodleMat.color;
        hologramSaucePackMatDefaultColor = hologramSaucePackMat.color;

        SetHologramHouseNoodle(false);
        SetHologramSaucePack(false);
    }

    public void SetKettleGrabable()
    {
        kettleGO.layer = grabableLayer;
    }

    private void TrySettingKettleGrabable()
    {
        if (saucePackIsOnHologram && currentNoodleScript.NoodleStatus == NoodleStatus.OnHouseHologram)
            SetKettleGrabable();
    }

    public void SetHologramHouseNoodle(bool shouldTurnOn)
    {
        hologramHouseNoodleMat.color = shouldTurnOn ? hologramHouseNoodleMatDefaultColor : Color.clear;
    }

    public void SetHologramSaucePack(bool shouldTurnOn)
    {
        hologramSaucePackMat.color = shouldTurnOn ? hologramSaucePackMatDefaultColor : Color.clear;
    }

    public void PutCurrentNoodleOnHologramHouse()
    {
        if (currentNoodleScript.NoodleStatus == NoodleStatus.SaucePackInstantiated)
        {
            currentNoodleScript.PutOnHologram(hologramHouseNoodle.transform.position, hologramHouseNoodle.transform.rotation);
            TrySettingKettleGrabable();
        }
    }

    public void PutCurrentSaucePackOnHologram()
    {
        currentSaucePackScript.PutOnHologram(hologramSaucePack.transform.position, hologramSaucePack.transform.rotation);
        saucePackIsOnHologram = true;
        TrySettingKettleGrabable();
    }

    public void SetCurrentNoodle(GameObject noodle)
    {
        currentNoodleGO = noodle;
        currentSmokeGO = noodle.transform.Find("Smoke").gameObject;
        currentWaterGO = noodle.transform.Find("Water").gameObject;

        currentNoodleScript = noodle.GetComponent<Noodle>();

        currentWaterMat = currentWaterGO.GetComponent<MeshRenderer>().material;
        currentNoodleCollider = currentNoodleGO.GetComponent<Collider>();

        waterStartPos = currentWaterGO.transform.localPosition;
        waterStartScale = currentWaterGO.transform.localScale;
    }

    public void SetCurrentSaucePack(GameObject saucePack)
    {
        currentSaucePackGO = saucePack;
        currentSaucePackScript = saucePack.GetComponent<SaucePack>();
    }

    public void AddWaterToNoodle()
    {
        currentDrops++;
        float t = Mathf.Clamp01((float)currentDrops / requiredDrops);

        currentWaterGO.transform.localPosition = Vector3.Lerp(waterStartPos, waterEndPos, t);
        currentWaterGO.transform.localScale = Vector3.Lerp(waterStartScale, waterEndScale, t);
        
        

        if (t > 0.5f)
        {
            currentSmokeGO.SetActive(true);
        }

        if (t > 0.9f)
        {
            hologramKettle.SetActive(true);
        }
    }

    public void AddSauceToWater()
    {
        currentNoodleGO.layer = interactableLayer;
        currentNoodleCollider.enabled = true;

        StartCoroutine(LerpColor());
    }

    private IEnumerator LerpColor()
    {
        Color startColor = currentWaterMat.color;

        float elapsedTime = 0f;

        while (elapsedTime < timeToSauceWater)
        {
            currentWaterMat.color = Color.Lerp(startColor, saucedWaterColor, elapsedTime / timeToSauceWater);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentWaterMat.color = saucedWaterColor;
    }

}
