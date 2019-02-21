using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class MapApi : MonoBehaviour {
    public Crier crier;
    public OptionPage optionPage;
    public Deals deals;
    public string categories = "categories=restaurants";
    string sort = "&sort_by=best_match";
    private static string key = "y4wR1MkkIapOv4PPb64-gq5sfkDhHUe8sPdGF8kypp7qTepyPj6uY-bfZBmoJuI-oZgBhsOXZIG1fAkv6HLzSVxkFSrE_40vVxzWcTJjjZqeoq7B2pudX1CmPZEmW3Yx";

    public IEnumerator YelpNearbySearch(string lat, string lon) {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", "Bearer " + key);
        lat = "&latitude=" + lat;
        lon = "&longitude=" + lon;
        WWW www = new WWW("https://api.yelp.com/v3/businesses/search?" + categories + sort + lat + lon, null, headers);
        yield return www;
        JSONNode json = JSON.Parse(www.text);
        crier.nearbyY = 7;
        crier.nearbyList.Clear();
        crier.loadingList.Clear();
        foreach (JSONNode business in json["businesses"].Values) {
            string id = business["id"];
            string name = business["name"];
            string imageURL = business["image_url"];
            string number = business["display_phone"];
            string distance = (Mathf.Round(Mathf.RoundToInt(business["distance"]) * 0.00621371f) / 10f).ToString();
            string url = business["url"];

            string address = business["location"]["address1"];
            string city = business["location"]["city"];
            string state = business["location"]["state"];
            string country = business["location"]["country"];
            string fullAddress = address + ", " + city + ", " + state;

            string[] data = new string[] { id, name, imageURL, number, distance, fullAddress, url };
            crier.initCard(crier.cardPage, data);
        }
    }

    public IEnumerator More(string lat, string lon) {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", "Bearer " + key);
        lat = "&latitude=" + lat;
        lon = "&longitude=" + lon;
        WWW www = new WWW("https://api.yelp.com/v3/businesses/search?" + categories + sort + lat + lon, null, headers);
        yield return www;
        JSONNode json = JSON.Parse(www.text);
        int count = 0;
        foreach (JSONNode business in json["businesses"].Values) {
            if (count < 20) {
                count++;
                continue;
            }
            string id = business["id"];
            string name = business["name"];
            string imageURL = business["image_url"];
            string number = business["display_phone"];
            string distance = (Mathf.Round(Mathf.RoundToInt(business["distance"]) * 0.00621371f) / 10f).ToString();
            string url = business["url"];

            string address = business["location"]["address1"];
            string city = business["location"]["city"];
            string state = business["location"]["state"];
            string country = business["location"]["country"];
            string fullAddress = address + ", " + city + ", " + state;

            string[] data = new string[] { id, name, imageURL, number, distance, fullAddress, url };
            crier.initCard(crier.cardPage, data);
        }
    }

    public IEnumerator YelpTextSearch(string term, string lat, string lon, string location = "") {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", "Bearer " + key);

        term = "&term=" + term;
        if (location != "") {
            location = "&location=" + location;
            lat = "";
            lon = "";
        } else {
            lat = "&latitude=" + lat;
            lon = "&longitude=" + lon;
        }
        WWW www = new WWW("https://api.yelp.com/v3/businesses/search?" + categories + sort + term + location + lat + lon, null, headers);
        yield return www;
        JSONNode json = JSON.Parse(www.text);
        crier.loadingList.Clear();
        crier.searchY = 140;
        foreach (JSONNode business in json["businesses"].Values) {
            string id = business["id"];
            string name = business["name"];
            string imageURL = business["image_url"];
            string number = business["display_phone"];
            string distance = (Mathf.Round(Mathf.RoundToInt(business["distance"]) * 0.00621371f) / 10f).ToString();
            string url = business["url"];

            string address = business["location"]["address1"];
            string city = business["location"]["city"];
            string state = business["location"]["state"];
            string country = business["location"]["country"];
            string fullAddress = address + ", " + city + ", " + state;

            string[] data = new string[] { id, name, imageURL, number, distance, fullAddress, url };

            crier.initCard(crier.searchPage, data);
        }
    }

    public IEnumerator Favorites(List<string> favorites) {
        crier.favoritesPage.transform.GetChild(1).gameObject.SetActive(false);
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", "Bearer " + key);
        crier.favoritesY = 7;
        foreach (string pid in favorites) {
            WWW www = new WWW("https://api.yelp.com/v3/businesses/" + pid, null, headers);
            yield return www;
            JSONNode business = JSON.Parse(www.text);

            string id = business["id"];
            string name = business["name"];
            string imageURL = business["image_url"];
            string number = business["display_phone"];
            float lat = business["coordinates"]["latitude"];
            float lon = business["coordinates"]["longitude"];
            float fDist = CalculateDistance(lat, Input.location.lastData.latitude, lon, Input.location.lastData.longitude);
            string distance = (Mathf.Round(Mathf.RoundToInt(fDist * 0.00621371f) / 10f).ToString());
            string url = business["url"];

            string address = business["location"]["address1"];
            string city = business["location"]["city"];
            string state = business["location"]["state"];
            string country = business["location"]["country"];
            string fullAddress = address + ", " + city + ", " + state;

            string[] data = new string[] { id, name, imageURL, number, distance, fullAddress, url };

            crier.initCard(crier.favoritesPage, data);
        }
    }

    public IEnumerator photo(string imageURL, RawImage raw, CardController card, bool rotated = false) {
        raw.gameObject.SetActive(false);
        if (!string.IsNullOrEmpty(imageURL)) {
            WWW www = new WWW(imageURL);
            yield return www;

            if (raw) { // hasn't been refreshed
                raw.texture = www.texture;
                raw.SetNativeSize();

                float ratio = raw.texture.width / (float)raw.texture.height;
                raw.GetComponent<AspectRatioFitter>().aspectRatio = ratio;

                raw.gameObject.SetActive(true);
            }
        }

        if (card != null) {
            card.doneLoading = true;
            crier.CheckLoading();
        }
    }

    public IEnumerator SetOwnerPath(string pid) {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", "Bearer " + key);

        WWW www = new WWW("https://api.yelp.com/v3/businesses/" + pid, null, headers);
        yield return www;
        JSONNode json = JSON.Parse(www.text);
        string pName = json["name"];
        string address = json["location"]["address1"];
        string city = json["location"]["city"];
        string state = json["location"]["state"];
        string country = json["location"]["country"];
        if (city == "")
            city = "null";
        if (state == "")
            state = "null";
        if (country == "")
            country = "null";

        crier.ownerPath = "/Places/" + country + "/" + state + "/" + city + "/";
        crier.dealsPage.GetComponent<Deals>().placeName.text = pName;
        crier.ownerBotBar.SetActive(true);
    }

    private float CalculateDistance(float lat_1, float lat_2, float long_1, float long_2) {
        int R = 6371;
        var lat_rad_1 = Mathf.Deg2Rad * lat_1;
        var lat_rad_2 = Mathf.Deg2Rad * lat_2;
        var d_lat_rad = Mathf.Deg2Rad * (lat_2 - lat_1);
        var d_long_rad = Mathf.Deg2Rad * (long_2 - long_1);
        var a = Mathf.Pow(Mathf.Sin(d_lat_rad / 2), 2) + (Mathf.Pow(Mathf.Sin(d_long_rad / 2), 2) * Mathf.Cos(lat_rad_1) * Mathf.Cos(lat_rad_2));
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var total_dist = R * c * 1000; // convert to meters
        return total_dist;
    }
}