namespace Nanomesh
{
    public readonly struct SymmetricMatrix
    {
        public readonly double m0, m1, m2, m3, m4, m5, m6, m7, m8, m9;

        public SymmetricMatrix(in double m0, in double m1, in double m2, in double m3, in double m4, in double m5, in double m6, in double m7, in double m8, in double m9)
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

        public SymmetricMatrix(in double a, in double b, in double c, in double d)
        {
            m0 = a * a;
            m1 = a * b;
            m2 = a * c;
            m3 = a * d;

            m4 = b * b;
            m5 = b * c;
            m6 = b * d;

            m7 = c * c;
            m8 = c * d;

            m9 = d * d;
        }

        public static SymmetricMatrix operator +(in SymmetricMatrix a, in SymmetricMatrix b)
        {
            return new SymmetricMatrix(
                a.m0 + b.m0, a.m1 + b.m1, a.m2 + b.m2, a.m3 + b.m3,
                a.m4 + b.m4, a.m5 + b.m5, a.m6 + b.m6,
                a.m7 + b.m7, a.m8 + b.m8,
                a.m9 + b.m9
            );
        }

        public double DeterminantXYZ()
        {
            return
                m0 * m4 * m7 +
                m2 * m1 * m5 +
                m1 * m5 * m2 -
                m2 * m4 * m2 -
                m0 * m5 * m5 -
                m1 * m1 * m7;
        }

        public double DeterminantX() {
            return
                m1 * m5 * m8 +
                m3 * m4 * m7 +
                m2 * m6 * m5 -
                m3 * m5 * m5 -
                m1 * m6 * m7 -
                m2 * m4 * m8;
        }

        public double DeterminantY()
        {
            return
                m0 * m5 * m8 +
                m3 * m1 * m7 +
                m2 * m6 * m2 -
                m3 * m5 * m2 -
                m0 * m6 * m7 -
                m2 * m1 * m8;
        }

        public double DeterminantZ()
        {
            return
                m0 * m4 * m8 +
                m3 * m1 * m5 +
                m1 * m6 * m2 -
                m3 * m4 * m2 -
                m0 * m6 * m5 -
                m1 * m1 * m8;
        }

        public override string ToString()
        {
            return $"{m0} {m1} {m2} {m3}| {m4} {m5} {m6} | {m7} {m8} | {m9}";
        }
    }
}