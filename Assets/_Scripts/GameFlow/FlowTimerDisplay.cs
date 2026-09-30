using TMPro;
using UnityEngine;

public class FlowTimerDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlowManager flowManager;
    [SerializeField] private TextMeshProUGUI exploreText;
    [SerializeField] private TextMeshProUGUI purchaseText;

    [Header("Text Templates")]
    [TextArea][SerializeField] private string exploreHeader = "絵画ショップを散策する";
    [SerializeField] private string exploreFormat = "購入可能時間まで あと{0}分{1}秒";
    [TextArea][SerializeField] private string purchaseHeader = "気に入った絵画を購入する";
    [SerializeField] private string purchaseFormat = "閉店まで あと{0}分{1}秒";

    private float timeUntilPurchase;
    private float purchaseTimeRemaining;
    private bool isExploreTimerActive;
    private bool isPurchaseTimerActive;
    private Phase currentPhase = Phase.Init;

    private void OnEnable()
    {
        if (flowManager == null)
        {
            return;
        }

        flowManager.PhaseChanged += HandlePhaseChanged;
        HandlePhaseChanged(flowManager.CurrentPhase);
    }

    private void OnDisable()
    {
        if (flowManager == null)
        {
            return;
        }

        flowManager.PhaseChanged -= HandlePhaseChanged;
    }

    private void Update()
    {
        UpdateExploreTimer();
        UpdatePurchaseTimer();
    }

    private void UpdateExploreTimer()
    {
        if (!isExploreTimerActive)
        {
            if (exploreText != null)
            {
                exploreText.text = string.Empty;
            }
            return;
        }

        if (currentPhase == Phase.Explore || currentPhase == Phase.Recommend)
        {
            timeUntilPurchase = Mathf.Max(0f, timeUntilPurchase - Time.deltaTime);
            if (exploreText != null)
            {
                var minutes = Mathf.FloorToInt(timeUntilPurchase / 60f);
                var seconds = Mathf.FloorToInt(timeUntilPurchase % 60f);
                exploreText.text = $"{exploreHeader}\n" + string.Format(exploreFormat, minutes, seconds);
            }
        }
        else
        {
            isExploreTimerActive = false;
            if (exploreText != null)
            {
                exploreText.text = string.Empty;
            }
        }
    }

    private void UpdatePurchaseTimer()
    {
        if (!isPurchaseTimerActive)
        {
            if (purchaseText != null)
            {
                purchaseText.text = string.Empty;
            }
            return;
        }

        if (currentPhase == Phase.Purchase)
        {
            purchaseTimeRemaining = Mathf.Max(0f, purchaseTimeRemaining - Time.deltaTime);
            if (purchaseText != null)
            {
                var minutes = Mathf.FloorToInt(purchaseTimeRemaining / 60f);
                var seconds = Mathf.FloorToInt(purchaseTimeRemaining % 60f);
                purchaseText.text = $"{purchaseHeader}\n" + string.Format(purchaseFormat, minutes, seconds);
            }
        }
        else
        {
            isPurchaseTimerActive = false;
            if (purchaseText != null)
            {
                purchaseText.text = string.Empty;
            }
        }
    }

    private void HandlePhaseChanged(Phase phase)
    {
        currentPhase = phase;

        switch (phase)
        {
            case Phase.Explore:
                timeUntilPurchase = flowManager != null
                    ? flowManager.ExploreDuration + flowManager.RecommendDuration
                    : 0f;
                isExploreTimerActive = timeUntilPurchase > 0f;
                break;
            case Phase.Recommend:
                var recommendDuration = flowManager != null ? flowManager.RecommendDuration : 0f;
                if (!isExploreTimerActive || timeUntilPurchase <= 0f || timeUntilPurchase > recommendDuration)
                {
                    timeUntilPurchase = recommendDuration;
                }
                isExploreTimerActive = recommendDuration > 0f;
                break;
            case Phase.Purchase:
                purchaseTimeRemaining = flowManager != null ? flowManager.PurchaseDuration : 0f;
                isPurchaseTimerActive = purchaseTimeRemaining > 0f;
                break;
            default:
                isExploreTimerActive = false;
                isPurchaseTimerActive = false;
                timeUntilPurchase = 0f;
                purchaseTimeRemaining = 0f;
                break;
        }
    }
}
