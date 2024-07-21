using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopChangeToVideoExtended
{
    internal class Flow
    {
        public bool pinned;
        public int startTime;
        public fPoint[][] flow;
        int[][] intersections;
        public bool interesected;
        int tMax;
        const float minDist = 1f / 2000;
        const float maxDist = 1f / 200;
        float hitTime;
        public Flow(bool pinned, int startTime, int tMax, fPoint[] startFlow)
        {
            this.tMax = tMax;
            this.pinned = pinned;
            this.startTime = startTime;
            flow = new fPoint[FlowReadWrite.maxNumFrames][];
            flow[0] = startFlow;
            intersections = new int[2][] { new int[2] { -1, -1 }, new int[2] { -1, -1 } };


        }

        public void IntersectionCheck(int time, float boxX1, float boxX2, float boxHeight)
        {
            List<int>[,] grid = new List<int>[(int)((boxX2 - boxX1) / (2 * maxDist)) + 1, (int)(boxHeight / (2 * maxDist)) + 1];
            for (int i = 0; i < (int)((boxX2 - boxX1) / (2 * maxDist)) + 1; i++)
            {
                for (int j = 0; j < ((int)(boxHeight / (2 * maxDist)) + 1); j++)
                {
                    grid[i, j] = new List<int>();
                }
            }
            for (int i = 0; i < flow[time].Length - 1; i++)
            {
                int xIndex = (int)((flow[time][i].X - boxX1) / (2 * maxDist));
                int yIndex = (int)(flow[time][i].Y / (2 * maxDist));

                if (0 <= xIndex && xIndex <= (int)((boxX2 - boxX1) / (2 * maxDist)) && 0 <= yIndex && yIndex <= (int)(boxHeight / (2 * maxDist)))
                {

                    grid[xIndex, yIndex].Add(i);
                }
            }

            hitTime = 1;
            for (int i = 1; i < (int)((boxX2 - boxX1) / (2 * maxDist)); i++)
            {
                for (int j = 1; j < ((int)(boxHeight / (2 * maxDist))); j++)
                {
                    int[] LocalPoints = (grid[i, j].Concat(grid[i - 1, j - 1]).Concat(grid[i, j - 1]).Concat(grid[i + 1, j - 1]).Concat(grid[i - 1, j]).Concat(grid[i - 1, j + 1]).Concat(grid[i, j + 1]).Concat(grid[i + 1, j]).Concat(grid[i + 1, j + 1])).ToArray();
                    for (int k = 0; k < LocalPoints.Length; k++)
                    {
                        for (int l = k + 1; l < LocalPoints.Length; l++)
                        {
                            if (LocalPoints[k] != LocalPoints[l] - 1 && LocalPoints[k] != LocalPoints[l] + 1 && !intersections[0].Contains(LocalPoints[l]) && !intersections[0].Contains(LocalPoints[k]))
                            {
                                float rCollision = -1;//  0.01f * Tau;
                                float s = 0, t = 0;
                                float segmentDistance = fPoint.minDistBetweenSegments(flow[time][LocalPoints[l]], flow[time][LocalPoints[l] + 1], flow[time - startTime][LocalPoints[k]], flow[time][LocalPoints[k] + 1], ref s, ref t);
                                float testHitTime = -1;
                                if (segmentDistance < rCollision || intervalCollision(LocalPoints[l], LocalPoints[k], 0, 1, time, ref testHitTime))
                                {
                                    interesected = true;
                                    if (testHitTime < hitTime)
                                    {
                                        hitTime = testHitTime;
                                        intersections[0][0] = LocalPoints[k] < LocalPoints[l] ? LocalPoints[k] : LocalPoints[l];
                                        intersections[0][1] = LocalPoints[k] < LocalPoints[l] ? LocalPoints[l] : LocalPoints[k];
                                        intersections[1] = intersections[0];
                                        flow[time][LocalPoints[l]].intersectP = true;
                                        flow[time][LocalPoints[k]].intersectP = true;
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        private static float maxDistBetweenSegments(fPoint startPoint, fPoint endPoint, fPoint startPointVelocity, fPoint endPointVelocity, float t0, float t1)
        {
            fPoint startPointAtt0 = startPoint + t0 * startPointVelocity;
            fPoint startPointAtt1 = startPoint + t1 * startPointVelocity;
            fPoint endPointAtt0 = endPoint + t0 * endPointVelocity;
            fPoint endPointAtt1 = endPoint + t1 * endPointVelocity;
            return (new float[] { fPoint.Dist(startPointAtt1, startPointAtt0), fPoint.Dist(endPointAtt1, endPointAtt0) }).Max();

        }

        private static float minimumDistancBetweenSegmentsAtTime(fPoint startPointSeg1, fPoint endPointSeg1, fPoint startPointSeg1Velocity, fPoint endPointSeg1Velocity, fPoint startPointSeg2, fPoint endPointSeg2, fPoint startPointSeg2Velocity, fPoint endPointSeg2Velocity, float t0)
        {
            float s = 0, t = 0;
            return fPoint.minDistBetweenSegments(startPointSeg1 + t0 * startPointSeg1Velocity, endPointSeg1 + t0 * endPointSeg1Velocity, startPointSeg2 + t0 * startPointSeg2Velocity, endPointSeg2 + t0 * endPointSeg2Velocity, ref s, ref t);
        }
        private bool intervalCollision(int p1, int p2, float t0, float t1, int time, ref float hitTime)
        {
            fPoint startPointSeg1 = flow[(time - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][p1];
            fPoint endPointSeg1 = flow[(time  - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][p1 + 1];
            fPoint startPointSeg1Velocity = flow[time ][p1] - flow[time][p1];
            fPoint endPointSeg1Velocity = flow[time][p1 + 1] - flow[time    ][p1 + 1];

            fPoint startPointSeg2 = flow[(time - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][p2];
            fPoint endPointSeg2 = flow[(time - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][p2 + 1];
            fPoint startPointSeg2Velocity = flow[time][p2] - flow[(time - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][p2];
            fPoint endPointSeg2Velocity = flow[time][p2 + 1] - flow[(time - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][p2 + 1];

            float maxMovSeg1 = maxDistBetweenSegments(startPointSeg1, endPointSeg1, startPointSeg1Velocity, endPointSeg1Velocity, t0, t1);
            float maxMovSeg2 = maxDistBetweenSegments(startPointSeg2, endPointSeg2, startPointSeg2Velocity, endPointSeg2Velocity, t0, t1);

            float maxMove = maxMovSeg1 + maxMovSeg2;
            float minDistStart = minimumDistancBetweenSegmentsAtTime(startPointSeg1, endPointSeg1, startPointSeg1Velocity, endPointSeg1Velocity, startPointSeg2, endPointSeg2, startPointSeg2Velocity, endPointSeg2Velocity, t0);
            if (minDistStart > maxMove) return false;

            float minDistEnd = minimumDistancBetweenSegmentsAtTime(startPointSeg1, endPointSeg1, startPointSeg1Velocity, endPointSeg1Velocity, startPointSeg2, endPointSeg2, startPointSeg2Velocity, endPointSeg2Velocity, t1);
            if (minDistEnd > maxMove) return false;

            const float intervalEpsilon = 0.0000001f;  //float.Epsilon;
            if (t1 - t0 < intervalEpsilon)
            {

                hitTime = t0;
                //float[] fenijf = fPoint.intersectionCoeffictients(t0 * P1Velocity + P1, t0 * Q1Velocity + Q1, t0 * P2Velocity + P2, t0 * Q2Velocity + Q2);
                return true;
            }
            float midTime = (t0 + t1) / 2;
            if (intervalCollision(p1, p2, t0, midTime, time, ref hitTime)) return true;
            return intervalCollision(p1, p2, midTime, t1, time, ref hitTime);
        }

        public Flow Split(int currentIndex, int currentTime)
        {
            int numberOfRefinedNodes = 5;
            List<fPoint> newFlow = new List<fPoint>();
            List<fPoint> oldFlow = new List<fPoint>();
            for (int i = 0; i < flow[currentIndex].Length; i++)
            {
                flow[currentIndex][i] = hitTime * (flow[currentIndex][i] - flow[(currentIndex - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][i]) + flow[(currentIndex - 1 + FlowReadWrite.maxNumFrames) % FlowReadWrite.maxNumFrames][i];
            }


            for (int i = 0; i < flow[currentIndex].Length; i++)
            {
                if (i <= intersections[0][0] || i > intersections[0][1])
                {
                    oldFlow.Add(flow[currentIndex][i]);
                    if (i < intersections[0][0] && i > intersections[0][0] - numberOfRefinedNodes || i > intersections[0][1] && i < intersections[0][1] + numberOfRefinedNodes)
                    {
                        float numberOfInBetweenPoints = fPoint.Dist(flow[currentIndex][i], flow[currentIndex][i + 1]) / (2 * minDist);
                        for (int j = 1; j < numberOfInBetweenPoints; j++)
                        {
                            oldFlow.Add(j / numberOfInBetweenPoints * (flow[currentIndex][i + 1] - flow[currentIndex][i]) + flow[currentIndex][i]);
                        }

                    }

                }
                else if (i > intersections[1][0] && i <= intersections[1][1])
                {
                    newFlow.Add(flow[currentIndex][i]);
                }

            }

            /*
            bool rm = true;
            do
            {
                rm = false;
                for (int i = 1; i < oldFlow.Count - 1; i++)
                {
                    if (fPoint.Dist(oldFlow[i - 1], oldFlow[i + 1]) < 3*maxDist)
                    {
                        rm = true;
                        oldFlow.RemoveAt(i);
                    }
                }
            } while (rm);



            rm = true;
            do
            {
                rm = false;
                for (int i = 1; i < newFlow.Count - 1; i++)
                {
                    if (fPoint.Dist(newFlow[i - 1], newFlow[i + 1]) < 3 * maxDist)
                    {
                        rm = true;
                        newFlow.RemoveAt(i);
                    }
                }
            } while (rm);
            interesected = false;
            */
            interesected = false;
            flow[currentIndex] = oldFlow.ToArray();
            return new Flow(false, currentTime, tMax, newFlow.ToArray());

        }

        public void ApplyForcing(int time)
        {
            float ForcingMagnitude = 0.005f; //125f/6; // MathfTau / 0.0006f; //0.005f  for 0.0006f
            fPoint[] NewPoints = flow[time];
            fPoint[] normals = new fPoint[NewPoints.Length];
            int length = normals.Length;
            for (int i = 0; i < NewPoints.Length; i++)
            {
                fPoint leftnormal = new fPoint(-NewPoints[i].Y + NewPoints[(i - 1 + length) % length].Y, NewPoints[i].X - NewPoints[(i - 1 + length) % length].X);
                fPoint rightnormal = new fPoint(-NewPoints[(i + 1) % length].Y + NewPoints[i].Y, NewPoints[(i + 1) % length].X - NewPoints[i].X);
                fPoint normal = new fPoint(leftnormal.X + rightnormal.X, leftnormal.Y + rightnormal.Y);
                float mag = normal.Magnitude();
                normals[i] = ForcingMagnitude / mag * normal;
            }
            for (int i = (pinned ? 1 : 0); i < NewPoints.Length - (pinned ? 1 : 0); i++)
            {
                NewPoints[i] -= normals[i];

            }
        }

        public void TotalReMesh(int currentTime)
        {
            bool remesh;
            int N;

            fPoint[] OldPoints = flow[currentTime];
            fPoint[] newPoints = new fPoint[OldPoints.Length];
            for (int i = 0; i < newPoints.Length; i++) newPoints[i] = OldPoints[i].Copy();
            do
            {
                remesh = false;
                N = newPoints.Length;
                for (int i = 0; i < N - (pinned ? 1 : 0); i++)
                {
                    float consecutiveDist = fPoint.Dist(newPoints[(i + 1) % N], newPoints[i]);

                    float adjacentDist = maxDist;
                    if (consecutiveDist <= minDist || consecutiveDist >= maxDist || adjacentDist <= minDist)
                    {
                        remesh = true;
                    }
                }
                if (remesh)
                {
                    newPoints = ReMeshSweep(newPoints);
                    if (newPoints.Length <= 3)
                    {
                        return;
                    }
                    //OldPoints = newPoints;
                }

            } while (remesh);
            flow[currentTime] = newPoints;
        }

        fPoint[] ReMeshSweep(fPoint[] OldPoints)
        {

            List<fPoint> newPoints = new List<fPoint>
            {
                (OldPoints[0])
            };
            for (int i = 1; i < OldPoints.Length; i++)
            {
                float dist = fPoint.Dist(newPoints[newPoints.Count - 1], OldPoints[i]);
                //float adjacentDist = maxDist;
                //if(pinned && i < OldPoints.Length -1) adjacentDist = fPoint.Dist(OldPoints[(i - 1)], OldPoints[i+1]);

                if (dist <= minDist)
                {
                    float sum = dist;
                    while (sum <= minDist && i < OldPoints.Length - 1)
                    {
                        i++;
                        sum += fPoint.Dist(OldPoints[(i - 1)], OldPoints[i]);
                    }
                    if (i < OldPoints.Length - 1) newPoints.Add(OldPoints[i]);


                }
                else if (dist >= maxDist)
                {
                    newPoints.Add(fPoint.midPoint(OldPoints[i], OldPoints[i - 1]));
                    newPoints.Add(OldPoints[i]);
                }
                else
                {
                    newPoints.Add(OldPoints[i]);
                }
            }

            if (!pinned && fPoint.Dist(OldPoints[0], OldPoints[OldPoints.Length - 1]) >= maxDist)
            {
                newPoints.Add(fPoint.midPoint(OldPoints[0], OldPoints[OldPoints.Length - 1]));
            }
            if (!pinned && fPoint.Dist(OldPoints[0], OldPoints[OldPoints.Length - 1]) <= minDist)
            {
                newPoints.Remove(OldPoints[OldPoints.Length - 1]);
            }
            return newPoints.ToArray();
        }
    }
}
