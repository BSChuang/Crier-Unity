using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deals : MonoBehaviour {
    public FB fb;
    public Crier crier;
    public CameraController camCont;
    public Toggle costsToggle;
    public InputField shortInfoField;
    public InputField infoField;
    public InputField hourField;
    public InputField minuteField;
    public Text placeName;

    public void MakeDeal() {
        int hours = 0;
        int minutes = 0;
        if (hourField.text != "") {
            hours = int.Parse(hourField.text);
        } else {
            hours = 0;
        }

        if (minuteField.text != "") {
            minutes = int.Parse(minuteField.text);
        } else {
            minutes = 0;
        }

        if (infoField.text == "")
            infoField.text = shortInfoField.text;

        crier.ErrorMessage("Deal Created!", 1);
        fb.SetDeal(hours, minutes, shortInfoField.text, infoField.text);
        shortInfoField.text = "";
        hourField.text = "";
        minuteField.text = "";
        infoField.text = "";
    }

    public void SelectedDropdown() {
        if (crier.ownerRedeem) {
            costsToggle.interactable = true;
        } else {
            costsToggle.isOn = false;
            costsToggle.interactable = false;
        }
    }

    public void UploadNewBanner() {
        camCont.isComment = false;
        camCont.Activate();
        camCont.gameObject.SetActive(true);
        camCont.blackBars.SetActive(true);
    }

    public void RemoveBanner() {
        fb.DeleteImage("bannerImages/" + crier.ownerPath + "/banner.jpg");
    }
}
