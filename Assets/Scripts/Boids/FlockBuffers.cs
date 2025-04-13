
using System.Runtime.InteropServices;
using UnityEngine;

namespace McFlockSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BoidsStructureBuffer
    {
  
        public BoidsStructureBuffer(Vector4 worldPosition, Vector4 veclocity, Vector4 acceleration, Matrix4x4 localToWorld)
        {
            WorldPosition = worldPosition;
            Velocity = veclocity;
            Acceleration = acceleration;
            LocalToWorld = localToWorld;
        }
        public Vector4 WorldPosition;
        public Vector4 Velocity;
        public Vector4 Acceleration;
        public Matrix4x4 LocalToWorld;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ObstaclesBuffer
    {
        public ObstaclesBuffer(Vector4 position, Vector4 size, Matrix4x4 rotation)
        {
            Position = position;
            Size = size;
            Rotation = rotation;
        }
        public Vector4 Position; // x - posX, y - posY, z - posZ, w - (type 1: Sphere, 2: Box, 3: Custom)
        public Vector4 Size;
        public Matrix4x4 Rotation;
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
