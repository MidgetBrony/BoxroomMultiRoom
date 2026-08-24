using ModsPanel;
using SteamShelf.Save;
using System.Collections.Generic;

namespace BoxroomMultiRoom
{
    /// <summary>
    /// Builds MultiRoom's door picker through the shared ModsPanel UI framework.
    /// MultiRoom owns the workflow and data; ModsPanel owns rendering, scaling,
    /// scrolling, focus, cursor state, and BOXROOM visual resources.
    /// </summary>
    internal static class LinkMenu
    {
        private static ModMenu menu;
        private static bool preservePendingSourceOnClose;

        public static bool CreateTwoWayLink { get; private set; } = true;

        public static void OpenForSource(DoorEndpoint source)
        {
            PendingDoorLink.SetSource(source);
            preservePendingSourceOnClose = false;

            menu = ModsUi.CreateMenu(
                "Rusty.BoxroomMultiRoom.LinkDoor",
                "Link Door",
                "Source: " + GetRoomName(source.Slot) + "\n" +
                "Door " + source.X + ", " + source.Y + "\n" + source.Facing);
            menu.Eyebrow = "MULTIROOM";
            menu.CloseText = "CANCEL";
            menu.Closed = OnMenuClosed;
            menu.AddHeading("Choose a destination")
                .AddLabel("Select the room containing the other door.")
                .AddToggle(
                    "Create a two-way link",
                    () => CreateTwoWayLink,
                    value => CreateTwoWayLink = value)
                .AddSpacer(12f);

            List<int> slots = SaveManager.Instance?.GetAvailableSlots() ?? new List<int>();
            int destinationCount = 0;
            foreach (int slot in slots)
            {
                if (slot == source.Slot)
                    continue;

                int selectedSlot = slot;
                destinationCount++;
                menu.AddButton(
                    GetRoomName(selectedSlot),
                    () => SelectDestination(selectedSlot),
                    "SLOT " + selectedSlot);
            }

            if (destinationCount == 0)
            {
                menu.AddLabel(
                    "No other rooms were found. Create another room, then try again.");
            }

            menu.Show();
        }

        public static void ShowMessage(string text)
        {
            ModsUi.ShowToast(text, 4f);
        }

        private static void SelectDestination(int slot)
        {
            // The source must survive this intentional close and the scene load so
            // the next Shift-click can complete the two-step link operation.
            preservePendingSourceOnClose = true;
            menu?.Close();
            ShowMessage("Room loaded — Shift-click the destination door");
            Core.LoadSlot(slot);
        }

        private static void OnMenuClosed()
        {
            if (!preservePendingSourceOnClose)
                PendingDoorLink.Clear();

            preservePendingSourceOnClose = false;
            menu = null;
        }

        private static string GetRoomName(int slot)
        {
            SlotPreviewData preview = SaveManager.Instance?.GetPreviewData(slot);
            return preview == null || string.IsNullOrEmpty(preview.SlotName)
                ? "ROOM " + slot
                : preview.SlotName.ToUpperInvariant();
        }
    }
}
