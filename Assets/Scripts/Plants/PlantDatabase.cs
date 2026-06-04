using System;
using System.Linq;
using UnityEngine;

public static class PlantDatabase
{
    private static PlantData[] _plants;

    public static PlantData[] All => _plants ??= Load();

    public static PlantData[] GetUnlocked() =>
        All.Where(p => p.unlockAt <= SaveManager.Instance.Milestones.earthPercent).ToArray();

    public static PlantData GetByName(string plantName) =>
        Array.Find(All, p => p.name == plantName);

    public static PlantData GetLatestUnlocked() =>
        GetUnlocked() is { Length: > 0 } unlocked ? unlocked[^1] : All[0];

    public static PlantData GetRandom()
    {
        PlantData[] unlocked = GetUnlocked();
        if (unlocked.Length == 0) return All[0];

        // Weight by position — later unlocks are proportionally more likely
        float total = 0f;
        for (int i = 0; i < unlocked.Length; i++) total += i + 1;

        float roll = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < unlocked.Length; i++)
        {
            cumulative += i + 1;
            if (roll < cumulative) return unlocked[i];
        }

        return unlocked[^1];
    }

    private static PlantData[] Load()
    {
        PlantDatabaseSO db = Resources.Load<PlantDatabaseSO>("PlantDatabase");
        if (db == null)
        {
            Debug.LogError("[PlantDatabase] PlantDatabase SO not found in Resources/");
            return Array.Empty<PlantData>();
        }
        return db.plants;
    }
}
