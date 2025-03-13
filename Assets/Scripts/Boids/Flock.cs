namespace McFlockSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class Flock : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private bool _QTreeOptimizationEnabled;

        [SerializeField] private QtreeManager _QtreeManager;
        [SerializeField] private FlockArea _FlockArea;
        [SerializeField] private float _CohesionStrength;
        [SerializeField] private float _SeparationStrength;
        [SerializeField] private float _AligmentStrength;
        [SerializeField] private float _WallAvoidanceStrength;
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

        private void Start()
        {
            _QtreeManager.Initialize();
        }

        private void Update()
        {
            if (_QTreeOptimizationEnabled)
            {
                _QtreeManager.Initialize();
                foreach (var boid in Boids)
                {
                    _QtreeManager.AddBoid(boid);
                }
            }
            foreach (var boid in Boids)
            {
                var otherBoids = _QTreeOptimizationEnabled ? _QtreeManager.CollectClosesBoids(boid) : Boids;
                int totalAligement = 0;
                Vector3 aligementAccelaration = Vector3.zero;

                int totalChoesion = 0;
                Vector3 cohesionPosition = Vector3.zero;

                int totalSeparation = 0;
                Vector3 separationDir = Vector3.zero;
                foreach (var otherBoid in otherBoids)
                {
                    if (boid == otherBoid)
                    {
                        continue;
                    }
                    
                    if(boid.Aligment(ref aligementAccelaration, otherBoid))
                    {
                        totalAligement++;
                    }


                    if(boid.Cohesion(ref  cohesionPosition, otherBoid))
                    {
                        totalChoesion++;
                    }

                    if(boid.Separation(ref separationDir, otherBoid))
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
                    boid.UpdateAccelaration(cohesionPosition * _CohesionStrength);
                }
                if(totalSeparation > 0)
                {
                    separationDir /= totalSeparation;
                    boid.UpdateAccelaration(separationDir * _SeparationStrength);
                }
                boid.AvoidWalls(_FlockArea, _WallAvoidanceStrength);
                boid.UpdateBoid();
            }
        }


        #endregion Unity Methods

        #region Private Methods

        #endregion Private Methods
    }
}//McFlockSystem
