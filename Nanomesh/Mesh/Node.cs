namespace Nanomesh
{
    public struct Node
    {
        public int position;
        public int attribute;
        public int sibling;
        public int relative;

        public void MarkRemoved()
        {
            position = -10;
        }

        public bool IsRemoved => position == -10;

        public override string ToString()
        {
            return $"sibl:{sibling} rela:{relative} posi:{position} attr:{attribute}";
        }
    }
}