
public struct LevelInfo
{
    public string id;
    public string author;
    public string data;
    public int numRatings;
    public float avgRating;

    public LevelInfo(string id, string author, string data, int numRatings, float avgRating)
    {
        this.id = id;
        this.author = author;
        this.data = data;
        this.numRatings = numRatings;
        this.avgRating = avgRating;
    }
}
