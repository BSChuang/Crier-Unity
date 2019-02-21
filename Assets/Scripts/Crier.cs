using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class Crier : MonoBehaviour {
    public FB fb;
    public MapApi mapApi;
    public GoogleAds gAds;
    public FirebaseAuth auth;
    public RectTransform canvas;

    public string radius;
    public string type;
    public string keyword;

    public string waitTime;
    public string ownerId;
    public string ownerPath;
    public bool ownerRedeem;
    public int placeSelected;
    public int redeemPoints;
    public GameObject pages;
    public GameObject topBar;
    public GameObject ownerBotBar;
    public GameObject basicBotBar;

    public Dictionary<string, List<string>> places;

    public Text title;
    public GameObject backButton;

    public GameObject cardPage;
    public GameObject cardPrefab;
    public GameObject dealsPage;
    public GameObject searchPage;
    public GameObject favoritesPage;
    public GameObject profilePage;
    public GameObject infoPage;
    public GameObject optionPage;
    public GameObject cameraPage;

    public InfoController ic;

    public InputField searchField;
    public InputField locationField;

    public GameObject errorMessage;
    public CardController currentCard;
    public List<CardController> nearbyList;
    public List<string> favorites;

    public List<CardController> loadingList;

    public int sortOption;

    private void Awake() {
        Application.targetFrameRate = 60;
        if (Application.platform == RuntimePlatform.IPhonePlayer && UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX) {
            topBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 270);
            pages.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -270);
            pages.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 1214f);
            basicBotBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 140);
            ownerBotBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 140);
        }
    }
    void Start() {
        auth = FirebaseAuth.DefaultInstance;
        if (Application.platform == RuntimePlatform.Android)
            fb.FixAndroid();
        else
            StartCoroutine(StartLocation());
    }

    public IEnumerator StartLocation() {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            StartCoroutine(mapApi.YelpNearbySearch(41.010504.ToString(), (-73.879259).ToString()));
            yield break;
        }

        if (!Input.location.isEnabledByUser) {
            CDebug("Location not enabled");
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed) {
            ErrorMessage("Unable to determine device location.");
            yield break;
        } else {
            StartCoroutine(mapApi.YelpNearbySearch(Input.location.lastData.latitude.ToString(), Input.location.lastData.longitude.ToString()));                
        }
        Input.location.Stop();
    }

    public float nearbyY = 0;
    public float searchY = 0;
    public float favoritesY = 0;
    public void initCard(GameObject page, string[] data) {
        GameObject card = Instantiate(cardPrefab, page.transform.GetChild(0));
        CardController cc = card.GetComponent<CardController>();
        cc.crier = this;

        cc.id = data[0];
        cc.cardName = data[1];
        cc.name = data[1];
        cc.imageURL = data[2];
        cc.phoneNumber = data[3];
        cc.distance = data[4];
        cc.floatDist = float.Parse(cc.distance);
        cc.fullAddress = data[5];
        cc.url = data[6];

        if (page.name == "Nearby Page") {
            nearbyList.Add(cc);
            nearbyList.Sort();
            RectTransform rectTransform = page.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, nearbyY + card.GetComponent<RectTransform>().sizeDelta.y + 10);
            nearbyY = 0;
            for (int i = 0; i < nearbyList.Count; i++) {
                nearbyList[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -nearbyY, 0);
                nearbyY += card.GetComponent<RectTransform>().sizeDelta.y + 10; //+ 5;
            }
        } else if (page.name == "Favorites Page") {
            RectTransform rectTransform = page.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, favoritesY + card.GetComponent<RectTransform>().sizeDelta.y + 10);
            card.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -favoritesY, 0);
            favoritesY += card.GetComponent<RectTransform>().sizeDelta.y + 5;
        } else if (page.name == "Search Page") {
            RectTransform rectTransform = page.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, searchY + card.GetComponent<RectTransform>().sizeDelta.y + 10);
            card.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -searchY, 0);
            searchY += card.GetComponent<RectTransform>().sizeDelta.y + 5;
        }

        card.name = cc.cardName;
        if (cc.cardName.Length > 40)
            cc.cardName = cc.cardName.Substring(0, 39) + "...";
        cc.cardText.text = cc.cardName + "\n" + cc.distance + " mi";
        cc.waitTimeText.text = "???";
        cc.scroll = cardPage.transform.GetChild(0).GetComponent<RectTransform>();
        fb.GetComments(cc);
        fb.GetDeal(cc);

        loadingList.Add(cc);

        fb.GetBannerPhotoURL(cc.id, cc);
    }

    public void initInfoPage(GameObject card) {
        if (gAds.clicks > 7)
            gAds.TryInterstitial();
        else
            gAds.clicks++;

        CardController cc = card.GetComponent<CardController>();
        currentCard = cc;
        title.text = cc.cardName;
        StartCoroutine(mapApi.photo(cc.imageURL, ic.raw, null, false));
        ic.addressText.text = cc.fullAddress;
        ic.numberText.text = cc.phoneNumber;
        if (cc.waitTime == "")
            cc.waitTime = "???";
        ic.waitTimeText.text = cc.waitTime + " minute wait";
        ic.url = cc.url;
        ic.SetBusynessBar(0);
        if (cc.busyness != "")
            ic.SetBusynessBar(int.Parse(cc.busyness));

        if (cc.dealInfo != "") {
            ic.dealTimeText.gameObject.SetActive(true);
            ic.dealText.gameObject.SetActive(true);
            ic.dealImage.gameObject.SetActive(true);
            endTime = System.DateTime.ParseExact(cc.dealEndTime, "yyyyMMddHHmmss", null);
            ic.dealText.text = cc.dealInfo + "\n(Press here to redeem!)";
            if (cc.costs)
                ic.dealText.text += "(Requires 1 redeem token)";

            ic.dealText.fontSize = Mathf.RoundToInt(35 - ic.dealText.text.Length / 10);
        } else {
            ic.dealTimeText.gameObject.SetActive(false);
            ic.dealText.gameObject.SetActive(false);
            ic.dealImage.gameObject.SetActive(false);
        }

        /*if (favorites.Contains(cc.id)) {
            var colors = ic.favoriteButton.colors;
            colors.normalColor = new Color(200 / 255f, 72 / 255f, 83 / 255f);
            colors.highlightedColor = new Color(255 / 255f, 97 / 255f, 111 / 255f);
            colors.pressedColor = new Color(255 / 255f, 97 / 255f, 111 / 255f);
            ic.favoriteButton.colors = colors;
        } else {
            var colors = ic.favoriteButton.colors;
            colors.normalColor = new Color(93 / 255f, 255 / 255f, 132 / 255f);
            colors.highlightedColor = new Color(109 / 255f, 255 / 255f, 192 / 255f);
            colors.pressedColor = new Color(109 / 255f, 255 / 255f, 192 / 255f);
            ic.favoriteButton.colors = colors;
        }*/

        // COMMENTS -----------------------------------------

        if (cc.comments.Count != 0) {
            float commentY = 940;
            foreach (Dictionary<string, object> commentDict in cc.comments) {
                // Calculate time
                string sTime = commentDict["timestamp"].ToString();
                System.DateTime dateTime = System.DateTime.ParseExact(sTime, "yyyyMMddHHmmss", null);
                System.DateTime nowTime = System.DateTime.Now.ToUniversalTime();
                System.TimeSpan diffTime = nowTime.Subtract(dateTime);

                string stime;
                if (diffTime.Hours > 4) // If its been more than 4 hours, skip comment
                    continue;
                if (diffTime.Hours != 0)
                    stime = diffTime.Hours + "h";
                else if (diffTime.Minutes != 0)
                    stime = diffTime.Minutes + "m";
                else
                    stime = diffTime.Seconds + "s";

                GameObject comment = Instantiate(ic.commentPrefab, ic.transform.GetChild(1)) as GameObject;
                CommentController comCont = comment.GetComponent<CommentController>();
                ic.comConts.Add(comCont);
                comCont.fb = fb;

                comment.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -commentY, 0);
                RectTransform rectTransform = infoPage.transform.GetChild(1).GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(0, commentY + comCont.GetComponent<RectTransform>().sizeDelta.y + 10);
                commentY += comCont.GetComponent<RectTransform>().sizeDelta.y + 10;
                //ic.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>().sizeDelta = Vector2.up * commentY;

                int foo = int.Parse(commentDict["busyness"].ToString());
                comCont.busyness = int.Parse(commentDict["busyness"].ToString());
                comCont.comment = (string)commentDict["comment"];
                comCont.username = (string)commentDict["username"];
                comCont.key = (string)commentDict["key"];
                comCont.time = long.Parse(commentDict["timestamp"].ToString());
                comCont.uid = (string)commentDict["uid"];
                comCont.waitTime = int.Parse(commentDict["waitTime"].ToString());
                if (commentDict.ContainsKey("photoPath")) {
                    comCont.photoPath = (string)commentDict["photoPath"];
                    comCont.loading.SetActive(true);
                    fb.SetCommentPhotoURL(comCont.photoPath, comCont);
                } else {
                    comCont.photoImage.transform.parent.gameObject.SetActive(false);
                }

                comCont.leftTitleText.text = comCont.waitTime + " minute wait";
                comCont.rightTitleText.text = comCont.username + "\n" + stime + " ago";
                comCont.commentText.text = comCont.comment;
                comCont.SetBusynessBar();

                if (fb.user != null && comCont.uid == fb.user.UserId) {
                    //comCont.editButton.SetActive(true);
                    comCont.deleteButton.SetActive(true);
                }

            }
        } else {
            RectTransform rectTransform = infoPage.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 0);
        }

        backButton.SetActive(true);
    }

    public void CheckLoading() {
        bool allDone = true;
        for (int i = 0; i < loadingList.Count; i++) {
            if (!loadingList[i].doneLoading) {
                allDone = false;
                break;
            }
        }
        if (allDone) {
            cardPage.transform.GetChild(1).gameObject.SetActive(false);
            searchPage.transform.GetChild(1).gameObject.SetActive(false);
            for (int i = 0; i < loadingList.Count; i++) {
                loadingList[i].gameObject.SetActive(true);
            }
        }
    }

    public void SearchOnClick() {
        foreach (Transform child in searchPage.transform.GetChild(0))
            if (child.GetComponent<CardController>())
                Destroy(child.gameObject);
        string query = searchField.text;
        string location = locationField.text;
        if (query.Length != 0) {
            searchPage.transform.GetChild(1).gameObject.SetActive(true);
            if (Application.platform == RuntimePlatform.WindowsEditor)
                StartCoroutine(mapApi.YelpTextSearch(query, 37.78584.ToString(), (-122.4064).ToString(), location));
            else
                StartCoroutine(mapApi.YelpTextSearch(query, Input.location.lastData.latitude.ToString(), Input.location.lastData.longitude.ToString(), location));
        }

    }


    bool slideInBool = false;
    bool slideOutBool = false;
    public void slideIn() {
        ic.gameObject.SetActive(true);
        slideInBool = true;
    }

    public void Back() {
        backButton.SetActive(false);
        ic.camera.gameObject.SetActive(false);
        ic.camera.Deactivate();
        slideOutBool = true;
        ic.ApplyUpvotes();
        ic.Clear();
        if (cardPage)
            title.text = "Nearby";
        else
            title.text = "Search";
    }

    public void Refresh() {
        Handheld.Vibrate();
        ErrorMessage("Refreshing...", 2);

        if (infoPage.activeSelf && currentCard != null) {
            initInfoPage(currentCard.gameObject);
        } else {
            RefreshPages();
        }
    }
    public void RefreshPages() {
        if (cardPage.activeSelf) {
            cardPage.transform.GetChild(1).gameObject.SetActive(true);
            RectTransform rectTransform = cardPage.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 0);
            foreach (Transform child in cardPage.transform.GetChild(0))
                Destroy(child.gameObject);
            StartCoroutine(StartLocation());
        } else if (searchPage.activeSelf) {
            searchPage.transform.GetChild(1).gameObject.SetActive(true);
            RectTransform rectTransform = searchPage.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 0);
            foreach (Transform child in searchPage.transform.GetChild(0)) {
                if (child.GetComponent<CardController>())
                    Destroy(child.gameObject);
            }

            StartCoroutine(mapApi.YelpTextSearch(searchField.text, Input.location.lastData.latitude.ToString(), Input.location.lastData.longitude.ToString(), locationField.text));
        } else if (favoritesPage.activeSelf) {
            RectTransform rectTransform = favoritesPage.transform.GetChild(0).GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 0);
            foreach (Transform child in favoritesPage.transform.GetChild(0)) {
                if (child.GetComponent<CardController>())
                    Destroy(child.gameObject);
            }

            StartCoroutine(mapApi.Favorites(favorites));
        }
    }

    public void ErrorMessage(string message, int type = 0) {
        if (type == 0) {
            errorMessage.GetComponent<Image>().color = new Color32(87, 71, 71, 255);
            errorMessage.transform.GetChild(0).GetComponent<Text>().color = new Color32(246, 147, 141, 255);
        } else if (type == 1) {
            errorMessage.GetComponent<Image>().color = new Color32(60, 85, 79, 255);
            errorMessage.transform.GetChild(0).GetComponent<Text>().color = new Color32(29, 167, 115, 255);
        } else  {
            errorMessage.GetComponent<Image>().color = new Color32(70, 91, 94, 255);
            errorMessage.transform.GetChild(0).GetComponent<Text>().color = new Color32(104, 179, 191, 255);
        }
        errorTime = 3;
        errorMessage.gameObject.SetActive(true);
        errorMessage.transform.GetChild(0).GetComponent<Text>().text = message;
    }

    float errorTime = 0;
    float speed = 7500;

    private void Update() {

        // ErrorMessage Fade
        if (errorTime > 0) {
            errorTime -= Time.deltaTime;
            if (errorTime < 1) {
                Image errorImage;
                Text errorTextImage;
                errorImage = errorMessage.GetComponent<Image>();
                errorTextImage = errorMessage.transform.GetChild(0).GetComponent<Text>();
                var tempColor = errorImage.color;
                var tempTextColor = errorTextImage.color;
                tempColor.a = errorTime / 1f;
                tempTextColor.a = errorTime / 1f;
                errorImage.color = tempColor;
                errorTextImage.color = tempTextColor;
            }
        }
        if (errorTime < 0) {
            errorMessage.gameObject.SetActive(false);
            errorTime = 0;
        }

        // Info Slide Out
        if (slideOutBool) {
            ic.transform.Translate(Vector3.right * speed * Time.deltaTime);
            if (ic.GetComponent<RectTransform>().anchoredPosition.x >= canvas.rect.width) {
                slideOutBool = false;
                ic.gameObject.SetActive(false);
                ic.GetComponent<RectTransform>().anchoredPosition = Vector3.right * canvas.GetComponent<RectTransform>().sizeDelta.x;
            }
        }

        // Info Slide In
        if (slideInBool) {
            ic.transform.Translate(Vector3.left * speed * Time.deltaTime);
            if (ic.GetComponent<RectTransform>().anchoredPosition.x <= 0) {
                slideInBool = false;
                ic.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            }
        }

        // Updates Info Page Deal Time
        if (currentCard && currentCard.dealInfo != null)
            TimeRemain();
    }

    System.DateTime endTime;
    private void TimeRemain() {
        System.TimeSpan diff = endTime - System.DateTime.Now.ToUniversalTime();
        ic.dealTimeText.text = AddZeroes(diff.Hours) + ":" + AddZeroes(diff.Minutes) + ":" + AddZeroes(diff.Seconds);
    }

    public string AddZeroes(int time) {
        if (time <= 0)
            return "00";
        if (time < 10)
            return "0" + time;
        else
            return time.ToString();
    }

    public void pageOnClick(GameObject button) {
        disablePages();
        Color32 green = new Color32(255, 127, 80, 255);
        if (button.name == "Nearby") {
            cardPage.SetActive(true);
            basicBotBar.transform.GetChild(0).GetChild(1).GetComponent<Image>().color = green;
            ownerBotBar.transform.GetChild(0).GetChild(1).GetComponent<Image>().color = green;
            basicBotBar.transform.GetChild(0).GetChild(2).GetComponent<Text>().color = green;
            ownerBotBar.transform.GetChild(0).GetChild(2).GetComponent<Text>().color = green;
        }
        else if (button.name == "Business") {
            dealsPage.SetActive(true);
            ownerBotBar.transform.GetChild(1).GetChild(1).GetComponent<Image>().color = green;
            ownerBotBar.transform.GetChild(1).GetChild(2).GetComponent<Text>().color = green;
        }
        else if (button.name == "Search") {
            searchPage.SetActive(true);
            basicBotBar.transform.GetChild(1).GetChild(1).GetComponent<Image>().color = green;
            ownerBotBar.transform.GetChild(2).GetChild(1).GetComponent<Image>().color = green;
            basicBotBar.transform.GetChild(1).GetChild(2).GetComponent<Text>().color = green;
            ownerBotBar.transform.GetChild(2).GetChild(2).GetComponent<Text>().color = green;
        } else if (button.name == "Favorites") {
            favoritesPage.SetActive(true);
            basicBotBar.transform.GetChild(2).GetChild(1).GetComponent<Image>().color = green;
            ownerBotBar.transform.GetChild(3).GetChild(1).GetComponent<Image>().color = green;
            basicBotBar.transform.GetChild(2).GetChild(2).GetComponent<Text>().color = green;
            ownerBotBar.transform.GetChild(3).GetChild(2).GetComponent<Text>().color = green;

            if (favorites.Count != 0 && favoritesPage.transform.GetChild(0).childCount == 0)
                StartCoroutine(mapApi.Favorites(favorites));

        } else if (button.name == "Profile") {
            profilePage.SetActive(true);
            fb.CheckUser(fb.user, null);
            basicBotBar.transform.GetChild(3).GetChild(1).GetComponent<Image>().color = green;
            ownerBotBar.transform.GetChild(4).GetChild(1).GetComponent<Image>().color = green;
            basicBotBar.transform.GetChild(3).GetChild(2).GetComponent<Text>().color = green;
            ownerBotBar.transform.GetChild(4).GetChild(2).GetComponent<Text>().color = green;
        }
        else if (button.name == "Options")
            optionPage.SetActive(true);
        title.text = button.name;
    }

    public void disablePages() {
        Back();
        cardPage.SetActive(false);
        dealsPage.SetActive(false);
        searchPage.SetActive(false);
        favoritesPage.SetActive(false);
        profilePage.SetActive(false);
        optionPage.SetActive(false);
        cameraPage.SetActive(false);
        profilePage.GetComponent<Profile>().emailSignupPage.SetActive(false);
        cameraPage.GetComponent<CameraController>().Deactivate();

        Color32 gray = new Color32(90, 90, 90, 255);
        foreach (Transform trans in basicBotBar.transform) {
            if (trans.tag != "Ignore") {
                trans.GetChild(1).GetComponent<Image>().color = gray;
                trans.GetChild(2).GetComponent<Text>().color = gray;
            }
        }
        foreach (Transform trans in ownerBotBar.transform) {
            if (trans.tag != "Ignore") {
                trans.GetChild(1).GetComponent<Image>().color = gray;
                trans.GetChild(2).GetComponent<Text>().color = gray;
            }
        }

    }

    public void OpenClose(GameObject go) {
        if (go.name == "Write Comment" && string.IsNullOrEmpty(fb.userId)) {
            ErrorMessage("Must be signed in!");
        } else if (!fb.user.IsEmailVerified)
            ErrorMessage("Email is not verified!");
        else {
            go.SetActive(!go.activeSelf);
        }
    }


    public void CDebug(string text) {
        optionPage.GetComponent<OptionPage>().debugPage.transform.GetChild(0).GetComponent<Text>().text += "\n" + text;
    }

    public void SignOut() {
        auth.SignOut();
        Google.GoogleSignIn.DefaultInstance.SignOut();
        Profile profile = profilePage.GetComponent<Profile>();
        profile.accountPage.SetActive(false);
        profile.signInPage.SetActive(true);
        ownerBotBar.SetActive(false);
        ErrorMessage("Logged out!", 1);
    }
}
