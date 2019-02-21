using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoController : MonoBehaviour {
    public FB fb;
    public Crier crier;
    public GoogleAds ads;
    public CameraController camera;

    public RawImage raw;
    public InputField commentField;
    public InputField waitTimeField;
    public InputField busynessField;
    public GameObject writeCommentGO;
    public Text addressText;
    public Text numberText;
    public Text waitTimeText;
    public string url;
    public Text dealTimeText;
    public Text dealText;
    public GameObject redeemedGO;
    public Text redeemedDealText;
    public GameObject dealImage;
    public GameObject busynessBar;
    public GameObject commentPrefab;
    public Sprite costImage;
    public Sprite noCostImage;
    public Toggle tookPhoto;
    public List<CommentController> comConts;
    public Button favoriteButton;

    public GameObject enhanceGO;
    public RawImage enhancedImage;

    public void Clear() {
        writeCommentGO.SetActive(false);
        commentField.text = "";
        waitTimeField.text = "";
        busynessField.text = "";
        tookPhoto.isOn = false;
        redeemedGO.SetActive(false);
        EnhanceExit();
        foreach (Transform child in transform.GetChild(1)) {
            if (child.name == "Comment(Clone)")
                Destroy(child.gameObject);
        }
    }

    public void Submit() {
        if (crier.currentCard.floatDist <= 5f)
            fb.SetComment(crier.currentCard.id, crier.currentCard, waitTimeField.text, busynessField.text, commentField.text, tookPhoto.isOn, camera.takenImageBytes);
        else
            crier.ErrorMessage("Too far from establishment!", 0);
        Clear();
    }

    public void ApplyUpvotes() {
        foreach (CommentController comCont in comConts) {
            comCont.changed = false;

            if (fb.user != null)
                comCont.ApplyUpvote();
        }
        comConts.Clear();
    }

    public void BusynessLimit(InputField inputField) {
        if (inputField.text == "")
            return;
        int busynessInt = int.Parse(inputField.text);
        if (busynessInt > 5 || busynessInt < 0) {
            inputField.text = Mathf.Clamp(busynessInt, 0, 5).ToString();
        }
    }

    public void SetBusynessBar(int busynessInt) {
        for (int i = 0; i < 5; i++) {
            busynessBar.transform.GetChild(i + 1).gameObject.SetActive(false);
        }
        for (int i = 0; i < busynessInt; i++) {
            busynessBar.transform.GetChild(i + 1).gameObject.SetActive(true);
        }
    }

    public void TryRedeemDeal() {
        if (!crier.currentCard.redeemed && crier.currentCard.costs) {
            if (fb.user != null) {
                fb.CheckRedeemPoints(this);
            } else {
                crier.ErrorMessage("Must be logged in!");
            }
        }
        else
            RedeemedDeal();
    }

    public void RedeemedDeal() {
        redeemedGO.SetActive(true);
        CardController card = crier.currentCard;
        redeemedDealText.text = "<b>" + card.cardName + "</b>: \"" + card.dealInfo + "\"\n\n"
            + "Show this to a staff member to redeem your deal.";
    }

    public void Favorite() {
        if (string.IsNullOrEmpty(fb.userId))
            crier.ErrorMessage("Must be signed in!");
        else if (!fb.user.IsEmailVerified)
            crier.ErrorMessage("Must verify email!");
        else {
            if (crier.favorites.Contains(crier.currentCard.id)) {
                fb.SetUnfavorite(crier.currentCard);

                favoriteButton.transform.GetChild(0).GetComponent<Text>().text = "Favorite";
                var colors = favoriteButton.colors;
                colors.normalColor = new Color(93 / 255f, 255 / 255f, 132 / 255f);
                colors.highlightedColor = new Color(109 / 255f, 255 / 255f, 192 / 255f);
                colors.pressedColor = new Color(109 / 255f, 255 / 255f, 192 / 255f);
                favoriteButton.colors = colors;
            } else {
                fb.SetFavorite(crier.currentCard);

                favoriteButton.transform.GetChild(0).GetComponent<Text>().text = "Unfavorite";
                var colors = favoriteButton.colors;
                colors.normalColor = new Color(200 / 255f, 72 / 255f, 83 / 255f);
                colors.highlightedColor = new Color(255 / 255f, 97 / 255f, 111 / 255f);
                colors.pressedColor = new Color(255 / 255f, 97 / 255f, 111 / 255f);
                favoriteButton.colors = colors;
            }

        }

    }

    public void OpenCamera() {
        crier.cameraPage.GetComponent<CameraController>().isComment = true;
        crier.cameraPage.GetComponent<CameraController>().takenImage.gameObject.SetActive(false);
        crier.cameraPage.SetActive(true);
        crier.cameraPage.GetComponent<CameraController>().Activate();
        crier.cameraPage.GetComponent<CameraController>().TakeAnotherPhoto();
    }

    public void RemovePhoto() {
        tookPhoto.isOn = false;
    }

    public void ScalePhoto() {
        raw.texture = crier.currentCard.raw.texture;
        raw.SetNativeSize();
        RectTransform rt = raw.GetComponent<RectTransform>();

        float scale = 740f / rt.sizeDelta.x;

        rt.sizeDelta = rt.sizeDelta * scale;
        raw.gameObject.SetActive(true);
    }

    public void Yelp() {
        Application.OpenURL(url);
    }

    public void EnhanceImage(RawImage raw) {
        enhanceGO.SetActive(true);
        RawImage enhanced = enhanceGO.transform.GetChild(0).GetComponent<RawImage>();
        enhanced.texture = raw.texture;
        enhanced.SetNativeSize();

        enhanced.transform.Rotate(Vector3.back * 90);
        RectTransform rect = enhanced.GetComponent<RectTransform>();
        float scale = 0;

        if (Mathf.RoundToInt(Mathf.Abs(rect.rotation.eulerAngles.z)) == 90 ||Mathf.RoundToInt(Mathf.Abs(rect.rotation.eulerAngles.z)) == 270) {
            if (rect.sizeDelta.x > rect.sizeDelta.y)
                scale = GetComponent<RectTransform>().rect.height / rect.rect.width;
            else
                scale = GetComponent<RectTransform>().rect.width / rect.rect.height;

        } else {
            if (rect.sizeDelta.x > rect.sizeDelta.y)
                scale = GetComponent<RectTransform>().rect.width / rect.rect.width;
            else
                scale = GetComponent<RectTransform>().rect.height / rect.rect.height;
        }
        rect.sizeDelta = rect.sizeDelta * scale;

    }

    public void EnhanceExit() {
        enhancedImage.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        enhancedImage.GetComponent<RectTransform>().Rotate(-enhancedImage.GetComponent<RectTransform>().rotation.eulerAngles);
        enhanceGO.SetActive(false);
    }

    public void IconDescription(bool icon) {
        if (icon)
            crier.ErrorMessage("The Establishment's Wait Time", 2);
        else
            crier.ErrorMessage("The Establishment's Busyness", 2);
    }

    private void Update() {
        float bounds = crier.canvas.GetComponent<RectTransform>().rect.width * 2/3;
        if (enhanceGO.activeSelf && (Mathf.Abs(enhancedImage.GetComponent<RectTransform>().anchoredPosition.x) > bounds || Mathf.Abs(enhancedImage.GetComponent<RectTransform>().anchoredPosition.y) > bounds))
            EnhanceExit();
    }
}
