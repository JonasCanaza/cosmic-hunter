using UnityEngine;

public class MobileUIAdapter : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Editor Testing")]
    [SerializeField] private bool simulateMobileInEditor = false;
#endif

    [Header("Panels Configuration")]
    [SerializeField] private GameObject[] mobilePanels;
    [SerializeField] private GameObject[] pcPanels;

    private void Awake()
    {
        UpdatePlatformUI();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdatePlatformUI();
        }
    }
#endif

    private void UpdatePlatformUI()
    {
        bool isMobile = false;

#if UNITY_EDITOR
        isMobile = simulateMobileInEditor;
#elif UNITY_ANDROID
        isMobile = true;
#else
        isMobile = false;
#endif

        ApplyPlatformUI(isMobile);
    }

    private void ApplyPlatformUI(bool isMobile)
    {
        SetPanelsActive(mobilePanels, isMobile);
        SetPanelsActive(pcPanels, !isMobile);
    }

    private void SetPanelsActive(GameObject[] panels, bool isActive)
    {
        if (panels == null)
        {
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(isActive);
            }
        }
    }
}