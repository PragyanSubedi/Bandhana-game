using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bandhana.Battle;
using Bandhana.Data;

namespace Bandhana.Core
{
    public static class SaveSystem
    {
        const string FileName = "bandhana_save.json";

        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave() => File.Exists(Path);

        public static void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }

        // Snapshot the current game state (party + scene + player position).
        public static SaveData Capture(Vector2 playerPos, string sceneName)
        {
            var d = new SaveData
            {
                sceneName     = sceneName,
                playerX       = playerPos.x,
                playerY       = playerPos.y,
                saveTimestamp = DateTime.UtcNow.ToString("o"),
            };
            foreach (var u in GameManager.Instance.party)
            {
                if (u?.spirit == null) continue;
                var pm = new PartyMemberData
                {
                    spiritAssetName = u.spirit.name,
                    level           = u.level,
                    currentHP       = u.currentHP,
                };
                foreach (var slot in u.moves)
                {
                    if (slot?.move == null) continue;
                    pm.moves.Add(new MoveStateData
                    {
                        moveAssetName = slot.move.name,
                        currentPP     = slot.currentPP,
                    });
                }
                d.party.Add(pm);
            }
            return d;
        }

        public static bool Save(SaveData data)
        {
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(data, prettyPrint: true));
                Debug.Log($"[Bandhana] Saved to {Path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bandhana] Save failed: {e}");
                return false;
            }
        }

        public static SaveData Load()
        {
            if (!File.Exists(Path)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bandhana] Load failed: {e}");
                return null;
            }
        }

        // Apply save data to GameManager.party + queue scene+position load.
        public static bool ApplyAndLoad(SaveData data)
        {
            if (data == null) return false;
            var db = BandhanaDB.I;
            if (db == null)
            {
                Debug.LogError("[Bandhana] BandhanaDB.asset not found in Resources. Run 'Bandhana > Build Database'.");
                return false;
            }

            var gm = GameManager.Instance;
            gm.party.Clear();

            foreach (var pm in data.party)
            {
                var spirit = db.Spirit(pm.spiritAssetName);
                if (spirit == null)
                {
                    Debug.LogWarning($"[Bandhana] Save references unknown spirit '{pm.spiritAssetName}'.");
                    continue;
                }
                var u = new BattleUnit(spirit, pm.level) { currentHP = pm.currentHP };
                u.moves.Clear();
                foreach (var ms in pm.moves)
                {
                    var move = db.Move(ms.moveAssetName);
                    if (move == null) continue;
                    u.moves.Add(new MoveSlot(move) { currentPP = ms.currentPP });
                }
                gm.party.Add(u);
            }

            SaveContext.SetPending(new Vector2(data.playerX, data.playerY));

            if (!Application.CanStreamedLevelBeLoaded(data.sceneName))
            {
                Debug.LogError($"[Bandhana] Saved scene '{data.sceneName}' is not in Build Settings.");
                return false;
            }
            SceneManager.LoadScene(data.sceneName);
            return true;
        }
    }
}
