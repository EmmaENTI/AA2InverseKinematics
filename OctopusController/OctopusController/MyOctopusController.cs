using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController
    {
        MyTentacleController[] _tentacles = new MyTentacleController[4];
        Transform _currentRegion;
        Transform _target;
        Transform[] _randomTargets;
        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        #region Public Methods

        // DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin { set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }

        public void TestLogging(string objectName)
        {
            Debug.Log("Hello, I am initializing my Octopus Controller in object " + objectName);
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            InitializeTentacles(tentacleRoots, randomTargets);
        }

        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot()
        {
            Debug.Log("Shoot");
        }

        public void UpdateTentacles()
        {
            UpdateCCD();
        }

        #endregion

        #region Private and Internal Methods

        void UpdateCCD()
        {
            for (int i = 0; i < _tentacles.Length; i++)
            {
                bool done = false;
                float rotationAngle;
                float cos;
                float error = 0.1f;
                int tries = 0;

                while (!done && tries < 10)
                {
                    for (int j = _tentacles[i].Bones.Length - 1; j >= 0; j--)
                    {
                        UpdateBoneRotation(i, j, out rotationAngle, out cos);
                    }

                    tries++;

                    CheckIfDone(i, error, ref done);
                }
            }
        }

        void UpdateBoneRotation(int i, int j, out float rotationAngle, out float cos)
        {
            Vector3 E_R = _tentacles[i].EndEffectorSphereTail[0].transform.position - _tentacles[i].Bones[j].transform.position;
            Vector3 T_R = _randomTargets[i].transform.position - _tentacles[i].Bones[j].transform.position;

            if (E_R.magnitude * T_R.magnitude <= 0.001f)
                cos = 1;
            else
                cos = Vector3.Dot(E_R, T_R) / (E_R.magnitude * T_R.magnitude);

            rotationAngle = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, cos)));
            rotationAngle = (float)NormalizeAngle(rotationAngle) * Mathf.Rad2Deg;

            _tentacles[i].Bones[j].Rotate(Vector3.Cross(E_R, T_R).normalized, rotationAngle, Space.World);
        }

        void CheckIfDone(int i, float error, ref bool done)
        {
            float x = Mathf.Abs(_tentacles[i].EndEffectorSphereTail[0].transform.position.x - _randomTargets[i].transform.position.x);
            float y = Mathf.Abs(_tentacles[i].EndEffectorSphereTail[0].transform.position.y - _randomTargets[i].transform.position.y);
            float z = Mathf.Abs(_tentacles[i].EndEffectorSphereTail[0].transform.position.z - _randomTargets[i].transform.position.z);

            if (x < error && y < error && z < error)
                done = true;
        }

        double NormalizeAngle(double angle)
        {
            angle = angle % (2.0 * Mathf.PI);
            if (angle < -Mathf.PI)
                angle += 2.0 * Mathf.PI;
            else if (angle > Mathf.PI)
                angle -= 2.0 * Mathf.PI;
            return angle;
        }

        void InitializeTentacles(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            for (int i = 0; i < tentacleRoots.Length; i++)
            {
                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i], TentacleMode.TENTACLE);
            }
            _randomTargets = randomTargets;
        }

        #endregion
    }
}
