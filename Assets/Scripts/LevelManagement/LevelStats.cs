using System.Collections.Generic;

public struct LevelStats
{
    public int width;
    public int height;

    // --- auto counted stuff
    public int clones;
    public int exits;
    public int boxes;
    public int spikes;
    public int buttons;
    public int pushbuttons;
    public int doors;
    public int moveableBoxes;
    public int movingPlatforms;
    public int portals;
    public int pistons;
    public int cloneDistance;
    public int exitDistance;
    public int walls;
    public int crumbling;

    // --- solver filled stuff
    public int minSteps;
    public int numSolutions;
    public int unusedTiles;
    public List<LevelSimulation.Solution> solutions;

    public string error;
}
