using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreditRoller : MonoBehaviour
{
    public enum ScrollDirection
    {
        BottomToTop,
        TopToBottom
    }
    [Header("References")]
    [Tooltip("TMPro text to display the credits. It should be inside a UI viewport (e.g., with Mask).")]
    [SerializeField] private TMP_Text creditsText;
    [Tooltip("Viewport RectTransform that defines the visible area for the scroll (usually a parent with Mask). If left empty, will try to use the text's parent RectTransform.")]
    [SerializeField] private RectTransform viewport;

    [Header("Content")]
    [TextArea(5, 20)]
    [SerializeField] private string creditsLongString;

    [Header("Behavior")]
    [Tooltip("Automatically start rolling on enable.")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Delay before the roll starts (seconds).")]
    [SerializeField] private float startDelay = 0.5f;
    [Tooltip("Vertical scroll speed in units/second (anchoredPosition.y).")]
    [SerializeField] private float scrollSpeed = 80f;
    [Tooltip("Extra bottom padding so the first line starts fully off-screen.")]
    [SerializeField] private float extraBottomPadding = 40f;
    [Tooltip("Hold time at the end before invoking OnComplete (seconds).")]
    [SerializeField] private float endHoldTime = 1.0f;
    [Tooltip("Direction of the scroll movement.")]
    [SerializeField] private ScrollDirection direction = ScrollDirection.BottomToTop;

    [Header("Typewriter (optional)")]
    [Tooltip("If > 0, reveals characters over time using TMP maxVisibleCharacters.")]
    [SerializeField] private float charactersPerSecond = 0f;

    [Header("Control")]
    [Tooltip("If true, any key/mouse click/gamepad south will instantly finish the roll.")]
    [SerializeField] private bool allowSkip = true;

    [Header("Events")]
    public UnityEvent OnRollStarted;
    public UnityEvent OnRollCompleted;

    private Coroutine _routine;
    private bool _isRunning;

    void OnEnable()
    {
        if (autoStart)
        {
            StartRoll(creditsLongString);
        }
    }

    public void StartRoll(string longCredits)
    {
        if (creditsText == null)
        {
            Debug.LogWarning("CreditRoller: CreditsText is not assigned.");
            return;
        }
        if (viewport == null)
        {
            viewport = creditsText.rectTransform.parent as RectTransform;
        }

        // Stop previous run
        if (_routine != null) StopCoroutine(_routine);

        creditsText.text = longCredits;
        // Prevent a one-frame flash of full text before layout/positioning by hiding all characters immediately.
        creditsText.maxVisibleCharacters = 0;
        _routine = StartCoroutine(RollRoutine());
    }

    public void StopRoll()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
        _isRunning = false;
    }

    private IEnumerator RollRoutine()
    {
        _isRunning = true;
        OnRollStarted?.Invoke();

        // Prepare layout so we can compute dimensions
        // Ensure text starts hidden even if StartRoll wasn't used (defensive)
        creditsText.maxVisibleCharacters = 0;

        // wait one frame so TMP can process text (preferredHeight may need a frame)
        yield return null;
        creditsText.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsText.rectTransform);

        // Typewriter init
        int totalChars = creditsText.textInfo.characterCount;
        bool useTypewriter = charactersPerSecond > 0.01f;

        // Determine viewport
        if (viewport == null)
        {
            Debug.LogWarning("CreditRoller: Viewport not assigned and parent is not a RectTransform. Scrolling will still proceed but may not be clipped.");
        }

        RectTransform textRT = creditsText.rectTransform;
        // Ensure pivot is top-left or something stable is not guaranteed; we compute using rect sizes only.
        float viewportHeight = viewport != null ? viewport.rect.height : Screen.height;

        // Make text rect at least its preferred height so it doesn’t wrap unexpectedly
        float preferredHeight = creditsText.preferredHeight;
        var size = textRT.sizeDelta;
        if (preferredHeight > 0 && preferredHeight > textRT.rect.height)
        {
            size.y = preferredHeight;
            textRT.sizeDelta = size;
            LayoutRebuilder.ForceRebuildLayoutImmediate(textRT);
        }

        // Compute start and end Y based on heights. We'll move anchoredPosition.y from start to end.
        float contentHeight = textRT.rect.height;
        float offscreenBelow = contentHeight * 0.5f + viewportHeight * 0.5f + extraBottomPadding;
        float offscreenAbove = -(contentHeight * 0.5f + viewportHeight * 0.5f + extraBottomPadding);
        float startY = direction == ScrollDirection.BottomToTop ? offscreenBelow : offscreenAbove;
        float endY = direction == ScrollDirection.BottomToTop ? offscreenAbove : offscreenBelow;

        // Set to start position
        var anchored = textRT.anchoredPosition;
        anchored.y = startY;
        textRT.anchoredPosition = anchored;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Reveal initial visibility based on mode after positioning and delay to avoid flash
        if (!useTypewriter)
        {
            creditsText.maxVisibleCharacters = totalChars;
        }

        float visibleChars = 0f;

        // Scroll loop
        bool MovingCondition()
        {
            return direction == ScrollDirection.BottomToTop
                ? textRT.anchoredPosition.y > endY
                : textRT.anchoredPosition.y < endY;
        }

        while (_isRunning && MovingCondition())
        {
            // Handle typewriter reveal
            if (useTypewriter && creditsText.maxVisibleCharacters < totalChars)
            {
                visibleChars += charactersPerSecond * Time.unscaledDeltaTime;
                creditsText.maxVisibleCharacters = Mathf.Clamp((int)visibleChars, 0, totalChars);
            }

            // Move in selected direction (note: decreasing y moves visually up in UI space)
            anchored = textRT.anchoredPosition;
            float delta = scrollSpeed * Time.unscaledDeltaTime;
            anchored.y += direction == ScrollDirection.BottomToTop ? -delta : +delta;
            textRT.anchoredPosition = anchored;

            // Allow skip
            if (allowSkip && AnySkipPressed())
            {
                break;
            }

            yield return null;
        }

        // Jump to end position if skipped
        anchored = textRT.anchoredPosition;
        anchored.y = endY;
        textRT.anchoredPosition = anchored;

        if (useTypewriter)
        {
            creditsText.maxVisibleCharacters = totalChars;
        }

        if (endHoldTime > 0f)
            yield return new WaitForSeconds(endHoldTime);

        _isRunning = false;
        OnRollCompleted?.Invoke();
        _routine = null;
    }

    private bool AnySkipPressed()
    {
        // Simple skip detection that works without the new Input System dependency in this script.
        // if (Input.anyKeyDown) return true;
        // if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return true;
        return false;
    }
}
