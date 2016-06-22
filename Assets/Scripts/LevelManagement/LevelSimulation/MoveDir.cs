namespace LevelSimulation
{
    public struct MoveDir
    {
        public byte id;
        public int dr;
        public int dc;
        public string name;

        public MoveDir(string name, int dr, int dc, byte id)
        {
            this.id = id;
            this.dr = dr;
            this.dc = dc;
            this.name = name;
        }
    }
}