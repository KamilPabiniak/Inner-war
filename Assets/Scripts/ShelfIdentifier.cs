using UnityEngine;

public class ShelfIdentifier : MonoBehaviour
{
    public string baseName;

    [Header("Text Coloring Options")]
    public bool useTextColor = false;
    public Color textColor = Color.white;

    [Header("Background Coloring Options")]
    public bool useBackgroundColor = false;
    public Color backgroundColor = Color.gray;
    [Tooltip("Corner radius for background.")]
    public float backgroundCornerRadius = 4f;

    [Header("Symbol Coloring Options")]
    [Tooltip("Color symbols with the same color as text.")]
    public bool colorSymbolsSame = false;
    [Tooltip("Color symbols with a different color.")]
    public bool colorSymbolsDifferent = false;
    public Color symbolsColor = Color.white;

    [Header("Special Symbol Configuration")]
    public SpecialSymbolType specialSymbolType = SpecialSymbolType.Default;
    [Tooltip("Custom symbol – used when 'Custom' is selected.")]
    public string customSymbol = "";
    [Tooltip("Display symbols on the left side.")]
    public bool displayLeftSymbols = true;
    [Tooltip("Display symbols on the right side.")]
    public bool displayRightSymbols = true;
    [Tooltip("Reduction percentage for left symbols when right symbols are disabled (100 = no reduction).")]
    public int reduceLeftSymbolsPercent = 100;
    [Tooltip("Reduction percentage for right symbols when left symbols are disabled (100 = no reduction).")]
    public int reduceRightSymbolsPercent = 100;

    // New option for text alignment when both symbol options are disabled.
    public enum TextAlignmentOption { Left, Center, Right }
    public TextAlignmentOption textAlignment = TextAlignmentOption.Left;

    public enum SpecialSymbolType
    {
        Default,
        Percent,
        Ampersand,
        At,
        Parentheses,
        Tilde,
        Custom
    }

    // New enum for choosing how the gradient is applied.
    public enum GradientDirection
    {
        LeftToRight,
        CenterOutward
    }

    [Header("Gradient Options")]
    [Tooltip("Use gradient for text instead of a single color.")]
    public bool useTextGradient = false;
    [Tooltip("Gradient colors for text.")]
    public Color[] textGradientColors = new Color[2] { Color.white, Color.white };
    [Tooltip("Direction of the text gradient.")]
    public GradientDirection textGradientDirection = GradientDirection.LeftToRight;

    [Tooltip("Use gradient for background instead of a single color.")]
    public bool useBackgroundGradient = false;
    [Tooltip("Gradient colors for background.")]
    public Color[] backgroundGradientColors = new Color[2] { Color.gray, Color.gray };
    [Tooltip("Direction of the background gradient.")]
    public GradientDirection backgroundGradientDirection = GradientDirection.CenterOutward;

    [Tooltip("Use gradient for symbols instead of a single color.")]
    public bool useSymbolsGradient = false;
    [Tooltip("Gradient colors for symbols.")]
    public Color[] symbolsGradientColors = new Color[2] { Color.white, Color.white };
    [Tooltip("Direction of the symbols gradient.")]
    public GradientDirection symbolsGradientDirection = GradientDirection.LeftToRight;

    private void OnValidate()
    {
        if (colorSymbolsSame && colorSymbolsDifferent)
        {
            colorSymbolsDifferent = false;
        }
        reduceLeftSymbolsPercent = Mathf.Clamp(reduceLeftSymbolsPercent, 0, 100);
        reduceRightSymbolsPercent = Mathf.Clamp(reduceRightSymbolsPercent, 0, 100);
    }
}
