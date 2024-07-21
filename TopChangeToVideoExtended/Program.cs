using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Accord.Video.FFMPEG;
using System.IO;

namespace TopChangeToVideoExtended
{
    internal class Program
    {
        const int N = 500;
        const int tMax = 30000;

        const float Tau = 0.0012f;
        static void Main(string[] args)
        {


            CSF.Flow(N, tMax, Tau, x, y);

        }

       

        static float r(float t)
        {

            return 1;// 2 * (0.5f + (float)Math.Sin(10 * t)); //(3 * (float)Math.Sin(t) + 2) / 2;
        }

        static float x(float t)
        {
            return 2f * (t - 0.5f); //r(t) * (float)Math.Cos(Math.PI*t); //2 * t - 1;
        }

        static float y(float t)
        {
            return 0;// r(t) * (float)Math.Sin(Math.PI * t); //0;
        }

        
    }
}

