using GoogleMobileAds.Api;
using UnityEngine;
using System;

public class GoogleAds : MonoBehaviour {
    public FB fb;
    public int clicks;
    private string interId;
    private BannerView bannerView;
    InterstitialAd interstitial;
    public RewardBasedVideoAd rewardBasedVideo;

	// Use this for initialization
	public void Start () {
        string appId = "ca-app-pub-5391633317099406~3933951365";
        MobileAds.Initialize(appId);
        rewardBasedVideo = RewardBasedVideoAd.Instance;
        RequestBanner();
        RequestInterstitial();
    }

    private void RequestBanner() {
        string adUnitId = "";
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            adUnitId = "ca-app-pub-5391633317099406/4361751801";

        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
        bannerView.Show();
    }

    public void RequestInterstitial() {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            interId = "ca-app-pub-5391633317099406/3482458981";

        RequestNewInterstitial();
        interstitial.OnAdClosed += HandleOnAdClosed;
    }

    public void RequestNewInterstitial() {
        interstitial = new InterstitialAd(interId);
        AdRequest adrequest = new AdRequest.Builder().Build();
        interstitial.LoadAd(adrequest);
    }

    public void TryInterstitial() {
        if (interstitial.IsLoaded())
            interstitial.Show();
    }

    public void HandleOnAdClosed(object sender, EventArgs args) {
        clicks = 0;
        interstitial.Destroy();
        RequestNewInterstitial();
    }
}
