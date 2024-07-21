using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopChangeToVideoExtended
{
    public struct fPoint
    {
        public float X;
        public float Y;
        //public int N;
        public bool intersectP;
        public fPoint(float X, float Y) : this()
        {
            this.X = X;
            this.Y = Y;
            //N = -1;
            //intersectP = false;
        }

        public static fPoint operator +(fPoint P1, fPoint P2)
        {
            return new fPoint(P1.X + P2.X, P1.Y + P2.Y);
        }

        public static fPoint operator *(float a, fPoint P1)
        {
            return new fPoint(a * P1.X, a * P1.Y);
        }

        public static fPoint operator -(fPoint P1, fPoint P2)
        {
            return P1 + (-1) * P2;
        }

        public static fPoint operator /(fPoint P1, float a)
        {
            return 1 / a * P1;
        }

        public static float Dist(fPoint P1, fPoint P2)
        {
            return (P1 - P2).Magnitude();
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public fPoint Copy()
        {
            return new fPoint(this.X, this.Y);
        }
        public static fPoint midPoint(fPoint P1, fPoint P2)
        {
            return (P1 + P2) / 2;
        }

        private static float Clamp(float t, float t0, float t1)
        {
            if (t < t0) return t0;
            if (t > t1) return t1;
            return t;
        }

        public static float Dot(fPoint P1, fPoint P2)
        {
            return P1.X * P2.X + P1.Y * P2.Y;
        }
        public static float minDistBetweenSegments(fPoint P1, fPoint Q1, fPoint P2, fPoint Q2, ref float s, ref float t)
        {

            fPoint c1, c2;
            const float epsilon = 0.00001f;
            fPoint d1 = Q1 - P1;
            fPoint d2 = Q2 - P2;
            fPoint r = P1 - P2;
            float a = Dot(d1, d1);
            float e = Dot(d2, d2);
            float f = Dot(d2, r);

            if (a <= epsilon && e <= epsilon)
            {
                s = 0;
                t = 0;
                c1 = P1;
                c2 = P2;
                return Dist(P1, P2);
            }
            if (a <= epsilon)
            {
                s = 0;
                t = f / e;
                t = Clamp(t, 0, 1);
            }
            else
            {
                float c = Dot(d1, r);
                if (e <= epsilon)
                {
                    t = 0;
                    s = Clamp(-c / a, 0, 1);
                }
                else
                {
                    float b = Dot(d1, d2);
                    float denom = a * e - b * b;

                    if (denom != 0)
                    {
                        s = Clamp((b * f - c * e) / denom, 0, 1);
                    }
                    else
                    {
                        s = 0;
                    }
                    t = (b * s + f) / e; //Dot((StartPointSeg1 + s * d1) - StartPointSeg2, d2) / Dot(d2, d2);

                    if (t < 0)
                    {
                        t = 0;
                        s = Clamp(-c / a, 0, 1);
                    }
                    else if (t > 1)
                    {
                        t = 1;
                        s = Clamp((b - c) / a, 0, 1);
                    }
                }
            }

            c1 = P1 + s * d1;
            c2 = P2 + t * d2;
            return Dist(c1, c2);
        }
    }



}
