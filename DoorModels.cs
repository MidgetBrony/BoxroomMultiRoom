using SteamShelf.Rooms;
using System;
using System.Collections.Generic;

namespace BoxroomMultiRoom
{
    [Serializable]
    public sealed class DoorEndpoint
    {
        public int Slot;
        public int X;
        public int Y;
        public Facing Facing;

        public DoorEndpoint()
        {
        }

        public DoorEndpoint(int slot, int x, int y, Facing facing)
        {
            Slot = slot;
            X = x;
            Y = y;
            Facing = facing;
        }

        public string Key =>
            $"{Slot}:{X}:{Y}:{(int)Facing}";

        public DoorEndpoint Clone() =>
            new DoorEndpoint(Slot, X, Y, Facing);
    }

    [Serializable]
    public sealed class DoorLink
    {
        public DoorEndpoint Source;
        public DoorEndpoint Target;
        public bool TwoWay = true;
    }

    [Serializable]
    public sealed class DoorLinkFile
    {
        public int Version = 1;
        public List<DoorLink> Links = new List<DoorLink>();
    }

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
