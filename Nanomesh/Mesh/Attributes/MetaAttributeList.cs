using System;

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

        public abstract MetaAttributeList AddAttributeType<T>()
            where T : unmanaged, IInterpolable<T>;

        public abstract bool Equals(int indexA, int indexB, int attribute);

        public abstract void Interpolate(int attribute, int indexA, int indexB, double ratio);
    }

    public class EmptyMetaAttributeList : MetaAttributeList
    {
        private int _length;

        public EmptyMetaAttributeList(int length)
        {
            _length = length;
        }

        public override IMetaAttribute this[int index]
        {
            get => throw new System.Exception();
            set => throw new System.Exception();
        }

        public override MetaAttributeList CreateNew(int length) => new EmptyMetaAttributeList(length);

        public override unsafe bool Equals(int indexA, int indexB, int attribute)
        {
            return false;
        }

        public override void Interpolate(int attribute, int indexA, int indexB, double ratio)
        {
            throw new System.Exception();
        }

        public override MetaAttributeList AddAttributeType<T>()
        {
            return new MetaAttributeList<T>(_length);
        }

        public override int Count => 0;

        public override int CountPerAttribute => 0;
    }

    public class MetaAttributeList<T0> : MetaAttributeList
        where T0 : unmanaged, IInterpolable<T0>
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
            switch (attribute)
            {
                case 0:
                    return _attributes[indexA].Get<T0>(0).Equals(_attributes[indexB].Get<T0>(0));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Interpolate(int attribute, int indexA, int indexB, double ratio)
        {
            _attributes[indexA].attr0 = _attributes[indexA].Get<T0>(0).Interpolate(_attributes[indexB].Get<T0>(0), ratio);
            _attributes[indexB].attr0 = _attributes[indexA].attr0;
        }

        public override MetaAttributeList AddAttributeType<T>()
        {
            var newAttributes = new MetaAttributeList<T0, T>(_attributes.Length);
            for (int i = 0; i < Count; i++)
                newAttributes.Set(new MetaAttribute<T0, T>(_attributes[i].attr0, default(T)), i);
            return newAttributes;
        }

        public override int Count => _attributes.Length;

        public override int CountPerAttribute => 1;
    }

    public class MetaAttributeList<T0, T1> : MetaAttributeList
        where T0 : unmanaged, IInterpolable<T0>
        where T1 : unmanaged, IInterpolable<T1>
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
            switch (attribute)
            {
                case 0:
                    return _attributes[indexA].Get<T0>(0).Equals(_attributes[indexB].Get<T0>(0));
                case 1:
                    return _attributes[indexA].Get<T1>(1).Equals(_attributes[indexB].Get<T1>(1));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Interpolate(int attribute, int indexA, int indexB, double ratio)
        {
            switch (attribute)
            {
                case 0:
                    _attributes[indexA].attr0 = _attributes[indexA].Get<T0>(0).Interpolate(_attributes[indexB].Get<T0>(0), ratio);
                    _attributes[indexB].attr0 = _attributes[indexA].attr0;
                    break;
                case 1:
                    _attributes[indexA].attr1 = _attributes[indexA].Get<T1>(1).Interpolate(_attributes[indexB].Get<T1>(1), ratio);
                    _attributes[indexB].attr1 = _attributes[indexA].attr1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override MetaAttributeList AddAttributeType<T>()
        {
            var newAttributes = new MetaAttributeList<T0, T1, T>(_attributes.Length);
            for (int i = 0; i < Count; i++)
                newAttributes.Set(new MetaAttribute<T0, T1, T>(_attributes[i].attr0, _attributes[i].attr1, default(T)), i);
            return newAttributes;
        }

        public override int Count => _attributes.Length;

        public override int CountPerAttribute => 2;
    }

    public class MetaAttributeList<T0, T1, T2> : MetaAttributeList
        where T0 : unmanaged, IInterpolable<T0>
        where T1 : unmanaged, IInterpolable<T1>
        where T2 : unmanaged, IInterpolable<T2>
    {
        private MetaAttribute<T0, T1, T2>[] _attributes;

        public MetaAttributeList(int length)
        {
            _attributes = new MetaAttribute<T0, T1, T2>[length];
        }

        public override IMetaAttribute this[int index]
        {
            get => _attributes[index];
            set => _attributes[index] = (MetaAttribute<T0, T1, T2>)value;
        }

        public void Set(MetaAttribute<T0, T1, T2> value, int index)
        {
            _attributes[index] = value;
        }

        private void Get(MetaAttribute<T0, T1, T2> value, int index)
        {
            _attributes[index] = value;
        }

        public override MetaAttributeList CreateNew(int length) => new MetaAttributeList<T0, T1, T2>(length);

        public override unsafe bool Equals(int indexA, int indexB, int attribute)
        {
            switch (attribute)
            {
                case 0:
                    return _attributes[indexA].Get<T0>(0).Equals(_attributes[indexB].Get<T0>(0));
                case 1:
                    return _attributes[indexA].Get<T1>(1).Equals(_attributes[indexB].Get<T1>(1));
                case 2:
                    return _attributes[indexA].Get<T2>(2).Equals(_attributes[indexB].Get<T2>(2));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Interpolate(int attribute, int indexA, int indexB, double ratio)
        {
            switch (attribute)
            {
                case 0:
                    _attributes[indexA].attr0 = _attributes[indexA].Get<T0>(0).Interpolate(_attributes[indexB].Get<T0>(0), ratio);
                    _attributes[indexB].attr0 = _attributes[indexA].attr0;
                    break;
                case 1:
                    _attributes[indexA].attr1 = _attributes[indexA].Get<T1>(1).Interpolate(_attributes[indexB].Get<T1>(1), ratio);
                    _attributes[indexB].attr1 = _attributes[indexA].attr1;
                    break;
                case 2:
                    _attributes[indexA].attr2 = _attributes[indexA].Get<T2>(2).Interpolate(_attributes[indexB].Get<T2>(2), ratio);
                    _attributes[indexB].attr2 = _attributes[indexA].attr2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override MetaAttributeList AddAttributeType<T>()
        {
            var newAttributes = new MetaAttributeList<T0, T1, T2, T>(_attributes.Length);
            for (int i = 0; i < Count; i++)
                newAttributes.Set(new MetaAttribute<T0, T1, T2, T>(_attributes[i].attr0, _attributes[i].attr1, _attributes[i].attr2, default(T)), i);
            return newAttributes;
        }

        public override int Count => _attributes.Length;

        public override int CountPerAttribute => 2;
    }

    public class MetaAttributeList<T0, T1, T2, T3> : MetaAttributeList
        where T0 : unmanaged, IInterpolable<T0>
        where T1 : unmanaged, IInterpolable<T1>
        where T2 : unmanaged, IInterpolable<T2>
        where T3 : unmanaged, IInterpolable<T3>
    {
        private MetaAttribute<T0, T1, T2, T3>[] _attributes;

        public MetaAttributeList(int length)
        {
            _attributes = new MetaAttribute<T0, T1, T2, T3>[length];
        }

        public override IMetaAttribute this[int index]
        {
            get => _attributes[index];
            set => _attributes[index] = (MetaAttribute<T0, T1, T2, T3>)value;
        }

        public void Set(MetaAttribute<T0, T1, T2, T3> value, int index)
        {
            _attributes[index] = value;
        }

        private void Get(MetaAttribute<T0, T1, T2, T3> value, int index)
        {
            _attributes[index] = value;
        }

        public override MetaAttributeList CreateNew(int length) => new MetaAttributeList<T0, T1, T2, T3>(length);

        public override unsafe bool Equals(int indexA, int indexB, int attribute)
        {
            switch (attribute)
            {
                case 0:
                    return _attributes[indexA].Get<T0>(0).Equals(_attributes[indexB].Get<T0>(0));
                case 1:
                    return _attributes[indexA].Get<T1>(1).Equals(_attributes[indexB].Get<T1>(1));
                case 2:
                    return _attributes[indexA].Get<T2>(2).Equals(_attributes[indexB].Get<T2>(2));
                case 3:
                    return _attributes[indexA].Get<T3>(3).Equals(_attributes[indexB].Get<T3>(3));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Interpolate(int attribute, int indexA, int indexB, double ratio)
        {
            switch (attribute)
            {
                case 0:
                    _attributes[indexA].attr0 = _attributes[indexA].Get<T0>(0).Interpolate(_attributes[indexB].Get<T0>(0), ratio);
                    _attributes[indexB].attr0 = _attributes[indexA].attr0;
                    break;
                case 1:
                    _attributes[indexA].attr1 = _attributes[indexA].Get<T1>(1).Interpolate(_attributes[indexB].Get<T1>(1), ratio);
                    _attributes[indexB].attr1 = _attributes[indexA].attr1;
                    break;
                case 2:
                    _attributes[indexA].attr2 = _attributes[indexA].Get<T2>(2).Interpolate(_attributes[indexB].Get<T2>(2), ratio);
                    _attributes[indexB].attr2 = _attributes[indexA].attr2;
                    break;
                case 3:
                    _attributes[indexA].attr3 = _attributes[indexA].Get<T3>(3).Interpolate(_attributes[indexB].Get<T3>(3), ratio);
                    _attributes[indexB].attr3 = _attributes[indexA].attr3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override MetaAttributeList AddAttributeType<T>()
        {
            throw new NotImplementedException();
        }

        public override int Count => _attributes.Length;

        public override int CountPerAttribute => 2;
    }
}