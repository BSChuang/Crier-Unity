using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour {
    public FB fb;
    public Crier crier;
    public InfoController ic;
    WebCamTexture mCamera;
    public RawImage takenImage;
    public RawImage rawImage;
    public bool isComment;
    public GameObject useButton;
    public GameObject takeButton;
    public GameObject anotherButton;
    public GameObject blackBars;
    bool takePicture;
    [HideInInspector]
    public byte[] takenImageBytes;

    public void Activate() {
        mCamera = new WebCamTexture();
        mCamera.filterMode = FilterMode.Trilinear;
        rawImage.texture = mCamera;

        if (!mCamera.isPlaying)
            mCamera.Play();
    }

    public void Deactivate() {
        blackBars.SetActive(false);
        if (mCamera && mCamera.isPlaying)
            mCamera.Stop();
    }

    public void TakePhoto() {
        takePicture = true;
        //Texture2D photo = new Texture2D(mCamera.width, mCamera.height);
        //photo.SetPixels(mCamera.GetPixels());
        //photo.Apply();

        //takenImage.material.mainTexture = photo;
        //takenImage.gameObject.SetActive(true);
        //takenImageBytes = photo.EncodeToPNG();
        //takeButton.SetActive(false);
        //useButton.SetActive(true);
        //anotherButton.SetActive(true);
    }

    public void UsePhoto() {
        Deactivate();
        gameObject.SetActive(false);
        if (isComment)
            ic.tookPhoto.isOn = true;
        else {
            crier.ErrorMessage("Banner Updated!", 1);
            int index = crier.placeSelected;
            fb.SetBanner(takenImageBytes);
        }
    }

    public void TakeAnotherPhoto() {
        takenImage.gameObject.SetActive(false);
        takeButton.SetActive(true);
        useButton.SetActive(false);
        anotherButton.SetActive(false);
    }

    private void Update() {
        if (mCamera.isPlaying) {
            if (mCamera.width < 100) {
                Debug.Log("Still waiting another frame for correct info...");
                return;
            }

            // change as user rotates iPhone or Android:

            int cwNeeded = mCamera.videoRotationAngle;
            // Unity helpfully returns the _clockwise_ twist needed
            // guess nobody at Unity noticed their product works in counterclockwise:
            int ccwNeeded = -cwNeeded;

            // IF the image needs to be mirrored, it seems that it
            // ALSO needs to be spun. Strange: but true.
            if (mCamera.videoVerticallyMirrored) ccwNeeded += 180;

            RectTransform rawImageRT = rawImage.GetComponent<RectTransform>();

            // you'll be using a UI RawImage, so simply spin the RectTransform
            rawImageRT.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

            float videoRatio = (float)mCamera.width / (float)mCamera.height;

            AspectRatioFitter rawImageARF = rawImage.GetComponent<AspectRatioFitter>();

            // you'll be using an AspectRatioFitter on the Image, so simply set it
            rawImageARF.aspectRatio = videoRatio;

            // alert, the ONLY way to mirror a RAW image, is, the uvRect.
            // changing the scale is completely broken.
            if (mCamera.videoVerticallyMirrored)
                rawImage.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
            else
                rawImage.uvRect = new Rect(0, 0, 1, 1);  // means no flip

            if (takePicture) {
                Texture2D photo = new Texture2D(rawImage.texture.width, rawImage.texture.height);
                photo.SetPixels(mCamera.GetPixels());
                photo.Apply();

                takenImage.texture = photo;
                takenImage.gameObject.SetActive(true);
                takenImage.GetComponent<AspectRatioFitter>().aspectRatio = rawImage.GetComponent<AspectRatioFitter>().aspectRatio;
                takenImageBytes = photo.EncodeToPNG();
                takeButton.SetActive(false);
                useButton.SetActive(true);
                anotherButton.SetActive(true);

                takePicture = false;
            }

            // devText.text =
            //  videoRotationAngle+"/"+ratio+"/"+mCamera.videoVerticallyMirrored;
        }
    }
}