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

        public Transform[] Bones { get => _bones; }
        public Transform[] EndEffector { get => _endEffectorSphere; }

        // Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            tentacleMode = mode;

            switch (tentacleMode)
            {
                case TentacleMode.LEG:
                    LoadLegJoints(root);
                    break;
                case TentacleMode.TAIL:
                    LoadTailJoints(root);
                    break;
                case TentacleMode.TENTACLE:
                    LoadTentacleJoints(root);
                    break;
            }
            return Bones;
        }

        private void LoadLegJoints(Transform root)
        {
            _bones = new Transform[3];
            root = root.GetChild(0);

            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i] = root;
                root = root.GetChild(1);
            }

            _endEffectorSphere = new Transform[1];
            _endEffectorSphere[0] = root;
        }

        private void LoadTailJoints(Transform root)
        {
            _bones = new Transform[5];
            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i] = root;
                root = root.GetChild(1);
            }

            _endEffectorSphere = new Transform[1];
            _endEffectorSphere[0] = root;
        }

        private void LoadTentacleJoints(Transform root)
        {
            _bones = new Transform[50];
            root = root.GetChild(0).GetChild(0).GetChild(0);

            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i] = root;
                root = root.GetChild(0);
            }

            _endEffectorSphere = new Transform[1];
            _endEffectorSphere[0] = root;
        }
    }
}

