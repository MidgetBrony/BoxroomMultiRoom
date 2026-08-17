using SteamShelf.Rooms;
using System;
using System.Collections.Generic;

namespace BoxroomMultiRoom
{
    /// <summary>
    /// Serializable identity of a placed BOXROOM door. This contains only stable
    /// save data; never persist a Unity GameObject or component reference.
    /// </summary>
    [Serializable]
    public sealed class DoorEndpoint
    {
        /// <summary>The BOXROOM save-slot number containing the door.</summary>
        public int Slot;

        /// <summary>The door-owning tile's horizontal grid coordinate.</summary>
        public int X;

        /// <summary>The door-owning tile's vertical grid coordinate.</summary>
        public int Y;

        /// <summary>The wall edge on which BOXROOM placed the door.</summary>
        public Facing Facing;

        /// <summary>Required by JSON.NET when deserializing the link file.</summary>
        public DoorEndpoint()
        {
        }

        /// <summary>Creates an endpoint from resolved BOXROOM room data.</summary>
        public DoorEndpoint(int slot, int x, int y, Facing facing)
        {
            Slot = slot;
            X = x;
            Y = y;
            Facing = facing;
        }

        /// <summary>A compact stable key useful for comparisons and log messages.</summary>
        public string Key =>
            $"{Slot}:{X}:{Y}:{(int)Facing}";

        /// <summary>
        /// Copies endpoint values so pending state and persisted links do not share
        /// a mutable instance supplied by UI or runtime code.
        /// </summary>
        public DoorEndpoint Clone() =>
            new DoorEndpoint(Slot, X, Y, Facing);
    }

    /// <summary>A directed door connection, optionally traversable in reverse.</summary>
    [Serializable]
    public sealed class DoorLink
    {
        /// <summary>The door from which the link was created.</summary>
        public DoorEndpoint Source;

        /// <summary>The destination door.</summary>
        public DoorEndpoint Target;

        /// <summary>Whether Target may also resolve back to Source.</summary>
        public bool TwoWay = true;
    }

    /// <summary>Root object written to rooms_link.json.</summary>
    [Serializable]
    public sealed class DoorLinkFile
    {
        /// <summary>Schema version reserved for future migrations.</summary>
        public int Version = 1;

        /// <summary>All configured door connections.</summary>
        public List<DoorLink> Links = new List<DoorLink>();
    }

    /// <summary>
    /// Carries an arrival endpoint through a destructive Unity scene reload.
    /// Static state survives while scene-owned GameObjects do not.
    /// </summary>
    internal static class PendingTeleport
    {
        public static DoorEndpoint Destination { get; private set; }

        public static bool HasDestination => Destination != null;

        public static void Set(DoorEndpoint destination)
        {
            Destination = destination?.Clone();
        }

        public static void Clear()
        {
            Destination = null;
        }
    }

    /// <summary>
    /// Carries the first half of the Shift-click linking workflow while the player
    /// selects and loads a different room.
    /// </summary>
    internal static class PendingDoorLink
    {
        public static DoorEndpoint Source { get; private set; }

        public static bool HasSource => Source != null;

        public static void SetSource(DoorEndpoint source)
        {
            Source = source?.Clone();
        }

        public static void Clear()
        {
            Source = null;
        }
    }
}
