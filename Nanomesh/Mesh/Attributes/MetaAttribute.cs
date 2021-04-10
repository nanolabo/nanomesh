using System;

namespace Nanomesh
{
    public unsafe interface IMetaAttribute
    {
        IMetaAttribute Set<K>(int index, K value) where K : unmanaged;
        K Get<K>(int index) where K : unmanaged;
    }

    public unsafe struct MetaAttribute<T0> : IMetaAttribute
        where T0 : unmanaged
    {
        public T0 attr0;

        public MetaAttribute(T0 attr0)
        {
            this.attr0 = attr0;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public unsafe struct MetaAttribute<T0, T1> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
    {
        public T0 attr0;
        public T1 attr1;

        public MetaAttribute(T0 attr0, T1 attr1)
        {
            this.attr0 = attr0;
            this.attr1 = attr1;
        }

        public unsafe K Get<K>(int index) where K : unmanaged
        {
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                case 1:
                    fixed (T1* k = &attr1)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                case 1:
                    fixed (T1* k = &attr1)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public unsafe struct MetaAttribute<T0, T1, T2> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
    {
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
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                case 1:
                    fixed (T1* k = &attr1)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                case 2:
                    fixed (T2* k = &attr2)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                case 1:
                    fixed (T1* k = &attr1)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                case 2:
                    fixed (T2* k = &attr2)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public unsafe struct MetaAttribute<T0, T1, T2, T3> : IMetaAttribute
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
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
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                case 1:
                    fixed (T1* k = &attr1)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                case 2:
                    fixed (T2* k = &attr2)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                case 3:
                    fixed (T3* k = &attr3)
                    {
                        K* kk = (K*)k;
                        return kk[0];
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Shorter idea but only C“ 8.0 :
            //fixed (void* v = &this)
            //{
            //    byte* b = (byte*)v;
            //    b += Positions[index];
            //    return ((K*)b)[0];
            //};
        }

        public IMetaAttribute Set<K>(int index, K value) where K : unmanaged
        {
            switch (index)
            {
                case 0:
                    fixed (T0* k = &attr0)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                case 1:
                    fixed (T1* k = &attr1)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                case 2:
                    fixed (T2* k = &attr2)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                case 3:
                    fixed (T3* k = &attr3)
                    {
                        K* kk = (K*)k;
                        kk[0] = value;
                        return this;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}