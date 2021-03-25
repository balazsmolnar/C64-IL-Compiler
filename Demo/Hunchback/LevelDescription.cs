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
        public WallType WallType;
        public EnemyType EnemyType;
        public Colors Color;

        public static LevelDescription[] Levels =>
            new[]
            {
                new LevelDescription {
                    WallType = WallType.Wall,
                    EnemyType = EnemyType.FireBallRL,
                    Color = Colors.Grey1
                },
                new LevelDescription {
                    WallType = WallType.EmptyPits,
                    EnemyType = EnemyType.None,
                    Color = Colors.Grey1
                }
            };
    }

}