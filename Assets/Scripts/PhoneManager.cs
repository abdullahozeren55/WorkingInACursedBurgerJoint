using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    public bool CanUsePhone;

    [SerializeField] private GameObject phoneGO;
    [SerializeField] private GameObject crosshairGO;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private float timeToTakePhoneOut = 0.3f;
    [SerializeField] private float phoneOutYPos = -0.027f;
    [SerializeField] private float phoneInYPos = -0.33f;
    
    private RectTransform crosshairRectTransform;
    private FirstPersonController firstPersonController;
    private KeyCode phoneKey = KeyCode.Tab;
    private bool isPhoneOut;

    private Vector3 phoneOutPos;
    private Vector3 phoneInPos;
    private Vector3 crosshairStartPos;

    private Vector2 mousePosWorkspace;
    private Vector2 localPosWorkspace;

    private Coroutine phoneInOutCoroutine;

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

        firstPersonController = FindFirstObjectByType<FirstPersonController>();
        crosshairRectTransform = crosshairGO.GetComponent<RectTransform>();

        isPhoneOut = false;

        phoneOutPos = new Vector3 (phoneGO.transform.localPosition.x, phoneOutYPos, phoneGO.transform.localPosition.z);
        phoneInPos = new Vector3 (phoneOutPos.x, phoneInYPos, phoneOutPos.z);
        crosshairStartPos = crosshairRectTransform.localPosition;

        phoneInOutCoroutine = null;
        
        phoneGO.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(phoneKey) && CanUsePhone)
        {
            if (!isPhoneOut && firstPersonController.CanMove)
            {
                TakePhoneOut();
            }
            else
            {
                PutPhoneBack();
            }
        }
        else if (isPhoneOut)
        {
            mousePosWorkspace = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(crosshairRectTransform.parent.GetComponent<RectTransform>(), mousePosWorkspace, null, out localPosWorkspace);
            crosshairRectTransform.localPosition = localPosWorkspace;
        }
    }

    private void TakePhoneOut()
    {
        firstPersonController.CanMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        isPhoneOut = true;

        if (!phoneGO.activeSelf)
        {
            phoneGO.SetActive(true);
            SetTime();
        }

        if (phoneInOutCoroutine != null) StopCoroutine(phoneInOutCoroutine);

        phoneInOutCoroutine = StartCoroutine(PhoneGoesOut());
    }

    private void PutPhoneBack()
    {
        isPhoneOut = false;

        if (phoneInOutCoroutine != null) StopCoroutine(phoneInOutCoroutine);

        phoneInOutCoroutine = StartCoroutine(PhoneGoesIn());
    }

    private void SetTime()
    {
        DateTime currentTime = DateTime.Now;

        // Extract the hour and minute
        int hour = currentTime.Hour;
        int minute = currentTime.Minute;

        string formattedTime = string.Format("{0:D2}:{1:D2}", hour, minute);
        timeText.text = formattedTime;
    }

    private IEnumerator PhoneGoesOut()
    {
        Vector3 startPos = phoneGO.transform.localPosition;

        float elapsedTime = 0f;

        while (elapsedTime < timeToTakePhoneOut)
        {
            phoneGO.transform.localPosition = Vector3.Lerp(startPos, phoneOutPos, elapsedTime / timeToTakePhoneOut);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phoneGO.transform.localPosition = phoneOutPos;

        phoneInOutCoroutine = null;
    }

    private IEnumerator PhoneGoesIn()
    {
        Vector3 startPos = phoneGO.transform.localPosition;
        Vector3 crosshairPos = crosshairRectTransform.localPosition;

        float elapsedTime = 0f;

        while (elapsedTime < timeToTakePhoneOut)
        {
            phoneGO.transform.localPosition = Vector3.Lerp(startPos, phoneInPos, elapsedTime / timeToTakePhoneOut);
            crosshairRectTransform.localPosition = Vector3.Lerp(crosshairPos, crosshairStartPos, elapsedTime / timeToTakePhoneOut);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phoneGO.transform.localPosition = phoneInPos;
        crosshairRectTransform.localPosition = crosshairStartPos;

        phoneGO.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        firstPersonController.CanMove = true;

        phoneInOutCoroutine = null;
    }
}
