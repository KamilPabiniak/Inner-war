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
    public readonly bool foldable;
    public readonly bool hideMe;
    public Color color; 

    public AdvancedHeader(
        string header,
        int fontSize = 12,
        float topSpace = 5f,
        float bottomSpace = 5f,
        float r = 192f,
        float g = 192f,
        float b = 192f,
        TextAnchor alignment = TextAnchor.MiddleLeft,
        FontStyle fontStyle = FontStyle.Normal,
        bool foldable = false,
        bool hideMe = false)
    {
        this.header = header;
        this.fontSize = fontSize;
        this.topSpace = topSpace;
        this.bottomSpace = bottomSpace;
        color = new Color(r, g, b);
        this.alignment = alignment;
        this.fontStyle = fontStyle;
        this.foldable = foldable;
        this.hideMe = hideMe;
    }
}