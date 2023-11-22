using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{
    internal class MyTentacleController
    {
        TentacleMode tentacleMode;
        Transform[] _bones;
        Transform[] _endEffectorSphere;

        public Transform[] Bones => _bones;
        public Transform[] EndEffector => _endEffectorSphere;

        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            tentacleMode = mode;

            switch (tentacleMode)
            {
                case TentacleMode.LEG:
                    _bones = LoadJoints(root, 3, out _endEffectorSphere);
                    break;
                case TentacleMode.TAIL:
                    _bones = LoadJoints(root, 5, out _endEffectorSphere);
                    break;
                case TentacleMode.TENTACLE:
                    _bones = LoadTentacleJoints(root);
                    break;
            }

            return Bones;
        }

        private Transform[] LoadJoints(Transform root, int numJoints, out Transform[] endEffectors)
        {
            Transform[] joints = new Transform[numJoints];
            root = root.GetChild(0);

            for (int i = 0; i < numJoints; i++)
            {
                joints[i] = root;
                root = root.GetChild(1);
            }

            endEffectors = new Transform[] { root };
            return joints;
        }

        private Transform[] LoadTentacleJoints(Transform root)
        {
            int numJoints = 50;
            Transform[] joints = new Transform[numJoints];

            for (int i = 0; i < 3; i++)
            {
                root = root.GetChild(0);
            }

            for (int i = 0; i < numJoints - 1; i++) // Decrease the loop count by 1 to exclude the end effector
            {
                joints[i] = root;
                root = root.GetChild(0);
            }

            _endEffectorSphere = new Transform[] { root };
            return joints;
        }
    }
}
