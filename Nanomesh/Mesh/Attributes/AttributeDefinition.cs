namespace Nanomesh
{
    public struct AttributeDefinition
    {
        public double weight;
        public AttributeType type;

        public AttributeDefinition(AttributeType type)
        {
            weight = 1;
            this.type = type;
        }

        public AttributeDefinition(AttributeType type, double weight)
        {
            this.weight = weight;
            this.type = type;
        }
    }
}