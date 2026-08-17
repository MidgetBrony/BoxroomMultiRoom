using SteamShelf.Save;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxroomMultiRoom
{
    internal static class LinkMenu
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        // Colours sampled from BOXROOM's own SelectableColorSet assets and save-slot prefab.
        private static readonly Color Ink = new Color(0.1255f, 0.2392f, 0.2510f, 1f);
        private static readonly Color DeepBlue = new Color(0.1294f, 0.3020f, 0.3529f, 1f);
        private static readonly Color Blue = new Color(0.2353f, 0.4627f, 0.4863f, 1f);
        private static readonly Color Outline = new Color(0.2471f, 0.3647f, 0.3765f, 1f);
        private static readonly Color Paper = new Color(0.9529f, 0.9137f, 0.9137f, 1f);
        private static readonly Color Red = new Color(0.7373f, 0.3373f, 0.3059f, 1f);

        private static Vector2 scroll;
        private static bool visible;
        private static string message;
        private static float messageUntil;
        private static CursorLockMode oldLockMode;
        private static bool oldCursorVisible;

        private static Texture2D whiteTexture;
        private static Texture2D backgroundTexture;
        private static GUIStyle titleStyle;
        private static GUIStyle eyebrowStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle detailStyle;
        private static GUIStyle roomButtonStyle;
        private static GUIStyle cancelButtonStyle;
        private static GUIStyle toastStyle;
        private static GUIStyle toggleLabelStyle;

        public static bool CreateTwoWayLink { get; private set; } = true;

        public static void OpenForSource(DoorEndpoint source)
        {
            PendingDoorLink.SetSource(source);
            visible = true;
            scroll = Vector2.zero;
            message = null;

            oldLockMode = Cursor.lockState;
            oldCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void ShowMessage(string text)
        {
            message = text;
            messageUntil = Time.realtimeSinceStartup + 4f;
        }

        public static void Draw()
        {
            EnsureStyles();

            float scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            if (scale <= 0f)
                return;

            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3((Screen.width - ReferenceWidth * scale) * 0.5f,
                            (Screen.height - ReferenceHeight * scale) * 0.5f, 0f),
                Quaternion.identity,
                new Vector3(scale, scale, 1f));

            if (!string.IsNullOrEmpty(message) && Time.realtimeSinceStartup < messageUntil)
                DrawToast(message);

            if (visible)
                DrawMenu();

            GUI.matrix = previousMatrix;
        }

        private static void DrawMenu()
        {
            DoorEndpoint source = PendingDoorLink.Source;
            if (source == null)
            {
                Close(clearPending: false);
                return;
            }

            Event current = Event.current;
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                Close(clearPending: true);
                current.Use();
                return;
            }

            // The real pause/load UI fills the screen with a muted patterned layer.
            GUI.color = new Color(0.035f, 0.075f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(0f, 0f, ReferenceWidth, ReferenceHeight),
                backgroundTexture != null ? backgroundTexture : whiteTexture, ScaleMode.StretchToFill);
            GUI.color = Color.white;

            Rect panel = new Rect(270f, 105f, 1380f, 870f);
            DrawRect(panel, Paper);
            DrawRect(new Rect(panel.x, panel.y, 360f, panel.height), DeepBlue);
            DrawRect(new Rect(panel.x + 360f, panel.y, 8f, panel.height), Blue);

            GUI.Label(new Rect(325f, 175f, 250f, 42f), "MULTIROOM", eyebrowStyle);
            GUI.Label(new Rect(325f, 218f, 250f, 125f), "LINK\nDOOR", titleStyle);
            DrawRect(new Rect(325f, 365f, 190f, 6f), Blue);

            GUI.Label(new Rect(325f, 410f, 250f, 30f), "SOURCE DOOR", eyebrowStyle);
            GUI.Label(new Rect(325f, 450f, 245f, 115f),
                GetRoomName(source.Slot) + "\n" +
                "Door " + source.X + ", " + source.Y + "\n" +
                source.Facing,
                detailStyle);

            GUI.Label(new Rect(325f, 640f, 250f, 30f), "LINK OPTIONS", eyebrowStyle);
            Rect toggleRect = new Rect(325f, 688f, 245f, 54f);
            if (GUI.Button(toggleRect, GUIContent.none, GUIStyle.none))
                CreateTwoWayLink = !CreateTwoWayLink;
            DrawRect(new Rect(toggleRect.x, toggleRect.y + 7f, 70f, 38f),
                CreateTwoWayLink ? Blue : Outline);
            DrawRect(new Rect(toggleRect.x + (CreateTwoWayLink ? 38f : 4f), toggleRect.y + 11f, 30f, 30f), Paper);
            GUI.Label(new Rect(toggleRect.x + 85f, toggleRect.y, 160f, toggleRect.height),
                "TWO-WAY", toggleLabelStyle);

            if (GUI.Button(new Rect(325f, 835f, 245f, 70f), "CANCEL  [ESC]", cancelButtonStyle))
                Close(clearPending: true);

            GUI.Label(new Rect(705f, 165f, 780f, 56f), "CHOOSE A DESTINATION", titleStyle);
            GUI.Label(new Rect(710f, 225f, 760f, 45f),
                "Select the room containing the other door.", bodyStyle);

            List<int> slots = SaveManager.Instance?.GetAvailableSlots() ?? new List<int>();
            List<int> destinations = new List<int>();
            foreach (int slot in slots)
            {
                if (slot != source.Slot)
                    destinations.Add(slot);
            }

            Rect viewRect = new Rect(700f, 300f, 855f, 590f);
            float rowHeight = 104f;
            float contentHeight = Mathf.Max(viewRect.height, destinations.Count * rowHeight);
            scroll = GUI.BeginScrollView(viewRect, scroll,
                new Rect(0f, 0f, viewRect.width - 24f, contentHeight), false, true);

            if (destinations.Count == 0)
            {
                GUI.Label(new Rect(10f, 30f, 760f, 80f),
                    "NO OTHER ROOMS FOUND\nCreate another room first, then try again.", bodyStyle);
            }

            for (int index = 0; index < destinations.Count; index++)
            {
                int slot = destinations[index];
                Rect buttonRect = new Rect(5f, index * rowHeight, 790f, 84f);
                string roomName = GetRoomName(slot);
                if (GUI.Button(buttonRect, roomName + "\n<size=20>SLOT " + slot + "</size>", roomButtonStyle))
                {
                    visible = false;
                    RestoreCursor();
                    ShowMessage("Room loaded — Shift-click the destination door");
                    Core.LoadSlot(slot);
                    GUIUtility.ExitGUI();
                }
            }

            GUI.EndScrollView();
        }

        private static string GetRoomName(int slot)
        {
            SlotPreviewData preview = SaveManager.Instance?.GetPreviewData(slot);
            return preview == null || string.IsNullOrEmpty(preview.SlotName)
                ? "ROOM " + slot
                : preview.SlotName.ToUpperInvariant();
        }

        private static void DrawToast(string text)
        {
            Rect toast = new Rect(560f, 52f, 800f, 72f);
            DrawRect(new Rect(toast.x - 5f, toast.y - 5f, toast.width + 10f, toast.height + 10f), Paper);
            DrawRect(toast, DeepBlue);
            GUI.Label(toast, text.ToUpperInvariant(), toastStyle);
        }

        private static void Close(bool clearPending)
        {
            visible = false;
            if (clearPending)
                PendingDoorLink.Clear();
            RestoreCursor();
        }

        private static void RestoreCursor()
        {
            Cursor.lockState = oldLockMode;
            Cursor.visible = oldCursorVisible;
        }

        private static void EnsureStyles()
        {
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                whiteTexture.name = "BoxroomMultiRoom_White";
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
            }

            if (backgroundTexture == null)
            {
                Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>();
                foreach (Texture2D texture in textures)
                {
                    if (texture != null && texture.name == "PauseMenuBackgroundPattern")
                    {
                        backgroundTexture = texture;
                        break;
                    }
                }
            }

            if (titleStyle != null)
                return;

            Font boxroomFont = FindBoxroomFont();
            titleStyle = TextStyle(boxroomFont, 50, Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            titleStyle.richText = true;
            eyebrowStyle = TextStyle(boxroomFont, 22, Paper, FontStyle.Bold, TextAnchor.MiddleLeft);
            bodyStyle = TextStyle(boxroomFont, 28, Ink, FontStyle.Normal, TextAnchor.UpperLeft);
            detailStyle = TextStyle(boxroomFont, 28, Paper, FontStyle.Normal, TextAnchor.UpperLeft);
            detailStyle.wordWrap = true;
            toggleLabelStyle = TextStyle(boxroomFont, 25, Paper, FontStyle.Bold, TextAnchor.MiddleLeft);
            toastStyle = TextStyle(boxroomFont, 25, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);

            roomButtonStyle = new GUIStyle(GUI.skin.button)
            {
                font = boxroomFont,
                fontSize = 31,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                padding = new RectOffset(32, 20, 8, 8),
                border = new RectOffset(0, 0, 0, 0)
            };
            SetStyleState(roomButtonStyle.normal, Paper, Ink);
            SetStyleState(roomButtonStyle.hover, Blue, Paper);
            SetStyleState(roomButtonStyle.active, DeepBlue, Paper);
            SetStyleState(roomButtonStyle.focused, Blue, Paper);

            cancelButtonStyle = new GUIStyle(roomButtonStyle)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 8, 8)
            };
            SetStyleState(cancelButtonStyle.normal, Red, Paper);
            SetStyleState(cancelButtonStyle.hover, Paper, Red);
            SetStyleState(cancelButtonStyle.active, Ink, Paper);
            SetStyleState(cancelButtonStyle.focused, Paper, Red);
        }

        private static Font FindBoxroomFont()
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (Font font in fonts)
            {
                if (font != null && font.name.IndexOf("Lilita", StringComparison.OrdinalIgnoreCase) >= 0)
                    return font;
            }
            return GUI.skin.font;
        }

        private static GUIStyle TextStyle(Font font, int size, Color color,
            FontStyle fontStyle, TextAnchor alignment)
        {
            return new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = size,
                fontStyle = fontStyle,
                alignment = alignment,
                normal = { textColor = color }
            };
        }

        private static void SetStyleState(GUIStyleState state, Color background, Color text)
        {
            state.background = whiteTexture;
            state.textColor = text;
            // GUIStyle cannot tint individual state textures, so the native palette is
            // applied through a generated per-colour texture.
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "BoxroomMultiRoom_Style";
            texture.SetPixel(0, 0, background);
            texture.Apply();
            state.background = texture;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previous;
        }
    }
}
