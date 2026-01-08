using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : Singleton<AdManager>
{
    private const string NO_ADS_KEY = "NO_ADS";

    private BannerView banner;
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;
    private AppOpenAd appOpenAd;

    [Header("AdMob Test IDs")]
    private string bannerID = "ca-app-pub-3940256099942544/6300978111";
    private string interID = "ca-app-pub-3940256099942544/1033173712";
    private string rewardID = "ca-app-pub-3940256099942544/5224354917";
    private string appOpenID = "ca-app-pub-3940256099942544/9257395921";

    public bool IsNoAds => PlayerPrefs.GetInt(NO_ADS_KEY, 0) == 1;

    private void Start()
    {
        if (IsNoAds)
        {
            Debug.Log("No Ads Enabled - Skip Ads Init");
            return;
        }

        MobileAds.Initialize(init =>
        {
            LoadBanner();
            LoadInterstitial();
            LoadRewarded();
            LoadAppOpen();
        });
    }

    // ===================== NO ADS =====================
    public void DisableAllAds()
    {
        PlayerPrefs.SetInt(NO_ADS_KEY, 1);
        PlayerPrefs.Save();

        if (banner != null)
        {
            banner.Destroy();
            banner = null;
        }

        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        appOpenAd = null;

        Debug.Log("All Ads Disabled");
    }

    // ===================== BANNER =====================
    public void LoadBanner()
    {
        if (IsNoAds) return;

        if (banner != null)
            banner.Destroy();

        banner = new BannerView(bannerID, AdSize.Banner, AdPosition.Bottom);
        banner.LoadAd(new AdRequest());
    }

    // ===================== INTERSTITIAL =====================
    public void LoadInterstitial()
    {
        if (IsNoAds) return;

        InterstitialAd.Load(interID, new AdRequest(), (ad, error) =>
        {
            if (error != null) return;

            interstitial = ad;
            interstitial.OnAdFullScreenContentClosed += () =>
            {
                interstitial.Destroy();
                interstitial = null;
                LoadInterstitial();
            };
        });
    }

    public void ShowInterstitial()
    {
        if (IsNoAds) return;

        if (interstitial != null && interstitial.CanShowAd())
            interstitial.Show();
    }

    // ===================== REWARDED =====================
    public void LoadRewarded()
    {
        if (IsNoAds) return;

        RewardedAd.Load(rewardID, new AdRequest(), (ad, error) =>
        {
            if (error != null) return;

            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                rewardedAd.Destroy();
                rewardedAd = null;
                LoadRewarded();
            };
        });
    }

    public void ShowRewarded(Action onRewardAndClosed)
    {
        if (IsNoAds) return;
        if (rewardedAd == null || !rewardedAd.CanShowAd()) return;

        bool rewardEarned = false;

        rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            if (rewardEarned)
                onRewardAndClosed?.Invoke();
        };

        rewardedAd.Show(reward =>
        {
            rewardEarned = true;
        });
    }

    // ===================== APP OPEN =====================
    public void LoadAppOpen()
    {
        if (IsNoAds) return;

        AppOpenAd.Load(appOpenID, new AdRequest(), (ad, error) =>
        {
            if (error != null)
            {
                Debug.Log("AppOpen Load Fail");
                return;
            }

            appOpenAd = ad;
            appOpenAd.OnAdFullScreenContentClosed += () =>
            {
                appOpenAd = null;
                LoadAppOpen();
            };
        });
    }

    public void ShowAppOpen(Action onClosed)
    {
        if (IsNoAds)
        {
            onClosed?.Invoke();
            return;
        }

        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            appOpenAd.OnAdFullScreenContentClosed += () =>
            {
                appOpenAd = null;
                LoadAppOpen();
                onClosed?.Invoke();
            };
            appOpenAd.Show();
        }
        else
        {
            onClosed?.Invoke();
        }
    }
}
