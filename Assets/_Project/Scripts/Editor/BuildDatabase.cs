#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Bandhana.Data;

namespace Bandhana.EditorTools
{
    // Populates Assets/_Project/Resources/BandhanaDB.asset with refs to every
    // SpiritSO and MoveSO in the project, plus the TypeChart. Runs idempotently.
    public static class BuildDatabase
    {
        const string ResourcesDir = "Assets/_Project/Resources";
        const string DBPath       = "Assets/_Project/Resources/BandhanaDB.asset";
        const string ChartPath    = "Assets/_Project/Data/Types/TypeChart.asset";

        [MenuItem("Bandhana/Build Database")]
        public static void Build()
        {
            Directory.CreateDirectory(ResourcesDir);

            var db = AssetDatabase.LoadAssetAtPath<BandhanaDB>(DBPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<BandhanaDB>();
                AssetDatabase.CreateAsset(db, DBPath);
            }

            db.spirits.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:SpiritSO"))
                db.spirits.Add(AssetDatabase.LoadAssetAtPath<SpiritSO>(AssetDatabase.GUIDToAssetPath(guid)));

            db.moves.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:MoveSO"))
                db.moves.Add(AssetDatabase.LoadAssetAtPath<MoveSO>(AssetDatabase.GUIDToAssetPath(guid)));

            db.typeChart = AssetDatabase.LoadAssetAtPath<TypeChart>(ChartPath);

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
