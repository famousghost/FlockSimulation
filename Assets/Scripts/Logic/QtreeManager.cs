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

        public void Update(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;
            if(BackLeftDown != null)
            {
                BackLeftDown.Update(position + new Vector3(-size.x, -size.y, -size.z) * Offset, size * 0.5f);
            }
            if (BackLeftUp != null)
            {
                BackLeftUp.Update(position + new Vector3(-size.x, size.y, -size.z) * Offset, size * 0.5f);
            }
            if (BackRightDown != null)
            {
                BackRightDown.Update(position + new Vector3(size.x, -size.y, -size.z) * Offset, size * 0.5f);
            }
            if (BackRightUp != null)
            {
                BackRightUp.Update(position + new Vector3(size.x, size.y, -size.z) * Offset, size * 0.5f);
            }
            if (FrontLeftDown != null)
            {
                FrontLeftDown.Update(position + new Vector3(-size.x, -size.y, size.z) * Offset, size * 0.5f);
            }
            if (FrontLeftUp != null)
            {
                FrontLeftUp.Update(position + new Vector3(-size.x, size.y, -size.z) * Offset, size * 0.5f);
            }
            if (FrontRightDown != null)
            {
                FrontRightDown.Update(position + new Vector3(size.x, -size.y, size.z) * Offset, size * 0.5f);
            }
            if (FrontRightUp != null)
            {
                FrontRightUp.Update(position + new Vector3(size.x, size.y, size.z) * Offset, size * 0.5f);
            }
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

            if(Boids.Count > MaxAmount)
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

        private bool AddBoid(ref Qtree qtree, Boid boid, Vector3 newPos)
        {
            if (qtree == null)
            {
                qtree = new Qtree(newPos, Size * 0.5f, MaxAmount);
            }
            return qtree.AddBoid(boid);
        }

        public void Clear()
        {
            if (BackLeftDown != null)
                BackLeftDown.Clear();
            if (BackLeftUp != null)
                BackLeftUp.Clear();
            if (BackRightDown != null)
                BackRightDown.Clear();
            if(BackRightUp != null)
                BackRightUp.Clear();
            if (FrontLeftDown != null)
                FrontLeftDown.Clear();
            if (FrontLeftUp != null)
                FrontLeftUp.Clear();
            if (FrontRightDown != null)
                FrontRightDown.Clear();
            if (FrontRightUp != null)
                FrontRightUp.Clear();
            BackLeftDown = null;
            BackLeftUp = null;
            BackRightDown = null;
            BackRightUp = null;
            FrontLeftDown = null;
            FrontLeftUp = null;
            FrontRightDown = null;
            FrontRightUp = null;
            if (Boids != null)
                Boids.Clear();
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
        #endregion Private Methods
    }

    public sealed class QtreeManager : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private FlockArea _FlockArea;
        [SerializeField] private Qtree _Qtree;
        [SerializeField] private int _MaxBoidsAmount;
        #endregion Inspector Variables

        #region Public Methods
        public void Initialize()
        {
            _Qtree = new Qtree(_FlockArea.transform.position, _FlockArea.transform.localScale, _MaxBoidsAmount);
        }

        public void Update()
        {
            _Qtree.Update(_FlockArea.transform.position, _FlockArea.transform.localScale);
        }

        public void AddBoid(Boid boid)
        {
            _Qtree.AddBoid(boid);
        }

        public void Clear()
        {
            _Qtree.Clear();
        }
        #endregion Public Methods

        #region Unity Methods
        private void OnDrawGizmos()
        {
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
