using System;

namespace Nanolabo
{
    public struct SymmetricMatrix
    {
        public float m0;
        public float m1;
        public float m2;
        public float m3;
        public float m4;
        public float m5;
        public float m6;
        public float m7;
        public float m8;
        public float m9;

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return m0;
                    case 1:
                        return m1;
                    case 2:
                        return m2;
                    case 3:
                        return m3;
                    case 4:
                        return m4;
                    case 5:
                        return m5;
                    case 6:
                        return m6;
                    case 7:
                        return m7;
                    case 8:
                        return m8;
                    case 9:
                        return m9;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public SymmetricMatrix(float c)
        {
            this.m0 = c;
            this.m1 = c;
            this.m2 = c;
            this.m3 = c;
            this.m4 = c;
            this.m5 = c;
            this.m6 = c;
            this.m7 = c;
            this.m8 = c;
            this.m9 = c;
        }

        public SymmetricMatrix(float m0, float m1, float m2, float m3,
            float m4, float m5, float m6, float m7, float m8, float m9)
        {
            this.m0 = m0;
            this.m1 = m1;
            this.m2 = m2;
            this.m3 = m3;
            this.m4 = m4;
            this.m5 = m5;
            this.m6 = m6;
            this.m7 = m7;
            this.m8 = m8;
            this.m9 = m9;
        }

        public SymmetricMatrix(float a, float b, float c, float d)
        {
            this.m0 = a * a;
            this.m1 = a * b;
            this.m2 = a * c;
            this.m3 = a * d;

            this.m4 = b * b;
            this.m5 = b * c;
            this.m6 = b * d;

            this.m7 = c * c;
            this.m8 = c * d;

            this.m9 = d * d;
        }

        public static SymmetricMatrix operator +(SymmetricMatrix a, SymmetricMatrix b)
        {
            return new SymmetricMatrix(
                a.m0 + b.m0, a.m1 + b.m1, a.m2 + b.m2, a.m3 + b.m3,
                a.m4 + b.m4, a.m5 + b.m5, a.m6 + b.m6,
                a.m7 + b.m7, a.m8 + b.m8,
                a.m9 + b.m9
            );
        }

        internal float Determinant1()
        {
            return
                m0 * m4 * m7 +
                m2 * m1 * m5 +
                m1 * m5 * m2 -
                m2 * m4 * m2 -
                m0 * m5 * m5 -
                m1 * m1 * m7;
        }

        internal float Determinant2()
        {
            return
                m1 * m5 * m8 +
                m3 * m4 * m7 +
                m2 * m6 * m5 -
                m3 * m5 * m5 -
                m1 * m6 * m7 -
                m2 * m4 * m8;
        }

        internal float Determinant3()
        {
            return
                m0 * m5 * m8 +
                m3 * m1 * m7 +
                m2 * m6 * m2 -
                m3 * m5 * m2 -
                m0 * m6 * m7 -
                m2 * m1 * m8;
        }

        internal float Determinant4()
        {
            return
                m0 * m4 * m8 +
                m3 * m1 * m5 +
                m1 * m6 * m2 -
                m3 * m4 * m2 -
                m0 * m6 * m5 -
                m1 * m1 * m8;
        }

        public float Determinant(
            int a11, int a12, int a13,
            int a21, int a22, int a23,
            int a31, int a32, int a33)
        {
            return
                this[a11] * this[a22] * this[a33] +
                this[a13] * this[a21] * this[a32] +
                this[a12] * this[a23] * this[a31] -
                this[a13] * this[a22] * this[a31] -
                this[a11] * this[a23] * this[a32] -
                this[a12] * this[a21] * this[a33];
        }
    }
}