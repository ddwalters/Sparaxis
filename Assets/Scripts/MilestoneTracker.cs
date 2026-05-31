using System;

[Serializable]
public class MilestoneTracker
{
    #region Tutorial
    public bool hasSeenComputer;
    public bool hasSeenPrinter;
    public bool hasSeenGarden;
    public bool hasSeenShuttle;

    public bool seedlingCaptian; // Tutorial complete, the world is slowly healing :)
    #endregion

    #region Gameplay
    public float earthPercent; // Earth milestones determine game completeion, I.e. 100%
    public float earthEfficiency; 
    public float earthGrowthSpeed;
    #endregion
}
