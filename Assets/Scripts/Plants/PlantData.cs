using System;

[Serializable]
public class PlantData
{
    public string name;
    public float unlockAt; // based on earthPercent threshold (0-100)
    public string bonusStat;  // "Effective", "Speed", "Resistance", or "" for none
    public float bonusAmount;
}

[Serializable]
class PlantList
{
    public PlantData[] plants;
}
