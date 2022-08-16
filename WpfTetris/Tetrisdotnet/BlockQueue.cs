namespace Tetrisdotnet
{
    using System;
    public class BlockQueue
    {
        private readonly Block[] blocks = new Block[]
        {
            new Tetrisdotnet.IBlock(),
            new Tetrisdotnet.JBlock(),
            new Tetrisdotnet.LBlock(),
            new Tetrisdotnet.OBlock(),
            new Tetrisdotnet.SBlock(),
            new Tetrisdotnet.TBlock(),
            new Tetrisdotnet.ZBlock()
        };

        private readonly Random random = new Random();

        public Block NextBlock { get; private set; }

        public BlockQueue()
        {
            NextBlock = RandomBlock();
        }

        private Block RandomBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }

        public Block GetAndUpdate()
        {
            Block block = NextBlock;

            do
            {
                NextBlock = RandomBlock();
            }
            while (block.Id == NextBlock.Id);

            return block;
        }
    }
}
