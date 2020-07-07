using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IndicatorCoreHandler : MonoBehaviour {
	public GameObject centerIndicator;
	public GameObject[] ancierallyIndicators;

	List<Vector3> startPosAncierally;
	Vector3 startCenterPos;
	private bool[] canReveal;
	bool canRevealCenter = false;
	// Use this for initialization
	void Start () {
		for (int a = 0; a < ancierallyIndicators.Length; a++)
		{
			ancierallyIndicators[a].SetActive(false);
		}
		startPosAncierally = ancierallyIndicators.Select(a => a.transform.localPosition).ToList();
		startCenterPos = centerIndicator.transform.localPosition;
		canReveal = new bool[ancierallyIndicators.Length];
	}
	bool isPlayingAnim = false;
	public IEnumerator HandleIndicatorModification(int num)
	{
		isPlayingAnim = true;
		StartCoroutine(HandleCollaspeAnim());
		while (isPlayingAnim)
			yield return new WaitForSeconds(Time.deltaTime);
		switch (num)
		{
			case 1:
				{
					for (int x = 0; x < canReveal.Length; x++)
						canReveal[x] = false;
					canRevealCenter = true;
					break;
				}
			case 2:
			case 3:
				{
					for (int x = 0; x < canReveal.Length; x++)
						canReveal[x] = x % (canReveal.Length - 1) == 0;
					canRevealCenter = num % 2 == 0;
					break;
				}
			case 4:
				{
					for (int x = 0; x < canReveal.Length; x++)
						canReveal[x] = x % 3 == 0;
					canRevealCenter = false;
					break;
				}
			case 10:
				{
					for (int x = 0; x < canReveal.Length; x++)
						canReveal[x] = true;
					canRevealCenter = false;
					break;
				}
		}
		StartCoroutine(HandleRevealAnim());
	}

	public IEnumerator HandleRevealAnim()
	{
		centerIndicator.SetActive(canRevealCenter);
		for (int x = 0; x <= 10; x++)
		{
			for (int a=0;a<ancierallyIndicators.Length;a++)
			{
				ancierallyIndicators[a].transform.localPosition = startPosAncierally[a] * (x / 10f) + startCenterPos * ((10-x)/10f);
				ancierallyIndicators[a].SetActive(canReveal[a]);
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		isPlayingAnim = false;
		yield return true;
	}
	public IEnumerator HandleCollaspeAnim()
	{
		
		for (int x = 10; x >= 0; x--)
		{
			for (int a = 0; a < ancierallyIndicators.Length; a++)
			{
				ancierallyIndicators[a].transform.localPosition = startPosAncierally[a] * (x / 10f) + startCenterPos * ((10 - x) / 10f);
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		for (int a = 0; a < ancierallyIndicators.Length; a++)
		{
			ancierallyIndicators[a].SetActive(false);
		}
		centerIndicator.SetActive(true);
		isPlayingAnim = false;
		yield return true;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
