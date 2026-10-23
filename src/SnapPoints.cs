// Snap points for pieces that lack them. The game snaps pieces point to point, using the "snappoint" children each
// piece carries. Many mods' pieces have none, or only one or two in the middle (Modular Ladders' ladders have just a
// top and a bottom on their centre line, so they can't sit flush against a wall). This works out points from each
// piece's own shape: the corners of its outer box, the middle of its top and bottom edges, and its top and bottom
// centres, each named for where it is ("Bottom back left corner"). It only changes your own game: the points are added
// to pieces as they appear (the placement ghost included), never to the saved world.
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace PotatoHammer
{
    internal static class SnapPoints
    {
        private const string Section = "3 - Snap points";
        private const string Prefix = "Potato snap: ";
        private static ConfigEntry<bool> on, automatic;
        private static ConfigEntry<int> fewest;
        private static ConfigEntry<KeyboardShortcut> key;
        private static ConfigEntry<string> chosen, refused;
        // The game's own prefabs, recorded before any mod adds its own; only mods' pieces get points automatically.
        private static readonly HashSet<string> Vanilla = new HashSet<string>();
        private static readonly Dictionary<string, List<KeyValuePair<string, Vector3>>> Points = new Dictionary<string, List<KeyValuePair<string, Vector3>>>();
        private static readonly Dictionary<string, bool> Automatic = new Dictionary<string, bool>();
        private static readonly AccessTools.FieldRef<List<Piece>> AllPieces = AccessTools.StaticFieldRefAccess<List<Piece>>(AccessTools.Field(typeof(Piece), "s_allPieces"));

        internal static void Bind(ConfigFile config)
        {
            on = config.Bind(Section, "Add snap points", true,
                "Give pieces that lack them snap points from their shape: corners, edge middles, top and bottom centres. Only your own building changes.");
            automatic = config.Bind(Section, "Automatic for mods' pieces", true,
                "Mods' pieces with only one or two snap points of their own get them without asking. Pieces with none (usually decorations meant to go anywhere) and the game's own pieces only get them when you pick them.");
            fewest = config.Bind(Section, "Automatic up to", 2,
                new ConfigDescription("Mods' pieces with at least one and at most this many snap points of their own get them automatically.", new AcceptableValueRange<int>(1, 8)));
            key = config.Bind(Section, "Pick key", new KeyboardShortcut(KeyCode.K, KeyCode.LeftControl),
                "With the hammer out, look at a built piece and press this to give every piece of its kind snap points, or take them away again. The console command snappoints does the same.");
            chosen = config.Bind(Section, "Picked pieces", "",
                "Kept by the Pick key and the snappoints command (the pieces' internal names); no need to edit.");
            refused = config.Bind(Section, "Turned off pieces", "",
                "Kept by the Pick key and the snappoints command (the pieces' internal names); no need to edit.");
            on.SettingChanged += (_, __) => RefreshAll();
            automatic.SettingChanged += (_, __) => { Automatic.Clear(); RefreshAll(); };
            fewest.SettingChanged += (_, __) => { Automatic.Clear(); RefreshAll(); };
            try { CommandManager.Instance.AddConsoleCommand(new Command()); }
            catch (Exception e) { Plugin.Log.LogWarning("Couldn't add the snappoints command: " + e.Message); }
        }

        private static HashSet<string> List(ConfigEntry<string> entry) =>
            new HashSet<string>(entry.Value.Split(',').Select(part => part.Trim()).Where(part => part.Length > 0));

        private static void Save(ConfigEntry<string> entry, HashSet<string> names) => entry.Value = string.Join(",", names.OrderBy(name => name));

        private static bool Wants(string prefab)
        {
            if (!on.Value) return false;
            if (List(refused).Contains(prefab)) return false;
            if (List(chosen).Contains(prefab)) return true;
            return automatic.Value && IsAutomatic(prefab);
        }

        private static bool IsAutomatic(string prefab)
        {
            if (Automatic.TryGetValue(prefab, out bool known)) return known;
            bool result = false;
            var source = ZNetScene.instance ? ZNetScene.instance.GetPrefab(prefab) : null;
            if (source && Vanilla.Count > 0 && !Vanilla.Contains(prefab) && source.GetComponent<Piece>())
                { int count = Own(source.transform).Count; result = count >= 1 && count <= fewest.Value; }
            Automatic[prefab] = result;
            if (result) Plugin.Log.LogInfo($"Snap points added to {prefab} automatically (it had only a few of its own).");
            return result;
        }

        // The piece's own snap points (not ours), as the game finds them: its direct children tagged "snappoint".
        private static List<Transform> Own(Transform root)
        {
            var list = new List<Transform>();
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.CompareTag("snappoint") && !child.name.StartsWith(Prefix)) list.Add(child);
            }
            return list;
        }

        // Points for a kind of piece, worked out once from its prefab's shape.
        private static List<KeyValuePair<string, Vector3>> PointsFor(string prefab)
        {
            if (Points.TryGetValue(prefab, out var known)) return known;
            var list = new List<KeyValuePair<string, Vector3>>();
            var source = ZNetScene.instance ? ZNetScene.instance.GetPrefab(prefab) : null;
            if (source && Box(source.transform, out var min, out var max))
            {
                var existing = Own(source.transform).Select(point => source.transform.InverseTransformPoint(point.position)).ToList();
                float midX = (min.x + max.x) / 2f, midZ = (min.z + max.z) / 2f;
                foreach (float y in new[] { min.y, max.y })
                {
                    string level = y == min.y ? "Bottom" : "Top";
                    foreach (float z in new[] { min.z, max.z })
                        foreach (float x in new[] { min.x, max.x })
                            list.Add(new KeyValuePair<string, Vector3>($"{level} {(z == max.z ? "front" : "back")} {(x == max.x ? "right" : "left")} corner", new Vector3(x, y, z)));
                    list.Add(new KeyValuePair<string, Vector3>($"{level} front edge", new Vector3(midX, y, max.z)));
                    list.Add(new KeyValuePair<string, Vector3>($"{level} back edge", new Vector3(midX, y, min.z)));
                    list.Add(new KeyValuePair<string, Vector3>($"{level} left edge", new Vector3(min.x, y, midZ)));
                    list.Add(new KeyValuePair<string, Vector3>($"{level} right edge", new Vector3(max.x, y, midZ)));
                    list.Add(new KeyValuePair<string, Vector3>($"{level} centre", new Vector3(midX, y, midZ)));
                }
                // Points the piece already has (or a flat piece's doubled corners) aren't added twice.
                var kept = new List<KeyValuePair<string, Vector3>>();
                foreach (var point in list)
                    if (!existing.Any(p => (p - point.Value).sqrMagnitude < 0.0025f) && !kept.Any(p => (p.Value - point.Value).sqrMagnitude < 0.0025f))
                        kept.Add(point);
                list = kept;
            }
            Points[prefab] = list;
            return list;
        }

        // The piece's outer box in its own space: from its solid colliders, or from its models when it has none.
        private static bool Box(Transform root, out Vector3 min, out Vector3 max)
        {
            min = Vector3.positiveInfinity; max = Vector3.negativeInfinity;
            bool any = false;
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider.isTrigger) continue;
                Vector3 center, size;
                switch (collider)
                {
                    case BoxCollider box: center = box.center; size = box.size; break;
                    case SphereCollider sphere: center = sphere.center; size = Vector3.one * sphere.radius * 2f; break;
                    case CapsuleCollider capsule:
                        center = capsule.center;
                        size = Vector3.one * capsule.radius * 2f;
                        size[capsule.direction] = Mathf.Max(capsule.height, capsule.radius * 2f);
                        break;
                    case MeshCollider mesh when mesh.sharedMesh: center = mesh.sharedMesh.bounds.center; size = mesh.sharedMesh.bounds.size; break;
                    default: continue;
                }
                Grow(root, collider.transform, center, size, ref min, ref max);
                any = true;
            }
            if (!any)
                foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (!filter.sharedMesh) continue;
                    Grow(root, filter.transform, filter.sharedMesh.bounds.center, filter.sharedMesh.bounds.size, ref min, ref max);
                    any = true;
                }
            return any && (max - min).sqrMagnitude > 0.0001f;
        }

        private static void Grow(Transform root, Transform part, Vector3 center, Vector3 size, ref Vector3 min, ref Vector3 max)
        {
            for (int i = 0; i < 8; i++)
            {
                var corner = center + Vector3.Scale(size * 0.5f, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var local = root.InverseTransformPoint(part.TransformPoint(corner));
                min = Vector3.Min(min, local); max = Vector3.Max(max, local);
            }
        }

        // Adds or takes away a piece's points to match the settings.
        private static void Refresh(Piece piece)
        {
            if (!piece) return;
            var root = piece.transform;
            var ours = new List<GameObject>();
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name.StartsWith(Prefix)) ours.Add(root.GetChild(i).gameObject);
            string prefab = Utils.GetPrefabName(piece.gameObject);
            bool wanted = Wants(prefab);
            if (!wanted || ours.Count > 0)
            {
                if (!wanted) foreach (var go in ours) UnityEngine.Object.Destroy(go);
                return;
            }
            foreach (var point in PointsFor(prefab))
            {
                var go = new GameObject(Prefix + point.Key) { tag = "snappoint" };
                go.transform.SetParent(root, false);
                go.transform.localPosition = point.Value;
            }
        }

        private static void RefreshAll()
        {
            try
            {
                foreach (var piece in AllPieces().ToList()) Refresh(piece);
                var ghost = Player.m_localPlayer ? Traverse.Create(Player.m_localPlayer).Field("m_placementGhost").GetValue<GameObject>() : null;
                if (ghost) Refresh(ghost.GetComponent<Piece>());
            }
            catch (Exception e) { Plugin.Log.LogWarning("Couldn't update snap points: " + e.Message); }
        }

        // Turns points on or off for every piece of the kind you're looking at.
        internal static void Pick(Terminal context)
        {
            var player = Player.m_localPlayer;
            var piece = player ? player.GetHoveringPiece() : null;
            if (!piece)
            {
                Say(context, "Take out the hammer and look at a built piece first.");
                return;
            }
            string prefab = Utils.GetPrefabName(piece.gameObject);
            string name = Localization.instance.Localize(piece.m_name);
            var picked = List(chosen); var off = List(refused);
            bool had = Wants(prefab);
            picked.Remove(prefab); off.Remove(prefab);
            if (had) off.Add(prefab); else picked.Add(prefab);
            Save(chosen, picked); Save(refused, off);
            RefreshAll();
            if (!on.Value) Say(context, $"{name}: saved, but \"Add snap points\" is off in the settings.");
            else if (had) Say(context, $"{name}: back to its own snap points.");
            else
            {
                int count = PointsFor(prefab).Count;
                Say(context, count > 0 ? $"{name}: {count} snap points added (corners, edges and centres)." : $"{name}: couldn't work out its shape, so no points were added.");
            }
        }

        private static void Say(Terminal context, string text)
        {
            if (context) context.AddString(text);
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, text);
            Plugin.Log.LogInfo("Snap points: " + text);
        }

        internal static void Update()
        {
            if (!on.Value || !Player.m_localPlayer || !key.Value.IsDown()) return;
            if (global::Console.IsVisible() || Chat.instance && Chat.instance.HasFocus() || TextInput.IsVisible() || Menu.IsVisible() || InventoryGui.IsVisible()) return;
            if (!Player.m_localPlayer.InPlaceMode()) return;
            Pick(null);
        }

        private class Command : ConsoleCommand
        {
            public override string Name => "snappoints";
            public override string Help => "Look at a built piece with the hammer out: give every piece of its kind snap points, or take them away.";
            public override void Run(string[] args, Terminal context) => Pick(context);
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        private static class RecordVanilla
        {
            [HarmonyPriority(Priority.First)]
            private static void Prefix(ZNetScene __instance)
            {
                try
                {
                    Vanilla.Clear(); Points.Clear(); Automatic.Clear();
                    foreach (var prefab in __instance.m_prefabs) if (prefab) Vanilla.Add(prefab.name);
                    foreach (var prefab in __instance.m_nonNetViewPrefabs) if (prefab) Vanilla.Add(prefab.name);
                }
                catch (Exception e) { Plugin.Log.LogWarning("Couldn't list the game's own pieces: " + e.Message); }
            }
        }

        [HarmonyPatch(typeof(Piece), "Awake")]
        private static class AddToPiece
        {
            private static void Postfix(Piece __instance)
            {
                try { Refresh(__instance); }
                catch (Exception e) { Plugin.Log.LogWarning("Couldn't add snap points: " + e.Message); }
            }
        }
    }
}
