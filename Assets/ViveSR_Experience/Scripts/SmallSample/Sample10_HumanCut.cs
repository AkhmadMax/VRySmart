﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Vive.Plugin.SR.Experience
{
    [Serializable]
    public class Sample10_HumanCut : MonoBehaviour {
        public SegmentWay SegmentMethod;
        SegmentWay PreviousSegmentMethod;

        //public Camera VRCamera, AICamera;

        [Header("DEPTH")]
        public float MaxDistance = 200.0f;
        public float MinDistance = 40.0f;

        [Header("BACKGROUND COLOR")]
        public Color BackgroundColor;
        private bool initial = false;
	    // Update() is called once per frame after init()
	    void Update () {
            if(!initial)
            return;
            if (FrameworkStatus.WORKING != ViveSR.FrameworkStatus)
                return;
            ChangeSegmentMethod();
        }
        public void Init() {
            ViveSR_AIScene.Instance.EnableHumanCutProcess(!ViveSR_AIScene.IsHumanCutProcessing);
            ViveSR_AIScene.Instance.AISegmentPlaneLeft.initial(SegmentMethod);
            //AICamera.depth = VRCamera.depth-1;
            PreviousSegmentMethod = SegmentMethod;
            switch (SegmentMethod)
            {
                case SegmentWay.AI_SCENE:
                    ViveSR_AIScene.Instance.EnableAISceneProcess(!ViveSR_AIScene.IsAISceneProcessing);
                    break;
            }
            initial = true;
            ViveSR_AIScene.Instance.AISegmentPlaneLeft.GetMaxMinDistance(out MaxDistance, out MinDistance);
            ViveSR_AIScene.Instance.AISegmentPlaneLeft.GetBackgroundColor(out BackgroundColor);
        }
        private void ChangeSegmentMethod()
        {
            if (PreviousSegmentMethod == SegmentMethod)
                return;
            ViveSR_AIScene.Instance.AISegmentPlaneLeft.SetSegmentMethod(SegmentMethod);

            switch (PreviousSegmentMethod)
            {
                case SegmentWay.AI_SCENE:
                    ViveSR_AIScene.Instance.EnableAISceneProcess(!ViveSR_AIScene.IsAISceneProcessing);
                    break;
            }
            switch (SegmentMethod)
            {
                case SegmentWay.AI_SCENE:
                    ViveSR_AIScene.Instance.EnableAISceneProcess(!ViveSR_AIScene.IsAISceneProcessing);
                    break;
            }
            PreviousSegmentMethod = SegmentMethod;
        }
    }
}
