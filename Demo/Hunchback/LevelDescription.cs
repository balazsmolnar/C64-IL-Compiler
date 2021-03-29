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
        FireBall = 1,
        Arrow = 2,

        RightLeft = 4,
        LeftRight = 8,
        Top = 16,
        Bottom = 32
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
                    EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                    Color = Colors.Grey1
                },
                new LevelDescription {
                    WallType = WallType.Rope,
                    EnemyType = EnemyType.None,
                    Color = Colors.Grey1
                },
                new LevelDescription {
                    WallType = WallType.EmptyPits,
                    EnemyType = EnemyType.None,
                    Color = Colors.Grey1
                },
                new LevelDescription {
                    WallType = WallType.KnightPits,
                    EnemyType = EnemyType.None,
                    Color = Colors.Grey3
                },
                new LevelDescription {
                    WallType = WallType.KnightPits,
                    EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Top,
                    Color = Colors.Grey3
                },
                new LevelDescription {
                    WallType = WallType.EmptyPits,
                    EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                    Color = Colors.Grey3
                },
                new LevelDescription {
                    WallType = WallType.Rope,
                    EnemyType = EnemyType.None,
                    Color = Colors.Brown
                },
                new LevelDescription {
                    WallType = WallType.KnightPits,
                    EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                    Color = Colors.Brown
                },

            };
    }

}