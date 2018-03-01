using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDrivingRides
{
    class Program
    {
        public class Ride
        {
            public int N { get; set; }
            public int StartRow { get; set; }
            public int StartCol { get; set; }
            public int FinishRow { get; set; }
            public int FinishCol { get; set; }
            public int StartT { get; set; }
            public int FinishT { get; set; }
            
            public Ride BestNext { get; set; }

            public void IsNext(Ride r)
            {
                // if possible
                if (r.StartT < StartT + Length())
                    return;

                if (!(StartT + Length() + r.DistToStart(FinishRow, FinishCol) < r.FinishT - r.Length()))
                {
                    return;
                }

                if (BestNext == null)
                {
                    BestNext = r;
                    return;
                }

                if (r.DistToStart(FinishRow, FinishCol) < BestNext.DistToStart(FinishRow, FinishCol))
                {
                    BestNext = r;
                }
            }

            public int Score(int startTime, int bonus)
            {
                var l = Length();
                var res = 0;
                if (startTime + l <= FinishT)
                    res += l;

                if (startTime == StartT)
                    res += bonus;

                return res;
            }

            public int Length()
            {
                return Math.Abs(FinishRow - StartRow) + Math.Abs(FinishCol - StartCol);
            }

            public int DistToStart(int row, int col)
            {
                return Math.Abs(row - StartRow) + Math.Abs(row - StartCol);
            }

            public static int SortByStartT(Ride r1, Ride r2)
            {
                if (r1.StartT < r2.StartT)
                    return -1;
                if (r1.StartT > r2.StartT)
                    return 1;
                return 0;
            }

            public override string ToString()
            {
                return N.ToString();
            }
        }

        public class Problem
        {
            public int RowN { get; set; }
            public int ColN { get; set; }
            public int Fleet { get; set; }
            public int Bonus { get; set; }
            public long T { get; set; }

            public List<Ride> Rides { get; set; } = new List<Ride>();
        }
        
        public class Vehicle
        {
            public int N { get; set; }
            public List<Ride> Rides { get; set; } = new List<Ride>();
            public long Score { get; set; }
            public int CurRow { get; set; }
            public int CurCol { get; set; }
            public long CurT { get; set; }
            
            public List<int> RidesProcessing { get; } = new List<int>();

            public void ApplyRide(Ride r)
            {
                var d = r.DistToStart(CurRow, CurRow);
                CurT += d;
                if (r.StartT < CurT)
                    CurT = r.StartT;

                CurT += r.Length();
                CurRow = r.FinishRow;
                CurCol = r.FinishCol;

                Rides.Add(r);
            }
        }


        private static Problem ReadInput(string filename)
        {            
            var lines = File.ReadAllLines(filename);
            var head = lines[0].Split(' ');
            var res = new Problem()
            {
                RowN = Convert.ToInt32(head[0]),
                ColN = Convert.ToInt32(head[1]),
                Fleet = Convert.ToInt32(head[2]),
                // Skipping N
                Bonus = Convert.ToInt32(head[4]),
                T = Convert.ToInt64(head[0])
            };

            for (int i = 1; i < lines.Length; i++)
            {
                var l = lines[i].Split(' ');
                res.Rides.Add(new Ride() {
                    N = i - 1,
                    StartRow = Convert.ToInt32(l[0]),
                    StartCol = Convert.ToInt32(l[1]),
                    FinishRow = Convert.ToInt32(l[2]),
                    FinishCol = Convert.ToInt32(l[3]),
                    StartT = Convert.ToInt32(l[4]),
                    FinishT = Convert.ToInt32(l[5]),
                });
            }

            return res;
        }

        private static void WriteOutput(List<Vehicle> res, string filename, Problem problem)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            foreach (var v in res)
            {
                var rides = string.Join(" ", v.Rides.Select(r => r.N.ToString()));
                File.AppendAllText(filename, string.Format($"{v.Rides.Count} {rides}\n"));
            }

            if (res.Count < problem.Fleet)
            {
                for (int i = 0; i < problem.Fleet-res.Count; i++)
                {
                    File.AppendAllText(filename, string.Format($"0\n"));
                }
            }
        }
        
        private static long CalcRideSeq(Ride r, int bonus)
        {
            var startT = r.StartRow + r.StartCol;
            if (startT + r.Length() > r.FinishT)
                return 0;

            long score = r.Score(startT, bonus); 
            var cur = r;
            while (cur.BestNext!=null)
            {
                if (startT <= cur.StartT)
                    startT = cur.StartT;
                startT += cur.Length();

                cur = cur.BestNext;

                if (startT + cur.Length() > cur.FinishT)
                    return score;

                score += cur.Score(startT, bonus);
            }
            return score;
        }

        private static List<Vehicle> Algo(Problem problem)
        {
            var res = new List<Vehicle>();

            var ridesToGo = new List<Ride>();
            foreach (var r in problem.Rides)
            {
                ridesToGo.Add(r);
            }

            var prevC = ridesToGo.Count + 1;
            while (ridesToGo.Count > 0 && ridesToGo.Count != prevC && res.Count < problem.Fleet)
            {
                prevC = ridesToGo.Count;
                foreach (var r1 in ridesToGo)
                {
                    r1.BestNext = null;
                    foreach (var r2 in ridesToGo)
                    {
                        if (r1 != r2)
                            r1.IsNext(r2);
                    }
                }

                long bestProf = 0;
                Ride start = null;
                foreach (var r in ridesToGo)
                {
                    var prof = CalcRideSeq(r, problem.Bonus);
                    if (prof > bestProf)
                    {
                        bestProf = prof;
                        start = r;
                    }
                }

                if (start != null)
                {
                    var v = new Vehicle();
                    v.Rides.Add(start);
                    ridesToGo.Remove(start);
                    var cur = start;
                    while (cur.BestNext != null)
                    {
                        cur = cur.BestNext;
                        v.Rides.Add(cur);
                        ridesToGo.Remove(cur);
                    }
                    res.Add(v);
                }
            }

            return res;
        }


        static void Main(string[] args)
        {
            var problem = ReadInput(args[0]);
            var res = Algo(problem);
            WriteOutput(res, "output.out",problem);
        }
    }
}
