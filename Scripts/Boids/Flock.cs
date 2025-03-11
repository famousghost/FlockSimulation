namespace McFlockSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class Flock : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private GameObject _FlockBox;
        [SerializeField] private float _CohesionStrength;
        [SerializeField] private float _SeparationStrength;
        [SerializeField] private float _AligmentStrength;
        [SerializeField] private float _AvoidanceStrength;
        #endregion Inspector Variables

        #region Public Variables
        public List<Boid> Boids;
        public static Flock Instance;
        #endregion

        #region Public Methods
        public void AddBois(Boid boid)
        {
            if(Boids == null)
            {
                Boids = new List<Boid>();
            }
            if(Boids.Contains(boid))
            {
                return;
            }
            Boids.Add(boid);
        }

        public void RemoveBoid(Boid boid)
        {
            if (Boids == null)
            {
                Boids = new List<Boid>();
            }
            if (!Boids.Contains(boid))
            {
                return;
            }
            Boids.Remove(boid);
        }
        #endregion Public Methods

        #region Unity Methods
        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            foreach (var boid in Boids)
            {
                int totalAligement = 0;
                Vector3 aligementAccelaration = Vector3.zero;

                int totalChoesion = 0;
                Vector3 cohesionPosition = Vector3.zero;

                int totalSeparation = 0;
                Vector3 separationDir = Vector3.zero;
                foreach (var otherBoids in Boids)
                {
                    if (boid == otherBoids)
                    {
                        continue;
                    }
                    
                    if(boid.Aligment(ref aligementAccelaration, otherBoids))
                    {
                        totalAligement++;
                    }


                    if(boid.Cohesion(ref  cohesionPosition, otherBoids))
                    {
                        totalChoesion++;
                    }

                    if(boid.Separation(ref separationDir, otherBoids))
                    {
                        totalSeparation++;
                    }

                }
                if (totalAligement > 0)
                {
                    boid.UpdateAccelaration((aligementAccelaration / totalAligement) * _AligmentStrength);
                }
                if (totalChoesion > 0)
                {
                    cohesionPosition /= totalChoesion;
                    Vector3 velocity = (cohesionPosition - boid.transform.position) * _CohesionStrength;
                    boid.UpdateAccelaration(velocity);
                }
                if(totalSeparation > 0)
                {
                    separationDir /= totalSeparation;
                    boid.UpdateAccelaration(separationDir * _SeparationStrength);
                }
                boid.AvoidWalls(_AvoidanceStrength);
                boid.UpdateBoid();
            }
        }


        #endregion Unity Methods

        #region Private Methods

        #endregion Private Methods
    }
}//McFlockSystem
