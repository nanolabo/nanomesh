using System;

namespace Nanomesh
{
    public unsafe struct SymmetricMatrix
    {
        public fixed double m[10];

        public double this[int index] => m[index];

        public SymmetricMatrix(double m0, double m1, double m2, double m3, double m4, double m5, double m6, double m7, double m8, double m9)
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

        public SymmetricMatrix(double a, double b, double c, double d)
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

        public static SymmetricMatrix operator +(SymmetricMatrix a, SymmetricMatrix b)
        {
            return new SymmetricMatrix(
                a.m[0] + b.m[0], a.m[1] + b.m[1], a.m[2] + b.m[2], a.m[3] + b.m[3],
                a.m[4] + b.m[4], a.m[5] + b.m[5], a.m[6] + b.m[6],
                a.m[7] + b.m[7], a.m[8] + b.m[8],
                a.m[9] + b.m[9]
            );
        }

        public double Determinant(int a11, int a12, int a13, int a21, int a22, int a23, int a31, int a32, int a33)
        {
            return
                this[a11] * this[a22] * this[a33] +
                this[a13] * this[a21] * this[a32] +
                this[a12] * this[a23] * this[a31] -
                this[a13] * this[a22] * this[a31] -
                this[a11] * this[a23] * this[a32] -
                this[a12] * this[a21] * this[a33];
        }

        public double DeterminantXYZ()
        {
            return
                this[0] * this[4] * this[7] +
                this[2] * this[1] * this[5] +
                this[1] * this[5] * this[2] -
                this[2] * this[4] * this[2] -
                this[0] * this[5] * this[5] -
                this[1] * this[1] * this[7];
        }

        public double DeterminantX() {
            return
                this[1] * this[5] * this[8] +
                this[3] * this[4] * this[7] +
                this[2] * this[6] * this[5] -
                this[3] * this[5] * this[5] -
                this[1] * this[6] * this[7] -
                this[2] * this[4] * this[8];
        }

        public double DeterminantY()
        {
            return
                this[0] * this[5] * this[8] +
                this[3] * this[1] * this[7] +
                this[2] * this[6] * this[2] -
                this[3] * this[5] * this[2] -
                this[0] * this[6] * this[7] -
                this[2] * this[1] * this[8];
        }

        public double DeterminantZ()
        {
            return
                this[0] * this[4] * this[8] +
                this[3] * this[1] * this[5] +
                this[1] * this[6] * this[2] -
                this[3] * this[4] * this[2] -
                this[0] * this[6] * this[5] -
                this[1] * this[1] * this[8];
        }

        public override string ToString()
        {
            return $"{m[0]} {m[1]} {m[2]} {m[3]} | {m[4]} {m[5]} {m[6]} | {m[7]} {m[8]} | {m[9]}";
        }
    }
}