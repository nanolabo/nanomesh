namespace Nanomesh
{
    public unsafe interface IMetaAttribute
    {
        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged;
        public K Get<K>(int index) where K : unmanaged;
        public void* GetPtr(int index);
    }

    public unsafe struct MetaAttribute<T0> : IMetaAttribute
        where T0 : unmanaged
    {
        public static int[] _Positions;
        public static int[] Positions => _Positions ??= new[] { 0 };

        public T0 attr0;

        public MetaAttribute(T0 attr0)
        {
            this.attr0 = attr0;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                return ((K*)b)[0];
            };
        }

        public void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                K* k = (K*)b;
                k[0] = value;
                return this;
            };
        }
    }

    public unsafe struct MetaAttribute<T0, T1> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
    {
        public static int[] _Positions;
        public static int[] Positions => _Positions ??= new[] { 0, sizeof(T0) };

        public T0 attr0;
        public T1 attr1;

        public MetaAttribute(T0 attr0, T1 attr1)
        {
            this.attr0 = attr0;
            this.attr1 = attr1;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                return ((K*)b)[0];
            };
        }

        public unsafe void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                K* k = (K*)b;
                k[0] = value;
                return this;
            };
        }
    }

    public unsafe struct MetaAttribute<T0, T1, T2> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
    {
        public static int[] _Positions;
        public static int[] Positions => _Positions ??= new[] { 0, sizeof(T0), sizeof(T1) };

        public T0 attr0;
        public T1 attr1;
        public T2 attr2;

        public MetaAttribute(T0 attr0, T1 attr1, T2 attr2)
        {
            this.attr0 = attr0;
            this.attr1 = attr1;
            this.attr2 = attr2;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                return ((K*)b)[0];
            };
        }

        public unsafe void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                K* k = (K*)b;
                k[0] = value;
                return this;
            };
        }
    }

    public unsafe struct MetaAttribute<T0, T1, T2, T3> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        public static int[] _Positions;
        public static int[] Positions => _Positions ??= new[] { 0, sizeof(T0), sizeof(T1), sizeof(T2) };

        public T0 attr0;
        public T1 attr1;
        public T2 attr2;
        public T3 attr3;

        public MetaAttribute(T0 attr0, T1 attr1, T2 attr2, T3 attr3)
        {
            this.attr0 = attr0;
            this.attr1 = attr1;
            this.attr2 = attr2;
            this.attr3 = attr3;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                return ((K*)b)[0];
            };
        }

        public unsafe void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            fixed (void* v = &this)
            {
                byte* b = (byte*)v;
                b += Positions[index];
                K* k = (K*)b;
                k[0] = value;
                return this;
            };
        }
    }
}