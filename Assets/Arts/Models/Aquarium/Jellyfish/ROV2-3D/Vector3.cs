using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RVO
{
    unsafe public struct Vector3
    {

        internal fixed float val_[3];

        public Vector3(float[] val)
        {
            val_[0] = val[0];
            val_[1] = val[1];
            val_[2] = val[2];
        }

        public Vector3(float x, float y, float z)
        {
            val_[0] = x;
            val_[1] = y;
            val_[2] = z;
        }

        public float x() { return val_[0]; }

        public float y() { return val_[1]; }

        public float z() { return val_[2]; }

        public float this[int index] 
        {
            get { return val_[index]; }
            set { val_[index] = value; } 
        }
        public static Vector3 operator -(Vector3 a) 
        {
            return new Vector3(-a.val_[0], -a.val_[1], -a.val_[2]);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.val_[0] - b.val_[0], a.val_[1] - b.val_[1], a.val_[2] - b.val_[2]);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.val_[0] + b.val_[0], a.val_[1] + b.val_[1], a.val_[2] + b.val_[2]);
        }

        public static float operator *(Vector3 a, Vector3 b)
        {
            return a.val_[0] * b.val_[0] + a.val_[1] * b.val_[1] + a.val_[2] * b.val_[2];
        }

        public static Vector3 operator *(float s, Vector3 a)
        {
            return new Vector3(a.val_[0] * s, a.val_[1] * s, a.val_[2] * s);
        }

        public static Vector3 operator *(Vector3 a, float s)
        {
            return new Vector3(a.val_[0] * s, a.val_[1] * s, a.val_[2] * s);
        }

        public static Vector3 operator /(Vector3 a, float s)
        {
            return new Vector3(a.val_[0] / s, a.val_[1] / s, a.val_[2] / s);
        }
    }
}

