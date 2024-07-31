public struct StatImprovement
{
    public StatType StatType;
    public float Amount;

    public StatImprovement(StatType type, float amount)
    {
        StatType = type;
        Amount = amount;
    }
}

public enum StatType
{
    Bomb,
    Power,
    Speed
}
