using System.Collections;
using UnityEngine;
using TMPro;

public class KillFeed : MonoBehaviour
{
    public static KillFeed instance;

    [SerializeField] private GameObject killFeedPrefab;
    [SerializeField] private string killSymbol;
    [SerializeField] private string headShotSymbol;
    [SerializeField] private string wallBangSymbol;

    [Space]
    [SerializeField] private float waitBeforeFade;
    [SerializeField] private float fadeOutSpeed;

    private void Awake()
    {
        instance = this;
    }

    //Call this function to update the killFeed when a kill is made
    public void UpdateKillFeed(string enemyName, bool gotHeadShot, bool gotWallBanged)
    {
        //Setup kill symbols
        string symbols = killSymbol;
        if (gotWallBanged)
        {
            symbols += wallBangSymbol;
        }
        if (gotHeadShot)
        {
            symbols += headShotSymbol;
        }

        //Setup enemy name
        string dummyName = $"<color=red> {enemyName} </color>";

        //Create a killFeed message
        GameObject killFeedMessage = Instantiate(killFeedPrefab, transform);
        TextMeshProUGUI killText = killFeedMessage.GetComponentInChildren<TextMeshProUGUI>();
        killText.text = $"<color=blue> Player </color> {symbols} {dummyName}";

        StartCoroutine(FadeOutKillText(killFeedMessage, waitBeforeFade));
    }

    //Call this enumerator to fade out the killfeed message over time
    private IEnumerator FadeOutKillText(GameObject killFeedInstance, float time)
    {
        yield return new WaitForSeconds(time);

        CanvasGroup cGroup = killFeedInstance.GetComponent<CanvasGroup>();

        while(cGroup.alpha > 0)
         {
            cGroup.alpha -= Time.deltaTime / fadeOutSpeed;
            yield return null;
         }

        Destroy(killFeedInstance);
    }
}
