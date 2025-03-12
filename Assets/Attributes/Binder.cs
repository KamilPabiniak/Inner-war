using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class Binder : PropertyAttribute
{
    public readonly string header;
    public readonly int fontSize; 
    public readonly float topSpace; 
    public readonly float bottomSpace; 
    public readonly string colorHex;
    public readonly TextAnchor alignment;
    public readonly FontStyle fontStyle;
    public readonly bool foldAll;

    public Binder(
        string header,
        int fontSize = 12,
        float topSpace = 5f,
        float bottomSpace = 5f,
        string colorHex = "#FFFFFF",
        TextAnchor alignment = TextAnchor.MiddleLeft,
        FontStyle fontStyle = FontStyle.Normal,
        bool foldAll = false)
    {
        this.header = header;
        this.fontSize = fontSize;
        this.topSpace = topSpace;
        this.bottomSpace = bottomSpace;
        this.colorHex = colorHex;
        this.alignment = alignment;
        this.fontStyle = fontStyle;
        this.foldAll = foldAll;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class StopFold : PropertyAttribute { }