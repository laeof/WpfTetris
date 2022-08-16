namespace Tetrisdotnet
{
    public class IBlock : Block
    {
        public override int Id => 1;
        protected override Position StartOffset => new Position(-1, 3);

        protected override Position[][] Tiles => new Position[][]
        {
            new Position[] { new(1,0), new(1,1), new(1,2), new(1,3) },
            new Position[] { new(0,2), new(1,2), new(2,2), new(3,2) },
            new Position[] { new(2,0), new(2,1), new(2,2), new(2,3) },
            new Position[] { new(0,1), new(1,1), new(2,1), new(3,1) }
            /*new Position[] { new(0,0), new(0,1), new(0, 2), new(0,3), new(0,4),
                new(1, 0), new(1, 1), new(1, 2), new(1, 3), new(1,4),
                new(2, 0), new(2, 1), new(2, 2), new(2, 3), new(2,4),
                new(3,0), new(3,1), new(3, 2), new(3,3), new(3,4) }*/
        };

    }
}
