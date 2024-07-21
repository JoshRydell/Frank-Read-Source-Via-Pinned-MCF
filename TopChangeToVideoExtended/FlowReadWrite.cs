using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Accord.Video.FFMPEG;
using System.Runtime.CompilerServices;
namespace TopChangeToVideoExtended
{
    static internal class FlowReadWrite
    {
        public const int maxNumFrames = 30;
        const int BitmapHeight = 1018;
        const int BitmapWidth = 1900;
        const int scale = 25;
        const int recentre = 0; //0;;
        static float[] centre = new float[] { (float) BitmapWidth / 2, (float)BitmapHeight / 2 - recentre};


    public static void Write(Flow[] flows, int currentTime)
        {

            string path = Directory.GetCurrentDirectory() + "\\Frame" + (currentTime / maxNumFrames).ToString();
            BinaryWriter binWriter = new BinaryWriter(System.IO.File.Open(path, System.IO.FileMode.Create));
            binWriter.Flush();

            binWriter.Write(currentTime);
            binWriter.Write(flows.Length); //write number of flows first


            for (int i = 0; i < flows.Length; i++)
            {
                binWriter.Write(flows[i].pinned);
                binWriter.Write(flows[i].startTime);
                int numFrames = (currentTime - flows[i].startTime) < maxNumFrames ? currentTime - flows[i].startTime : maxNumFrames; 
                binWriter.Write(numFrames);
                for (int j = 0; j < numFrames; j++)
                {
                    binWriter.Write(flows[i].flow[j].Length);
                }
                for (int j = 0; j < numFrames; j++)
                {
                    for (int k = 0; k < flows[i].flow[j].Length; k++)
                    {
                        binWriter.Write(flows[i].flow[j][k].X);
                        binWriter.Write(flows[i].flow[j][k].Y);
                    }
                }


            }
            binWriter.Close();
        }

        public static void MakeVideo(int currentTime)
        {

            string pathWrite = "C:\\Users\\joshu\\Frames\\Videos\\Video" + (currentTime / maxNumFrames).ToString() + ".wmv";
            string pathRead = "C:\\Users\\joshu\\Frames\\Pictures";
            const int bitrate = 100000000;
            VideoFileWriter vwL = new VideoFileWriter();
            vwL.Open(pathWrite, BitmapWidth, BitmapHeight, new Accord.Math.Rational(1000, 16), VideoCodec.WMV1, bitrate);

            //VideoFileWriter vwP = new VideoFileWriter();
            //vwP.Open(path + "\\CSFPointVideo.wmv", BitmapWidth, BitmapHeight, new Accord.Math.Rational(1000, 16), VideoCodec.WMV1, bitrate);

            for (int i = 0; i < maxNumFrames; i++)
            {
                Bitmap bL = new Bitmap(pathRead + "\\L" + i.ToString() + ".bmp");
                vwL.WriteVideoFrame(bL);
                bL.Dispose();

                //Bitmap bP = new Bitmap(path + "\\Point\\P" + i.ToString() + ".bmp");
                //vwP.WriteVideoFrame(bP);
                //bP.Dispose();
            }

            vwL.Close();
            //vwP.Close();
        }

        public static async void DrawFrames(Flow[] Flows, int currentTime)
        {
            string path = "C:\\Users\\joshu\\Frames\\Pictures";
            for (int t = 0; t < maxNumFrames; t++)
            {
                if (t == maxNumFrames / 4) Console.WriteLine("25% of frames drawn");
                if (t == maxNumFrames / 2) Console.WriteLine("50% of frames drawn");
                if (t == 3*maxNumFrames / 4) Console.WriteLine("75% of frames drawn");
                Bitmap bLines = new Bitmap(BitmapWidth, BitmapHeight);
                Graphics gLines = Graphics.FromImage(bLines);
                gLines.FillRectangle(Brushes.White, 0, 0, BitmapWidth, BitmapHeight);

                //Bitmap bPoints = new Bitmap(BitmapWidth, BitmapHeight);
                //Graphics gPoints = Graphics.FromImage(bPoints);
                //gPoints.FillRectangle(Brushes.White, 0, 0, BitmapWidth, BitmapHeight);
                //Brush[] colours = new Brush[7] { Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Pink };
                Task[] dislocLines = new Task[Flows.Length];
                for (int j = 0; j < Flows.Length; j++)
                {
                    dislocLines[j] = Draw(j,t, currentTime, Flows, gLines);

                }
                await Task.WhenAll(dislocLines);

                //Console.WriteLine("Saving Frame: " + t.ToString());
                gLines.Dispose();
                bLines.Save(path + "\\L" + t.ToString() + ".bmp");
                bLines.Dispose();


                //gPoints.Dispose();
                //bPoints.Save(path + "\\Point\\P" + t.ToString() + ".bmp");
                //bPoints.Dispose();
                //Console.Read();
            }
        }

        private static Task Draw(int j, int t, int currentTime, Flow[] Flows, Graphics gLines )
        {
            if (t + currentTime - maxNumFrames >= Flows[j].startTime)
            {

                if (Flows[j].pinned)
                {

                    for (int i = 0; i < Flows[j].flow[t - Flows[j].startTime].Length - 1; i++)
                    {
                        gLines.DrawLine(Pens.Black, scale * Flows[j].flow[t - Flows[j].startTime][i].X + centre[0], scale * Flows[j].flow[t - Flows[j].startTime][i].Y + centre[1], scale * Flows[j].flow[t - Flows[j].startTime % maxNumFrames][(i + 1) % Flows[j].flow[t - Flows[j].startTime].Length].X + centre[0], scale * Flows[j].flow[t - Flows[j].startTime][(i + 1) % Flows[j].flow[t - Flows[j].startTime].Length].Y + centre[1]);
                    }


                    //for (int i = 0; i < Flows[j].flow[t - Flows[j].startTime].Length; i++)
                    //{
                    //     gPoints.FillEllipse(colours[i % colours.Length], scale * Flows[j].flow[t - Flows[j].startTime][i].X + centre[0], scale * Flows[j].flow[t - Flows[j].startTime][i].Y + centre[1], 2, 2);
                    //}

                }
                else
                {
                    for (int i = 0; i < Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames].Length; i++)
                    {

                        gLines.DrawLine(Pens.Black, scale * Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames][i].X + centre[0],
                            scale * Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames][i].Y + centre[1],
                            scale * Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames][(i + 1) % Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames].Length].X + centre[0],
                            scale * Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames][(i + 1) % Flows[j].flow[(t + currentTime - Flows[j].startTime) % maxNumFrames].Length].Y + centre[1]);


                        //gPoints.FillEllipse(colours[i % colours.Length], scale * Flows[j].flow[t - Flows[j].startTime][i].X + centre[0], scale * Flows[j].flow[t - Flows[j].startTime][i].Y + centre[1], 2, 2);


                    }

                }
            }

            return Task.CompletedTask;
        }

    }
}
