using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private Text movesText;
    [SerializeField] private Text matchesText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text levelText;

    [Header("Win/Fail UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Text levelCompleteTimeText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject levelFailedPanel;
    [SerializeField] private Button retryButton;
    // Full screen art backdrop shown behind the end panels, since the panels
    // themselves are small centered boxes, not full screen. Toggled with
    // them so the game grid isn't left showing through in between.
    [SerializeField] private GameObject endScreenBackground;

    [Header("Timer")]
    [SerializeField] private float urgentThresholdSeconds = 5f;
    [SerializeField] private float pulseAmplitude = 0.08f;
    [SerializeField] private float pulseSpeed = 6f; // radians/second, gentle not jarring

    private static readonly Color NormalTimerColor = HexColor("E8ECF4");
    private static readonly Color UrgentTimerColor = HexColor("F6AD55");

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private readonly List<CardIdentity> flippedCards = new List<CardIdentity>(2);
    private bool inputLocked;
    private int pairCount;
    private int moveCount;
    private int matchCount;

    private LevelData activeLevelData;
    private int columns = 4;
    private int rows = 4;

    private float timeRemaining;
    private bool timerRunning;
    private bool timerUrgent;

    // True once either the win or the fail condition has fired for the
    // current round. Keeps the two mutually exclusive per level attempt.
    private bool roundEnded;

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
        if (roundEnded || inputLocked || card == null)
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
            movesText.text = "Moves: " + moveCount;
        }

        if (matchesText != null)
        {
            matchesText.text = "Matches: " + matchCount + " / " + pairCount;
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + currentLevel;
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

        int displaySeconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = "Time: " + displaySeconds;
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
            levelCompleteTimeText.text = "Time Remaining: " + Mathf.CeilToInt(timeRemaining) + "s";
        }

        if (endScreenBackground != null)
        {
            endScreenBackground.SetActive(true);
        }
    }

    private void ShowLevelFailed()
    {
        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }

        if (endScreenBackground != null)
        {
            endScreenBackground.SetActive(true);
        }
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
    // Each checks the player's sound toggle first so it stays functionally
    // wired even with no audible difference yet.
    private void PlayFlipSound()
    {
        if (!SoundSettings.IsSoundEnabled())
        {
            return;
        }
    }

    private void PlayMatchSound()
    {
        if (!SoundSettings.IsSoundEnabled())
        {
            return;
        }
    }

    private void PlayMismatchSound()
    {
        if (!SoundSettings.IsSoundEnabled())
        {
            return;
        }
    }

    private void PlayLevelCompleteSound()
    {
        if (!SoundSettings.IsSoundEnabled())
        {
            return;
        }
    }
}
