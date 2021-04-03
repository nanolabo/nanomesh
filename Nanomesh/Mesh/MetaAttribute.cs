namespace Nanomesh
{
    public unsafe interface IMetaAttribute
    {
        public K Get<K>(int index) where K : unmanaged;
        public void* GetPtr(int index);
    }

    public unsafe struct MetaAttribute<T0> : IMetaAttribute
        where T0 : unmanaged
    {
        public static int[] Positions;

        static MetaAttribute()
        {
            Positions = new[] {
                sizeof(T0)
            };
        }

        public T0 attr0;

        public MetaAttribute(T0 attr0)
        {
            this.attr0 = attr0;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* b = &this) { return ((K*)b)[index]; };
        }

        public void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }
    }

    public unsafe struct MetaAttribute<T0, T1> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
    {
        public static int[] Positions;

        static MetaAttribute()
        {
            Positions = new[] {
                sizeof(T0),
                sizeof(T0) + sizeof(T1)
            };
        }

        public T0 attr0;
        public T1 attr1;

        public MetaAttribute(T0 attr0, T1 attr1)
        {
            this.attr0 = attr0;
            this.attr1 = attr1;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* b = &this) { return ((K*)b)[index]; };
        }

        public unsafe void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }
    }

    public unsafe readonly struct MetaAttribute<T0, T1, T2> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
    {
        public static int[] Positions;

        static MetaAttribute()
        {
            Positions = new[] {
                sizeof(T0),
                sizeof(T0) + sizeof(T1),
                sizeof(T0) + sizeof(T1) + sizeof(T2)
            };
        }

        public readonly T0 attr0;
        public readonly T1 attr1;
        public readonly T2 attr2;

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* b = &this) { return ((K*)b)[index]; };
        }

        public unsafe void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }
    }

    public unsafe readonly struct MetaAttribute<T0, T1, T2, T3> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        public static int[] Positions;

        static MetaAttribute()
        {
            Positions = new[] {
                sizeof(T0),
                sizeof(T0) + sizeof(T1),
                sizeof(T0) + sizeof(T1) + sizeof(T2),
                sizeof(T0) + sizeof(T1) + sizeof(T2) + sizeof(T3)
            };
        }

        public readonly T0 attr0;
        public readonly T1 attr1;
        public readonly T2 attr2;
        public readonly T3 attr3;

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            fixed (void* b = &this) { return ((K*)b)[index]; };
        }

        public unsafe void* GetPtr(int index)
        {
            fixed (void* b = &this) { return b; };
        }
    }
}