using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Back")]
    [SerializeField] private Button backButton;

    [Header("Sound")]
    [SerializeField] private Toggle soundToggle;

    [Header("Reset Progress")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private GameObject resetProgressConfirmPanel;
    [SerializeField] private Button resetProgressConfirmYesButton;
    [SerializeField] private Button resetProgressConfirmNoButton;

    [Header("Quit")]
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject quitConfirmPanel;
    [SerializeField] private Button quitConfirmYesButton;
    [SerializeField] private Button quitConfirmNoButton;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (soundToggle != null)
        {
            soundToggle.isOn = SoundSettings.IsSoundEnabled();
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        }

        if (resetProgressButton != null)
        {
            resetProgressButton.onClick.AddListener(OnResetProgressClicked);
        }

        if (resetProgressConfirmYesButton != null)
        {
            resetProgressConfirmYesButton.onClick.AddListener(OnResetProgressConfirmed);
        }

        if (resetProgressConfirmNoButton != null)
        {
            resetProgressConfirmNoButton.onClick.AddListener(OnResetProgressCancelled);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (quitConfirmYesButton != null)
        {
            quitConfirmYesButton.onClick.AddListener(OnQuitConfirmed);
        }

        if (quitConfirmNoButton != null)
        {
            quitConfirmNoButton.onClick.AddListener(OnQuitCancelled);
        }

        HideResetProgressConfirm();
        HideQuitConfirm();
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        SoundSettings.SetSoundEnabled(isOn);
    }

    public void OnResetProgressClicked()
    {
        if (resetProgressConfirmPanel != null)
        {
            resetProgressConfirmPanel.SetActive(true);
        }
    }

    private void OnResetProgressConfirmed()
    {
        ProgressData.ResetProgress();
        HideResetProgressConfirm();
    }

    private void OnResetProgressCancelled()
    {
        HideResetProgressConfirm();
    }

    private void HideResetProgressConfirm()
    {
        if (resetProgressConfirmPanel != null)
        {
            resetProgressConfirmPanel.SetActive(false);
        }
    }

    public void OnQuitClicked()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(true);
        }
    }

    private void OnQuitConfirmed()
    {
        Debug.Log("SettingsController: Quit confirmed, calling Application.Quit().");
        Application.Quit();
    }

    private void OnQuitCancelled()
    {
        HideQuitConfirm();
    }

    private void HideQuitConfirm()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
        }
    }
}
