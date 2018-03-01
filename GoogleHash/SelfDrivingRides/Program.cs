using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDrivingRides
{
    public class Program
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
            public List<TakenRide> Rides { get; set; } = new List<TakenRide>();
            public long Score { get; set; }
            public int CurRow { get; set; }
            public int CurCol { get; set; }
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

        private static void WriteOutput(List<Vehicle> res, string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            foreach (var v in res)
            {
                var rides = string.Join(" ", v.Rides.Select(r => r.Ride.N.ToString()));
                File.AppendAllText(filename, string.Format($"{v.Rides.Count} {rides}\n"));
            }
        }



        static void Main(string[] args)
        {
            var problem = ReadInput(args[0]);
            var res = new List<Vehicle>();
            for (int i = 0; i < problem.Fleet; i++)
            {
                res.Add(new Vehicle
                {
                    CurCol = 0,
                    CurRow = 0,
                    N = i
                });
            }

            var algo = new AvidRides(problem, res);

            WriteOutput(res, "output.out");
        }
    }

    public class AvidRides
    {
        private readonly Program.Problem _problem;
        private readonly List<Program.Ride> _rides;
        private Program.Vehicle[] _vehicles;

        public AvidRides(Program.Problem problem, List<Program.Vehicle> vehicles)
        {
            _problem = problem;
            _rides = problem.Rides.OrderBy(ride => ride.StartT).ToList();
            _vehicles = vehicles.ToArray();

            foreach (var vehicle in _vehicles)
            {
                var first = _rides.Select(ride => IfTake(null, ride)).FirstOrDefault(ride => ride.Score > 0);
                if (first == null)
                    break;
                TakeRide(first, vehicle, _rides);
            }
        }

        private void TakeRide(TakenRide ride, Program.Vehicle vehicle, List<Program.Ride> rides)
        {
            vehicle.Rides.Add(ride);
            rides.Remove(ride.Ride);
            TakenRide bestResult = null;
            foreach (var possibleRide in rides)
            {
                var result = IfTake(vehicle.Rides.Last(), possibleRide);
                var canRide = result.Score > 0;
                if (!canRide)
                    continue;
                if (bestResult == null)
                    bestResult = result;
                if (result.Score > bestResult.Score)
                {
                    bestResult = result;
                }
            }

            if (bestResult != null)
                TakeRide(bestResult, vehicle, rides);
        }

        private TakenRide IfTake(TakenRide lastRide, Program.Ride ride)
        {
            int possibleStart = 0;
            if (lastRide != null)
            {
                var lastRideRide = lastRide.Ride;
                var dist = Dist(lastRideRide, ride);
                possibleStart = lastRide.EndIteration + dist;
            }
            else
            {
                possibleStart = 0 + ride.DistToStart(0, 0);
            }

            var posEnd = possibleStart + ride.Length();
            var endIteration = GetEndIteration(ride, posEnd, possibleStart);
            return new TakenRide(ride, endIteration, possibleStart <= ride.StartT, _problem.Bonus);
        }

        private static int GetEndIteration(Program.Ride ride, int posEnd, int possibleStart)
        {
            if (posEnd <= ride.FinishT)
            {
                if (possibleStart <= ride.StartT)
                    return ride.StartT + ride.Length();
                else
                {
                    var delay = possibleStart - ride.StartT;
                    return ride.StartT + ride.Length() + delay;
                }
            }
            else
            {
                return -1;
            }
        }

        private int Dist(Program.Ride from, Program.Ride to)
        {
            return Math.Abs(from.FinishCol - to.StartCol) + Math.Abs(from.FinishRow - to.StartRow);
        }
    }

    public class TakenRide
    {
        public Program.Ride Ride { get; }
        public int EndIteration { get; }

        public TakenRide(Program.Ride ride, int endIteration, bool hasOffset, int bonus)
        {
            Ride = ride;
            EndIteration = endIteration;
            Score = 0;
            if (EndIteration >= 0)
            {
                Score = ride.Length() + (hasOffset ? 0 : bonus);
            }
        }

        public int Score { get; }
    }
}
