using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public enum CameraName
    {
        Null,
        FirstPerson,
        Monitor,
        Customer0
    }

    [System.Serializable]
    public class CameraEntry
    {
        public CameraName camName;
        public CinemachineVirtualCamera vCam;
    }

    [SerializeField] private CursorFollow cursorFollow;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private Monitor monitor;
    [Space]
    [SerializeField] private CameraEntry[] cameras;

    private FirstPersonController firstPersonController;
    private KeyCode interactKey;
    private bool isFocusedOnMonitor;
    private bool isFocused;

    private CameraEntry currentCam;
    private int basePriority = 10;

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
        interactKey = firstPersonController.throwKey;

        isFocusedOnMonitor = false;
        isFocused = false;
    }

    private void Update()
    {
        if ((isFocusedOnMonitor || isFocused) && !DialogueManager.Instance.IsInDialogue)
        {
            CheckDefocus();
        }
    }

    public void SwitchToCamera(CameraName name)
    {
        if (name == currentCam.camName || name == CameraName.Null)
            return;

        // Lower priority of current camera
        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == name)
            {
                currentCam.vCam.Priority = basePriority;
                currentCam = entry;
                currentCam.vCam.Priority = basePriority + 1;

                break;
            }
        }
    }

    public void InitializeCamera(CameraName name)
    {
        if (currentCam != null)
            return;

        foreach (CameraEntry entry in cameras)
        {
            if (entry.camName == name)
            {
                currentCam = entry;
                currentCam.vCam.Priority = basePriority + 1;

                break;
            }
        }
    }

    public CinemachineVirtualCamera GetCamera()
    {
        return currentCam.vCam;
    }

    public void SwitchToFirstPersonCamera()
    {
        SwitchToCamera(CameraName.FirstPerson);
    }

    private void CheckDefocus()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (isFocused)
                DefocusCameraForPhone();
            else
                DefocusCameraForMonitor();
            
        }
    }

    public void FocusCameraForMonitor()
    {
        if (firstPersonController.CanMove) firstPersonController.CanMove = false;
        crosshair.SetActive(false);

        PhoneManager.Instance.CanUsePhone = false;

        SwitchToCamera(CameraName.Monitor);

        Invoke("FinishMonitorFocus", 0.5f);
    }

    private void FinishMonitorFocus()
    {
        isFocusedOnMonitor = true;

        cursorFollow.StartCursorFollow();

        monitor.defocusText.SetActive(true);
    }

    private void DefocusCameraForMonitor()
    {
        cursorFollow.EndCursorFollow();
        monitor.defocusText.SetActive(false);
        SwitchToCamera(CameraName.FirstPerson);

        if (!firstPersonController.CanMove) firstPersonController.CanMove = true;
        crosshair.SetActive(true);

        PhoneManager.Instance.CanUsePhone = true;

        isFocusedOnMonitor = false;
    }

    private void FinishMonitorDefocus()
    {
        crosshair.SetActive(true);
        monitor.ChangeLayerToInteractable();

        if (!firstPersonController.CanMove) firstPersonController.CanMove = true;
        PhoneManager.Instance.CanUsePhone = true;

        isFocusedOnMonitor = false;
    }

    public void FocusCameraForPhone(Camera camera)
    {
        if (firstPersonController.CanMove) firstPersonController.CanMove = false;

        PhoneManager.Instance.CanUsePhone = false;

        //SwitchToCamera(phoneCam);

        Invoke("FinishPhoneFocus", 1f);
    }

    private void FinishPhoneFocus()
    {
        isFocused = true;
    }

    private void DefocusCameraForPhone()
    {
        SwitchToCamera(CameraName.FirstPerson);

        if (!firstPersonController.CanMove) firstPersonController.CanMove = true;
        PhoneManager.Instance.CanUsePhone = true;

        isFocused = false;
    }
}
