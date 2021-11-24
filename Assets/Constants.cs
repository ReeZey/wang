using System.Collections.Generic;
using UnityEngine;

public class Constants
{
        public static List<Vector3> directions = new List<Vector3>
        {
                new Vector3(0,1,0),
                new Vector3(0,-1,0),
                new Vector3(0,0,-1),
                new Vector3(1,0,0),
                new Vector3(0,0,1),
                new Vector3(-1,0,0)
        };

        public enum ChunkDirection
        {
                Top,
                Bottom,
                Front,
                Right,
                Back,
                Left
        }
}