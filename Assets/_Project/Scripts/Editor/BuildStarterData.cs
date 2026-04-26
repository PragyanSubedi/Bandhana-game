#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Bandhana.Data;

namespace Bandhana.EditorTools
{
    // M2: one-click generator for the starter data layer.
    // Idempotent — re-runs overwrite field values on existing assets but keep the GUIDs,
    // so any references to these assets in scenes/prefabs survive.
    public static class BuildStarterData
    {
        const string TypesDir = "Assets/_Project/Data/Types";
        const string MovesDir = "Assets/_Project/Data/Moves";
        const string SpiritsDir = "Assets/_Project/Data/Spirits";

        [MenuItem("Bandhana/Build Starter Data")]
        public static void Build()
        {
            Directory.CreateDirectory(TypesDir);
            Directory.CreateDirectory(MovesDir);
            Directory.CreateDirectory(SpiritsDir);

            var types = BuildTypes();
            var chart = BuildTypeChart(types);
            var moves = BuildMoves(types);
            BuildSpirits(types, moves);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Keep BandhanaDB in sync so save/load can resolve these by name.
            BuildDatabase.Build();

            EditorUtility.DisplayDialog("Bandhana — M2",
                "Built:\n" +
                $"  • {types.Count} types in {TypesDir}\n" +
                $"  • 1 type chart\n" +
                $"  • 12 moves in {MovesDir}\n" +
                $"  • 6 spirits in {SpiritsDir} (Damaru + 5 wild)\n\n" +
                "Browse them in the Project panel and tweak in the Inspector.",
                "OK");
        }

        // ── Types ─────────────────────────────────────────────────────────────
        static Dictionary<string, TypeSO> BuildTypes()
        {
            var defs = new (string name, string desc, Color color)[]
            {
                ("Vajra",  "Mirror-water. Clarity.",            new Color(0.55f, 0.85f, 0.95f)),
                ("Ratna",  "Jewel-earth. Equanimity.",          new Color(0.85f, 0.70f, 0.30f)),
                ("Padma",  "Lotus-fire. Discernment.",          new Color(0.95f, 0.45f, 0.45f)),
                ("Karma",  "Sword-wind. Action.",               new Color(0.65f, 0.65f, 0.75f)),
                ("Akasha", "Sky-space. Resonance.",             new Color(0.55f, 0.55f, 0.95f)),
                ("Tejas",  "Brilliance. Revealing light.",      new Color(0.95f, 0.90f, 0.55f)),
                ("Vayu",   "Storm-breath. Movement.",           new Color(0.75f, 0.85f, 0.95f)),
                ("Naga",   "Serpent-water. Hidden depth.",      new Color(0.45f, 0.35f, 0.65f)),
            };

            var dict = new Dictionary<string, TypeSO>();
            foreach (var (name, desc, color) in defs)
            {
                var t = GetOrCreate<TypeSO>($"{TypesDir}/Type_{name}.asset");
                t.typeName = name;
                t.description = desc;
                t.uiColor = color;
                EditorUtility.SetDirty(t);
                dict[name] = t;
            }
            return dict;
        }

        // ── Type chart ────────────────────────────────────────────────────────
        static TypeChart BuildTypeChart(Dictionary<string, TypeSO> t)
        {
            var chart = GetOrCreate<TypeChart>($"{TypesDir}/TypeChart.asset");
            chart.rows.Clear();

            void Add(string atk, string def, float mul)
            {
                var row = chart.rows.Find(r => r.attacker == t[atk]);
                if (row == null)
                {
                    row = new TypeChart.Row { attacker = t[atk], matchups = new() };
                    chart.rows.Add(row);
                }
                row.matchups.Add(new TypeChart.Entry { defender = t[def], multiplier = mul });
            }

            // Triangle 1 — elemental: Vajra > Padma > Karma > Vajra
            Add("Vajra", "Padma", 2f); Add("Padma", "Vajra", 0.5f);
            Add("Padma", "Karma", 2f); Add("Karma", "Padma", 0.5f);
            Add("Karma", "Vajra", 2f); Add("Vajra", "Karma", 0.5f);

            // Triangle 2 — planar: Ratna > Vayu > Akasha > Ratna
            Add("Ratna",  "Vayu",   2f); Add("Vayu",   "Ratna",  0.5f);
            Add("Vayu",   "Akasha", 2f); Add("Akasha", "Vayu",   0.5f);
            Add("Akasha", "Ratna",  2f); Add("Ratna",  "Akasha", 0.5f);

            // Spiritual axis — light/shadow
            Add("Tejas", "Naga",  2f); Add("Naga", "Tejas", 0.5f);
            Add("Vajra", "Tejas", 2f); Add("Tejas", "Vajra", 0.5f);

            EditorUtility.SetDirty(chart);
            return chart;
        }

        // ── Moves ─────────────────────────────────────────────────────────────
        static Dictionary<string, MoveSO> BuildMoves(Dictionary<string, TypeSO> t)
        {
            var defs = new (string key, string display, string desc, string typeKey, MoveCategory cat, int pow, int acc, int pp)[]
            {
                ("Pulse",       "Pulse",        "A drum-beat that ripples outward. Damaru's first prayer.", null,     MoveCategory.Spiritual, 40, 100, 25),
                ("MirrorShard", "Mirror Shard", "A sliver of still water, hurled until it breaks.",          "Vajra",  MoveCategory.Spiritual, 60, 100, 15),
                ("StoneMantra", "Stone Mantra", "A syllable so old it has weight.",                          "Ratna",  MoveCategory.Physical,  65, 100, 15),
                ("LotusFlame",  "Lotus Flame",  "A petal opens and burns.",                                  "Padma",  MoveCategory.Spiritual, 60, 95,  15),
                ("SwordMudra",  "Sword Mudra",  "A gesture sharper than the blade it remembers.",            "Karma",  MoveCategory.Physical,  70, 95,  10),
                ("SkyVeil",     "Sky Veil",     "A field of stillness that smothers strikes.",              "Akasha", MoveCategory.Status,     0, 100, 20),
                ("SunBrand",    "Sun Brand",    "Light pressed so close it sears.",                          "Tejas",  MoveCategory.Spiritual, 65, 100, 15),
                ("StormBreath", "Storm Breath", "A long exhale that drags the wind with it.",                "Vayu",   MoveCategory.Spiritual, 60, 95,  15),
                ("CoilStrike",  "Coil Strike",  "A patient muscle finally remembers itself.",                "Naga",   MoveCategory.Physical,  65, 100, 15),
                ("CalmBreath",  "Calm Breath",  "Inhale. The body forgives a wound.",                        null,     MoveCategory.Status,     0, 100, 10),
                ("Tame",        "Tame",         "A whispered name. The opponent softens.",                   null,     MoveCategory.Status,     0, 100, 15),
                ("Resonance",   "Resonance",    "Two drums find each other. Both shake.",                    "Akasha", MoveCategory.Spiritual, 50, 100, 20),
            };

            var dict = new Dictionary<string, MoveSO>();
            foreach (var d in defs)
            {
                var m = GetOrCreate<MoveSO>($"{MovesDir}/Move_{d.key}.asset");
                m.moveName = d.display;
                m.description = d.desc;
                m.type = d.typeKey != null ? t[d.typeKey] : null;
                m.category = d.cat;
                m.power = d.pow;
                m.accuracy = d.acc;
                m.pp = d.pp;
                m.priority = 0;
                EditorUtility.SetDirty(m);
                dict[d.key] = m;
            }
            return dict;
        }

        // ── Spirits ──────────────────────────────────────────────────────────
        static void BuildSpirits(Dictionary<string, TypeSO> t, Dictionary<string, MoveSO> m)
        {
            // Damaru — starter, balanced, the canonical "Helping" bond.
            var damaru = GetOrCreate<SpiritSO>($"{SpiritsDir}/Spirit_Damaru.asset");
            damaru.spiritName  = "Damaru";
            damaru.codexEntry  = "I beat with the world's oldest rhythm. Strike me, and the cosmos answers.";
            damaru.primaryType = t["Akasha"];
            damaru.secondaryType = null;
            damaru.baseHP = 50; damaru.baseAttack = 45; damaru.baseDefense = 45;
            damaru.baseSpAttack = 60; damaru.baseSpDefense = 60; damaru.baseSpeed = 50;
            damaru.learnset = new List<LearnEntry>
            {
                new() { level = 1,  move = m["Pulse"] },
                new() { level = 4,  move = m["CalmBreath"] },
                new() { level = 8,  move = m["SkyVeil"] },
                new() { level = 12, move = m["Resonance"] },
                new() { level = 16, move = m["SwordMudra"] },
            };
            damaru.evolutionLevel = 16; // → Heruka / Bhairava / Karuna-Heruka (assets pending Act 6)
            EditorUtility.SetDirty(damaru);

            // Khyaak — mischievous house spirit; glass cannon.
            var khyaak = GetOrCreate<SpiritSO>($"{SpiritsDir}/Spirit_Khyaak.asset");
            khyaak.spiritName  = "Khyaak";
            khyaak.codexEntry  = "I am the laughter at the back of the kitchen, the spilled rice, the missing key. Catch me if you can.";
            khyaak.primaryType = t["Vayu"];
            khyaak.secondaryType = null;
            khyaak.baseHP = 35; khyaak.baseAttack = 45; khyaak.baseDefense = 30;
            khyaak.baseSpAttack = 55; khyaak.baseSpDefense = 35; khyaak.baseSpeed = 80;
            khyaak.learnset = new List<LearnEntry>
            {
                new() { level = 1, move = m["Pulse"] },
                new() { level = 3, move = m["StormBreath"] },
                new() { level = 7, move = m["Tame"] },
            };
            khyaak.evolutionLevel = 0;
            EditorUtility.SetDirty(khyaak);

            // Naga (juvenile) — patient water-serpent.
            var naga = GetOrCreate<SpiritSO>($"{SpiritsDir}/Spirit_Naga.asset");
            naga.spiritName  = "Naga";
            naga.codexEntry  = "My lineage coils beneath the lake. I am the river's quiet warning.";
            naga.primaryType = t["Naga"];
            naga.secondaryType = t["Vajra"];
            naga.baseHP = 55; naga.baseAttack = 60; naga.baseDefense = 50;
            naga.baseSpAttack = 50; naga.baseSpDefense = 55; naga.baseSpeed = 40;
            naga.learnset = new List<LearnEntry>
            {
                new() { level = 1, move = m["CoilStrike"] },
                new() { level = 5, move = m["MirrorShard"] },
                new() { level = 9, move = m["CalmBreath"] },
            };
            naga.evolutionLevel = 0;
            EditorUtility.SetDirty(naga);

            // Garu — garuda chick, sky-attuned striker.
            var garu = GetOrCreate<SpiritSO>($"{SpiritsDir}/Spirit_Garu.asset");
            garu.spiritName  = "Garu";
            garu.codexEntry  = "I was born above the snowline. The wind taught me to call it kin.";
            garu.primaryType = t["Akasha"];
            garu.secondaryType = t["Vayu"];
            garu.baseHP = 45; garu.baseAttack = 65; garu.baseDefense = 40;
            garu.baseSpAttack = 50; garu.baseSpDefense = 40; garu.baseSpeed = 70;
            garu.learnset = new List<LearnEntry>
            {
                new() { level = 1, move = m["Pulse"] },
                new() { level = 4, move = m["SkyVeil"] },
                new() { level = 9, move = m["SwordMudra"] },
            };
            garu.evolutionLevel = 0;
            EditorUtility.SetDirty(garu);

            // Yeti (juvenile) — slow, stony defender.
            var yeti = GetOrCreate<SpiritSO>($"{SpiritsDir}/Spirit_Yeti.asset");
            yeti.spiritName  = "Yeti";
            yeti.codexEntry  = "I do not hide. You simply do not look at the snow long enough.";
            yeti.primaryType = t["Ratna"];
            yeti.secondaryType = null;
            yeti.baseHP = 75; yeti.baseAttack = 65; yeti.baseDefense = 70;
            yeti.baseSpAttack = 30; yeti.baseSpDefense = 50; yeti.baseSpeed = 25;
            yeti.learnset = new List<LearnEntry>
            {
                new() { level = 1, move = m["StoneMantra"] },
                new() { level = 6, move = m["Tame"] },
            };
            yeti.evolutionLevel = 0;
            EditorUtility.SetDirty(yeti);

            // Mayura — sacred peacock; high spiritual attack, optional Act 4 bond.
            var mayura = GetOrCreate<SpiritSO>($"{SpiritsDir}/Spirit_Mayura.asset");
            mayura.spiritName  = "Mayura";
            mayura.codexEntry  = "Every feather is a sutra. I shed them only for those who wait.";
            mayura.primaryType = t["Padma"];
            mayura.secondaryType = t["Tejas"];
            mayura.baseHP = 60; mayura.baseAttack = 45; mayura.baseDefense = 55;
            mayura.baseSpAttack = 80; mayura.baseSpDefense = 65; mayura.baseSpeed = 55;
            mayura.learnset = new List<LearnEntry>
            {
                new() { level = 1, move = m["LotusFlame"] },
                new() { level = 5, move = m["SunBrand"] },
                new() { level = 9, move = m["CalmBreath"] },
            };
            mayura.evolutionLevel = 0;
            EditorUtility.SetDirty(mayura);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }
    }
}
#endif
