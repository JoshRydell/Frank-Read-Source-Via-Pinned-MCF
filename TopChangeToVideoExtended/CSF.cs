using Accord.MachineLearning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TopChangeToVideoExtended;
using System.Threading;
using Accord;
namespace TopChangeToVideoExtended
{
    internal class CSF
    {
        static float a = (float)(1);
        static int numMeshPoints;

        public static async void Flow(int N, int tMax, float Tau, Func<float, float> x, Func<float, float> y)
        {
            StreamWriter lengthWriter = new StreamWriter("C:\\Users\\joshu\\Frames\\length.txt");
            StreamWriter logLengthWriter = new StreamWriter("C:\\Users\\joshu\\Frames\\log length.txt");
            StreamWriter streamWriterVideoNames = new StreamWriter("C:\\Users\\joshu\\Frames\\fileNames.txt"); 
            numMeshPoints = N;
            float h = (float)(a / (N - 1));
            fPoint[] startFlow = new fPoint[numMeshPoints];

            for (int i = 0; i < N; i++)
            {
                startFlow[i] = new fPoint(x(i * h), y(i * h));
            }
            List<Flow> flows = new List<Flow>
            {
                new Flow(true, 0, tMax, startFlow)
            };
            for (int i = 0; i < tMax-1; i++)
            {


                float length = 0f;


                int startingNumberOfFlows = flows.Count;
                Task<float>[] tasks = new Task<float>[startingNumberOfFlows];
                for (int j = 0; j < startingNumberOfFlows; j++)
                {


                    tasks[j] = CalcStep(i, j, flows, Tau);



                }
                await Task.WhenAll(tasks);
                for(int j = 0; j < startingNumberOfFlows; j++)
                {
                    length += tasks[j].Result;
                }
                Console.WriteLine(i.ToString());
                if ((i + 2) % FlowReadWrite.maxNumFrames == 0)
                {
                    FlowReadWrite.DrawFrames(flows.ToArray(), i + 2);
                    FlowReadWrite.MakeVideo(i + 1);
                    streamWriterVideoNames.WriteLine("file 'C:\\Users\\joshu\\Frames\\Videos\\Video" + ((i+1)/FlowReadWrite.maxNumFrames).ToString()  + ".wmv'");
                    //FlowReadWrite.Write(flows.ToArray(), i+1);
                }
                lengthWriter.Write(length.ToString() +",");
                logLengthWriter.Write(Math.Log(length).ToString() + ",");
            }
            lengthWriter.Close();
            logLengthWriter.Close();
            streamWriterVideoNames.Close();
        }

        private static Task<float> CalcStep(int i, int j, List<Flow> flows, float Tau)
        {
            float length = 0f;
            int index = i - flows[j].startTime;
            flows[j].flow[(index + 1) % FlowReadWrite.maxNumFrames] = flows[j].pinned? StepPinned(flows[j].flow[index % FlowReadWrite.maxNumFrames], Tau).Result : StepUnPinned(flows[j].flow[index % FlowReadWrite.maxNumFrames], Tau).Result;
            flows[j].ApplyForcing((index+1) % FlowReadWrite.maxNumFrames);


            if (flows[j].pinned)
            {
                flows[j].IntersectionCheck((index + 1) % FlowReadWrite.maxNumFrames, flows[j].flow[0][0].X, flows[j].flow[0][flows[j].flow[0].Length - 1].X, -flows[j].flow[0][0].X + flows[j].flow[0][flows[j].flow[0].Length - 1].X);
                if (flows[j].interesected)
                {
                    Flow newFlow = flows[j].Split((index + 1) % FlowReadWrite.maxNumFrames, i + 1);
                    if (newFlow.flow[0].Count() > 100) flows.Add(newFlow);

                }
            }
                    
            flows[j].TotalReMesh((index + 1) % FlowReadWrite.maxNumFrames);
            for (int k = 1; k < flows[j].flow[(index + 1) % FlowReadWrite.maxNumFrames].Count() + (flows[j].pinned ? 0 : 1); k++)
            {
                length += fPoint.Dist(flows[j].flow[(index + 1) % FlowReadWrite.maxNumFrames][k % flows[j].flow[(index + 1) % FlowReadWrite.maxNumFrames].Count()], flows[j].flow[(index + 1) % FlowReadWrite.maxNumFrames][k - 1]);
            }
            
            return Task.FromResult(length);
        }

        static async Task<fPoint[]>  StepUnPinned(fPoint[] OldPoints, float Tau)
        {

            fPoint[] newPoints = new fPoint[OldPoints.Length];
            int N = newPoints.Length;


            float[] q = new float[N];
            for (int i = 0; i < N; i++)
            {
                q[(i + 1) % N] = fPoint.Dist(OldPoints[(i + 1) % N], OldPoints[i]);

            }

            float[] dX = new float[N];
            float[] dY = new float[N];
            float[] b = new float[N];
            float[] a = new float[N];
            float[] c = new float[N];
            for (int i = 0; i < N; i++)
            {
                dX[i] = 1 / (2 * Tau) * OldPoints[i].X * (q[i] + q[(i + 1) % N]);
                dY[i] = 1 / (2 * Tau) * OldPoints[i].Y * (q[i] + q[(i + 1) % N]);
                b[i] = (q[i] + q[(i + 1) % N]) / (2 * Tau) + 1 / q[(i + 1) % N] + 1 / q[i];
                a[i] = -1 / q[i];
                c[i] = -1 / q[(i + 1) % N];


            }


            Task<float[]> CalcX = periodicTriDiagSolver(N, a, b, c, dX);
            Task< float[]> CalcY = periodicTriDiagSolver(N, a, b, c, dY);
            await Task.WhenAll(CalcX, CalcY);
            float[] X = CalcX.Result;
            float[] Y = CalcY.Result;
            if (X.Contains(float.NaN))
            {
                //throw new Exception("nan");
            }
            for (int i = 0; i < X.Length; i++)
            {
                newPoints[i] = new fPoint(X[i], Y[i]);

            }
            //ApplyForcing(newPoints);
            return newPoints;//otalReMesh(newPoints);

        }

        static async Task<fPoint[]> StepPinned(fPoint[] OldPoints, float Tau)
        {

            fPoint[] newPoints = new fPoint[OldPoints.Length];//TotalReMesh(OldPoints, minDist, maxDist);
            int N = newPoints.Length;


            float[] q = new float[N];
            for (int i = 0; i < N; i++)
            {
                q[(i + 1) % N] = fPoint.Dist(OldPoints[(i + 1) % N], OldPoints[i]);

            }


            float[] dX = new float[N - 2];
            float[] dY = new float[N - 2];
            float[] b = new float[N - 2];
            float[] a = new float[N - 2];
            float[] c = new float[N - 2];
            for (int i = 1; i < N - 1; i++)
            {
                dX[i - 1] = 1 / (2 * Tau) * OldPoints[i].X * (q[i] + q[(i + 1) % N]);
                dY[i - 1] = 1 / (2 * Tau) * OldPoints[i].Y * (q[i] + q[(i + 1) % N]);
                b[i - 1] = (q[i] + q[(i + 1) % N]) / (2 * Tau) + 1 / q[(i + 1) % N] + 1 / q[i];
                a[i - 1] = -1 / q[i];
                c[i - 1] = -1 / q[(i + 1) % N];
            }
            dX[0] += OldPoints[0].X / q[1];
            dY[0] += OldPoints[0].Y / q[1];
            dX[N - 3] += OldPoints[N - 1].X / q[N - 1];
            dY[N - 3] += OldPoints[N - 1].Y / q[N - 1];

            Task < float[]> CalcX = triDiagSolver(N - 2, a, b, c, dX);
            Task < float[]> CalcY = triDiagSolver(N - 2, a, b, c, dY);
            await Task.WhenAll(CalcX, CalcY);
            float[] X = CalcX.Result;
            float[] Y = CalcY.Result;
            for (int i = 0; i < N; i++)
            {
                if (i == 0 || i == N - 1)
                {
                    newPoints[i] = OldPoints[i];
                }
                else
                {
                    newPoints[i] = new fPoint(X[i - 1], Y[i - 1]);
                }
            }


            return newPoints;

        }

        static Task<float[]> triDiagSolver(int N, float[] a, float[] b, float[] c, float[] d)
        {
            float[] cPrime = new float[N];
            float[] dPrime = new float[N];
            float[] x = new float[N];
            for (int i = 0; i < N; i++)
            {
                if (i == 0)
                {
                    cPrime[i] = c[i] / b[i];
                    dPrime[i] = d[i] / b[i];
                }
                else
                {
                    if (i < N - 1) cPrime[i] = c[i] / (b[i] - a[i] * cPrime[i - 1]);
                    dPrime[i] = (d[i] - a[i] * dPrime[i - 1]) / (b[i] - a[i] * cPrime[i - 1]);
                }
            }

            for (int i = N - 1; i >= 0; i--)
            {
                if (i == N - 1)
                {
                    x[i] = dPrime[i];
                }
                else
                {
                    x[i] = dPrime[i] - cPrime[i] * x[i + 1];
                }
            }

            return Task.FromResult(x);
        }
        //algorithmn from wiki
        static async Task<float[]> periodicTriDiagSolver(int N, float[] a, float[] b, float[] c, float[] d)
        {

            float gamma = b[0] + 1;



            float[] Ba = new float[N];
            float[] Bb = new float[N];
            float[] Bc = new float[N];
            float[] u = new float[N];
            float[] v = new float[N];

            for (int i = 0; i < N; i++)
            {
                if (i == 0)
                {
                    Bb[i] = b[i] - gamma;
                    Ba[i] = 0;
                    Bc[i] = c[i];
                    u[i] = gamma;
                    v[i] = 1;
                }
                else if (i == N - 1)
                {
                    Bb[i] = b[i] - c[i] * a[0] / gamma;
                    Ba[i] = a[i];
                    Bc[i] = 0;
                    u[i] = c[i];
                    v[i] = a[0] / gamma;
                }
                else
                {
                    Bb[i] = b[i];
                    Ba[i] = a[i];
                    Bc[i] = c[i];
                }
            }

            Task < float[]> CalcY = triDiagSolver(N, Ba, Bb, Bc, d);
            Task< float[]> CalcQ = triDiagSolver(N, Ba, Bb, Bc, u);
            await Task.WhenAll(CalcY, CalcQ);
            float[] y = CalcY.Result;
            float[] q = CalcQ.Result;
            float[] x = new float[N];

            float vTy = y[0] + v[N - 1] * y[N - 1];
            float vTq = q[0] + v[N - 1] * q[N - 1];

            for (int i = 0; i < N; i++)
            {
                x[i] = y[i] - vTy * q[i] / (1 + vTq);
            }


            return x;

        }
    }
}
