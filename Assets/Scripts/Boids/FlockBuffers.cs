
using System.Runtime.InteropServices;
using UnityEngine;

namespace McFlockSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BoidsStructureBuffer
    {
        public BoidsStructureBuffer(Vector4 worldPosition, Vector4 worldDirection, Vector4 veclocity, Vector4 acceleration, Vector4 wallAvoidanceDebug, Matrix4x4 localToWorld)
        {
            WorldPosition = worldPosition;
            WorldDirection = worldDirection;
            Velocity = veclocity;
            Acceleration = acceleration;
            WallAvoidanceDebug = wallAvoidanceDebug;
            LocalToWorld = localToWorld;
        }
        public Vector4 WorldPosition;
        public Vector4 WorldDirection;
        public Vector4 Velocity;
        public Vector4 Acceleration;
        public Vector4 WallAvoidanceDebug;
        public Matrix4x4 LocalToWorld;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ObstaclesBuffer
    {
        public ObstaclesBuffer(Vector4 position, Vector4 size)
        {
            Position = position;
            Size = size;
        }
        public Vector4 Position; // x - posX, y - posY, z - posZ, w - (type 1: Sphere, 2: Box, 3: Custom)
        public Vector4 Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FlockForces
    {
        float _Cohesion;
        float _Alignment;
        float _Separation;
        float _WallAvoidanceStrength;
    }

}//McFlockSystem
