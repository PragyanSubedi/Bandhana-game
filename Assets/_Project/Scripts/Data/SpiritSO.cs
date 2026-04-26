using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bandhana.Data
{
    [Serializable]
    public class LearnEntry
    {
        public int level;
        public MoveSO move;
    }

    [CreateAssetMenu(fileName = "Spirit_", menuName = "Bandhana/Spirit", order = 3)]
    public class SpiritSO : ScriptableObject
    {
        [Header("Identity")]
        public string spiritName;
        [TextArea(3, 6)] public string codexEntry; // written in first person, by the spirit

        [Header("Typing")]
        public TypeSO primaryType;
        public TypeSO secondaryType; // optional

        [Header("Base stats")]
        [Range(1, 255)] public int baseHP = 50;
        [Range(1, 255)] public int baseAttack = 50;
        [Range(1, 255)] public int baseDefense = 50;
        [Range(1, 255)] public int baseSpAttack = 50;
        [Range(1, 255)] public int baseSpDefense = 50;
        [Range(1, 255)] public int baseSpeed = 50;

        [Header("Learnset")]
        public List<LearnEntry> learnset = new();

        [Header("Evolution (branched)")]
        public SpiritSO balancedEvolution;   // e.g. Damaru -> Heruka -> Yidam
        public SpiritSO wrathfulEvolution;   // e.g. Damaru -> Bhairava -> Mahakala
        public SpiritSO peacefulEvolution;   // e.g. Damaru -> Karuna-Heruka -> Tara
        public int evolutionLevel = 16;

        [Header("Presentation")]
        public Sprite frontSprite;
        public Sprite backSprite;
        public AudioClip drumCry;
    }
}
