using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShelfIdentifier))]
public class ShelfIdentifierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ShelfIdentifier shelf = (ShelfIdentifier)target;

        // Edit base name.
        shelf.baseName = EditorGUILayout.TextField("Base Name", shelf.baseName);

        EditorGUILayout.Space();
        // Text color options.
        shelf.useTextColor = EditorGUILayout.Toggle("Use Text Color", shelf.useTextColor);
        if (shelf.useTextColor)
        {
            shelf.textColor = EditorGUILayout.ColorField("Text Color", shelf.textColor);
        }

        EditorGUILayout.Space();
        // Background color options.
        shelf.useBackgroundColor = EditorGUILayout.Toggle("Use Background Color", shelf.useBackgroundColor);
        if (shelf.useBackgroundColor)
        {
            shelf.backgroundColor = EditorGUILayout.ColorField("Background Color", shelf.backgroundColor);
        }
        shelf.roundCorner = EditorGUILayout.Toggle("Round Corner", shelf.roundCorner);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Symbol Coloring Options", EditorStyles.boldLabel);
        bool newSymbolsSame = EditorGUILayout.Toggle("Color Symbols Same as Text", shelf.colorSymbolsSame);
        bool newSymbolsDifferent = EditorGUILayout.Toggle("Color Symbols Different", shelf.colorSymbolsDifferent);
        if (newSymbolsSame && newSymbolsDifferent)
        {
            newSymbolsDifferent = false;
        }
        shelf.colorSymbolsSame = newSymbolsSame;
        shelf.colorSymbolsDifferent = newSymbolsDifferent;
        if (shelf.colorSymbolsDifferent)
        {
            shelf.symbolsColor = EditorGUILayout.ColorField("Symbols Color", shelf.symbolsColor);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Special Symbol Configuration", EditorStyles.boldLabel);
        shelf.specialSymbolType = (ShelfIdentifier.SpecialSymbolType)EditorGUILayout.EnumPopup("Symbol Type", shelf.specialSymbolType);
        if (shelf.specialSymbolType == ShelfIdentifier.SpecialSymbolType.Custom)
        {
            shelf.customSymbol = EditorGUILayout.TextField("Custom Symbol", shelf.customSymbol);
        }
        shelf.displayLeftSymbols = EditorGUILayout.Toggle("Display Left Symbols", shelf.displayLeftSymbols);
        shelf.displayRightSymbols = EditorGUILayout.Toggle("Display Right Symbols", shelf.displayRightSymbols);

        if (shelf.displayLeftSymbols && !shelf.displayRightSymbols)
        {
            shelf.reduceLeftSymbolsPercent = EditorGUILayout.IntSlider("Left Symbols Reduction (%)", shelf.reduceLeftSymbolsPercent, 0, 100);
        }
        if (shelf.displayRightSymbols && !shelf.displayLeftSymbols)
        {
            shelf.reduceRightSymbolsPercent = EditorGUILayout.IntSlider("Right Symbols Reduction (%)", shelf.reduceRightSymbolsPercent, 0, 100);
        }
        if (!shelf.displayLeftSymbols && !shelf.displayRightSymbols)
        {
            shelf.textAlignment = (ShelfIdentifier.TextAlignmentOption)EditorGUILayout.EnumPopup("Text Alignment", shelf.textAlignment);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gradient Options", EditorStyles.boldLabel);
        // Text Gradient
        shelf.useTextGradient = EditorGUILayout.Toggle("Use Text Gradient", shelf.useTextGradient);
        if (shelf.useTextGradient)
        {
            shelf.textGradientDirection = (ShelfIdentifier.GradientDirection)EditorGUILayout.EnumPopup("Text Gradient Direction", shelf.textGradientDirection);
            SerializedObject soText = new SerializedObject(shelf);
            SerializedProperty textGradProp = soText.FindProperty("textGradientColors");
            EditorGUILayout.PropertyField(textGradProp, true);
            soText.ApplyModifiedProperties();
        }
        // Background Gradient
        shelf.useBackgroundGradient = EditorGUILayout.Toggle("Use Background Gradient", shelf.useBackgroundGradient);
        if (shelf.useBackgroundGradient)
        {
            shelf.backgroundGradientDirection = (ShelfIdentifier.GradientDirection)EditorGUILayout.EnumPopup("Background Gradient Direction", shelf.backgroundGradientDirection);
            SerializedObject soBg = new SerializedObject(shelf);
            SerializedProperty bgGradProp = soBg.FindProperty("backgroundGradientColors");
            EditorGUILayout.PropertyField(bgGradProp, true);
            soBg.ApplyModifiedProperties();
        }
        // Symbols Gradient
        shelf.useSymbolsGradient = EditorGUILayout.Toggle("Use Symbols Gradient", shelf.useSymbolsGradient);
        if (shelf.useSymbolsGradient)
        {
            shelf.symbolsGradientDirection = (ShelfIdentifier.GradientDirection)EditorGUILayout.EnumPopup("Symbols Gradient Direction", shelf.symbolsGradientDirection);
            SerializedObject soSymbols = new SerializedObject(shelf);
            SerializedProperty symGradProp = soSymbols.FindProperty("symbolsGradientColors");
            EditorGUILayout.PropertyField(symGradProp, true);
            soSymbols.ApplyModifiedProperties();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(shelf);
        }
    }
}
