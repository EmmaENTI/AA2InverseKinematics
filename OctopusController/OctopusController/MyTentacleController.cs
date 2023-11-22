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
        Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            tentacleMode = mode;

            switch (tentacleMode){
                case TentacleMode.LEG:

                    break;
                case TentacleMode.TAIL:

                    break;
                case TentacleMode.TENTACLE:

                    break;
            }
            return Bones;
        }
    }
}
