public static class AnxietyBehaviorFactory
{
    public static IFearBehavior GetBehavior(float fearThreshold)
    {
        if (fearThreshold <= 20)
            return new CalmBehavior();
        if (fearThreshold <= 40)
            return new SlightFearBehavior();
        if (fearThreshold <= 60)
            return new ModerateFearBehavior();
        if (fearThreshold <= 80)
            return new HighFearBehavior();
        if (fearThreshold <= 99)
            return new ExtremeFearBehavior();
        return new FaintBehavior();
    }

    public static float GetThreshold(float fearLevel)
    {
        if (fearLevel <= 20 + 5) return 20;
        if (fearLevel <= 40 + 5) return 40;
        if (fearLevel <= 60 + 5) return 60;
        if (fearLevel <= 80 + 5) return 80;
        if (fearLevel <= 99) return 99;
        return 100; // Maksymalny próg
    }
}
