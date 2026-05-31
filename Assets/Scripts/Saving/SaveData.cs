using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public Vector2 playerFacingDirection;
    public float playTimeSeconds;
    public MilestoneTracker milestones;
}

[Serializable]
public class SaveSlotMeta
{
    public int slot;
    public string timestamp;
    public float playTimeSeconds;
    public float earthPercent;
    public bool isEmpty;
}