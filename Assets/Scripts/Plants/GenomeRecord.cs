using System;

[Serializable]
public class GenomeRecord
{
    public string plantName;
    public int score;

    public PlantData Plant => PlantDatabase.GetByName(plantName);
}
