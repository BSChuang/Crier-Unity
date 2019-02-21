using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CommentController : MonoBehaviour {
    public FB fb;
    public int busyness;
    public int waitTime;
    public string comment;
    public string username;
    public long time;
    public string key;
    public string uid;
    public bool upvoted;
    public GameObject upvoteGO;
    public Text upvoteCountText;
    public int upvoteCount;
    public bool changed;
    public float delay;
    public GameObject busynessBar;
    public string photoPath;
    public RawImage photoImage;
    public GameObject loading;
    public GameObject deleteButton;
    public GameObject editButton;

    public Text commentText;
    public Text leftTitleText;
    public Text rightTitleText;

    void Start() {
        fb.UpvoteCount(this);
        if (fb.user != null)
            fb.CheckUpvote(this);
    }

    void Update() {
        if (changed)
            delay += Time.deltaTime;
        if (delay > 2) {
            changed = false;
            delay = 0;
            ApplyUpvote();
        }
    }

    public void ApplyUpvote() {
        fb.Upvote(this);
    }

    public void ClientUpvote() {
        if (string.IsNullOrEmpty(fb.userId))
            fb.crier.ErrorMessage("Must be signed in!");
        else {
            changed = true;
            upvoted = !upvoted;
            if (upvoted) {
                upvoteCount++;
            } else {
                upvoteCount--;
            }
            ChangeColor();
            upvoteCountText.text = upvoteCount.ToString();
        }
    }

    public void ChangeColor() {
        if (!upvoted)
            upvoteGO.GetComponent<Image>().color = new Color32(82, 91, 94, 255);
        else
            upvoteGO.GetComponent<Image>().color = new Color32(93, 255, 132, 255);
    }

    public void SetBusynessBar() {
        int busynessInt = busyness;
        for (int i = 0; i < busynessInt; i++) {
            busynessBar.transform.GetChild(i+1).gameObject.SetActive(true);
        }
    }

    public IEnumerator SetPhoto(string url) {
        WWW www = new WWW(url);
        yield return www;
        photoImage.texture = www.texture;
        photoImage.SetNativeSize();
        RectTransform rt = photoImage.GetComponent<RectTransform>();
        float scale = 1f;
        if (rt.sizeDelta.x < rt.sizeDelta.y)
            scale = 150 / rt.sizeDelta.x;
        else
            scale = 150 / rt.sizeDelta.y;

        rt.sizeDelta = rt.sizeDelta * scale;
        rt.gameObject.SetActive(true);
        loading.SetActive(false);
    }

    public void Enhance(RawImage raw) {
        fb.crier.ic.EnhanceImage(raw);
    }

    public void Delete() {
        fb.DeleteComment(key);
        gameObject.SetActive(false);
    }

    public void Edit() {

    }
}