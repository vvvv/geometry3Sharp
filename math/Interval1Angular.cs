using System;

namespace g3
{
    //   TODO: should check that a <= b !!
    public struct Interval1dAngular
    {
        public double a;
        public double b;

        public static double ConstrainAngle(double input)
        {
            if (input < 0)
                return input + Math.PI * 2;
            else if (input > Math.PI * 2)
                return input - Math.PI * 2;
            else
                return input;
        }

        public Interval1dAngular(double f) { a = b = ConstrainAngle(f); }
        public Interval1dAngular(double x, double y) { this.a = ConstrainAngle(x); this.b = ConstrainAngle(y); }
        public Interval1dAngular(double[] v2) { a = ConstrainAngle(v2[0]); b = ConstrainAngle(v2[1]); }
        public Interval1dAngular(Interval1dAngular copy) { a = copy.a; b = copy.b; }

        public bool IsConstant
        {
            get { return b == a; }
        }

        public bool Contains(double d)
        {
            double dConstrained = ConstrainAngle(d);
            if (a < b)
                return a <= dConstrained && dConstrained <= b;
            else
                return a <= dConstrained || dConstrained <= b;
        }

        public override string ToString()
        {
            return string.Format("[{0:F8},{1:F8}]", a, b);
        }
    }
}
