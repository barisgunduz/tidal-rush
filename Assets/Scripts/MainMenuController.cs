using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Continue / Play")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Text continueButtonLabel;

    [Header("Settings")]
    [SerializeField] private Button settingsButton;

    [Header("Quit")]
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject quitConfirmPanel;
    [SerializeField] private Button quitConfirmYesButton;
    [SerializeField] private Button quitConfirmNoButton;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
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

        HideQuitConfirm();
        UpdateContinueLabel();
    }

    // Fresh install has no saved progress beyond level 1, so the button
    // reads "PLAY" then. Any progress past level 1 switches it to "CONTINUE".
    // Uppercase to match the mockup's bold uppercase button typography.
    private void UpdateContinueLabel()
    {
        if (continueButtonLabel == null)
        {
            return;
        }

        bool hasProgress = ProgressData.GetUnlockedLevel() > 1;
        continueButtonLabel.text = hasProgress ? "CONTINUE" : "PLAY";
    }

    public void OnContinueClicked()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void OnSettingsClicked()
    {
        SceneManager.LoadScene("Settings");
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
        Debug.Log("MainMenuController: Quit confirmed, calling Application.Quit().");
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
