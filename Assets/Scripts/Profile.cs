using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;

public class Profile : MonoBehaviour {
    public FB fb;
    public Crier crier;
    public FirebaseUser user;
    public Google.GoogleSignInUser googleUser;

    public GameObject topButtons;
    public GameObject signInPage;
    public GameObject accountPage;
    public Text nameText;
    public Text joinText;
    public RawImage userImage;
    public Text currRankText;
    public Text nextRankText;
    public Image rankBarFront;
    public Text pointsText;
    public Text claimText;
    public Text redeemPointsText;

    public GameObject emailSignupPage;
    public InputField emailField;
    public InputField passwordField;
    public InputField confirmField;
    public InputField firstnameField;
    public InputField lastnameField;

    public GameObject emailLoginPage;
    public InputField emailLoginField;
    public InputField passwordLoginField;

    public void ProfileInit() {
        signInPage.SetActive(false);
        accountPage.SetActive(true);
    }

    public void UpdateProfile(string username, string joinDate, string points, string redeemPoints) {
        nameText.text = username;
        joinText.text = joinDate;
        redeemPointsText.text = redeemPoints;
        int iPoints = int.Parse(points);
        pointsText.text = (iPoints % 100).ToString();
        currRankText.text = "Rank " + (iPoints / 100).ToString();
        nextRankText.text = "Rank " + ((iPoints / 100) + 1).ToString();
        rankBarFront.GetComponent<RectTransform>().anchoredPosition = new Vector2(-550f + 550f * ((iPoints % 100) / 100f), 0);

        if (!string.IsNullOrEmpty(fb.token))
            fb.reference.Child("/Users/basics/" + user.UserId + "/token").SetValueAsync(fb.token);
        fb.SaveLocation();
        crier.CDebug("User values successfully set.");
        if (googleUser != null)
            StartCoroutine(SetPhoto());
    }

    IEnumerator SetPhoto() {
        string userPhotoUrl = googleUser.ImageUrl.ToString();
        if (!string.IsNullOrEmpty(userPhotoUrl)) {
            WWW www = new WWW(userPhotoUrl);
            yield return www;
            userImage.texture = www.texture;
            userImage.SetNativeSize();
            RectTransform rt = userImage.GetComponent<RectTransform>();
            float scale = 1f;
            scale = 256 / rt.sizeDelta.x;
            rt.sizeDelta = rt.sizeDelta * scale;
            userImage.transform.parent.gameObject.SetActive(true);
        }
    }

    public void ChangePage(GameObject page) {
        accountPage.SetActive(false);
        page.SetActive(true);
    }

    public void StartEmailSignup() {
        emailSignupPage.SetActive(true);
    }

    public void EndEmailSignup() {
        if (emailField.text == "" || passwordField.text == "" || confirmField.text == "" || firstnameField.text == "" || lastnameField.text == "") {
            crier.ErrorMessage("One or more fields are empty!");
            return;
        } else if (!(emailField.text.IndexOf('@') > 0)) {
            crier.ErrorMessage("Not a valid email!");
            return;
        } else if (passwordField.text != confirmField.text) {
            crier.ErrorMessage("Passwords do not match!");
            return;
        } else if (passwordField.text.Length < 6) {
            crier.ErrorMessage("Password must be at least 6 characters!");
            return;
        }

        fb.EmailSignup(emailField.text, passwordField.text, firstnameField.text, lastnameField.text);
        emailSignupPage.SetActive(false);
        accountPage.SetActive(true);
    }

    public void StartEmailLogin() {
        emailLoginPage.SetActive(true);
    }

    public void EndEmailLogin() {
        if (emailLoginField.text == "" || passwordLoginField.text == "") {
            crier.ErrorMessage("One or more fields are empty!");
            return;
        } else if (!(emailLoginField.text.IndexOf('@') > 0)) {
            crier.ErrorMessage("Not a valid email!");
            return;
        }

        crier.ErrorMessage("Logging in...", 2);
        fb.EmailLogin(emailLoginField.text, passwordLoginField.text);
    }

    public void CloseLogin() {
        emailLoginPage.SetActive(false);
    }
    public void CloseSignup() {
        emailSignupPage.SetActive(false);
    }
}
