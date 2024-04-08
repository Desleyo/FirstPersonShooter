using System.Collections;
using UnityEngine;
using TMPro;

public class Killfeed : MonoBehaviour
{
    public static Killfeed instance;

    [SerializeField] private GameObject killfeedPrefab;
    [SerializeField] private string killSymbol;
    [SerializeField] private string headshotSymbol;
    [SerializeField] private string wallbangSymbol;

    [Space]
    [SerializeField] private float waitBeforeFade;
    [SerializeField] private float fadeOutSpeed;

    private void Awake()
    {
        instance = this;
    }

    //Call this function to update the killFeed when a kill is made
    public void UpdateKillFeed(string enemyName, bool gotHeadshot, bool gotWallbanged)
    {
        //Setup kill symbols
        string symbols = killSymbol;
        if (gotWallbanged)
        {
            symbols += wallbangSymbol;
        }

        if (gotHeadshot)
        {
            symbols += headshotSymbol;
        }

        //Setup enemy name
        string dummyName = $"<color=red> {enemyName} </color>";

        //Create a killFeed message
        GameObject killFeedMessage = Instantiate(killfeedPrefab, transform);
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
