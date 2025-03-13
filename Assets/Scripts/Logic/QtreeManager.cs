namespace McFlockSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class Qtree
    {

        #region Public Variables

        public Vector3 Position;
        public Vector3 Size;
        public int MaxAmount;
        public float Offset;

        //Recursive QTree
        [SerializeField] public Qtree BackLeftDown;
        [SerializeField] public Qtree BackLeftUp;
        [SerializeField] public Qtree BackRightDown;
        [SerializeField] public Qtree BackRightUp;
        [SerializeField] public Qtree FrontLeftDown;
        [SerializeField] public Qtree FrontLeftUp;
        [SerializeField] public Qtree FrontRightDown;
        [SerializeField] public Qtree FrontRightUp;

        public List<Boid> Boids;
        #endregion Public Variables

        #region Public Methods
        public Qtree(Vector3 position, Vector3 size, int maxAmount)
        {
            Boids = new List<Boid>();
            Position = position;
            Size = size;
            MaxAmount = maxAmount;
            BackLeftDown = null;
            BackLeftUp = null;
            BackRightDown = null;
            BackRightUp = null;
            FrontLeftDown = null;
            FrontLeftUp = null;
            FrontRightDown = null;
            FrontRightUp = null;
            Offset = 0.25f;
        }

        public void Draw()
        {
            if (BackLeftDown != null)
            {
                BackLeftDown.Draw();
            }
            if (BackLeftUp != null)
            {
                BackLeftUp.Draw();
            }
            if (BackRightDown != null)
            {
                BackRightDown.Draw();
            }
            if (BackRightUp != null)
            {
                BackRightUp.Draw();
            }
            if (FrontLeftDown != null)
            {
                FrontLeftDown.Draw();
            }
            if (FrontLeftUp != null)
            {
                FrontLeftUp.Draw();
            }
            if (FrontRightDown != null)
            {
                FrontRightDown.Draw();
            }
            if (FrontRightUp != null)
            {
                FrontRightUp.Draw();
            }
            Gizmos.DrawWireCube(Position, Size);
        }
        public bool AddBoid(Boid boid)
        {
            if(!IsInsideArea(boid.transform))
            {
                return false;
            }

            if(Boids.Count >= MaxAmount)
            {
                if(AddBoid(ref BackLeftDown, boid, Position + new Vector3(-Size.x, -Size.y, -Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref BackLeftUp, boid, Position + new Vector3(-Size.x, Size.y, -Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref BackRightDown, boid, Position + new Vector3(Size.x, -Size.y, -Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref BackRightUp, boid, Position + new Vector3(Size.x, Size.y, -Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref FrontLeftDown, boid, Position + new Vector3(-Size.x, -Size.y, Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref FrontLeftUp, boid, Position + new Vector3(-Size.x, Size.y, Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref FrontRightDown, boid, Position + new Vector3(Size.x, -Size.y, Size.z) * Offset))
                {
                    return true;
                }
                if(AddBoid(ref FrontRightUp, boid, Position + new Vector3(Size.x, Size.y, Size.z) * Offset))
                {
                    return true;
                }
                return false;
            }

            Boids.Add(boid);
            return true;
        }

        public void CollectNeighbours(Boid boid, ref List<Boid> neighbourBoids)
        {
            if (!SphereCollision(boid))
            {
                return;
            }
            Collect(BackLeftDown, boid, ref neighbourBoids);
            Collect(BackRightDown, boid, ref neighbourBoids);
            Collect(BackLeftUp, boid, ref neighbourBoids);
            Collect(BackRightUp, boid, ref neighbourBoids);
            Collect(FrontLeftDown, boid, ref neighbourBoids);
            Collect(FrontRightDown, boid, ref neighbourBoids);
            Collect(FrontLeftUp, boid, ref neighbourBoids);
            Collect(FrontRightUp, boid, ref neighbourBoids);
            foreach(var otherBoid in Boids)
            {
                neighbourBoids.Add(otherBoid);
            }
        }

        private void Collect(Qtree qtree, Boid boid, ref List<Boid> boids)
        {
            if(qtree != null)
            {
                qtree.CollectNeighbours(boid, ref boids);
            }
        }

        private bool AddBoid(ref Qtree qtree, Boid boid, Vector3 newPos)
        {
            if (qtree == null)
            {
                qtree = new Qtree(newPos, Size * 0.5f, MaxAmount);
            }
            return qtree.AddBoid(boid);
        }

        public Vector3 GetMinVert()
        {
            return GetMinVertex();
        }

        public Vector3 GetMaxVert()
        {
            return GetMaxVertex();
        }

        public bool IsInsideArea(Transform boid)
        {
            Vector3 minVert = GetMinVertex();
            Vector3 maxVert = GetMaxVertex();

            return ((boid.position.x - minVert.x) >= 0.0f && (boid.position.y - minVert.y) >= 0.0f && (boid.position.z - minVert.z) >= 0.0f)
                   && ((boid.position.x - maxVert.x) <= 0.0f && (boid.position.y - maxVert.y) <= 0.0f && (boid.position.z - maxVert.z) <= 0.0f);
        }

        public bool SphereCollision(Boid boid)
        {
            Vector3 minVert = GetMinVertex();
            Vector3 maxVert = GetMaxVertex();
            var boidTransform = boid.transform;
            float x = FindClosestToZero(boidTransform.position.x, minVert.x, maxVert.x);
            float y = FindClosestToZero(boidTransform.position.y, minVert.y, maxVert.y);
            float z = FindClosestToZero(boidTransform.position.z, minVert.z, maxVert.z);
            Vector3 closestPoint = new Vector3(x, y, z);
            Vector3 sphereToClosestPoint = closestPoint - boidTransform.position;
            return  Vector3.Dot(sphereToClosestPoint, sphereToClosestPoint) <= (boid.CollisionRadius * boid.CollisionRadius);
        }

        #endregion Public Methods

        #region Private Methods
        private Vector3 GetMaxVertex()
        {
            Vector3 pos = Position;
            Vector3 localScale = Size;
            return pos + localScale * 0.5f;

        }

        private Vector3 GetMinVertex()
        {
            Vector3 pos = Position;
            Vector3 localScale = Size;
            return pos - localScale * 0.5f;
        }

        private float FindClosestToZero(float pos, float minVert, float maxVert)
        {
            if (pos >= minVert && pos <= maxVert)
            {
                return pos;
            }
            if (pos <= minVert)
            {
                return minVert;
            }
            return maxVert;
        }
        #endregion Private Methods
    }

    public sealed class QtreeManager : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private bool _DebugEnable;
        [SerializeField] private FlockArea _FlockArea;
        [SerializeField] private Qtree _Qtree;
        [SerializeField] private int _MaxBoidsAmount;
        #endregion Inspector Variables

        #region Public Methods
        public void Initialize()
        {
            _Qtree = new Qtree(_FlockArea.transform.position, _FlockArea.transform.localScale, _MaxBoidsAmount);
        }

        public List<Boid> CollectClosesBoids(Boid boid)
        {
            List<Boid> neighbours = new List<Boid>();
            _Qtree.CollectNeighbours(boid, ref neighbours);
            return neighbours;
        }

        public void AddBoid(Boid boid)
        {
            _Qtree.AddBoid(boid);
        }

        #endregion Public Methods

        #region Unity Methods
        private void OnDrawGizmos()
        {
            if(!_DebugEnable)
            {
                return;
            }
            Gizmos.color = Color.blue;
            Draw(_Qtree.BackLeftDown, "BackLeftDown");
            Draw(_Qtree.BackRightDown, "BackRightDown");
            Draw(_Qtree.BackLeftUp, "BackLeftUp");
            Draw(_Qtree.BackRightUp, "BackRightUp");
            Draw(_Qtree.FrontLeftDown, "FrontLeftDown");
            Draw(_Qtree.FrontRightDown, "FrontRightDown");
            Draw(_Qtree.FrontLeftUp, "FrontLeftUp");
            Draw(_Qtree.FrontRightUp, "FrontRightUp");
        }

        private void Draw(Qtree qtree, string name)
        {
            if (qtree != null)
            {
                qtree.Draw();
            }
        }
        #endregion Unity Methods
    }
}//McFlockSystem
