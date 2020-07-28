using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleIndicatorHandler : MonoBehaviour {

	public KMSelectable anySelectable;
	public KMBombModule modSelf;
	public IndicatorCoreHandlerEX CoreHandlerEX;
	public ProgressBarHandler barHandler;
	private int curCount = 1;
	private bool canRun = false;
	// Use this for initialization
	void Start () {
		anySelectable.OnInteract += delegate
		{

			canRun = !canRun;
			return false;
		};
	}





	// Update is called once per frame
	void Update () {
		if (canRun)
			barHandler.curProgress += Time.deltaTime;
		if (barHandler.curProgress > barHandler.maxProgress)
		{
			barHandler.curProgress = 0;
			curCount = Mathf.Min(curCount + 1, 12);

			if (curCount > 11)
			{
				modSelf.HandlePass();
				curCount = 1;
			}
			StartCoroutine(CoreHandlerEX.HandleIndicatorModification(curCount));
		}
	}
}
