using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("Back")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button backToMenuButton;

    [Header("Audio - Sound Effects")]
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Image sfxToggleTrack;
    [SerializeField] private RectTransform sfxToggleHandle;
    [SerializeField] private TextMeshProUGUI sfxOnOffText;

    [Header("Audio - Music")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Image musicToggleTrack;
    [SerializeField] private RectTransform musicToggleHandle;
    [SerializeField] private TextMeshProUGUI musicOnOffText;

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

    // Toggle-on and toggle-off tinting, matching BRAND.md: teal is the calm/
    // positive/enabled color, the muted slate is used for the disabled state.
    private static readonly Color ToggleOnColor = HexColor("4FD1C5");
    private static readonly Color ToggleOffColor = HexColor("34435F");
    private static readonly Color OnOffTextOnColor = HexColor("4FD1C5");
    private static readonly Color OnOffTextOffColor = HexColor("8B93A7");

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackClicked);
        }

        SetupToggle(sfxToggle, sfxToggleTrack, sfxToggleHandle, sfxOnOffText,
            SoundSettings.IsSfxEnabled(), SoundSettings.SetSfxEnabled);

        SetupToggle(musicToggle, musicToggleTrack, musicToggleHandle, musicOnOffText,
            SoundSettings.IsMusicEnabled(), SoundSettings.SetMusicEnabled);

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

    // Wires a toggle to its persisted preference and its own track/handle/
    // ON-OFF-text visuals. The Toggle component only tracks isOn state and
    // raycast/click handling here, since Image.Type.Sliced distorts these
    // pre-baked track and handle sprites (see BACKLOG.md item 30) - both
    // are plain Type.Simple sprites moved and tinted directly instead of
    // relying on Toggle's built-in checkmark graphic.
    private void SetupToggle(Toggle toggle, Image track, RectTransform handle, TextMeshProUGUI onOffText, bool initialOn, System.Action<bool> persist)
    {
        if (toggle == null)
        {
            return;
        }

        toggle.SetIsOnWithoutNotify(initialOn);
        ApplyToggleVisual(track, handle, onOffText, initialOn);

        toggle.onValueChanged.AddListener(isOn =>
        {
            persist(isOn);
            ApplyToggleVisual(track, handle, onOffText, isOn);
        });
    }

    private void ApplyToggleVisual(Image track, RectTransform handle, TextMeshProUGUI onOffText, bool isOn)
    {
        if (track != null)
        {
            track.color = isOn ? ToggleOnColor : ToggleOffColor;
        }

        if (handle != null)
        {
            float trackHalfWidth = track != null ? track.rectTransform.rect.width / 2f : 50f;
            float handleRadius = handle.rect.width / 2f;
            const float padding = 6f;
            float throwX = trackHalfWidth - padding - handleRadius;
            handle.anchoredPosition = new Vector2(isOn ? throwX : -throwX, handle.anchoredPosition.y);
        }

        if (onOffText != null)
        {
            onOffText.text = isOn ? "ON" : "OFF";
            onOffText.color = isOn ? OnOffTextOnColor : OnOffTextOffColor;
        }
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

    private static Color HexColor(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + hex, out c);
        return c;
    }
}
