using DG.Tweening;
using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        InitAdsService();
        InitBannerAds();
        //InitInterstitialAds();
        InitRewardedAds();

        //EnableBottomBannerAd(true);
       
        LoadInterstitialAd();
        LoadRewardedAd();



    }

    // UI 버튼의 OnClick 이벤트에 연결할 메서드 (반드시 public이어야 합니다)
    public void OnBannerButtonClicked()
    {
        // 버튼이 눌리면 6초 뒤에 배너 광고 활성화 함수를 실행합니다.
        DOVirtual.DelayedCall(6f, () => EnableBottomBannerAd(true));
    }

    private void InitAdsService()
    {
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            Debug.Log("Google Mobile Ads initialization complete.");

            // Google Mobile Ads events are raised off the Unity Main thread. If you need to
            // access UnityEngine objects after initialization,
            // use MobileAdsEventExecutor.ExecuteInUpdate(). For more information, see:
            // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
        });
    }

    #region BannerAds
    private BannerView m_BottomBannerView;
    private string m_BottomBannerAdId = string.Empty;
    private const string AOS_BANNER_TEST_AD_ID = "ca-app-pub-8784400496294217/1976841623";
    private const string IOS_BANNER_TEST_AD_ID = "";
    private const string AOS_BOTTOM_BANNER_AD_ID = "ca-app-pub-8784400496294217/1976841623";
    private const string IOS_BOTTOM_BANNER_AD_ID = "";

    private void InitBannerAds()
    {
        SetBottomBannerAdId();
    }

    private void SetBottomBannerAdId()
    {
        m_BottomBannerAdId = AOS_BANNER_TEST_AD_ID;
    }

    public void EnableBottomBannerAd(bool value)
    {
        if (value)
        {
            if (m_BottomBannerView == null)
            {
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                m_BottomBannerView = new BannerView(m_BottomBannerAdId, adaptiveSize, AdPosition.Bottom);

                // create ad request
                AdRequest request = new AdRequest();
                // load the banner with the request
                m_BottomBannerView.LoadAd(request);
                ListenToBottomBannerAdEvents();
            }
            else
            {
                m_BottomBannerView.Show();
            }
        }
        else
        {
            if (m_BottomBannerView != null)
            {
                m_BottomBannerView.Hide();
            }
        }
    }

    private void ListenToBottomBannerAdEvents()
    {
        if (m_BottomBannerView == null)
        {
            Debug.LogError("m_BottomBannerView is null.");
            return;
        }

        m_BottomBannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log($"m_BottomBannerView loaded an ad with response : {m_BottomBannerView.GetResponseInfo()}");
        };

        m_BottomBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError($"m_BottomBannerView failed to load an ad with error : {error}");
        };

        m_BottomBannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"m_BottomBannerView paid {adValue.Value}{adValue.CurrencyCode}.");
        };

        m_BottomBannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log($"m_BottomBannerView recorded an impression.");
        };

        m_BottomBannerView.OnAdClicked += () =>
        {
            Debug.Log($"m_BottomBannerView was clicked.");
        };

        m_BottomBannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log($"m_BottomBannerView full screen content opened.");
        };

        m_BottomBannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log($"m_BottomBannerView full screen content closed.");
        };
    }
    #endregion

    #region InterstitialAds
    private InterstitialAd m_InterstitialAd;

#if UNITY_ANDROID
    private const string INTERSTITIAL_AD_UNIT_ID = "";
#elif UNITY_IPHONE
    private const string INTERSTITIAL_AD_UNIT_ID = "";
#else
    private const string INTERSTITIAL_AD_UNIT_ID = "unused";
#endif

    private void InitInterstitialAds()
    {
        // Initialize interstitial ads
    }

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (m_InterstitialAd != null)
        {
            m_InterstitialAd.Destroy();
            m_InterstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        InterstitialAd.Load(INTERSTITIAL_AD_UNIT_ID, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                return;
            }

            Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
            m_InterstitialAd = ad;
            ListenToInterstitialAdEvents(m_InterstitialAd);
        });
    }

    public void ShowInterstitialAd()
    {
        if (m_InterstitialAd != null && m_InterstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            m_InterstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    private void ListenToInterstitialAdEvents(InterstitialAd interstitialAd)
    {
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
        };

        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };

        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };

        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };

        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };

        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Interstitial ad failed to open full screen content with error : {error}");
            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
    }

    private void DestroyInterstitialAd()
    {
        if (m_InterstitialAd != null)
        {
            m_InterstitialAd.Destroy();
            m_InterstitialAd = null;
        }
    }
    #endregion

    #region RewardedAds
    private RewardedAd m_RewardedAd;

#if UNITY_ANDROID
    private const string REWARDED_AD_UNIT_ID = "ca-app-pub-8784400496294217/1342416806";
#elif UNITY_IPHONE
    private const string REWARDED_AD_UNIT_ID = "";
#else
    private const string REWARDED_AD_UNIT_ID = "unused";
#endif

    private void InitRewardedAds()
    {
        // Initialize rewarded ads
        //ca-app-pub-5275088611290339/6365912697
        //ca-app-pub-5275088611290339/9369837213
    }

    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (m_RewardedAd != null)
        {
            m_RewardedAd.Destroy();
            m_RewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedAd.Load(REWARDED_AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                return;
            }

            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
            m_RewardedAd = ad;
            ListenToRewardedAdEvents(m_RewardedAd);
        });
    }

    public void ShowRewardedAd()
    {
        if (m_RewardedAd != null && m_RewardedAd.CanShowAd())
        {
            Debug.Log("Showing rewarded ad.");
            m_RewardedAd.Show((Reward reward) =>
            {
                // The ad was shown and the user earned a reward.
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                OnRewardEarned(reward);
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
            // 광고가 준비되지 않았다면 새로 로드
            LoadRewardedAd();
        }
    }

    private void OnRewardEarned(Reward reward)
    {
        // 여기에 보상 지급 로직 추가
        Debug.Log($"보상 지급: {reward.Amount} {reward.Type}");
        GameManager.instance.RewardGo = true;
        if (SoundManager.instance != null)
            SoundManager.instance.Click();
        // 가장 안정: 씬 리로드
        Time.timeScale = 1f;
        AudioListener.pause = false;
        DOTween.KillAll();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // GameManager.Instance.RestartGame();
    }

    private void ListenToRewardedAdEvents(RewardedAd rewardedAd)
    {
        rewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
        };

        rewardedAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };

        rewardedAd.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };

        rewardedAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };

        rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };

        rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Rewarded ad failed to open full screen content with error : {error}");
            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }

    private void DestroyRewardedAd()
    {
        if (m_RewardedAd != null)
        {
            m_RewardedAd.Destroy();
            m_RewardedAd = null;
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (m_BottomBannerView != null)
        {
            m_BottomBannerView.Destroy();
            m_BottomBannerView = null;
        }

        DestroyInterstitialAd();
        DestroyRewardedAd();
    }
}

