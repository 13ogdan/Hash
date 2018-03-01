﻿using System;
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
            public int StartRow { get; set; }
            public int StartCol { get; set; }
            public int FinishRow { get; set; }
            public int FinishCol { get; set; }
            public int StartT { get; set; }
            public int FinishT { get; set; }
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

        static void Main(string[] args)
        {
            var problem = ReadInput(args[0]);
        }
    }
}