// Plik: Assets/Editor/CursorHotspotEditor.cs
using UnityEngine;
using UnityEditor;

public class CursorHotspotEditor : EditorWindow
{
    private Texture2D cursorTexture;
    private Vector2 hotspot = Vector2.zero;

    // Rozmiar wyœwietlanego podgl¹du w oknie (w pikselach GUI)
    private const float previewSize = 256f;

    // Przesuniêcie myszki wzglêdem obiektu GUI (do obliczeñ)
    private Vector2 scrollPos;

    [MenuItem("Tools/Cursor Hotspot Visualizer")]
    public static void ShowWindow()
    {
        var window = GetWindow<CursorHotspotEditor>("Hotspot Visualizer");
        window.minSize = new Vector2(300, 350);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // 1. Pole do wybrania tekstury kursora
        EditorGUILayout.LabelField("Wybierz teksturê kursora:", EditorStyles.boldLabel);
        cursorTexture = (Texture2D)EditorGUILayout.ObjectField(cursorTexture, typeof(Texture2D), false);

        if (cursorTexture == null)
        {
            EditorGUILayout.HelpBox("Przeci¹gnij tutaj teksturê (Texture2D) kursora, np. 32×32 PNG.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField(
            $"Rozmiar Ÿród³owy: {cursorTexture.width}×{cursorTexture.height} px",
            EditorStyles.miniLabel
        );
        GUILayout.Space(5);

        // 2. Pole, w którym wyœwietlimy podgl¹d tekstury w sta³ej, np. 256×256 px
        Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));
        GUI.Box(previewRect, GUIContent.none); // obramowanie

        // Obliczamy wspó³czynnik skalowania, by dopasowaæ teksturê do previewSize
        float scale = Mathf.Min(previewSize / cursorTexture.width, previewSize / cursorTexture.height);
        float dispWidth = cursorTexture.width * scale;
        float dispHeight = cursorTexture.height * scale;
        Rect texRect = new Rect(
            previewRect.x + (previewSize - dispWidth) / 2f,
            previewRect.y + (previewSize - dispHeight) / 2f,
            dispWidth,
            dispHeight
        );

        // Wyœwietlamy teksturê
        EditorGUI.DrawPreviewTexture(texRect, cursorTexture);

        // Jeœli u¿ytkownik klika wewn¹trz texRect, przeliczamy piksele na wspó³rzêdne hotspotu
        Vector2 mousePos = Event.current.mousePosition;
        if (Event.current.type == EventType.MouseDown && texRect.Contains(mousePos))
        {
            // Przekszta³camy GUI->prawdziwe piksele: (mousePos - texRect.min) / scale
            Vector2 localOnTex = (mousePos - new Vector2(texRect.x, texRect.y)) / scale;
            // Odwracamy oœ Y, bo GUI ma (0,0) w lewym górnym rogu, a piksele w teksturze te¿ liczone od góry
            float px = Mathf.Clamp(localOnTex.x, 0, cursorTexture.width - 1);
            float py = Mathf.Clamp(localOnTex.y, 0, cursorTexture.height - 1);

            hotspot = new Vector2(Mathf.Round(px), Mathf.Round(py));

            Repaint();
        }

        // 3. Rysujemy marker (np. kó³ko) w miejscu hotspotu
        Vector2 hotspotInGUI = new Vector2(
            texRect.x + (hotspot.x * scale),
            texRect.y + (hotspot.y * scale)
        );
        float markerSize = 10f;
        Rect markerRect = new Rect(
            hotspotInGUI.x - markerSize / 2f,
            hotspotInGUI.y - markerSize / 2f,
            markerSize,
            markerSize
        );
        EditorGUI.DrawRect(markerRect, Color.red);

        // 4. Pokazujemy aktualne wartoœci hotspotu w pikselach
        GUILayout.Space(previewSize + 10 - GUILayoutUtility.GetLastRect().y);
        EditorGUILayout.LabelField("Aktualny hotspot (px):", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("X:");
        hotspot.x = EditorGUILayout.IntField((int)hotspot.x);
        EditorGUILayout.PrefixLabel("Y:");
        hotspot.y = EditorGUILayout.IntField((int)hotspot.y);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 5. Przycisk do skopiowania kodu
        if (GUILayout.Button("Skopiuj kod do u¿ycia w Cursor.SetCursor"))
        {
            string code = $"Cursor.SetCursor(myCursorTexture, new Vector2({(int)hotspot.x}f, {(int)hotspot.y}f), CursorMode.Auto);";
            EditorGUIUtility.systemCopyBuffer = code;
            Debug.Log($"Skopiowano do schowka: {code}");
        }

        // 6. Porada
        EditorGUILayout.HelpBox(
            "Kliknij w wyœwietlon¹ teksturê, aby ustawiæ hotspot. Mo¿esz te¿ wpisaæ rêcznie X i Y.\n"
            + "Nastêpnie kliknij „Skopiuj kod...”, by wkleiæ to do swojego skryptu.",
            MessageType.Info
        );
    }
}
