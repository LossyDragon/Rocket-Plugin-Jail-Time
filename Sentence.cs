﻿using System;
using UnityEngine;

namespace ApokPT.RocketPlugins
{
    class Sentence
    {
        public Cell Cell { get; private set; }
        public uint Time { get; private set; }
        public DateTime End { get; private set; }
        public Vector3 Location { get; private set; }


        public Sentence(Cell cell, uint time, Vector3 location)
        {
            Cell = cell;
            Time = time;
            End = DateTime.Now.AddSeconds(time);
            Location = location;
        }
    }
}
