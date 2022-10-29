using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorCoreHandlerEX : MonoBehaviour {
	public GameObject centerIndicator, indicatorStart, indicatorEnd;
	public GameObject[] ancierallyIndicators;

	Vector3 startCenterPos, endLPos, endRPos;
	List<Vector3> targetAncilerryPos = new List<Vector3>();

	bool canRevealCenter = false;
	// Use this for initialization
	void Start () {
		for (int a = 0; a < ancierallyIndicators.Length; a++)
		{
			ancierallyIndicators[a].SetActive(false);
		}
		startCenterPos = centerIndicator.transform.localPosition;
		endLPos = indicatorStart.transform.localPosition;
		endRPos = indicatorEnd.transform.localPosition;
		indicatorStart.SetActive(false);
		indicatorEnd.SetActive(false);
	}
	bool isPlayingAnim = false;
	public bool GetPlayingAnimBool()
    {
		return isPlayingAnim;
    }
	public IEnumerator HandleIndicatorModification(int num)
	{
		isPlayingAnim = true;
		StartCoroutine(HandleCollaspeAnim());
		yield return new WaitWhile(delegate { return isPlayingAnim; });

		targetAncilerryPos.Clear();

		canRevealCenter = num % 2 == 1;
		if (num > 0)
		{
			if (num != 1)
			{
				indicatorStart.SetActive(true);
				indicatorEnd.SetActive(true);
				for (int x = 1; x < num - 1; x++)
				{
					Vector3 curTargetPos = endLPos * (num - 1 - x) / (num - 1) + endRPos * x / (num - 1);
					if (!curTargetPos.Equals(startCenterPos))
						targetAncilerryPos.Add(curTargetPos);
				}
			}
			else
			{
				indicatorStart.SetActive(false);
				indicatorEnd.SetActive(false);
			}
		}
		

		isPlayingAnim = true;
		StartCoroutine(HandleRevealAnim());
	}

	public IEnumerator HandleRevealAnim()
	{
		centerIndicator.SetActive(canRevealCenter);
        for (float x = 0; x <= 1f; x += Time.deltaTime * 4)
		{
			for (int a = 0; a < targetAncilerryPos.Count; a++)
			{
                ancierallyIndicators[a].transform.localPosition = targetAncilerryPos[a] * x + startCenterPos * (1f - x);
				ancierallyIndicators[a].SetActive(true);
			}
			indicatorEnd.transform.localPosition = endRPos * x + startCenterPos * (1f - x);
            indicatorStart.transform.localPosition = endLPos * x + startCenterPos * (1f - x);
			yield return null;
		}
		for (int a = 0; a < targetAncilerryPos.Count; a++)
		{
			ancierallyIndicators[a].transform.localPosition = targetAncilerryPos[a];
			ancierallyIndicators[a].SetActive(true);
		}
		indicatorEnd.transform.localPosition = endRPos;
		indicatorStart.transform.localPosition = endLPos;
		isPlayingAnim = false;
		yield return true;
	}
	public IEnumerator HandleCollaspeAnim()
	{

        for (float x = 1; x >= 0; x -= Time.deltaTime * 4)
		{
			for (int a = 0; a < targetAncilerryPos.Count; a++)
			{
				ancierallyIndicators[a].transform.localPosition = targetAncilerryPos[a] * x + startCenterPos * (1f - x);
			}
			indicatorEnd.transform.localPosition = endRPos * x + startCenterPos * (1f - x);
			indicatorStart.transform.localPosition = endLPos * x + startCenterPos * (1f - x);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		for (int a = 0; a < targetAncilerryPos.Count; a++)
		{
			ancierallyIndicators[a].transform.localPosition = startCenterPos;
			ancierallyIndicators[a].SetActive(true);
		}
		indicatorEnd.transform.localPosition = startCenterPos;
		indicatorStart.transform.localPosition = startCenterPos;
		for (int a = 0; a < ancierallyIndicators.Length; a++)
		{
			ancierallyIndicators[a].SetActive(false);
		}
		indicatorEnd.SetActive(false);
		indicatorStart.SetActive(false);
		centerIndicator.SetActive(true);
		isPlayingAnim = false;
		yield return true;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
