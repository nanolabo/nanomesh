namespace Nanomesh
{
    public struct AttributeDefinition
    {
        public double weight;
        public AttributeType type;
        public int id;

        public AttributeDefinition(AttributeType type)
        {
            this.weight = 1;
            this.type = type;
            this.id = 0;
        }

        public AttributeDefinition(AttributeType type, double weight)
        {
            this.weight = weight;
            this.type = type;
            this.id = 0;
        }

        public AttributeDefinition(AttributeType type, double weight, int id)
        {
            this.weight = weight;
            this.type = type;
            this.id = id;
        }
    }
}