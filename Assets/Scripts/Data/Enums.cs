namespace DivineDrift.Data
{
    /// <summary>
    /// The three guiding philosophies a population can follow.
    /// Each grants a distinct profile of perks (see PhilosophyDefinition).
    /// </summary>
    public enum Philosophy
    {
        Attack,
        Cooperation,
        Defense
    }

    /// <summary>
    /// Terrain types that cover the planet cells. Each modifies a population's
    /// effective military strength while it holds the cell during conflict.
    /// </summary>
    public enum TerrainType
    {
        Plains,    // neutral baseline
        Desert,    // reduces military strength
        Forest,    // slight defensive bonus, slows expansion
        Mountain   // adds military strength
    }

    /// <summary>
    /// Historical eras. The basic scenario starts in Bronze.
    /// Eras act as global modifiers AND gate parts of each population's tech tree.
    /// </summary>
    public enum Era
    {
        Bronze,
        Iron,
        Classical,
        Medieval,
        Renaissance,
        Industrial,
        Modern
    }

    /// <summary>
    /// High-level intent the player assigns to a spawned Agent.
    /// </summary>
    public enum AgentDirective
    {
        Expand,        // push borders outward toward the nearest frontier
        ExpandToward,  // push toward a specific target cell/population
        Ally,          // seek cooperation with a target population
        Defend,        // consolidate and reinforce current borders
        Convert        // raise religious fervour internally
    }

    /// <summary>
    /// Relationship state between two populations.
    /// </summary>
    public enum DiplomacyState
    {
        Neutral,
        AtWar,
        Cooperating,  // trade + science sharing
        Allied        // cooperating + non-aggression
    }

    /// <summary>
    /// Outcome of a resolved battle for a single contested cell.
    /// </summary>
    public enum BattleResult
    {
        AttackerWins,
        DefenderWins,
        Stalemate
    }

    /// <summary>
    /// How fast in-game time advances. Maps real minutes -> in-game years.
    /// 1 real minute = N in-game years.
    /// </summary>
    public enum TimeScaleStep
    {
        Paused,
        Years1PerMinute,
        Years5PerMinute,
        Years20PerMinute,
        Years50PerMinute,
        Years100PerMinute
    }
}
