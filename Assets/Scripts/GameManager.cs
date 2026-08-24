using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    [Header("Level Data")]
    // Testing hook: set which level to load in the Inspector. A real level
    // select screen will replace this later.
    public int currentLevel = 1;
    [SerializeField] private LevelData[] levelDatabase;

    [Header("Grid")]
    [SerializeField] private float cellSpacing = 0.15f;
    [SerializeField, Range(0f, 0.3f)] private float horizontalMarginRatio = 0.08f;
    [SerializeField, Range(0f, 0.4f)] private float verticalMarginRatio = 0.1f;

    [Header("Portrait Reference Frame (9:16)")]
    [SerializeField] private float referenceAspectWidth = 9f;
    [SerializeField] private float referenceAspectHeight = 16f;

    [Header("Match Rules")]
    [SerializeField] private float mismatchFlipBackDelay = 0.7f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI matchesText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Win/Fail UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelCompleteTimeText;
    [SerializeField] private TextMeshProUGUI levelCompleteLevelText;
    [SerializeField] private TextMeshProUGUI levelCompleteMatchesText;
    [SerializeField] private Image levelCompleteStar1;
    [SerializeField] private Image levelCompleteStar2;
    [SerializeField] private Image levelCompleteStar3;
    [SerializeField] private Sprite starFilledSprite;
    [SerializeField] private Sprite starOutlineSprite;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject levelFailedPanel;
    [SerializeField] private TextMeshProUGUI levelFailedBestText;
    [SerializeField] private TextMeshProUGUI levelFailedLevelText;
    [SerializeField] private TextMeshProUGUI levelFailedTimeReachedText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backToMenuButton;
    // Full screen art backdrop shown behind the end panels, since the panels
    // themselves are small centered boxes, not full screen. Toggled with
    // them so the game grid isn't left showing through in between.
    [SerializeField] private GameObject endScreenBackground;
    // Container for the gameplay stat bar (Level/Time/Moves/Matches). Draw
    // order alone did not keep it behind the end panels (see BACKLOG.md
    // item 32), so it is explicitly hidden while an end panel is showing
    // and restored when gameplay resumes, instead of relying on sibling index.
    [SerializeField] private GameObject gameplayHud;

    [Header("Pause")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseBackToMenuButton;
    [SerializeField] private Toggle pauseSfxToggle;
    [SerializeField] private Image pauseSfxToggleTrack;
    [SerializeField] private RectTransform pauseSfxToggleHandle;
    [SerializeField] private TextMeshProUGUI pauseSfxOnOffText;
    [SerializeField] private Toggle pauseMusicToggle;
    [SerializeField] private Image pauseMusicToggleTrack;
    [SerializeField] private RectTransform pauseMusicToggleHandle;
    [SerializeField] private TextMeshProUGUI pauseMusicOnOffText;

    [Header("Hint")]
    [SerializeField] private Button hintButton;
    [SerializeField] private Image hintFrameImage;
    [SerializeField] private Image hintIconImage;
    [SerializeField] private TextMeshProUGUI hintCountText;

    // Reveal duration and disabled-state dimming, not covered by BRAND.md's
    // Layout/Spacing tokens since those are spacing values, not timings or
    // opacity - kept local to this feature like mismatchFlipBackDelay above.
    private const float HintRevealDuration = 1.5f;
    private const float HintDisabledAlpha = 0.35f;

    private int hintsRemaining;

    // Toggle-on and toggle-off tinting, matching the Settings scene's own
    // SettingsController (BRAND.md: teal is calm/enabled, muted slate is
    // disabled). Duplicated here rather than shared, following this
    // project's existing per-class HexColor convention.
    private static readonly Color ToggleOnColor = HexColor("4FD1C5");
    private static readonly Color ToggleOffColor = HexColor("34435F");
    private static readonly Color OnOffTextOnColor = HexColor("4FD1C5");
    private static readonly Color OnOffTextOffColor = HexColor("8B93A7");

    // Same teal/muted-gray split as the toggle colors above: teal for an
    // earned star, BRAND.md's muted secondary-text gray for an unearned one.
    private static readonly Color StarEarnedColor = HexColor("4FD1C5");
    private static readonly Color StarUnearnedColor = HexColor("8B93A7");

    [Header("Timer")]
    [SerializeField] private float urgentThresholdSeconds = 5f;
    [SerializeField] private float pulseAmplitude = 0.08f;
    [SerializeField] private float pulseSpeed = 6f; // radians/second, gentle not jarring

    // Matches the approved gameplay mockup: teal is the normal countdown
    // color (BRAND.md's calm/progress color), orange still signals urgency.
    private static readonly Color NormalTimerColor = HexColor("4FD1C5");
    private static readonly Color UrgentTimerColor = HexColor("F6AD55");

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private readonly List<CardIdentity> flippedCards = new List<CardIdentity>(2);
    private bool inputLocked;
    private int pairCount;
    private int moveCount;
    private int matchCount;
    private int lastStarsEarned;

    private LevelData activeLevelData;
    private int columns = 4;
    private int rows = 4;

    private float timeRemaining;
    private bool timerRunning;
    private bool timerUrgent;

    // True once either the win or the fail condition has fired for the
    // current round. Keeps the two mutually exclusive per level attempt.
    private bool roundEnded;

    // True while the Pause panel is up. Distinct from inputLocked: inputLocked
    // covers the brief flip/match resolution window and must never pause the
    // timer, while isPaused is the explicit player pause and must freeze it.
    private bool isPaused;

    // Furthest level unlocked for play, persisted locally via ProgressData.
    // Level 1 is always unlocked, so a fresh install with nothing saved
    // defaults to 1.
    private int unlockedLevel = 1;

    // Highest level currently authored (Level_15.asset). Progression clamps here.
    private const int MaxLevel = 15;

    public LevelData ActiveLevelData => activeLevelData;

    private void Start()
    {
        Random.InitState(System.Environment.TickCount);
        WireEndScreenButtons();
        SetupPauseToggles();
        LoadUnlockedLevel();
        currentLevel = unlockedLevel;
        LoadLevelData();
        SetupCamera();
        SpawnGrid();
    }

    private void LoadUnlockedLevel()
    {
        unlockedLevel = ProgressData.GetUnlockedLevel();
    }

    private void SaveUnlockedLevelIfHigher(int newlyUnlockedLevel)
    {
        if (newlyUnlockedLevel <= unlockedLevel)
        {
            return;
        }

        unlockedLevel = newlyUnlockedLevel;
        ProgressData.SetUnlockedLevelIfHigher(newlyUnlockedLevel);
    }

    // Debug hook: right-click the GameManager component header in the
    // Inspector to run this, or call it directly from any script/test code.
    // Production reset flow (MainMenu's New Game button) calls
    // ProgressData.ResetProgress() directly instead, since MainMenu has no
    // GameManager instance to call this through.
    [ContextMenu("Reset Progress (Testing)")]
    public void ResetProgressForTesting()
    {
        ProgressData.ResetProgress();
        unlockedLevel = 1;
        Debug.Log("GameManager: progress reset to level 1.");
    }

    private void WireEndScreenButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (pauseBackToMenuButton != null)
        {
            pauseBackToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintClicked);
        }
    }

    // Mirrors SettingsController's own toggle wiring exactly, so the Pause
    // panel's Sound Effects and Music rows read and persist through the
    // same SoundSettings statics the Settings scene uses.
    private void SetupPauseToggles()
    {
        SetupToggle(pauseSfxToggle, pauseSfxToggleTrack, pauseSfxToggleHandle, pauseSfxOnOffText,
            SoundSettings.IsSfxEnabled(), SoundSettings.SetSfxEnabled);

        SetupToggle(pauseMusicToggle, pauseMusicToggleTrack, pauseMusicToggleHandle, pauseMusicOnOffText,
            SoundSettings.IsMusicEnabled(), SoundSettings.SetMusicEnabled);
    }

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

    public void OnPauseClicked()
    {
        // Explicit guard: the pause button already lives under gameplayHud
        // (hidden during Level Complete/Failed), but this blocks any stray
        // trigger outright rather than relying on that alone.
        if (roundEnded || isPaused)
        {
            return;
        }

        isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Same explicit hide mechanism as ShowLevelComplete/ShowLevelFailed
        // (BACKLOG.md item 32) - draw order alone lets the HUD bleed through
        // behind the panel. This also hides the pause button itself, since
        // it lives under gameplayHud.
        if (gameplayHud != null)
        {
            gameplayHud.SetActive(false);
        }
    }

    public void OnResumeClicked()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (gameplayHud != null)
        {
            gameplayHud.SetActive(true);
        }
    }

    // Fresh allowance per level attempt, not a persistent pool: 1-3 get 3,
    // 4-6 get 2, 7-9 get 1, 10+ get 0 (hint button stays visible but
    // disabled/dimmed there, matching the difficulty curve past the time
    // floor - see PRD.md's level progression table).
    private static int GetHintAllowanceForLevel(int level)
    {
        if (level <= 3)
        {
            return 3;
        }
        if (level <= 6)
        {
            return 2;
        }
        if (level <= 9)
        {
            return 1;
        }
        return 0;
    }

    // Weighted 0.6 toward moveRatio rather than timeRatio: past the 15s
    // time floor (PRD.md level 10+) timeRatio increasingly reflects
    // reaction speed rather than skill, while moveRatio (memory accuracy)
    // stays a meaningful signal at any grid size. Both ratios are already
    // normalized to the level's own par (LevelData's time limit and
    // pairCount), so the same thresholds apply uniformly across levels
    // without needing to scale per difficulty. Thresholds are tuned so
    // perfect moves alone always clears 2 stars, and 3 stars needs both
    // accuracy and time to spare.
    private static int CalculateStarRating(float timeRatio, float moveRatio)
    {
        float combinedScore = 0.4f * timeRatio + 0.6f * moveRatio;

        if (combinedScore >= 0.75f)
        {
            return 3;
        }

        if (combinedScore >= 0.45f)
        {
            return 2;
        }

        return 1;
    }

    private void UpdateHintUI()
    {
        if (hintCountText != null)
        {
            hintCountText.text = hintsRemaining.ToString();
        }

        bool hasHints = hintsRemaining > 0;
        if (hintButton != null)
        {
            hintButton.interactable = hasHints;
        }

        float alpha = hasHints ? 1f : HintDisabledAlpha;
        SetImageAlpha(hintFrameImage, alpha);
        SetImageAlpha(hintIconImage, alpha);
        SetTextAlpha(hintCountText, alpha);
    }

    private static void SetImageAlpha(Image img, float alpha)
    {
        if (img == null)
        {
            return;
        }
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private static void SetTextAlpha(TextMeshProUGUI txt, float alpha)
    {
        if (txt == null)
        {
            return;
        }
        Color c = txt.color;
        c.a = alpha;
        txt.color = c;
    }

    public void OnHintClicked()
    {
        if (roundEnded || isPaused || inputLocked || hintsRemaining <= 0)
        {
            return;
        }

        CardIdentity a, b;
        if (!TryFindRandomUnmatchedPair(out a, out b))
        {
            // Defensive no-op: hintsRemaining > 0 should always mean at
            // least one complete unmatched pair exists (pairs are only ever
            // matched together, never one card at a time), so this should
            // never trigger in practice.
            return;
        }

        a.FlipFaceUp();
        b.FlipFaceUp();

        inputLocked = true;
        hintsRemaining--;
        UpdateHintUI();

        StartCoroutine(HintRevealRoutine(a, b));
    }

    private IEnumerator HintRevealRoutine(CardIdentity a, CardIdentity b)
    {
        yield return new WaitForSeconds(HintRevealDuration);

        a.FlipFaceDown();
        b.FlipFaceDown();
        inputLocked = false;
    }

    // Shuffles which face-down card is checked first so repeated hints on
    // the same board don't always surface the same pair, then does a plain
    // linear scan for its match - the board is small enough (max 21 pairs)
    // that this is simpler than a lookup table for a one-off pick.
    private bool TryFindRandomUnmatchedPair(out CardIdentity a, out CardIdentity b)
    {
        List<CardIdentity> faceDown = new List<CardIdentity>();
        foreach (GameObject go in spawnedCards)
        {
            if (go == null)
            {
                continue;
            }
            CardIdentity identity = go.GetComponent<CardIdentity>();
            if (identity != null && identity.State == CardIdentity.CardState.FaceDown)
            {
                faceDown.Add(identity);
            }
        }

        for (int i = faceDown.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardIdentity temp = faceDown[i];
            faceDown[i] = faceDown[j];
            faceDown[j] = temp;
        }

        for (int i = 0; i < faceDown.Count; i++)
        {
            for (int j = i + 1; j < faceDown.Count; j++)
            {
                if (faceDown[i].IconIndex == faceDown[j].IconIndex
                    && faceDown[i].RotationDegrees == faceDown[j].RotationDegrees)
                {
                    a = faceDown[i];
                    b = faceDown[j];
                    return true;
                }
            }
        }

        a = null;
        b = null;
        return false;
    }

    private void LoadLevelData()
    {
        activeLevelData = ResolveLevelData(currentLevel);
        if (activeLevelData == null)
        {
            Debug.LogWarning("GameManager: no LevelData found for level " + currentLevel + ", falling back to a 4x4 grid.");
            columns = 4;
            rows = 4;
            return;
        }

        columns = activeLevelData.columns;
        rows = activeLevelData.rows;
    }

    private LevelData ResolveLevelData(int level)
    {
        if (levelDatabase == null)
        {
            return null;
        }

        foreach (LevelData data in levelDatabase)
        {
            if (data != null && data.levelNumber == level)
            {
                return data;
            }
        }

        return null;
    }

    private void Update()
    {
        // Runs unconditionally so the timer is never paused by the flip/match
        // resolution lock, and never affects that logic either.
        TickTimer();

        if (roundEnded)
        {
            return;
        }

        if (isPaused)
        {
            return;
        }

        if (inputLocked)
        {
            return;
        }

        if (!TryGetPointerDownPosition(out Vector2 screenPos))
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit == null)
        {
            return;
        }

        CardIdentity card = hit.GetComponent<CardIdentity>();
        if (card != null)
        {
            RequestFlip(card);
        }
    }

    private bool TryGetPointerDownPosition(out Vector2 screenPos)
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }

        Touchscreen touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = touch.primaryTouch.position.ReadValue();
            return true;
        }

        screenPos = default;
        return false;
    }

    // Entry point for a tap on a card. Called from Update()'s pointer/touch
    // detection, and safe to call directly (e.g. from tests or other input sources).
    public void RequestFlip(CardIdentity card)
    {
        if (roundEnded || inputLocked || isPaused || card == null)
        {
            return;
        }

        if (card.State != CardIdentity.CardState.FaceDown)
        {
            return;
        }

        card.FlipFaceUp();
        PlayFlipSound();
        flippedCards.Add(card);

        if (flippedCards.Count < 2)
        {
            return;
        }

        CardIdentity first = flippedCards[0];
        CardIdentity second = flippedCards[1];
        flippedCards.Clear();

        inputLocked = true;
        StartCoroutine(ResolvePair(first, second));
    }

    private IEnumerator ResolvePair(CardIdentity a, CardIdentity b)
    {
        bool isMatch = a.IconIndex == b.IconIndex && a.RotationDegrees == b.RotationDegrees;

        if (isMatch)
        {
            a.SetMatched();
            b.SetMatched();
            matchCount++;
            PlayMatchSound();
        }
        else
        {
            PlayMismatchSound();
            yield return new WaitForSeconds(mismatchFlipBackDelay);
            a.FlipFaceDown();
            b.FlipFaceDown();
        }

        moveCount++;
        UpdateUI();

        inputLocked = false;

        if (isMatch && matchCount >= pairCount)
        {
            OnAllPairsMatched();
        }
    }

    private void UpdateUI()
    {
        if (movesText != null)
        {
            movesText.text = moveCount.ToString();
        }

        if (matchesText != null)
        {
            // TMP parses rich text tags by default, tinting the live match
            // count teal to match the approved gameplay mockup's stat row.
            matchesText.text = "<color=#4FD1C5>" + matchCount + "</color> / " + pairCount;
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = currentLevel.ToString();
        }
    }

    private void ResetTimer()
    {
        float duration = activeLevelData != null ? activeLevelData.timeLimitSeconds : 60f;
        timeRemaining = duration;
        timerRunning = true;
        timerUrgent = false;

        if (timerText != null)
        {
            timerText.color = NormalTimerColor;
            timerText.transform.localScale = Vector3.one;
        }

        UpdateTimerDisplay();
    }

    private void TickTimer()
    {
        if (!timerRunning)
        {
            return;
        }

        if (isPaused)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            UpdateTimerDisplay();
            if (timerText != null)
            {
                timerText.transform.localScale = Vector3.one;
            }
            Debug.Log("GameManager: level timer reached zero.");
            OnTimerExpired();
            return;
        }

        UpdateTimerDisplay();
        SetTimerUrgent(timeRemaining <= urgentThresholdSeconds);

        if (timerUrgent && timerText != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            timerText.transform.localScale = Vector3.one * pulse;
        }
    }

    private void SetTimerUrgent(bool urgent)
    {
        if (timerUrgent == urgent)
        {
            return;
        }

        timerUrgent = urgent;

        if (timerText != null)
        {
            timerText.color = urgent ? UrgentTimerColor : NormalTimerColor;
            if (!urgent)
            {
                timerText.transform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = FormatMinutesSeconds(timeRemaining);
    }

    private static Color HexColor(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + hex, out c);
        return c;
    }

    private void OnAllPairsMatched()
    {
        if (roundEnded)
        {
            return;
        }

        roundEnded = true;
        timerRunning = false;

        int nextUnlockable = Mathf.Min(currentLevel + 1, MaxLevel);
        SaveUnlockedLevelIfHigher(nextUnlockable);

        float timeLimit = activeLevelData != null ? activeLevelData.timeLimitSeconds : 60f;
        float timeRatio = timeRemaining / timeLimit;
        float moveRatio = (float)pairCount / moveCount;
        lastStarsEarned = CalculateStarRating(timeRatio, moveRatio);
        ProgressData.SetBestStarsIfHigher(currentLevel, lastStarsEarned);

        PlayLevelCompleteSound();
        ShowLevelComplete();
    }

    private void OnTimerExpired()
    {
        if (roundEnded)
        {
            return;
        }

        roundEnded = true;
        ShowLevelFailed();
    }

    private void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        if (levelCompleteTimeText != null)
        {
            levelCompleteTimeText.text = FormatMinutesSeconds(timeRemaining);
        }

        if (levelCompleteLevelText != null)
        {
            levelCompleteLevelText.text = currentLevel.ToString();
        }

        if (levelCompleteMatchesText != null)
        {
            levelCompleteMatchesText.text = "<color=#4FD1C5>" + matchCount + "</color> / " + pairCount;
        }

        UpdateLevelCompleteStars();

        if (endScreenBackground != null)
        {
            endScreenBackground.SetActive(true);
        }

        if (gameplayHud != null)
        {
            gameplayHud.SetActive(false);
        }
    }

    private void UpdateLevelCompleteStars()
    {
        SetStarIcon(levelCompleteStar1, 1);
        SetStarIcon(levelCompleteStar2, 2);
        SetStarIcon(levelCompleteStar3, 3);
    }

    private void SetStarIcon(Image starImage, int starPosition)
    {
        if (starImage == null)
        {
            return;
        }

        bool earned = starPosition <= lastStarsEarned;
        starImage.sprite = earned ? starFilledSprite : starOutlineSprite;
        starImage.color = earned ? StarEarnedColor : StarUnearnedColor;
    }

    private void ShowLevelFailed()
    {
        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }

        if (levelFailedBestText != null)
        {
            levelFailedBestText.text = "<color=#F6AD55>" + matchCount + "</color> / " + pairCount;
        }

        if (levelFailedLevelText != null)
        {
            levelFailedLevelText.text = currentLevel.ToString();
        }

        if (levelFailedTimeReachedText != null)
        {
            levelFailedTimeReachedText.text = FormatMinutesSeconds(timeRemaining);
        }

        if (endScreenBackground != null)
        {
            endScreenBackground.SetActive(true);
        }

        if (gameplayHud != null)
        {
            gameplayHud.SetActive(false);
        }
    }

    // Shared with the gameplay countdown's mm:ss format so end panel time
    // values read consistently with the timer players just watched.
    private static string FormatMinutesSeconds(float seconds)
    {
        int displaySeconds = Mathf.CeilToInt(seconds);
        int minutes = displaySeconds / 60;
        int secs = displaySeconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, secs);
    }

    private void HideEndPanels()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(false);
        }

        if (endScreenBackground != null)
        {
            endScreenBackground.SetActive(false);
        }

        if (gameplayHud != null)
        {
            gameplayHud.SetActive(true);
        }
    }

    public void OnContinueClicked()
    {
        currentLevel = currentLevel < MaxLevel ? currentLevel + 1 : MaxLevel;
        LoadLevelData();
        SpawnGrid();
    }

    public void OnRetryClicked()
    {
        SpawnGrid();
    }

    public void OnBackToMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = referenceAspectHeight / 2f;
    }

    private void SpawnGrid()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("GameManager: Card prefab is not assigned.");
            return;
        }

        foreach (GameObject existing in spawnedCards)
        {
            if (existing != null)
            {
                Destroy(existing);
            }
        }
        spawnedCards.Clear();
        flippedCards.Clear();
        inputLocked = false;
        moveCount = 0;
        matchCount = 0;
        roundEnded = false;
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        hintsRemaining = GetHintAllowanceForLevel(currentLevel);
        UpdateHintUI();
        HideEndPanels();
        ResetTimer();
        UpdateLevelText();

        pairCount = (columns * rows) / 2;
        List<PairIdentity> cardIdentities = BuildShuffledCardIdentities(pairCount);

        float nativeCardSize = GetNativeCardSize();
        float usableWidth = referenceAspectWidth * (1f - 2f * horizontalMarginRatio);
        float usableHeight = referenceAspectHeight * (1f - 2f * verticalMarginRatio);
        float cellSizeFromWidth = (usableWidth - (columns - 1) * cellSpacing) / columns;
        float cellSizeFromHeight = (usableHeight - (rows - 1) * cellSpacing) / rows;
        float cellSize = Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);
        float scale = nativeCardSize > 0f ? cellSize / nativeCardSize : 1f;

        float gridWidth = columns * cellSize + (columns - 1) * cellSpacing;
        float gridHeight = rows * cellSize + (rows - 1) * cellSpacing;

        float startX = -gridWidth / 2f + cellSize / 2f;
        float startY = gridHeight / 2f - cellSize / 2f;

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = new Vector3(
                    startX + col * (cellSize + cellSpacing),
                    startY - row * (cellSize + cellSpacing),
                    0f);

                GameObject card = Instantiate(cardPrefab, position, Quaternion.identity, transform);
                card.name = "Card_" + row + "_" + col;
                card.transform.localScale = Vector3.one * scale;

                CardIdentity identity = ApplySymbol(card, cardIdentities[index]);
                identity.FlipFaceDown();

                spawnedCards.Add(card);
                index++;
            }
        }

        UpdateUI();
    }

    private float GetNativeCardSize()
    {
        Transform front = cardPrefab.transform.Find("Front");
        SpriteRenderer sr = front != null ? front.GetComponent<SpriteRenderer>() : null;
        if (sr != null && sr.sprite != null)
        {
            return sr.sprite.bounds.size.x;
        }
        return 1f;
    }

    // A pair's identity is (icon, rotation angle). With 8 base icons and up
    // to 4 rotation states, that is 32 unique identities - enough for level
    // 15's 21 pairs. Rotation is introduced gradually: levels 1-10 stay at
    // 0 degrees only (unchanged from before), 11-12 add 180, 13-15 add the
    // full set (0/90/180/270), lining up with the grid-size step at 13.
    private const int IconCount = 8;
    private static readonly int[] RotationsBeforeLevel11 = { 0 };
    private static readonly int[] RotationsLevel11To12 = { 0, 180 };
    private static readonly int[] RotationsLevel13Plus = { 0, 90, 180, 270 };

    private struct PairIdentity
    {
        public int IconIndex;
        public int RotationDegrees;
    }

    private int[] GetAllowedRotations()
    {
        if (currentLevel >= 13)
        {
            return RotationsLevel13Plus;
        }
        if (currentLevel >= 11)
        {
            return RotationsLevel11To12;
        }
        return RotationsBeforeLevel11;
    }

    private List<PairIdentity> BuildShuffledCardIdentities(int pairCount)
    {
        int[] allowedRotations = GetAllowedRotations();

        List<PairIdentity> pool = new List<PairIdentity>(IconCount * allowedRotations.Length);
        foreach (int rotation in allowedRotations)
        {
            for (int icon = 0; icon < IconCount; icon++)
            {
                pool.Add(new PairIdentity { IconIndex = icon, RotationDegrees = rotation });
            }
        }

        ShufflePairIdentities(pool);

        if (pairCount > pool.Count)
        {
            Debug.LogError("GameManager: level " + currentLevel + " needs " + pairCount + " pairs but only " + pool.Count + " unique (icon, rotation) identities are available.");
            pairCount = pool.Count;
        }

        List<PairIdentity> cardIdentities = new List<PairIdentity>(pairCount * 2);
        for (int i = 0; i < pairCount; i++)
        {
            cardIdentities.Add(pool[i]);
            cardIdentities.Add(pool[i]);
        }

        ShufflePairIdentities(cardIdentities);

        return cardIdentities;
    }

    private void ShufflePairIdentities(List<PairIdentity> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            PairIdentity temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private CardIdentity ApplySymbol(GameObject card, PairIdentity pairIdentity)
    {
        CardIdentity identity = card.GetComponent<CardIdentity>();
        if (identity == null)
        {
            identity = card.AddComponent<CardIdentity>();
        }

        identity.ApplyIcon(pairIdentity.IconIndex, pairIdentity.RotationDegrees);

        return identity;
    }

    // Audio hooks: no clips wired up yet, these are placeholders so sound
    // can be dropped in later without touching gameplay code again. See
    // BRAND.md's Sound Direction section for the intended feel of each one.
    // Each checks the player's Sfx toggle specifically (not Music) so it
    // stays functionally wired even with no audible difference yet.
    private void PlayFlipSound()
    {
        if (!SoundSettings.IsSfxEnabled())
        {
            return;
        }
    }

    private void PlayMatchSound()
    {
        if (!SoundSettings.IsSfxEnabled())
        {
            return;
        }
    }

    private void PlayMismatchSound()
    {
        if (!SoundSettings.IsSfxEnabled())
        {
            return;
        }
    }

    private void PlayLevelCompleteSound()
    {
        if (!SoundSettings.IsSfxEnabled())
        {
            return;
        }
    }
}
