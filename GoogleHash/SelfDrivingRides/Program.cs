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

            public int StartX { get { return StartRow; } }
            public int StartY { get { return StartCol; } }
            public int EndX { get { return FinishRow; } }
            public int EndY { get { return FinishCol; } }
            public int MinStartTime { get { return StartT;  } }
            public int MaxStartTime { get { return FinishT - Length(); } }

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

        public class CarState
        {
            private Ride currRide;
            private List<Ride> allRides = new List<Ride>();
            public Ride Ride { get { return currRide; }
                set 
                {
                    allRides.Add(value);
                    currRide = value;
                }
            }
            public List<Ride> Rides { get { return allRides; } }
            public int StartTime;
            public int Score;
            
            public CarState ()
            {
                currRide = null;
                StartTime = 0;
                Score = 0;                
            }

            public int ArriveTime(int x, int y)
            {
                if (currRide == null)
                {
                    return x + y;
                }
                return StartTime + Ride.Length() + Math.Abs(Ride.EndX - x) + Math.Abs(Ride.EndY - y);
            }
        }


        public class Problem
        {
            public int RowN { get; set; }
            public int ColN { get; set; }
            public int Fleet { get; set; }
            public int Bonus { get; set; }
            public long T { get; set; }
            public Random Random = new Random();

            public List<Ride> Rides { get; set; } = new List<Ride>();

            private void getNearest(List<CarState> states, Ride ride)
            {
                var minStart = 1000000000 + 1;
                CarState minCar = null;

                var cars = new List<CarState>();

                for (int i = 0; i < states.Count; i++)
                {
                    var car = states[i];
                    var arriveTime = car.ArriveTime(ride.StartX, ride.StartY);
                    
                    if (arriveTime <= ride.MinStartTime)
                    {
                        cars.Add(car);
                    }

                    if (arriveTime <= ride.MaxStartTime && arriveTime < minStart)
                    {
                        minCar = car;
                        minStart = arriveTime;
                    }
                }

                if (cars.Count > 0)
                {
                    var cnt = Random.Next(cars.Count);
                    Console.WriteLine(cnt + " " + cars.Count);
                    var car = cars[cnt];
                    car.Ride = ride;
                    car.StartTime = ride.MinStartTime;
                    car.Score = car.Score + Bonus + ride.Length();
                    return;
                }

                if (minCar != null)
                {
                    minCar.Ride = ride;
                    minCar.StartTime = minStart;
                    minCar.Score = minCar.Score + ride.Length();
                }                
            }

            public void Solve(List<CarState> states)
            {
                for (int i = 0; i < Rides.Count; i++)
                {
                    getNearest(states, Rides[i]);
                }
            }
        }
        
        public class Vehicle
        {
            public int N { get; set; }
            public List<Ride> Rides { get; set; } = new List<Ride>();
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

        private static void WriteOutput(List<CarState> res, string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            foreach (var v in res)
            {
                var rides = string.Join(" ", v.Rides.Select(r => r.N.ToString()));
                File.AppendAllText(filename, string.Format($"{v.Rides.Count} {rides}\n"));
            }
        }

        static List<string> test_names = new List<string>() { "a_example", "b_should_be_easy", "c_no_hurry", "d_metropolis", "e_high_bonus" };
        static int test = 1;

        static void Main(string[] args)
        {
            // var problem = ReadInput(args[0]);
            var problem = ReadInput(test_names[test] + ".in");
            var res = new List<CarState>();
            for (int i = 0; i < problem.Fleet; i++)
            {
                res.Add(new CarState());
            }

            problem.Solve(res);

            WriteOutput(res, test_names[test] + ".out");

            Console.ReadLine();
        }
    }
}
