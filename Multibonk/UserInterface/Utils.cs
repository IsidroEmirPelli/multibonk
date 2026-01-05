using MelonLoader;
using UnityEngine;

public static class Utils
{
    public static void HandleWindowDrag(ref Rect window, ref bool dragging, ref Vector2 dragOffset)
    {
        Event e = Event.current;
        Rect dragBar = new Rect(window.x, window.y, window.width, 20);

        if (e.type == EventType.MouseDown && dragBar.Contains(e.mousePosition))
        {
            dragging = true;
            dragOffset = e.mousePosition - new Vector2(window.x, window.y);
            e.Use();
        }
        else if (e.type == EventType.MouseUp)
        {
            dragging = false;
        }

        if (dragging && e.type == EventType.MouseDrag)
        {
            window.position = e.mousePosition - dragOffset;
            e.Use();
        }
    }

    public static void HandleTextFieldInput(ref string text, ref bool isFocused, Rect rect)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown)
        {
            isFocused = rect.Contains(e.mousePosition);
        }

        if (isFocused && e.type == EventType.KeyDown)
        {
            // Manejar Ctrl+V o Cmd+V (Pegar)
            if ((e.control || e.command) && e.keyCode == KeyCode.V)
            {
                string clipboard = GUIUtility.systemCopyBuffer;
                if (!string.IsNullOrEmpty(clipboard))
                {
                    text += clipboard;
                }
                e.Use();
                return;
            }

            // Manejar Ctrl+C o Cmd+C (Copiar)
            if ((e.control || e.command) && e.keyCode == KeyCode.C)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    GUIUtility.systemCopyBuffer = text;
                }
                e.Use();
                return;
            }

            // Manejar Ctrl+X o Cmd+X (Cortar)
            if ((e.control || e.command) && e.keyCode == KeyCode.X)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    GUIUtility.systemCopyBuffer = text;
                    text = "";
                }
                e.Use();
                return;
            }

            // Manejar Ctrl+A o Cmd+A (Seleccionar todo - copia el texto)
            if ((e.control || e.command) && e.keyCode == KeyCode.A)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    GUIUtility.systemCopyBuffer = text;
                }
                e.Use();
                return;
            }

            // Manejar Backspace
            if (e.keyCode == KeyCode.Backspace && text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
                e.Use();
                return;
            }

            // Manejar Delete
            if (e.keyCode == KeyCode.Delete && text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
                e.Use();
                return;
            }

            // Manejar caracteres normales
            if (e.character != '\0' && !char.IsControl(e.character))
            {
                text += e.character;
                e.Use();
            }
        }
    }

    public static string CustomTextField(string currentText, ref bool isFocused, Rect rect)
    {
        // not working
        Rect calculated = GUILayoutUtility.GetRect(new GUIContent(currentText), GUI.skin.textField, GUILayout.Width(rect.width), GUILayout.Height(rect.height));

        HandleTextFieldInput(ref currentText, ref isFocused, calculated);
        GUI.Box(calculated, currentText);
        return currentText;
    }
}