using System.Collections;
using System.Collections.Generic;

public class Place {
    public string placeName;
    public string waitTime;
    public string busyness;
    public string path;

    public Place(string placeName, string waitTime, string busyness, string path) {
        this.placeName = placeName;
        this.waitTime = waitTime;
        this.busyness = busyness;
        this.path = path;
    }
}
