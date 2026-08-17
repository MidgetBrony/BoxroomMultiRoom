using MelonLoader;
using Newtonsoft.Json;
using SteamShelf.Save;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BoxroomMultiRoom
{
    internal static class DoorLinkManager
    {
        private static DoorLinkFile data = new DoorLinkFile();

        public static string LinkPath { get; private set; }

        public static IReadOnlyList<DoorLink> Links => data.Links;

        public static void Initialize()
        {
            string savesDirectory =
                Path.Combine(Application.persistentDataPath, "Saves");

            Directory.CreateDirectory(savesDirectory);
            LinkPath = Path.Combine(savesDirectory, "rooms_link.json");
            Load();
        }

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

        private static bool SameDoor(DoorEndpoint a, DoorEndpoint b)
        {
            return a.Slot == b.Slot &&
                   a.X == b.X &&
                   a.Y == b.Y &&
                   a.Facing == b.Facing;
        }
    }
}
