using System;

namespace Nanolabo
{
    public unsafe struct SymmetricMatrixF
    {
        public fixed float m[10];

        public float this[int index] => m[index];

        public SymmetricMatrixF(float m0, float m1, float m2, float m3, float m4, float m5, float m6, float m7, float m8, float m9)
        {
            m[0] = m0;
            m[1] = m1;
            m[2] = m2;
            m[3] = m3;
            m[4] = m4;
            m[5] = m5;
            m[6] = m6;
            m[7] = m7;
            m[8] = m8;
            m[9] = m9;
        }

        public SymmetricMatrixF(float a, float b, float c, float d)
        {
            m[0] = a * a;
            m[1] = a * b;
            m[2] = a * c;
            m[3] = a * d;

            m[4] = b * b;
            m[5] = b * c;
            m[6] = b * d;

            m[7] = c * c;
            m[8] = c * d;

            m[9] = d * d;
        }

        public static SymmetricMatrixF operator +(SymmetricMatrixF a, SymmetricMatrixF b)
        {
            return new SymmetricMatrixF(
                a.m[0] + b.m[0], a.m[1] + b.m[1], a.m[2] + b.m[2], a.m[3] + b.m[3],
                a.m[4] + b.m[4], a.m[5] + b.m[5], a.m[6] + b.m[6],
                a.m[7] + b.m[7], a.m[8] + b.m[8],
                a.m[9] + b.m[9]
            );
        }

        public float Determinant(int a11, int a12, int a13, int a21, int a22, int a23, int a31, int a32, int a33)
        {
            return
                this[a11] * this[a22] * this[a33] +
                this[a13] * this[a21] * this[a32] +
                this[a12] * this[a23] * this[a31] -
                this[a13] * this[a22] * this[a31] -
                this[a11] * this[a23] * this[a32] -
                this[a12] * this[a21] * this[a33];
        }

        public override string ToString()
        {
            return $"{m[0]} {m[1]} {m[2]} {m[3]} | {m[4]} {m[5]} {m[6]} | {m[7]} {m[8]} | {m[9]}";
        }
    }
}