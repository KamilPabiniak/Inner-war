public static class AnxietyBehaviorFactory
{
    public static IFearBehavior GetBehavior(float fearLevel)
    {
        if (fearLevel <= 20)
            return new CalmBehavior();
        if (fearLevel <= 40)
            return new SlightFearBehavior();
        if (fearLevel <= 60)
            return new ModerateFearBehavior();
        if (fearLevel <= 80)
            return new HighFearBehavior();
        if (fearLevel <= 99)
            return new ExtremeFearBehavior();
        return new FaintBehavior();
    }
}