namespace Nanomesh
{
    public abstract class MetaAttributeList
    {
        public abstract IMetaAttribute this[int index]
        {
            get;
            set;
        }

        public abstract int Count { get; }

        public abstract int CountPerAttribute { get; }

        public abstract MetaAttributeList CreateNew(int length);

        public abstract bool Equals(int indexA, int indexB, int attribute);

        public abstract void Interpolate(int indexA, int indexB, double ratio);
    }

    public class EmptyMetaAttributeList : MetaAttributeList
    {
        public EmptyMetaAttributeList()
        {

        }

        public override IMetaAttribute this[int index]
        {
            get => throw new System.Exception();
            set => throw new System.Exception();
        }

        public override MetaAttributeList CreateNew(int length) => new EmptyMetaAttributeList();

        public override unsafe bool Equals(int indexA, int indexB, int attribute)
        {
            return false;
        }

        public override void Interpolate(int indexA, int indexB, double ratio)
        {
            throw new System.Exception();
        }

        public override int Count => 0;

        public override int CountPerAttribute => 0;
    }

    public class MetaAttributeList<T0> : MetaAttributeList
        where T0 : unmanaged, INumeric<T0>
    {
        private MetaAttribute<T0>[] _attributes;

        public MetaAttributeList(int length)
        {
            _attributes = new MetaAttribute<T0>[length];
        }

        public override IMetaAttribute this[int index]
        {
            get => _attributes[index];
            set => _attributes[index] = (MetaAttribute<T0>)value;
        }

        public void Set(MetaAttribute<T0> value, int index)
        {
            _attributes[index] = value;
        }

        private void Get(MetaAttribute<T0> value, int index)
        {
            _attributes[index] = value;
        }

        public override MetaAttributeList CreateNew(int length) => new MetaAttributeList<T0>(length);

        public override unsafe bool Equals(int indexA, int indexB, int attribute)
        {
            return _attributes[indexA].GetPtr(attribute) == _attributes[indexB].GetPtr(attribute);
        }

        public override void Interpolate(int indexA, int indexB, double ratio)
        {
            _attributes[indexA].attr0 = _attributes[indexA].Get<T0>(0).Multiply(ratio).Sum(_attributes[indexB].Get<T0>(0).Multiply(1 - ratio));
            _attributes[indexB].attr0 = _attributes[indexA].attr0;
        }

        public override int Count => _attributes.Length;

        public override int CountPerAttribute => 1;
    }

    public class MetaAttributeList<T0, T1> : MetaAttributeList
        where T0 : unmanaged, INumeric<T0>
        where T1 : unmanaged, INumeric<T1>
    {
        private MetaAttribute<T0, T1>[] _attributes;

        public MetaAttributeList(int length)
        {
            _attributes = new MetaAttribute<T0, T1>[length];
        }

        public override IMetaAttribute this[int index]
        {
            get => _attributes[index];
            set => _attributes[index] = (MetaAttribute<T0, T1>)value;
        }

        public void Set(MetaAttribute<T0, T1> value, int index)
        {
            _attributes[index] = value;
        }

        private void Get(MetaAttribute<T0, T1> value, int index)
        {
            _attributes[index] = value;
        }

        public override MetaAttributeList CreateNew(int length) => new MetaAttributeList<T0, T1>(length);

        public override unsafe bool Equals(int indexA, int indexB, int attribute)
        {
            return _attributes[indexA].GetPtr(attribute) == _attributes[indexB].GetPtr(attribute);
        }

        public override void Interpolate(int indexA, int indexB, double ratio)
        {
            _attributes[indexA].attr0 = _attributes[indexA].Get<T0>(0).Multiply(ratio).Sum(_attributes[indexB].Get<T0>(0).Multiply(1 - ratio));
            _attributes[indexB].attr0 = _attributes[indexA].attr0;

            _attributes[indexA].attr1 = _attributes[indexA].Get<T1>(1).Multiply(ratio).Sum(_attributes[indexB].Get<T1>(1).Multiply(1 - ratio));
            _attributes[indexB].attr1 = _attributes[indexA].attr1;
        }

        public override int Count => _attributes.Length;

        public override int CountPerAttribute => 2;
    }
}