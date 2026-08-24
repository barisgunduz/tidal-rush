using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Exposes the icon and label of a ButtonPrimaryTeal / ButtonSecondarySlate
// prefab instance so callers can swap content without touching layout.
// See BRAND.md's Layout and Spacing System section: button size, corner
// radius, and margins are fixed by these prefabs and should never be
// re-derived per screen.
public class ButtonContent : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI label;

    public Image Icon => icon;
    public TextMeshProUGUI Label => label;

    public void SetContent(Sprite iconSprite, string labelText)
    {
        if (icon != null)
        {
            icon.sprite = iconSprite;
        }

        if (label != null)
        {
            label.text = labelText;
        }
    }

    // BRAND.md distinguishes standalone menu buttons (630px) from
    // panel-adjacent buttons (828px, matching the panel above them). Both
    // widths come from the same prefab through this call rather than a
    // second prefab variant. Icon/label anchoredPosition.x scale with the
    // width ratio (not fixed offsets) so they stay balanced at either
    // width; Backdrop keeps its fixed glow/shadow padding beyond the
    // button edge instead of scaling that padding too.
    public void SetWidth(float newWidth)
    {
        RectTransform rootRt = (RectTransform)transform;
        float oldWidth = rootRt.sizeDelta.x;
        if (Mathf.Approximately(oldWidth, newWidth))
        {
            return;
        }

        float ratio = newWidth / oldWidth;
        rootRt.sizeDelta = new Vector2(newWidth, rootRt.sizeDelta.y);

        Transform fill = transform.Find("Fill");
        if (fill != null)
        {
            RectTransform fillRt = (RectTransform)fill;
            fillRt.sizeDelta = new Vector2(newWidth, fillRt.sizeDelta.y);
        }

        Transform border = transform.Find("Border");
        if (border != null)
        {
            RectTransform borderRt = (RectTransform)border;
            borderRt.sizeDelta = new Vector2(newWidth, borderRt.sizeDelta.y);
        }

        Transform backdrop = transform.Find("Backdrop");
        if (backdrop != null)
        {
            RectTransform backdropRt = (RectTransform)backdrop;
            float extraPadding = backdropRt.sizeDelta.x - oldWidth;
            backdropRt.sizeDelta = new Vector2(newWidth + extraPadding, backdropRt.sizeDelta.y);
        }

        if (icon != null)
        {
            RectTransform iconRt = icon.rectTransform;
            iconRt.anchoredPosition = new Vector2(iconRt.anchoredPosition.x * ratio, iconRt.anchoredPosition.y);
        }

        if (label != null)
        {
            RectTransform labelRt = label.rectTransform;
            labelRt.anchoredPosition = new Vector2(labelRt.anchoredPosition.x * ratio, labelRt.anchoredPosition.y);
        }
    }
}
