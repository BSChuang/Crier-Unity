using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IComparable<CardController> {
    public string id;
    public string cardName;
    public string fullAddress;
    public string url;
    public string phoneNumber;
    public string distance;
    public float floatDist;
    public string imageURL;
    public string waitTime;
    public string busyness;
    public string dealInfo;
    public string dealShortInfo;
    public string dealEndTime;
    public bool costs;
    public System.DateTime endTime;
    public string dealPath;
    public bool redeemed;
    public bool doneLoading;

    public List<Dictionary<string, object>> comments = new List<Dictionary<string, object>>();

    public Crier crier;
    public Text cardText;
    public Text waitTimeText;
    public GameObject costsImage;
    public GameObject noCostImage;
    public RawImage raw;
    public GameObject busynessBar;
    public GameObject busynessBack;
    public Text dealShortText;
    public GameObject dealTimeText;
    public RectTransform scroll;
    
    public int CompareTo(CardController card) {
        return floatDist.CompareTo(card.floatDist);
    }

    public void cardOnClick() {
        crier.initInfoPage(gameObject);
        crier.slideIn();
        SetCardBusyness(id, this);
    }

    public void SetCardBusyness(string placeId, CardController card) {
        if (card.comments.Count != 0) {
            List<string> busynesses = new List<string>();
            List<string> waitTimes = new List<string>();
            foreach (Dictionary<string, object> comment in card.comments) {
                busynesses.Add(comment["busyness"].ToString());
                waitTimes.Add(comment["waitTime"].ToString());
            }
            int avgWait = Mathf.RoundToInt(Average(waitTimes));
            int avgBusyness = Mathf.RoundToInt(Average(busynesses));
            card.waitTime = avgWait.ToString();
            card.busyness = avgBusyness.ToString();

            card.waitTimeText.text = avgWait.ToString() + " minute wait";
            card.setBusynessBar();
        }
    }
    private float Average(List<string> list) {
        float avg = 0;
        foreach (string value in list)
            if (value != "")
                avg += int.Parse(value);
        avg /= list.Count;
        return avg;
    }

    public void setBusynessBar() {
        busynessBar.SetActive(true);
        busynessBack.SetActive(true);
        int busynessInt = int.Parse(busyness);
        for (int i = 0; i < busynessInt; i++) {
            busynessBar.transform.GetChild(i+1).gameObject.SetActive(true);
        }
        for (int i = 0; i < busynessInt; i++) {
            busynessBar.transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
        }
    }

    private void FixedUpdate() {
        if (dealTimeText) {
            System.DateTime nowTime = System.DateTime.Now.ToUniversalTime();
            System.TimeSpan diff = endTime - nowTime;

            string stime = crier.AddZeroes(diff.Hours) + ":" + crier.AddZeroes(diff.Minutes) + ":" + crier.AddZeroes(diff.Seconds);
            dealTimeText.GetComponent<Text>().text = stime;
            dealShortText.text = dealShortInfo;
        }
    }
}
