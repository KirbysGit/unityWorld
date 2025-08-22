using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private bool useFadeAnimation = true;
    
    [Header("Styling")]
    [SerializeField] private string defaultPrompt = "Press E to interact";
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    
    private Coroutine fadeCoroutine;
    
    private void Start()
    {
        // Get components if not assigned
        if (promptText == null)
            promptText = GetComponentInChildren<TextMeshProUGUI>();
            
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
            
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        // Setup default appearance
        SetupDefaultAppearance();
        
        // Start hidden
        SetVisible(false, false);
    }
    
    private void SetupDefaultAppearance()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        if (promptText != null)
        {
            promptText.text = defaultPrompt;
            promptText.color = textColor;
        }
    }
    
    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = string.IsNullOrEmpty(text) ? defaultPrompt : text;
        }
    }
    
    public void SetVisible(bool visible, bool animate = true)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (animate && useFadeAnimation)
        {
            fadeCoroutine = StartCoroutine(FadeTo(visible ? 1f : 0f, visible ? fadeInDuration : fadeOutDuration));
        }
        else
        {
            SetAlpha(visible ? 1f : 0f);
        }
    }
    
    private System.Collections.IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            
            SetAlpha(currentAlpha);
            
            yield return null;
        }
        
        SetAlpha(targetAlpha);
        fadeCoroutine = null;
    }
    
    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
        else
        {
            // Fallback: modify individual UI elements
            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = alpha;
                backgroundImage.color = color;
            }
            
            if (promptText != null)
            {
                Color color = promptText.color;
                color.a = alpha;
                promptText.color = color;
            }
        }
    }
    
    // Public methods for external control
    public void Show(string customPrompt = null)
    {
        if (!string.IsNullOrEmpty(customPrompt))
        {
            SetPromptText(customPrompt);
        }
        SetVisible(true);
    }
    
    public void Hide()
    {
        SetVisible(false);
    }
    
    // Optional: Add some visual feedback
    public void Pulse()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        fadeCoroutine = StartCoroutine(PulseAnimation());
    }
    
    private System.Collections.IEnumerator PulseAnimation()
    {
        float originalAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        
        // Quick fade out and in
        yield return StartCoroutine(FadeTo(0.5f, 0.1f));
        yield return StartCoroutine(FadeTo(originalAlpha, 0.1f));
        
        fadeCoroutine = null;
    }
}
