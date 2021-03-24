using C64Lib;

namespace Hunchback
{
    public enum WallType
    {
        Wall,
        EmptyPits,
        KnightPits,
        Rope
    }

    public enum EnemyType
    {
        None,
        FireBallRL
    }

    public class LevelDescription
    {
        WallType WallType;
        EnemyType EnemyType;
        Colors Color;

        public static LevelDescription[] Levels =>
            new[]
            {
                new LevelDescription {
                    WallType = WallType.Wall,
                    EnemyType = EnemyType.FireBallRL,
                    Color = Colors.Grey1
                }
            };
    }

}