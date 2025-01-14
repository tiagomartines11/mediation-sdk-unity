//  ScaleMonkAds.cs
//
//  © 2020 ScaleMonk, Inc. All Rights Reserved.
// Licensed under the ScaleMonk SDK License Agreement
// https://www.scalemonk.com/legal/en-US/mediation-license-agreement/index.html 
//

using System;
#if UNITY_IOS
using ScaleMonk.Ads.iOS;
#elif UNITY_ANDROID
using ScaleMonk.Ads.Android;
#endif
using UnityEngine;

namespace ScaleMonk.Ads
{
    public class ScaleMonkAds
    {
        const string _label = "ScaleMonkAds";
        const string DEFAULT_TAG = "DEFAULT_TAG";

        static ScaleMonkAds _instance;
        readonly IAdsBinding _adsBinding;
        public string ApplicationId;

        /// <summary>
        /// Instance to use the Ads SDK.
        /// </summary>
        public static ScaleMonkAds SharedInstance
        {
            get
            {
                if (_instance == null)
                {
                    AdsLogger.LogError("{0} | You should call ScaleMonkAds.Initialize first");
                }

                return _instance;
            }
        }
        
        /// <summary>
        /// Initializes the SDK.
        ///
        /// <param name="applicationId">The identifier for the application that will be using the Mediation SDK</param>
        /// </summary>
        public static void Initialize(string applicationId)
        {
            if (_instance != null)
            {
                AdsLogger.LogWarning("{0} | Ads SDK already initialized", _label);
                return;
            }

            AdsLogger.LogWithFormat("{0} | Initializing Ads SDK", _label);

            _instance = new ScaleMonkAds(applicationId);
            ScaleMonkAdsMonoBehavior.Initialize(_instance);
            _instance.InitializeInternal();
        }
        
        /// <summary>
        /// Tells the ScaleMonk SDK whether the user has granted consent as prescribed by the GDPR laws and that data can be collected
        ///
        /// </summary>
        /// <param name="consent"> True if the user has granted consent, false otherwise
        /// </param>

        public void SetHasGDPRConsent(bool consent)
        {
            _adsBinding.SetHasGDPRConsent(consent);
        }
        
        /// <summary>
        /// Tells the ScaleMonk SDK whether the application is targeted to children and should only show age-appropriate ads
        ///
        /// </summary>
        /// <param name="isChildDirected"> True if the app is child directed, false otherwise
        /// </param>

        public void SetIsApplicationChildDirected(bool isChildDirected)
        {
            _adsBinding.SetIsApplicationChildDirected(isChildDirected);
        }
        
        /// <summary>
        /// Tells the ScaleMonk SDK that the user can't give consent for GDPR since they're underage
        ///
        /// </summary>
        /// <param name="consent"> True if the user is underage, false otherwise
        /// </param>

        public void SetUserCantGiveGDPRConsent(bool cantGiveConsent)
        {
            _adsBinding.SetUserCantGiveGDPRConsent(cantGiveConsent);
        }

        /// <summary>
        /// Displays an interstitial ad.
        ///
        /// If the display was successful, the event `InterstitialDisplayedEvent` will be called when the ad closes.
        /// Otherwise, the event `InterstitialNotDisplayedEvent` will be called.
        public void ShowInterstitial()
        {
            AdsLogger.LogWithFormat("{0} | Show interstitial at tag {1}", _label, DEFAULT_TAG);
            _adsBinding.ShowInterstitial(DEFAULT_TAG);
        }
        
        /// <summary>
        /// Displays an interstitial ad.
        ///
        /// If the display was successful, the event `InterstitialDisplayedEvent` will be called when the ad closes.
        /// Otherwise, the event `InterstitialNotDisplayedEvent` will be called.
        /// </summary>
        /// <param name="tag">The game tag from where the ad will be displayed (like menu or store).</param>
        public void ShowInterstitial(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Show interstitial at tag {1}", _label, tag);
            _adsBinding.ShowInterstitial(tag);
        }

        /// <summary>
        /// Displays a rewarded ad.
        ///
        /// If the display was successful, the event `RewardedDisplayedEvent` will be called when the ad closes.
        /// Otherwise, the event `RewardedNotDisplayedEvent` will be called.
        /// </summary>
        public void ShowRewarded()
        {
            AdsLogger.LogWithFormat("{0} | Show rewarded at tag {1}", _label, DEFAULT_TAG);
            _adsBinding.ShowRewarded(DEFAULT_TAG);
        }
        
        /// <summary>
        /// Displays a rewarded ad.
        ///
        /// If the display was successful, the event `RewardedDisplayedEvent` will be called when the ad closes.
        /// Otherwise, the event `RewardedNotDisplayedEvent` will be called.
        /// </summary>
        /// <param name="tag">The game tag from where the ad will be displayed (like menu or store).</param>
        public void ShowRewarded(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Show rewarded at tag {1}", _label, tag);
            _adsBinding.ShowRewarded(tag);
        }
        
        /// <summary>
        /// Creates a new Ads SDK
        /// </summary>
        /// <param name="applicationId">The application identifier to be used by CAdS and Exchange to identify the game</param>
        ScaleMonkAds(string applicationId)
        {
            ApplicationId = applicationId;
            _adsBinding = GetAdsBinding();
        }
        
        void InitializeInternal()
        {
            _adsBinding.Initialize(this);
        }
        
        static void CallAction(Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        
        /// <summary>
        /// Informs the rewarded display was successful. Use this callback to restart the app state and give rewards to the user.
        /// </summary>
        public static Action RewardedDisplayedEvent;

        /// <summary>
        /// Informs the rewarded display has started. Use this callback to pause any behavior you might need.
        /// </summary>
        public static Action RewardedStartedEvent;

        /// <summary>
        /// Informs the rewarded display was not successful.
        ///
        /// To see the reason for the failed display, check the reason field in the `ads:display-failed` analytics event.
        /// </summary>
        public static Action RewardedNotDisplayedEvent;

        /// <summary>
        /// Informs the rewarded ad was clicked.
        /// </summary>
        public static Action RewardedClickedEvent;

        /// <summary>
        /// Informs the interstitial display was successful. Use this callback to restart the app state and give rewards to the user.
        /// </summary>
        public static Action InterstitialDisplayedEvent;

        /// <summary>
        /// Informs the interstitial display was not successful.
        ///
        /// To see the reason for the failed display, check the reason field in the `ads:display-failed` analytics event.
        /// </summary>
        public static Action InterstitialNotDisplayedEvent;

        /// <summary>
        /// Informs the interstitial ad was clicked.
        /// </summary>
        public static Action InterstitialClickedEvent;
        
        /// <summary>
        /// Informs that an interstitial ad has been successfully cached and is ready to be shown.
        /// </summary>
        public static Action InterstitialReadyEvent;
        
        /// <summary>
        /// Informs that an interstitial ad has not been successfully cached and is not ready to be shown.
        /// </summary>
        public static Action InterstitialNotReadyEvent;
        
        /// <summary>
        /// Informs that a rewarded ad has been successfully cached and is ready to be shown.
        /// </summary>
        public static Action RewardedReadyEvent;
        
        /// <summary>
        /// Informs that a rewarded ad has not been successfully cached and is not ready to be shown.
        /// </summary>
        public static Action RewardedNotReadyEvent;
        
        #region Ads Native Binding Callbacks
        public void CompletedRewardedDisplay(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Rewarded displayed at tag {1}", _label, tag);
            CallAction(RewardedDisplayedEvent);
        }

        public void StartedRewardedDisplay(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Started rewarded at tag {1}", _label, tag);
            CallAction(RewardedStartedEvent);
        }

        public void ClickedRewarded(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Clicked rewarded at tag {1}", _label, tag);
            CallAction(RewardedClickedEvent);
        }

        public void FailedRewardedDisplay(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Rewarded not displayed at tag {1}", _label, tag);
            CallAction(RewardedNotDisplayedEvent);
        }

        public void CompletedInterstitialDisplay(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Interstitial displayed at tag {1}", _label, tag);
            CallAction(InterstitialDisplayedEvent);
        }

        public void ClickedInterstitial(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Clicked interstitial at tag {1}", _label, tag);
            CallAction(InterstitialClickedEvent);
        }

        public void FailedInterstitialDisplay(string tag)
        {
            AdsLogger.LogWithFormat("{0} | Interstitial not displayed at tag {1}", _label, tag);
            CallAction(InterstitialNotDisplayedEvent);
        }
        
        public void InterstitialReady()
        {
            AdsLogger.LogWithFormat("{0} | Interstitial ready to be displayed", _label);
            CallAction(InterstitialReadyEvent);
        }
        
        public void InterstitialNotReady()
        {
            AdsLogger.LogWithFormat("{0} | Interstitial not ready to be displayed", _label);
            CallAction(InterstitialNotReadyEvent);
        }
        
         public void RewardedReady()
        {
            AdsLogger.LogWithFormat("{0} | Rewarded ad ready to be displayed", _label);
            CallAction(RewardedReadyEvent);
        }

         public void RewardedNotReady()
         {
             AdsLogger.LogWithFormat("{0} | Rewarded ad not ready to be displayed", _label);
             CallAction(RewardedNotReadyEvent);  
         }
  
        #endregion
        
        static IAdsBinding GetAdsBinding()
        {
            IAdsBinding binding = null;

#if UNITY_EDITOR
            binding = new AdsEditorBinding();
#elif UNITY_ANDROID
            binding = new AdsAndroidBinding();
#elif UNITY_IOS
            binding = new AdsiOSBinding();
#endif
            return binding;
        }
    }
}