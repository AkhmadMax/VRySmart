using UnityEngine;
using UnityEngine.UI;
using Mediapipe.SelfieSegmentation;
using Unity.RenderStreaming;
using System;


public class VirtualBackgroundVisuallizer1 : MonoBehaviour
{
    [SerializeField] RawImage inputImageUI;
    [SerializeField] RawImage compositeImage;
    [SerializeField] SelfieSegmentationResource resource;
    [SerializeField] Shader shader;
    [SerializeField] Texture backGroundTexture;

    [SerializeField] private ReceiveVideoViewer receiveVideoViewer;


    SelfieSegmentation segmentation;
    Material material;

    void Start(){
        receiveVideoViewer.OnUpdateReceiveTexture += texture => inputImageUI.texture = texture;


        material = new Material(shader);
        compositeImage.material = material;

        segmentation = new SelfieSegmentation(resource);
    }

    void LateUpdate(){

        if(inputImageUI.texture)
        {
            //inputImageUI.texture = webCamInput.inputImageTexture;

            // Predict segmentation by neural network model.
            segmentation.ProcessImage(inputImageUI.texture);

            //Set segmentation texutre to `_MainTex` variable of shader.
            compositeImage.texture = segmentation.texture;

            material.SetTexture("_inputImage", inputImageUI.texture);
            material.SetTexture("_backImage", backGroundTexture);
        }

    } 

    void OnApplicationQuit(){
        segmentation.Dispose();
    }
}
