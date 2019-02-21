using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Messaging;
using SimpleJSON;

public class FB : MonoBehaviour {
    // Crier service account password = notasecret
    // crierabc@gmail.com
    // CrierChuang
    // 5/10/98
    public Crier crier;
    public Profile profile;
    public DatabaseReference reference;
    public FirebaseStorage storage;
    public FirebaseUser user;
    public string username;
    public string userId;
    public string token;

    StorageReference storageRef;
    FirebaseAuth auth;

    void Start () {
        auth = FirebaseAuth.DefaultInstance;

        reference = FirebaseDatabase.DefaultInstance.RootReference;

        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://crierabc-bf743.appspot.com");

        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    public void FixAndroid() {
        crier.ErrorMessage("Please wait", 2);
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                crier.ErrorMessage("Ready to go!", 1);
                StartCoroutine(crier.StartLocation());
            } else {
                crier.ErrorMessage("An error with Firebase has occurred.");
            }
        });
    }

    // PUSH NOTIFICATIONS -------------------------------------------------
    public void OnTokenReceived(object sender, TokenReceivedEventArgs token) {
        crier.CDebug("Received Registration Token: " + token.Token);
        this.token = token.Token;
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        crier.CDebug("Received a new message from: " + e.Message.From);
    }

    public void SaveLocation() {
        reference.Child("Users/basics/" + userId + "/lat").SetValueAsync(Input.location.lastData.latitude);
        reference.Child("Users/basics/" + userId + "/lon").SetValueAsync(Input.location.lastData.longitude);
    }

    // USERS ---------------------------------------------------------------

    public void EmailSignup(string email, string password, string first, string last) {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                crier.CDebug(task.Exception.ToString());
                crier.ErrorMessage("An error has occurred. Check Debug for details.");
                return;
            }

            // Firebase user has been created.
            FirebaseUser newUser = task.Result;
            newUser.SendEmailVerificationAsync();
            crier.ErrorMessage("Account created. Please verify email.", 1);
            CheckUser(newUser, null, first, last);
        });
    }
    
    public void EmailLogin(string email, string password) {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                crier.ErrorMessage("Email or password incorrect");
                return;
            }
            profile.emailLoginPage.SetActive(false);
            profile.accountPage.SetActive(true);
            FirebaseUser newUser = task.Result;
            crier.ErrorMessage("Logged in!", 1);
            CheckUser(newUser, null);
        });
    }

    public void CheckUser(FirebaseUser user, Google.GoogleSignInUser googleUser, string first = null, string last = null) { // For Signing in
        crier.CDebug("Checking user...");
        if (user == null)
            return;
        this.user = user;
        userId = user.UserId;
        FirebaseDatabase.DefaultInstance.GetReference("/Users/basics/" + user.UserId + "/").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("User check faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists) {
                    crier.CDebug("User does not exist. Making account.");
                    if (googleUser != null) {
                        if (string.IsNullOrEmpty(googleUser.GivenName))
                            first = "";
                        else
                            first = googleUser.GivenName;

                        if (string.IsNullOrEmpty(googleUser.FamilyName[0].ToString()))
                            last = "";
                        else
                            last = googleUser.FamilyName[0].ToString();

                        crier.CDebug("Making user account (Google Sign-in).");
                        username = first + " " + last + ".";
                        crier.CDebug("Username: " + username + ". Writing user now.");
                        writeNewUser(user.UserId, (first + " " + last + "."), DateTime.Now.ToShortDateString());
                        crier.CDebug("Wrote user. Updating profile.");
                        profile.UpdateProfile((first + " " + last + "."), DateTime.Now.ToShortDateString(), "0", "0");
                    } else if (first != null) {
                        crier.CDebug("Making user account (Email Sign-in).");
                        writeNewUser(user.UserId, (first + " " + last[0] + "."), DateTime.Now.ToShortDateString());
                        profile.UpdateProfile((first + " " + last[0] + "."), DateTime.Now.ToShortDateString(), "0", "0");
                    } else {
                        crier.CDebug("Not google and no first name");
                    }
                } else {
                    crier.CDebug("User exists, setting values.");
                    username = snapshot.Child("username").Value.ToString();
                    string joinDate = snapshot.Child("joinDate").Value.ToString();
                    string points = snapshot.Child("points").Value.ToString();
                    string redeemPoints = snapshot.Child("redeemPoints").Value.ToString();
                    profile.UpdateProfile(username, joinDate, points, redeemPoints);
                    GetFavorites();
                    CheckOwner();
                }
                crier.CDebug("User ID: " + userId);
            } else {
                Debug.Log("User check issue");
            }
        });
    }

    void writeNewUser(string userId, string name, string joinDate) {
        User user = new User(name, joinDate);
        string json = JsonUtility.ToJson(user);

        reference.Child("Users").Child("basics").Child(userId).SetRawJsonValueAsync(json);
    }

    // COMMENTS --------------------------------------------
    public void GetComments(CardController card) {
        FirebaseDatabase.DefaultInstance.GetReference("commentTimestamps/" + card.id).OrderByValue().LimitToFirst(20).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    foreach (DataSnapshot comment in snapshot.Children) {
                        string commentKey = comment.Key;
                        GetCommentCont(card, commentKey);
                    }
                } else {
                    card.cardText.text = card.cardText.text.Replace("Busyness: Unknown", "Busyness: null issue");
                }
            }
        });
    }

    public void GetCommentCont(CardController card, string commentKey) {
        FirebaseDatabase.DefaultInstance.GetReference("comments/" + commentKey).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["key"] = commentKey;
                    foreach (DataSnapshot data in snapshot.Children) {
                        object odata = data.Value;
                        string key = data.Key;
                        dict[key] = odata;
                    }
                    card.comments.Add(dict);
                    card.SetCardBusyness(card.id, card);
                } else {
                    card.cardText.text = card.cardText.text.Replace("Busyness: Unknown", "Busyness: null issue");
                }
            }
        });
    }

    public void SetComment(string placeId, CardController card, string waitTime, string busyness, string comment, bool tookPhoto, byte[] bytes) {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["Users/basics/" + user.UserId + "/tempComment/action"] = "New";
        childUpdates["Users/basics/" + user.UserId + "/tempComment/waitTime"] = waitTime;
        childUpdates["Users/basics/" + user.UserId + "/tempComment/busyness"] = busyness;
        childUpdates["Users/basics/" + user.UserId + "/tempComment/comment"] = comment;
        childUpdates["Users/basics/" + user.UserId + "/tempComment/timestamp"] = System.DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss");
        childUpdates["Users/basics/" + user.UserId + "/tempComment/username"] = username;
        childUpdates["Users/basics/" + user.UserId + "/tempComment/pid"] = card.id;

        if (tookPhoto) {
            string key = reference.Child("Users/basics/" + user.UserId + "/tempComment/photoKey").Push().Key;
            childUpdates["Users/basics/" + user.UserId + "/tempComment/photoKey"] = key;
            UploadImage(bytes, key);
        }

        reference.UpdateChildrenAsync(childUpdates);

        crier.ErrorMessage("Submitted!", 1);
        // CF double checks path is correct
        // CF points placeid comments to userid comments
    }

    public void DeleteComment(string key) {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["Users/basics/" + user.UserId + "/tempComment/action"] = "Delete";
        childUpdates["Users/basics/" + user.UserId + "/tempComment/key"] = key;
        reference.UpdateChildrenAsync(childUpdates);
        crier.ErrorMessage("Deleted Comment!", 1);
    }

    // FAVORITES -------------------------------------------------------------------------------

    public void GetFavorites() {
        crier.favorites.Clear();
        reference.Child("Users/basics/" + user.UserId + "/subscriptions").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot subscription in snapshot.Children) {
                    string key = subscription.Key;
                    crier.favorites.Add(key);
                }
            } else {
                Debug.Log("Other issue");
            }
        });
    }

    public void SetFavorite(CardController card) {
        crier.favorites.Add(card.id);
        reference.Child("subscribers/" + card.id + "/" + user.UserId).SetValueAsync(token);
        reference.Child("Users/basics/" + user.UserId + "/subscriptions/" + card.id).SetValueAsync(true);

        crier.ErrorMessage("Added to favorites!", 1);
    }

    public void SetUnfavorite(CardController card) {
        crier.favorites.Remove(card.id);
        reference.Child("subscribers/" + card.id + "/" + user.UserId).SetValueAsync(null);
        reference.Child("Users/basics/" + user.UserId + "/subscriptions/" + card.id).SetValueAsync(null);

        crier.ErrorMessage("Removed from favorites!", 1);
    }

    // UPVOTES --------------------------------------------------------
    public void Upvote(CommentController comCont) {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        if (comCont.upvoted) {
            childUpdates["comments/" + comCont.key + "/upvoted/" + userId] = true;
        } else {
            childUpdates["comments/" + comCont.key + "/upvoted/" + userId] = null;
        }
        reference.UpdateChildrenAsync(childUpdates);
    }
    public void UpvoteCount(CommentController comCont) {
        FirebaseDatabase.DefaultInstance.GetReference("comments/" + comCont.key + "/upvoted").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                long val = snapshot.ChildrenCount;
                comCont.upvoteCount = (int)val;
                comCont.upvoteCountText.text = val.ToString();
            } else {
                Debug.Log("Other issue");
            }
        });
    }

    public void CheckUpvote(CommentController comCont) {
        FirebaseDatabase.DefaultInstance.GetReference("comments/" + comCont.key + "/upvoted/" + user.UserId).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                bool val = snapshot.Exists;
                if (val) {
                    comCont.upvoted = true;
                } else {
                    comCont.upvoted = false;
                }
                comCont.ChangeColor();
            } else {
                Debug.Log("Other issue");
            }
        });
    }
    // DEALS ----------------------------------------------------------------------
    public void CheckOwner() {
        FirebaseDatabase.DefaultInstance.GetReference("/Users/owners/" + userId + "/").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("User check faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists) {
                    profile.claimText.text = "User";
                } else {
                    profile.claimText.text = "Owner";

                    foreach (DataSnapshot pid in snapshot.Children) { // For multiple place ownerships
                        crier.ownerId = pid.Key;

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        foreach (DataSnapshot data in pid.Children) { // Each data undeer the place
                            dict[data.Key] = data.Value;
                        }

                        crier.ownerRedeem = (bool)dict["canRedeem"];
                        crier.dealsPage.GetComponent<Deals>().costsToggle.interactable = crier.ownerRedeem;

                        StartCoroutine(crier.mapApi.SetOwnerPath(crier.ownerId));
                    }
                }
            }
        });
    }

    public void GetDeal(CardController card) {
        FirebaseDatabase.DefaultInstance.GetReference("deals").OrderByKey().EqualTo(card.id).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result.Child(card.id);
                if (snapshot.Exists) {
                    string endTime = snapshot.Child("endTime").Value.ToString();
                    string shortInfo = snapshot.Child("shortInfo").Value.ToString();
                    string info = snapshot.Child("info").Value.ToString();
                    bool costs = (bool)snapshot.Child("costs").Value;
                    card.dealEndTime = endTime;
                    card.endTime = System.DateTime.ParseExact(endTime, "yyyyMMddHHmmss", null);
                    card.dealShortInfo = shortInfo;
                    card.dealInfo = info;
                    card.dealTimeText.SetActive(true);
                    card.dealShortText.gameObject.SetActive(true);
                    card.costs = costs;
                    card.costsImage.SetActive(costs);
                    card.noCostImage.SetActive(!costs);
                    card.cardText.text = card.cardText.text.Replace("\n\n", "\n<color=yellow>Deal!</color>\n");
                    CheckRedeemedPath(card);
                }
            } else {
                Debug.Log("Other issue");
            }
        });
    }

    public void SetDeal(int hours, int minutes, string shortInfo, string dealInfo) {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();

        childUpdates["Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempDeal/shortInfo"] = shortInfo;
        childUpdates["Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempDeal/info"] = dealInfo;
        childUpdates["Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempDeal/endTime"] = System.DateTime.Now.AddHours(hours).AddMinutes(minutes).ToUniversalTime().ToString("yyyyMMddHHmmss");
        childUpdates["Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempDeal/milli"] = hours * 3600000 + minutes * 60000;
        childUpdates["Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempDeal/costs"] = crier.dealsPage.GetComponent<Deals>().costsToggle.isOn;
        childUpdates["Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempDeal/pName"] = crier.dealsPage.GetComponent<Deals>().placeName.text;
        reference.UpdateChildrenAsync(childUpdates);
    }

    // REDEEMED STUFF ---------------------------------------------------
    public void CheckRedeemedPath(CardController card) {
        string dealPath = card.dealPath;
        FirebaseDatabase.DefaultInstance.GetReference("deals/" + card.id + "/redeemed").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    card.redeemed = true;
                }
            } else {
                Debug.Log("Other issue");
            }
        });
    }

    public void CheckRedeemPoints(InfoController ic) {
        int redeemPoints = 0;
        FirebaseDatabase.DefaultInstance.GetReference("Users/basics/" + userId + "/redeemPoints").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    redeemPoints = int.Parse(snapshot.Value.ToString());
                    if (redeemPoints == 0) {
                        crier.ErrorMessage("Not enough redeem tokens!");
                    } else {
                        AddRedeemPoints(-1);
                        crier.currentCard.redeemed = true;
                        crier.ErrorMessage("Redeem token spent!", 1);
                        AddUserDealPath();
                        ic.RedeemedDeal();
                    }
                }
            } else {
                Debug.Log("Other issue");
            }
        });
    }

    void AddUserDealPath() {
        CardController card = crier.currentCard;
        reference.Child("deals/" + card.id + "/redeemed/" + userId).SetValueAsync(true);
    }

    public void AddRedeemPoints(int points) {
        int redeemPoints = 0;
        FirebaseDatabase.DefaultInstance.GetReference("Users/basics/" + userId + "/redeemPoints").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    redeemPoints = int.Parse(snapshot.Value.ToString());

                    Dictionary<string, object> childUpdates = new Dictionary<string, object>();
                    redeemPoints += points;
                    childUpdates["Users/basics/" + userId + "/redeemPoints"] = redeemPoints;
                    reference.UpdateChildrenAsync(childUpdates);
                }
            } else {
                Debug.Log("Other issue");
            }
        });
    }

    // IMAGES -------------------------------------------------------

    public void SetBanner(byte[] bytes) {
        string key = reference.Child("Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempBanner").Push().Key;
        reference.Child("Users/owners/" + user.UserId + "/" + crier.ownerId + "/tempBanner").SetValueAsync(key);
        UploadImage(bytes, key);
    }

    public void UploadImage(byte[] bytes, string key) {
        StorageReference photoRef = storageRef.Child("userImages/" + user.UserId + "/" + key + ".jpg");

        photoRef.PutBytesAsync(bytes).ContinueWith((Task<StorageMetadata> task) => {
            if (task.IsFaulted || task.IsCanceled) {
                Debug.Log(task.Exception.ToString());
                // Uh-oh, an error occurred!
            } else {
                // Metadata contains file metadata such as size, content-type, and download URL.
                StorageMetadata metadata = task.Result;
                crier.ErrorMessage("Finished Uploading!", 1);
            }
        });
    }

    public void DeleteImage(string path) {
        reference.Child("Users/owners/" + userId + "/" + crier.ownerId + "/banner").SetValueAsync(false);
        StorageReference photoRef = storageRef.Child(path);

        photoRef.DeleteAsync().ContinueWith(task => {
            if (task.IsCompleted) {
                crier.ErrorMessage("Banner deleted.", 1);
            } else {
                crier.ErrorMessage("An error has occurred.");
            }
        });
    } 

    public void SetCommentPhotoURL(string path, CommentController comCont) {
        StorageReference photoRef = storageRef.Child(path);
        photoRef.GetDownloadUrlAsync().ContinueWith((Task<Uri> task) => {
            if (!task.IsFaulted && !task.IsCanceled) {
                string url = task.Result.ToString();
                if (!string.IsNullOrEmpty(url)) {
                    comCont.rightTitleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-170, comCont.rightTitleText.GetComponent<RectTransform>().anchoredPosition.y);
                }

                StartCoroutine(comCont.SetPhoto(url));
            }
        });
    }

    public void GetBannerPhotoURL(string key, CardController card) {
        FirebaseDatabase.DefaultInstance.GetReference("banners/" + card.id).GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Faulted");
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    storageRef.Child("userImages/" + (string)snapshot.Child("uid").Value + "/" + (string)snapshot.Child("key").Value + ".jpg").GetDownloadUrlAsync().ContinueWith((Task<Uri> photoTask) => {
                        if (!photoTask.IsFaulted && !photoTask.IsCanceled) {
                            string url = photoTask.Result.ToString();
                            card.imageURL = url;
                            StartCoroutine(crier.mapApi.photo(url, card.raw, card));
                        }
                    });
                } else {
                    StartCoroutine(crier.mapApi.photo(card.imageURL, card.raw, card));
                }
            } else {
                Debug.Log("Other issue");
            }
        });


    }
}