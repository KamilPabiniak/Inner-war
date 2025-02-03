using UnityEditor;
using UnityEngine;

public class AdvancedHeader : PropertyAttribute
{
    public readonly string header;
    public readonly int fontSize; 
    public readonly float topSpace; 
    public readonly float bottomSpace; 
    public readonly TextAnchor alignment;
    public readonly FontStyle fontStyle;
    public readonly bool isFoldable;
    public readonly bool foldEverything;
    public readonly string colorHex;

    public AdvancedHeader(
        string header,
        int fontSize = 12,
        float topSpace = 5f,
        float bottomSpace = 5f,
        string colorHex = "#FFFFFF",
        TextAnchor alignment = TextAnchor.MiddleLeft,
        FontStyle fontStyle = FontStyle.Normal,
        bool isFoldable = false,
        bool foldEverything = false)
    {
        this.header = header;
        this.fontSize = fontSize;
        this.topSpace = topSpace;
        this.bottomSpace = bottomSpace;
        this.colorHex = colorHex;
        this.alignment = alignment;
        this.fontStyle = fontStyle;
        this.isFoldable = isFoldable;
        this.foldEverything = foldEverything;
    }
}