using MelonLoader;
using Newtonsoft.Json;
using SteamShelf.Save;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BoxroomMultiRoom
{
    /// <summary>
    /// Repository for the JSON link file. Keeping persistence behind this class
    /// prevents Harmony and UI code from depending on the on-disk format.
    /// </summary>
    internal static class DoorLinkManager
    {
        private static DoorLinkFile data = new DoorLinkFile();

        public static string LinkPath { get; private set; }

        public static IReadOnlyList<DoorLink> Links => data.Links;

        /// <summary>
        /// Places mod data beside BOXROOM's save data using Unity's portable
        /// persistentDataPath rather than assuming a Steam installation path.
        /// </summary>
        public static void Initialize()
        {
            string savesDirectory =
                Path.Combine(Application.persistentDataPath, "Saves");

            Directory.CreateDirectory(savesDirectory);
            LinkPath = Path.Combine(savesDirectory, "rooms_link.json");
            Load();
        }

        /// <summary>
        /// Reads the link file. A missing or invalid file produces an empty in-memory
        /// model so corrupt optional mod data does not prevent BOXROOM from starting.
        /// </summary>
        public static void Load()
        {
            try
            {
                if (!File.Exists(LinkPath))
                {
                    data = new DoorLinkFile();
                    Save();
                    return;
                }

                string json = File.ReadAllText(LinkPath);
                data = JsonConvert.DeserializeObject<DoorLinkFile>(json)
                       ?? new DoorLinkFile();

                data.Links ??= new List<DoorLink>();

                MelonLogger.Msg(
                    $"[MultiRoom] Loaded {data.Links.Count} door link(s).");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[MultiRoom] Failed to load rooms_link.json: {ex}");
                data = new DoorLinkFile();
            }
        }

        /// <summary>
        /// Writes indented JSON for easy backup, inspection, and manual recovery.
        /// </summary>
        public static void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(LinkPath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(
                    data,
                    Formatting.Indented);

                File.WriteAllText(LinkPath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[MultiRoom] Failed to save rooms_link.json: {ex}");
            }
        }

        /// <summary>
        /// Resolves outbound travel. Two-way links may also be traversed from their
        /// target, but one-way links match their source only.
        /// </summary>
        public static bool TryGetTarget(
            DoorEndpoint source,
            out DoorEndpoint target)
        {
            target = null;

            if (source == null)
                return false;

            foreach (DoorLink link in data.Links)
            {
                if (link?.Source == null || link.Target == null)
                    continue;

                if (SameDoor(link.Source, source))
                {
                    target = link.Target.Clone();
                    return true;
                }

                if (link.TwoWay && SameDoor(link.Target, source))
                {
                    target = link.Source.Clone();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gives a source door one unambiguous destination. For a two-way pair, the
        /// target cannot simultaneously remain the source of an older link.
        /// </summary>
        public static void AddOrReplace(
            DoorEndpoint source,
            DoorEndpoint target,
            bool twoWay)
        {
            if (source == null || target == null)
                throw new ArgumentNullException();

            data.Links.RemoveAll(link =>
                link?.Source != null &&
                SameDoor(link.Source, source));

            if (twoWay)
            {
                data.Links.RemoveAll(link =>
                    link?.Source != null &&
                    SameDoor(link.Source, target));
            }

            data.Links.Add(new DoorLink
            {
                Source = source.Clone(),
                Target = target.Clone(),
                TwoWay = twoWay
            });

            Save();

            MelonLogger.Msg(
                $"[MultiRoom] Linked {source.Key} -> {target.Key}" +
                (twoWay ? " (two-way)." : "."));
        }

        /// <summary>
        /// Removes every link involving an endpoint, regardless of direction.
        /// This is ready for a future in-game link-management screen.
        /// </summary>
        public static bool RemoveLink(DoorEndpoint endpoint)
        {
            int removed = data.Links.RemoveAll(link =>
                (link?.Source != null && SameDoor(link.Source, endpoint)) ||
                (link?.Target != null && SameDoor(link.Target, endpoint)));

            if (removed > 0)
            {
                Save();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Door identity is structural: room slot, grid coordinate, and wall facing.
        /// Runtime GameObjects cannot be serialized safely across scene loads.
        /// </summary>
        private static bool SameDoor(DoorEndpoint a, DoorEndpoint b)
        {
            return a.Slot == b.Slot &&
                   a.X == b.X &&
                   a.Y == b.Y &&
                   a.Facing == b.Facing;
        }
    }
}
