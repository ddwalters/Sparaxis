using System;
using System.Collections.Generic;

[Serializable]
public class MilestoneTracker
{
    #region Tutorial
    public bool hasSeenComputer;
    public bool hasSeenPrinter;
    public bool hasSeenGarden;
    public bool hasSeenShuttle;

    public bool seedlingCaptain; // Tutorial complete, the world is slowly healing :)
    #endregion

    #region Sequences
    public bool hasSequence;
    public List<GenomeRecord> genomeCollection = new List<GenomeRecord>();
    #endregion

    #region Gameplay
    public float earthPercent; // Earth milestones determine game completion, I.e. 100%
    public float earthEfficiency;
    public float earthGrowthSpeed;
    #endregion
}
