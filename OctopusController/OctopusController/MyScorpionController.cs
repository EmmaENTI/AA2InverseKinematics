using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public class MyScorpionController
    {
        // TAIL and LEGS
        Transform tailTarget;
        MyTentacleController _tail;
        bool isTailMoving = false;
        int maxLegs = 6;
        float animationRange;
        float[] tailJointsLength;
        float[] tailSolutions;
        float[] tailGradient;
        float tailSize;
        float learningStep;
        float deltaGradient;
        float tailTimeToMove = 0;
        Vector3[] tailAxis;
        Vector3[] tailOffsets;
        Vector3[] tailCopy;
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];

        #region Public Methods

        public void InitLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {
            InitializeLegs(LegRoots, LegFutureBases, LegTargets);
        }

        public void InitTail(Transform TailBase)
        {
            InitializeTail(TailBase);
        }

        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        public void NotifyStartWalk()
        {
            isTailMoving = true;
            tailTimeToMove = 0;
            animationRange = 5;
        }

        public void UpdateIK()
        {
            UpdateLegPos();
            if (isTailMoving)
            {
                tailTimeToMove += Time.deltaTime;
                if (tailTimeToMove < animationRange)
                    UpdateLegPos();
                else
                    isTailMoving = false;
            }
            UpdateTail();
        }

        #endregion

        #region Private Methods

        private void UpdateLegPos()
        {
            for (int i = 0; i < maxLegs; i++)
            {
                if ((Vector3.Distance(_legs[i].Bones[0].position, legFutureBases[i].position)) > 1)
                {
                    _legs[i].Bones[0].position = Vector3.Lerp(_legs[i].Bones[0].position, legFutureBases[i].position, 1.4f);
                }
                UpdateLegs(i);
            }
        }

        // Update tail position using Gradient Descent
        private void UpdateTail()
        {
            if ((_tail.Bones[0].position - tailTarget.position).magnitude <= tailSize)
            {
                for (int j = 0; j < 5; j++)
                {
                    if ((_tail.EndEffectorSphereTail[0].position - tailTarget.position).magnitude > 0.05f)
                    {
                        CalculateTailGradient();
                        UpdateTailSolutions();
                    }
                }
                ForwardKinematicsTail();
            }
        }

        // Update leg positions based on FABRIK method
        private void UpdateLegs(int leg)
        {
            for (int i = 0; i <= _legs[0].Bones.Length - 1; i++)
            {
                tailCopy[i] = _legs[leg].Bones[i].position;
            }

            CalculateTailJointsLength(leg);

            float targetRootDist = Vector3.Distance(tailCopy[0], legTargets[leg].position);
            if (targetRootDist < tailJointsLength.Sum())
            {
                PerformFABRIK(leg);
            }
        }

        // Gradient Descent function for the tail
        private float GradientFunction(Vector3 target, float[] solutions, Vector3[] axis, Vector3[] offsets, int i, float delta)
        {
            float gradient = 0;
            float auxAngle = solutions[i];
            float f_x = ErrorFunction(solutions, axis, offsets, target);
            solutions[i] += delta;
            float f_x_plus = ErrorFunction(solutions, axis, offsets, target);
            gradient = (f_x_plus - f_x) / delta;
            solutions[i] = auxAngle;
            return gradient;
        }

        // Forward Kinematics for the tail
        private void ForwardKinematicsTail()
        {
            Quaternion rotation = _tail.Bones[0].transform.rotation;
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(tailSolutions[i], tailAxis[i]);
                _tail.Bones[i].rotation = rotation;
            }
        }

        // Forward Kinematics general method
        private Vector3 ForwardKinematics(float[] solutions, Vector3[] axis, Vector3[] offsets)
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;
            Quaternion rotation = _tail.Bones[0].GetComponentInParent<Transform>().rotation;
            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(solutions[i - 1], axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * offsets[i];
                prevPoint = nextPoint;
            }
            return prevPoint;
        }

        // Error Function for the tail
        private float ErrorFunction(float[] solutions, Vector3[] axis, Vector3[] offsets, Vector3 target)
        {
            return (ForwardKinematics(solutions, axis, offsets) - target).magnitude;
        }

        #endregion

        #region Initialization Methods

        // Initialization of leg components
        private void InitializeLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            // Legs initialization
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                legFutureBases[i] = LegFutureBases[i];
                legTargets[i] = LegTargets[i];
            }
            tailCopy = new Vector3[_legs[0].Bones.Length];
            tailJointsLength = new float[_legs[0].Bones.Length - 1];
        }

        // Initialization of tail components
        private void InitializeTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            tailSolutions = new float[_tail.Bones.Length];
            tailAxis = new Vector3[_tail.Bones.Length];
            tailOffsets = new Vector3[_tail.Bones.Length];
            tailGradient = new float[_tail.Bones.Length];
            tailSize = 0;
            learningStep = 10;
            deltaGradient = 0.05f;

            InitializeTailBonesRotation();
            InitializeTailParameters();
        }

        // Initialize tail bones rotation to identity
        private void InitializeTailBonesRotation()
        {
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                _tail.Bones[i].rotation = Quaternion.identity;
            }
        }

        // Initialize tail parameters
        private void InitializeTailParameters()
        {
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                tailSolutions[i] = 0;
                if (i == 1) tailAxis[i] = new Vector3(0, 0, 1);
                else tailAxis[i] = new Vector3(1, 0, 0);
                if (i != _tail.Bones.Length - 1) tailOffsets[i] = _tail.Bones[i + 1].position - _tail.Bones[i].position;
                else tailOffsets[i] = _tail.EndEffectorSphereTail[0].position - _tail.Bones[i].position;
            }
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                tailSize += tailOffsets[i].magnitude;
            }
        }

        #endregion

        #region FABRIK Methods

        // Calculate leg joints length
        private void CalculateTailJointsLength(int leg)
        {
            for (int i = 0; i <= _legs[leg].Bones.Length - 2; i++)
            {
                tailJointsLength[i] = Vector3.Distance(_legs[leg].Bones[i].position, _legs[leg].Bones[i + 1].position);
            }
        }

        // Perform FABRIK method
        private void PerformFABRIK(int leg)
        {
            while (Vector3.Distance(tailCopy[tailCopy.Length - 1], legTargets[leg].position) != 0 || Vector3.Distance(tailCopy[0], _legs[leg].Bones[0].position) != 0)
            {
                tailCopy[tailCopy.Length - 1] = legTargets[leg].position;

                for (int i = _legs[leg].Bones.Length - 2; i >= 0; i--)
                {
                    UpdateTailCopyPositionBackward(i);
                }

                tailCopy[0] = _legs[leg].Bones[0].position;

                for (int i = 1; i < _legs[leg].Bones.Length - 1; i++)
                {
                    UpdateTailCopyPositionForward(i);
                }
            }

            UpdateLegsRotation(leg);
        }

        // Update tail copy position backward
        private void UpdateTailCopyPositionBackward(int i)
        {
            Vector3 vectorDirector = (tailCopy[i + 1] - tailCopy[i]).normalized;
            Vector3 movementVector = vectorDirector * tailJointsLength[i];
            tailCopy[i] = tailCopy[i + 1] - movementVector;
        }

        // Update tail copy position forward
        private void UpdateTailCopyPositionForward(int i)
        {
            Vector3 vectorDirector = (tailCopy[i - 1] - tailCopy[i]).normalized;
            Vector3 movementVector = vectorDirector * tailJointsLength[i - 1];
            tailCopy[i] = tailCopy[i - 1] - movementVector;
        }

        // Update legs rotation based on FABRIK method
        private void UpdateLegsRotation(int leg)
        {
            for (int i = 0; i <= _legs[leg].Bones.Length - 2; i++)
            {
                Vector3 direction = (tailCopy[i + 1] - tailCopy[i]).normalized;
                Vector3 antDir = (_legs[leg].Bones[i + 1].position - _legs[leg].Bones[i].position).normalized;
                Quaternion rot = Quaternion.FromToRotation(antDir, direction);
                _legs[leg].Bones[i].rotation = rot * _legs[leg].Bones[i].rotation;
            }
        }

        #endregion

        #region Gradient Descent Methods

        // Calculate tail gradient using Gradient Descent
        private void CalculateTailGradient()
        {
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                tailGradient[i] = GradientFunction(tailTarget.position, tailSolutions, tailAxis, tailOffsets, i, deltaGradient);
            }
        }

        // Update tail solutions using Gradient Descent
        private void UpdateTailSolutions()
        {
            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                tailSolutions[i] = tailSolutions[i] - learningStep * tailGradient[i];
            }
        }

        #endregion
    }

}
