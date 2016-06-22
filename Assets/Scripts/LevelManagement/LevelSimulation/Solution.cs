using System.Collections.Generic;

namespace LevelSimulation
{
    [System.Serializable]
    public struct Solution
    {
        public int steps;
        public string description;
        public List<MoveDir> steplist;

        public int NumSteps
        {
            get { return steplist.Count - 1; }
        }

        public Solution(int steps, List<MoveDir> steplist)
        {
            this.steps = steps;
            this.steplist = steplist;

            description = "";
            for (int i = 1; i < steplist.Count; i++)
            {
                if (i > 1)
                    description += "-";

                description += steplist[i].name;
            }
        }
    }
}
