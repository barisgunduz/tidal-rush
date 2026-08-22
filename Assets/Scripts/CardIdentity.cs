using System.Collections;
using UnityEngine;

public class CardIdentity : MonoBehaviour
{
    public enum CardState
    {
        FaceDown,
        FaceUp,
        Matched
    }

    // A pair's identity is (icon, rotation angle) - two cards only match if
    // both the icon and the rotation are the same.
    public int IconIndex;
    public int RotationDegrees;
    public CardState State { get; private set; } = CardState.FaceDown;

    // Real nautical icon sprites, index 0-7. Same order as BRAND.md's symbol
    // list: Anchor, Compass, Wave, Sailboat, Shell, Map, Dolphin, Lightning.
    [SerializeField] private Sprite[] iconSprites;

    // BRAND.md: symbols are "simple, single color line or flat icons" - all
    // icons render in one fixed color. Pairs are told apart by real icon
    // shape and rotation now, not by a per-pair tint (that was only needed
    // while every card used the same placeholder triangle).
    // Deep navy, sampled from the approved mockup (SS3) to read against the
    // flipped card's cream background - was off-white for the old dark card.
    private static readonly Color IconColor = HexColor("102640");

    // BRAND.md motion rules: flip stays well under 0.5s with no bounce or
    // elastic easing (plain linear), match confirmation stays under 200ms.
    private const float FlipHalfDuration = 0.15f;
    private const float MatchPulseDuration = 0.18f;
    private const float MatchPulseScaleBump = 0.15f;

    private static readonly Color MatchGlowColor = HexColor("4FD1C5");

    private Transform front;
    private Transform back;
    private SpriteRenderer backRenderer;
    private SpriteRenderer symbolRenderer;

    private Vector3 baseScale;
    private bool baseScaleCaptured;

    private Coroutine flipRoutine;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        front = transform.Find("Front");
        back = transform.Find("Back");
        backRenderer = back != null ? back.GetComponent<SpriteRenderer>() : null;

        Transform symbol = back != null ? back.Find("Symbol") : null;
        symbolRenderer = symbol != null ? symbol.GetComponent<SpriteRenderer>() : null;
    }

    // Assigns this card's (icon, rotation) identity and updates the visual
    // symbol to match: the correct sprite, fixed icon color, and rotation.
    public void ApplyIcon(int iconIndex, int rotationDegrees)
    {
        IconIndex = iconIndex;
        RotationDegrees = rotationDegrees;

        if (symbolRenderer == null)
        {
            return;
        }

        if (iconSprites != null && iconIndex >= 0 && iconIndex < iconSprites.Length && iconSprites[iconIndex] != null)
        {
            symbolRenderer.sprite = iconSprites[iconIndex];
        }
        symbolRenderer.color = IconColor;
        symbolRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);
    }

    public void FlipFaceUp()
    {
        EnsureBaseScaleCaptured();
        State = CardState.FaceUp;
        BeginFlip(back, front);
    }

    public void FlipFaceDown()
    {
        EnsureBaseScaleCaptured();
        State = CardState.FaceDown;
        BeginFlip(front, back);
    }

    public void SetMatched()
    {
        EnsureBaseScaleCaptured();
        State = CardState.Matched;
        PlayMatchPulse();
    }

    private void EnsureBaseScaleCaptured()
    {
        if (!baseScaleCaptured)
        {
            baseScale = transform.localScale;
            baseScaleCaptured = true;
        }
    }

    private void BeginFlip(Transform toShow, Transform toHide)
    {
        bool alreadyInTargetState = toShow != null && toShow.gameObject.activeSelf
            && (toHide == null || !toHide.gameObject.activeSelf);
        if (alreadyInTargetState)
        {
            // Nothing to animate - avoids a spurious flip effect the moment a
            // card spawns already in its default face-down state.
            return;
        }

        if (flipRoutine != null)
        {
            StopCoroutine(flipRoutine);
            flipRoutine = null;
        }
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (!gameObject.activeInHierarchy)
        {
            // Can't run a coroutine yet - just snap to the target state.
            if (toHide != null)
            {
                toHide.gameObject.SetActive(false);
            }
            if (toShow != null)
            {
                toShow.gameObject.SetActive(true);
            }
            return;
        }

        transform.localScale = baseScale;
        flipRoutine = StartCoroutine(FlipRoutine(toShow, toHide));
    }

    private IEnumerator FlipRoutine(Transform toShow, Transform toHide)
    {
        float t = 0f;
        while (t < FlipHalfDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / FlipHalfDuration);
            float scaleX = Mathf.Lerp(baseScale.x, 0f, ratio);
            transform.localScale = new Vector3(scaleX, baseScale.y, baseScale.z);
            yield return null;
        }
        transform.localScale = new Vector3(0f, baseScale.y, baseScale.z);

        if (toHide != null)
        {
            toHide.gameObject.SetActive(false);
        }
        if (toShow != null)
        {
            toShow.gameObject.SetActive(true);
        }

        t = 0f;
        while (t < FlipHalfDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / FlipHalfDuration);
            float scaleX = Mathf.Lerp(0f, baseScale.x, ratio);
            transform.localScale = new Vector3(scaleX, baseScale.y, baseScale.z);
            yield return null;
        }
        transform.localScale = baseScale;

        flipRoutine = null;
    }

    private void PlayMatchPulse()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        pulseRoutine = StartCoroutine(MatchPulseRoutine());
    }

    private IEnumerator MatchPulseRoutine()
    {
        Color originalColor = backRenderer != null ? backRenderer.color : default;

        float t = 0f;
        while (t < MatchPulseDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / MatchPulseDuration);
            // Triangular envelope: ramps up to the peak at the midpoint, then
            // back down - a single clean pulse, no bounce or oscillation.
            float envelope = ratio < 0.5f ? (ratio / 0.5f) : (1f - (ratio - 0.5f) / 0.5f);

            transform.localScale = baseScale * (1f + MatchPulseScaleBump * envelope);
            if (backRenderer != null)
            {
                backRenderer.color = Color.Lerp(originalColor, MatchGlowColor, envelope);
            }

            yield return null;
        }

        transform.localScale = baseScale;
        if (backRenderer != null)
        {
            backRenderer.color = originalColor;
        }

        pulseRoutine = null;
    }

    private static Color HexColor(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + hex, out c);
        return c;
    }
}
