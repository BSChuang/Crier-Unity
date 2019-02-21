using System.Collections.Generic;

public class User {
    public string username;
    public string joinDate;
    public int points;
    public int redeemPoints;

    public User(string username, string joinDate) {
        this.username = username;
        this.joinDate = joinDate;
        points = 0;
        redeemPoints = 0;
    }
}
