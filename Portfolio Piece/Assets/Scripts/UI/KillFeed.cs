using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeed : MonoBehaviour
{
    public static KillFeed killFeed;
    [SerializeField] GameObject killFeedPrefab;
    [SerializeField] string killSymbol;
    [SerializeField] string hsSymbol;
    [SerializeField] string wbSymbol;

    [Space, SerializeField] float waitBeforeFade;
    [SerializeField] float fadeOutSpeed;

    private void Awake()
    {
        killFeed = this;
    }

    public void UpdateKillFeed(string enemyName, bool gotHeadShot, bool gotWallBanged)
    {
        //Setup kill symbols
        string symbols = killSymbol;
        if (gotWallBanged)
            symbols += wbSymbol;
        if (gotHeadShot)
            symbols += hsSymbol;

        //Setup enemy name
        string dummyName = " <color=red>" + enemyName + "</color>";

        GameObject killFeedInstance = Instantiate(killFeedPrefab, transform);
        TextMeshProUGUI killText = killFeedInstance.GetComponentInChildren<TextMeshProUGUI>();
        killText.text = "<color=blue> Player </color>" + symbols + dummyName;

        StartCoroutine(FadeOutKillText(killFeedInstance, waitBeforeFade));
    }

    IEnumerator FadeOutKillText(GameObject killFeedInstance, float time)
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
