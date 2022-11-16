using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class UnfairsForgottenCiphersHandler : MonoBehaviour {

	public KMBombModule modSelf;
	public KMAudio mAudio;
	public KMBombInfo bombInfo;
	public Light[] displayLights;
	public GameObject animatorThingy;
	public ParticleSystem particleHandler;
	public KMSelectable[] coloredButtonSelectables;
	public KMSelectable screenSelectable, innerSelectable, outerSelectable;
	public TextMesh[] normalTextMeshes, bigTextMeshes, strikeTextMeshes;
	public IndicatorCoreHandlerEX indicatorCore;
	public MeshRenderer[] ledRenderers6, ledRenderers3, coloredButtonRenderers, screenRenderers;
	public ButtonPushAnim[] pushAnimsAll;

	private static readonly Color[] colorRefListsAll = {
		Color.black,
		new Color(0.5f, 0, 0),
		Color.red,
		new Color(0, 0.5f, 0),
		new Color(0.5f, 0.5f, 0),
		new Color(1, 0.5f, 0),
		Color.green,
		new Color(0.5f, 1f, 0),
		Color.yellow,
		new Color(0, 0, 0.5f),
		new Color(0.5f, 0, 0.5f),
		new Color(1, 0, 0.5f),
		new Color(0, 0.5f, 0.5f),
		Color.gray,
		new Color(1, 0.5f, 0.5f),
		new Color(0, 1, 0.5f),
		new Color(0.5f, 1, 0.5f),
		new Color(1, 1, 0.5f),
		Color.blue,
		new Color(0.5f, 0, 1),
		Color.magenta,
		new Color(0, 0.5f, 1),
		new Color(0.5f, 0.5f, 1),
		new Color(1, 0.5f, 1),
		Color.cyan,
		new Color(0.5f, 1, 1),
		Color.white,
	};
	private static readonly string[] colorRefNames = {
		"Black","Maroon","Red",
		"Forest","Olive","Orange",
		"Green","Lime","Yellow",
		"Indigo","Plum","Rose",
		"Teal","Gray","Salmon",
		"Jade","Mint","Cream",
		"Blue","Violet","Magenta",
		"Azure","Maya","Pink",
		"Cyan","Aqua","White",
	};

	Dictionary<char ,int[,]> operatorTableTernary = new Dictionary<char, int[,]> 
		{
			{'+', new int[,] {
				{ 0, 0, 1 },
				{ 0, 1, 2 },
				{ 1, 2, 2 },
			}},
			{'×', new int[,] {
				{ 2, 1, 0 },
				{ 1, 1, 1 },
				{ 0, 1, 2 },
			}},
			{'○', new int[,] {
				{ 0, 2, 1 },
				{ 2, 1, 0 },
				{ 1, 0, 2 },
			}},
			{'m', new int[,] {
				{ 0, 0, 0 },
				{ 0, 1, 1 },
				{ 0, 1, 2 },
			}},
			{'M', new int[,] {
				{ 0, 1, 2 },
				{ 1, 1, 2 },
				{ 2, 2, 2 },
			}},
			{'∅', new int[,] {
				{ 1, 0, 2 },
				{ 0, 1, 0 },
				{ 2, 0, 1 },
			}},
		};


	List<string> encodingsPagesTop, encodingsPagesSide;
	List<int> ledCountQuery = new List<int>();
	string decodedWords = "", inputtedWord = "", inputtedSequence = "";
	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", symbolRef = "-0+", inputStringRef = "ABCDEFGHIJKLMNOPQRSTUVWXYZ?";
	static int modIDCnt;
	int moduleID;
	bool displayModuleIDText, isSolved, swapTopAndRightScreens, interactable, solving, unlockedSubmission, isReady, playAltAnimOnLastSolve, disableStrikeOnAutosolve;
	int idxStrikeDisplay, expectedSafeDigit, curPageIdx = 0;
	int[] idx6ColorsDisplay, idx3ColorsDisplay;

	UnfairsForgottenCiphersSettings ufcSettings;

	void QuickLog(string value, params object[] args)
	{
		Debug.LogFormat("[Unfair's Forgotten Ciphers #{0}] {1}", moduleID, string.Format(value, args));
	}
	void Awake()
    {
		try
		{
			ModConfig<UnfairsForgottenCiphersSettings> fileSettings = new ModConfig<UnfairsForgottenCiphersSettings>("UnfairsForgottenSettings");
			ufcSettings = fileSettings.Settings;
			fileSettings.Settings = ufcSettings;
			playAltAnimOnLastSolve = ufcSettings.enableSpecificSolveAnim;
		}
		catch
		{
			Debug.LogWarningFormat("<Unfair's Cruel Revenge>: Settings do not work as intended! Using default settings!");
			playAltAnimOnLastSolve = true;
		}
	}

	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;

		modSelf.OnActivate += PrepModule;

		for (var x = 0; x < normalTextMeshes.Length; x++)
			normalTextMeshes[x].text = "";
		for (var x = 0; x < strikeTextMeshes.Length; x++)
			strikeTextMeshes[x].text = "";
		for (var x = 0; x < bigTextMeshes.Length; x++)
			bigTextMeshes[x].text = "";
		screenSelectable.OnInteract += delegate {
			if (interactable)
				HandleScreenPress();
			return false;
		};
        for (var x = 0; x < coloredButtonSelectables.Length; x++)
        {
			var y = x;
			coloredButtonSelectables[x].OnInteract += delegate {
				coloredButtonSelectables[y].AddInteractionPunch(0.5f);
				pushAnimsAll[y].AnimatePush();
				if (interactable)
					HandleColoredButtonPress(y);
				else
					pushAnimsAll[y].SetRetractState(false);
				return false;
			};
			coloredButtonSelectables[x].OnInteractEnded += delegate {
				if (interactable)
					HandleColoredButtonRelease(y);
				else
					pushAnimsAll[y].SetRetractState(true);
			};
        }
		innerSelectable.OnInteract += delegate {
			if (interactable)
				HandleInnerPress();
			pushAnimsAll.Last().AnimatePush();
			pushAnimsAll.Last().SetRetractState(false);
			return false;
		};
		innerSelectable.OnInteractEnded += delegate {
			innerSelectable.AddInteractionPunch(0.5f);
			if (interactable)
				HandleInnerRelease();
			pushAnimsAll.Last().SetRetractState(true);
		};
		outerSelectable.OnInteract += delegate {
			if (interactable)
				HandleOuterPress();
			return false;
		};
		animatorThingy.SetActive(false);
		var lossX = transform.lossyScale.x;
		foreach (var lighting in displayLights)
        {
			lighting.enabled = false;
			lighting.range *= lossX;
        }
	}

	IEnumerator FlickerColoredButtons(float delay = 0f, float rate = 0.1f)
    {
		if (delay > 0f)
			yield return new WaitForSeconds(delay);
        while (enabled)
        {
			var randomShades = Enumerable.Range(0, 4).ToArray().Shuffle();
			for (var x = 0; x < coloredButtonRenderers.Length; x++)
            {
				coloredButtonRenderers[x].material.color = new Color(randomShades[x] / 3f, randomShades[x] / 3f, randomShades[x] / 3f);
            }
			yield return new WaitForSeconds(rate);
        }
    }
	IEnumerator TypeScreenBig(string textNew, TextMesh affectedTMesh)
    {
		string[] typeSoundList = { "type_1", "type_2", "type_3" };
		for (var x = 0; x < textNew.Length + 1; x++)
        {
			affectedTMesh.text = textNew.Substring(0, x);
			mAudio.PlaySoundAtTransform(typeSoundList.PickRandom(), transform);
			yield return new WaitForSeconds(0.2f);
		}
		affectedTMesh.text = textNew;
		mAudio.PlaySoundAtTransform("line", transform);
		yield return new WaitForSeconds(0.3f);
		mAudio.PlaySoundAtTransform("line_2", transform);
	}
	IEnumerator HandleSolveAnimAlt()
    {
		var runner = StartCoroutine(FlickerColoredButtons(0f, 0.05f));
		mAudio.PlaySoundAtTransform("Toby Fox - [BIG SHOT] (Camellia Remix, Trimmed)", transform);
		for (float x = 0; x < 1f; x += Time.deltaTime * 5)
		{
			var curEase = Easing.InOutCirc(x, 0f, 1f, 1f);

			for (var p = 0; p < screenRenderers.Length; p++)
			{
				screenRenderers[p].material.color = Color.black * (1f - curEase) + (p == 0 ? Color.white : Color.red) * curEase;
			}
			yield return null;
		}
		bigTextMeshes[0].text = "[BIG]";
		bigTextMeshes[1].text = "[SOLVE]";
		bigTextMeshes[1].color = Color.red;
		strikeTextMeshes[1].text = "";
		StopCoroutine(runner);
		var lastColors = coloredButtonRenderers.Select(a => a.material.color).ToArray();
		for (float x = 0; x < 1f; x += Time.deltaTime / 4)
		{
			var curEase = Easing.InOutCirc(x, 0f, 1f, 1f);
			for (var p = 0; p < screenRenderers.Length; p++)
			{
				screenRenderers[p].material.color = Color.black * curEase + (p == 0 ? Color.white : Color.red) * (1f - curEase);
			}
			for (var p = 0; p < coloredButtonRenderers.Length; p++)
			{
				coloredButtonRenderers[p].material.color = Color.black * curEase + lastColors[p] * (1f - curEase);
			}
			yield return null;
		}
		bigTextMeshes[0].text = "";
		bigTextMeshes[1].text = "";
		bigTextMeshes[1].color = Color.white;
		foreach (var lighting in displayLights)
			lighting.enabled = false;
		yield break;
    }

	IEnumerator HandleSolveAnim()
    {
		var runner = StartCoroutine(FlickerColoredButtons(0.5f));
		mAudio.PlaySoundAtTransform("SolveSFXCMachine", transform);
		for (float x = 0; x < 1f; x += Time.deltaTime / 2)
        {
			var curEase = Easing.InOutCirc(x, 0f, 1f, 1f);

			for (var p = 0; p < screenRenderers.Length; p++)
            {
				screenRenderers[p].material.color = Color.black * (1f - curEase) + (p == 0 ? Color.white : Color.red) * curEase;
            }
			yield return null;
        }
		bigTextMeshes[0].text = "";
		strikeTextMeshes[1].text = "";
		StopCoroutine(runner);
		var lastColors = coloredButtonRenderers.Select(a => a.material.color).ToArray();
		for (float x = 0; x < 1f; x += Time.deltaTime / 2)
		{
			var curEase = Easing.InOutCirc(x, 0f, 1f, 1f);
			for (var p = 0; p < screenRenderers.Length; p++)
			{
				screenRenderers[p].material.color = Color.black * curEase + (p == 0 ? Color.white : Color.red) * (1f - curEase);
			}
			for (var p = 0; p < coloredButtonRenderers.Length; p++)
			{
				coloredButtonRenderers[p].material.color = Color.black * curEase + lastColors[p] * (1f - curEase);
			}
			yield return null;
		}
		for (var p = 0; p < screenRenderers.Length; p++)
		{
			screenRenderers[p].material.color = Color.black;
		}
		foreach (var lighting in displayLights)
			lighting.enabled = false;
		yield break;
    }
	IEnumerator HandleStartUpAnim()
    {
		DisplayCurrentPage();
		StartCoroutine(TypeScreenBig(bigTextMeshes[0].text, bigTextMeshes[0]));
		animatorThingy.SetActive(true);
		for (var x = 0; x < 4; x++)
		{
			pushAnimsAll[x].SetRetractState(x != curPageIdx);
			if (x == curPageIdx)
				pushAnimsAll[x].AnimatePush();
			coloredButtonRenderers[x].material.color = x == curPageIdx ? Color.black : Color.gray;
		}
		for (var x = 0; x < displayLights.Length; x++)
			displayLights[x].enabled = false;
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
			animatorThingy.transform.localScale = Vector3.one * Easing.InOutCirc(t, 0, 1f, 1f);
			yield return null;
        }
		particleHandler.Emit(90);
		animatorThingy.transform.localScale = Vector3.one;
		GetComponent<KMSelectable>().AddInteractionPunch(3f);
		mAudio.PlaySoundAtTransform("werraMetallicTrimmed", transform);
		for (var x = 0; x < displayLights.Length; x++)
			displayLights[x].enabled = true;
		interactable = true;
		foreach (var lighting in displayLights)
			lighting.enabled = true;

		yield break;
    }
	IEnumerator ChangeColoredButtonsAnim()
    {
		interactable = false;
		var lastColors = coloredButtonRenderers.Select(a => a.material.color).ToArray();
		if (solving)
        {
            for (float t = 0; t < 1f; t += Time.deltaTime * 10)
            {
                for (var x = 0; x < coloredButtonRenderers.Length; x++)
                {
					var newColor = Color.white * x / 3f + Color.black * ((3 - x) / 3f);
					coloredButtonRenderers[x].material.color = lastColors[x] * (1f - t) + newColor * t;
				}
				yield return null;
			}
			for (var x = 0; x < coloredButtonRenderers.Length; x++)
			{
				var newColor = Color.white * x / 3f + Color.black * ((3 - x) / 3f);
				coloredButtonRenderers[x].material.color = newColor;
			}
		}
		else
        {
			for (float t = 0; t < 1f; t += Time.deltaTime * 10)
			{
				for (var x = 0; x < coloredButtonRenderers.Length; x++)
				{
					var newColor = x == curPageIdx ? Color.black : Color.gray;
					coloredButtonRenderers[x].material.color = lastColors[x] * (1f - t) + newColor * t;
				}
				yield return null;
			}
			for (var x = 0; x < coloredButtonRenderers.Length; x++)
			{
				var newColor = Color.white * x / 3f + Color.black * ((3 - x) / 3f);
				coloredButtonRenderers[x].material.color = x == curPageIdx ? Color.black : Color.gray;
			}
		}
		interactable = true;
		yield break;
    }
	IEnumerator FlashLEDRed()
    {
        for (var x = 0; x < 6; x++)
        {
			ledRenderers3[1].material.color = x % 2 == 0 ? Color.red : Color.black;
			mAudio.PlaySoundAtTransform("wrong", transform);
			yield return new WaitForSeconds(0.1f);
        }
		yield break;
    }
	void TryChangeLEDCount(int amount)
    {
		if (ledCountQuery.Last() != amount)
			ledCountQuery.Add(amount);
    }
	void PlayRandomButtonSound(Transform anchor = null)
    {
		string[] possibleSounds = { "button1", "button2", "button3", "button4" };
		mAudio.PlaySoundAtTransform(possibleSounds.PickRandom(), anchor ?? transform);
	}
	void DisplaySubmissionHelp()
	{
		bigTextMeshes[swapTopAndRightScreens ? 1 : 0].text = inputtedWord;
		normalTextMeshes[swapTopAndRightScreens ? 0 : 1].text = string.Format("\n{0}", inputtedSequence);
		bigTextMeshes[swapTopAndRightScreens ? 0 : 1].text = "";
		normalTextMeshes[swapTopAndRightScreens ? 1 : 0].text = "";

		var sum = 0;
		for (var x = 0; x < inputtedSequence.Length; x++)
        {
			sum *= 3;
			sum += symbolRef.IndexOf(inputtedSequence[x]);
        }

		strikeTextMeshes[swapTopAndRightScreens ? 0 : 1].text = string.Format("-:{0} 0:{1} +:{2}\n",
			inputtedSequence.Length == 0 ? "A-I" : inputtedSequence.Length == 1 ? inputStringRef.Substring(9 * sum, 3) : inputStringRef.Substring(3 * sum, 1),
			inputtedSequence.Length == 0 ? "J-R" : inputtedSequence.Length == 1 ? inputStringRef.Substring(9 * sum + 3, 3) : inputStringRef.Substring(3 * sum + 1, 1),
			inputtedSequence.Length == 0 ? "S-Z" : inputtedSequence.Length == 1 ? inputStringRef.Substring(9 * sum + 6, 3) : inputStringRef.Substring(3 * sum + 2, 1));
		strikeTextMeshes[swapTopAndRightScreens ? 1 : 0].text = "";
	}
	void HandleScreenPress()
    {
		swapTopAndRightScreens ^= true;
		if (!solving)
			DisplayCurrentPage();
		else if (unlockedSubmission)
			DisplaySubmissionHelp();
	}
    void HandleOuterPress()
    {
        PlayRandomButtonSound();
        solving ^= true;
        if (solving)
        {
            for (var x = 0; x < strikeTextMeshes.Length; x++)
                strikeTextMeshes[x].text = "";
            for (var x = 0; x < bigTextMeshes.Length; x++)
                bigTextMeshes[x].text = "";
            for (var x = 0; x < normalTextMeshes.Length; x++)
                normalTextMeshes[x].text = "";
            TryChangeLEDCount(1);
            for (var x = 0; x < 4; x++)
            {
                pushAnimsAll[x].SetRetractState(true);
                //coloredButtonRenderers[x].material.color = Color.white * x / 3f + Color.black * ((3 - x) / 3f);
            }
            if (unlockedSubmission)
                DisplaySubmissionHelp();
            else
                for (var x = 0; x < normalTextMeshes.Length; x++)
                    normalTextMeshes[x].text = "WAITING";
        }
        else
        {
            inputtedSequence = "";
            inputtedWord = "";
            curPageIdx = 0;
            DisplayCurrentPage();
            for (var x = 0; x < 4; x++)
            {
                pushAnimsAll[x].SetRetractState(x != curPageIdx);
                if (x == curPageIdx)
                    pushAnimsAll[x].AnimatePush();
                //coloredButtonRenderers[x].material.color = x == curPageIdx ? Color.black : Color.gray;
            }
        }
		StartCoroutine(ChangeColoredButtonsAnim());
    }
	void HandleSubmissionUnlock()
    {
		unlockedSubmission = true;
		int timeLeftMod10 = Mathf.FloorToInt(bombInfo.GetTime() % 10);
		if (timeLeftMod10 != expectedSafeDigit && !disableStrikeOnAutosolve)
		{
			QuickLog("Submission was unlocked when the last seconds digit was {0} instead of {1}!", timeLeftMod10, expectedSafeDigit);
			StartCoroutine(FlashLEDRed());
			modSelf.HandleStrike();
		}
		else
			PlayRandomButtonSound();
		DisplaySubmissionHelp();
	}

	void HandleInnerPress()
    {
		if (solving)
        {
			if (unlockedSubmission)
			{
				if (inputtedSequence.Any())
					inputtedSequence = "";
				if (inputtedWord.EqualsIgnoreCase(decodedWords) ||
					inputtedWord.ToCharArray().SequenceEqual(decodedWords.ToCharArray()) ||
					inputtedWord == decodedWords ||
					inputtedWord.Select(a => inputStringRef.IndexOf(a)).SequenceEqual(decodedWords.Select(a => inputStringRef.IndexOf(a))) // Possible triggers for the correct word to be inputted.
					|| disableStrikeOnAutosolve)
                {
					isSolved = true;
					swapTopAndRightScreens = false;
					DisplaySubmissionHelp();
					QuickLog("Submitted the right word. Yeah. You got this.");
					modSelf.HandlePass();
					StartCoroutine(bombInfo.GetSolvedModuleIDs().Count >= bombInfo.GetSolvableModuleIDs().Count && playAltAnimOnLastSolve ? HandleSolveAnimAlt() : HandleSolveAnim());
					interactable = false;
                }
				else
                {
					QuickLog("Submitted {0}. That doesn't seem right...", inputtedWord.Any() ? string.Format("\"{0}\"",inputtedWord) : "literally nothing");
					modSelf.HandleStrike();
					mAudio.PlaySoundAtTransform("StrikeSFXColoredCipher", transform);
					solving = false;
					curPageIdx = 0;
					DisplayCurrentPage();
				}
				inputtedWord = "";
			}
			else
			{
				HandleSubmissionUnlock();
			}
		}
		else
        {
			var displayTop = normalTextMeshes[swapTopAndRightScreens ? 1 : 0];
			var displayRight = normalTextMeshes[swapTopAndRightScreens ? 0 : 1];
			switch (curPageIdx)
            {
				case 2:
					displayTop.text = idx6ColorsDisplay.Take(3).Select(a => colorRefNames[a]).Join("\n");
					displayRight.text = idx6ColorsDisplay.Skip(3).Select(a => colorRefNames[a]).Join("\n");
					break;
				case 3:
					displayTop.text = idx3ColorsDisplay.Select(a => colorRefNames[a]).Join("\n");
					displayRight.text = "";
					break;
            }
        }
    }
	void HandleInnerRelease()
    {
		if (!solving)
			DisplayCurrentPage();
	}

	void HandleColoredButtonPress(int idx)
    {
		if (solving)
        {
			if (unlockedSubmission)
			{
				switch(idx)
                {
					case 0:
						if (inputtedWord.Length < 6)
							inputtedSequence += "-";
						goto default;
					case 1:
						if (inputtedWord.Length < 6)
							inputtedSequence += "0";
						goto default;
					case 2:
						if (inputtedWord.Length < 6)
							inputtedSequence += "+";
						goto default;
					case 3:
						if (inputtedSequence != "")
							inputtedSequence = "";
						else if (inputtedWord != "")
							inputtedWord = inputtedWord.Substring(0, inputtedWord.Length - 1);
						goto default;
					default:
						if (inputtedSequence.Length >= 3)
						{
							PlayRandomButtonSound();
							var sum = 0;
							for (var x = 0; x < inputtedSequence.Length; x++)
							{
								sum *= 3;
								sum += symbolRef.IndexOf(inputtedSequence[x]);
							}
							inputtedWord += inputStringRef[sum];
							inputtedSequence = "";
						}
						else
							mAudio.PlaySoundAtTransform("KeyboardPress", coloredButtonSelectables[idx].transform);
						DisplaySubmissionHelp();
						break;
                }
			}
			else
			{
				HandleSubmissionUnlock();
			}
			pushAnimsAll[idx].SetRetractState(false);
		}
		else
        {
			mAudio.PlaySoundAtTransform("ArrowPress", coloredButtonSelectables[idx].transform);
			curPageIdx = idx;
			//Debug.LogFormat("Attempt to access page {0}", curPageIdx + 1);
			DisplayCurrentPage();
			for (var x = 0; x < 4; x++)
			{
				pushAnimsAll[x].SetRetractState(x != curPageIdx);
				coloredButtonRenderers[x].material.color = x == curPageIdx ? Color.black : Color.gray;
			}
			// Handle LED Anim here.
			TryChangeLEDCount(curPageIdx == 2 ? 6 : curPageIdx == 3 ? 3 : 1);
		}
    }
	void HandleColoredButtonRelease(int idx)
    {
		if (solving)
			pushAnimsAll[idx].SetRetractState(true);
    }

	void PrepModule()
	{
		decodedWords = CipherMachineData._allWords[6].PickRandom();
		encodingsPagesTop = new List<string>();
		encodingsPagesSide = new List<string>();
		QuickLog("Decoded Word: {0}", decodedWords);
		var outputEncodingHuffmanTree = "";
		var outputEncodingAll = "";
		var HuffmanEncodingsAll = new List<string> { "0", "1" };
		var possibleDigits = "01";

		var currentDigitString = "";
		QuickLog("Constructing Huffman Tree for Digits:");
		while (HuffmanEncodingsAll.Count < 10)
		{
			currentDigitString += possibleDigits.PickRandom();
			if (HuffmanEncodingsAll.Count(a => a.StartsWith(currentDigitString)) == 1)
			{
				outputEncodingHuffmanTree += currentDigitString;
				var idxObtained = HuffmanEncodingsAll.IndexOf(currentDigitString);
				QuickLog("Detected \"{0}\" with only 1 entry. Splitting and clearing...", currentDigitString);

				HuffmanEncodingsAll[idxObtained] += '0';
				if (idxObtained + 1 < HuffmanEncodingsAll.Count)
					HuffmanEncodingsAll.Insert(idxObtained + 1, currentDigitString + '1');
				else
					HuffmanEncodingsAll.Add(currentDigitString + '1');
				currentDigitString = "";
				QuickLog("Leafing nodes after splitting: [{0}]", HuffmanEncodingsAll.Join("],["));
			}
		}
		outputEncodingAll += outputEncodingHuffmanTree;
		QuickLog("Binary string obtained from constucting up to this point: {0}", outputEncodingHuffmanTree);
		QuickLog("Leafing nodes for digits: [{0}]", HuffmanEncodingsAll.Join("],["));
		QuickLog("Begin 3x3 Hill Cipher:");
		var digits = "0123456789";
		var matrix = new int[9];

		var attemptCount = 0;
		var det = 1;
		do
		{
			attemptCount++;
			for (var x = 0; x < 9; x++)
				matrix[x] = Random.Range(0, 26);

			det = matrix[0] * matrix[4] * matrix[8] + matrix[1] * matrix[5] * matrix[6] + matrix[2] * matrix[3] * matrix[7] -
				matrix[0] * matrix[5] * matrix[7] - matrix[1] * matrix[3] * matrix[8] - matrix[2] * matrix[4] * matrix[6];
			//Debug.Log(det);
			//Debug.Log(matrix.Join());
		}
		while (ObtainGCM(PMod(det, 26), 26) != 1);
		QuickLog("Generated 3x3 Matrix in {1} attempt{2}: {0}", matrix.Join(), attemptCount, attemptCount == 1 ? "" : "s");

		var stringedB = matrix.Select(a => a.ToString("00")).ToArray();
		var digitsEncodings = "";
		foreach (var aBString in stringedB)
		{
			foreach (var aBChr in aBString)
			{
				var idxDigit = digits.IndexOf(aBChr);
				digitsEncodings += HuffmanEncodingsAll[idxDigit];
			}
		}
		QuickLog("Encoded with the current Huffman Tree: {0}", digitsEncodings);
		outputEncodingAll += digitsEncodings;

		var resultEncryptingHill = "";
		for (var x = 0; x < decodedWords.Length / 3; x++)
		{
			var cur3Chars = decodedWords.Substring(3 * x, 3);
			var cur3AlphaPos = cur3Chars.Select(a => alphabet.IndexOf(a) + 1).ToArray();
			QuickLog("Alphabet positions of letters {0}-{1}: {2}", 3 * x + 1, 3 * x + 3, cur3AlphaPos.Join(", "));
			for (var p = 0; p < 3; p++)
			{
				var sumCurRow = Enumerable.Range(0, 3).Select(a => matrix[3 * p + a] * cur3AlphaPos[a]).Sum();
				QuickLog("{0} = {1}", Enumerable.Range(0, 3).Select(a => string.Format("{0} * {1}", matrix[3 * p + a], cur3AlphaPos[a])).Join(" + "), sumCurRow);
				QuickLog("modulo 26 = {0}", PMod(sumCurRow, alphabet.Length));
				resultEncryptingHill += alphabet[PMod(sumCurRow - 1, alphabet.Length)];
			}
		}
		QuickLog("Result after encrypting in Hill Cipher: {0}", resultEncryptingHill);

		QuickLog("Resuming Constructing Huffman Tree for Letters:");
		var nextOutputEncodingHuffmanTree = "";
		while (HuffmanEncodingsAll.Count < 26)
		{
			currentDigitString += possibleDigits.PickRandom();
			if (HuffmanEncodingsAll.Count(a => a.StartsWith(currentDigitString)) == 1)
			{
				nextOutputEncodingHuffmanTree += currentDigitString;
				var idxObtained = HuffmanEncodingsAll.IndexOf(currentDigitString);
				QuickLog("Detected \"{0}\" with only 1 entry. Splitting and clearing...", currentDigitString);

				HuffmanEncodingsAll[idxObtained] += '0';
				if (idxObtained + 1 < HuffmanEncodingsAll.Count)
					HuffmanEncodingsAll.Insert(idxObtained + 1, currentDigitString + '1');
				else
					HuffmanEncodingsAll.Add(currentDigitString + '1');

				currentDigitString = "";
				QuickLog("Leafing nodes after splitting: [{0}]", HuffmanEncodingsAll.Join("],["));
			}
		}
		QuickLog("Binary string obtained from constucting up to this point: {0}", nextOutputEncodingHuffmanTree);
		outputEncodingAll += nextOutputEncodingHuffmanTree;


		var allWordsSelected = new List<string>();
		var allWordsFromBank = CipherMachineData._allWords.Values.First().ToList();
		foreach (var listWords in CipherMachineData._allWords.Values.Skip(1))
			allWordsFromBank.AddRange(listWords);
		//var letterDistributionsAll = allWordsFromBank.Select(a => a.Distinct()).ToList();
		//var amountWordsPerLengthsAll = CipherMachineData._allWords.Values.Select(a => a.Count);
		//var distributionLengths = new[] { 3, 4, 5, 6, 7, 8 };
		attemptCount = 0;
		do
		{
			var preferredLengths = Enumerable.Range(4, 5).ToArray().Shuffle();

			allWordsSelected.Clear();
			var remainingLetters = alphabet.ToList();
			attemptCount++;
			while (remainingLetters.Any())
			{
				var bankSelected = (allWordsSelected.Count < preferredLengths.Length ? CipherMachineData._allWords[preferredLengths[allWordsSelected.Count]] : allWordsFromBank).Where(a => a.Intersect(remainingLetters).Any());
				var maxVal = bankSelected.Select(a => a.Intersect(remainingLetters).Count()).Max();
				var filteredBank = bankSelected.Where(a => a.Intersect(remainingLetters).Count() >= maxVal);
				var selectedWord = filteredBank.PickRandom();


				allWordsSelected.Add(selectedWord);
				remainingLetters.RemoveAll(a => selectedWord.Contains(a));
			}
		}
		while (allWordsSelected.Count > 9 && attemptCount < 32);
		allWordsSelected.Shuffle();

		QuickLog("Selected words after {1} attempt{2}: {0}", allWordsSelected.Join(), attemptCount, attemptCount == 1 ? "" : "s");
		var keyGenerated = allWordsSelected.Join("").Distinct().Join("");
		QuickLog("Generated Huffman Alphabet Key: {0}", keyGenerated);
		QuickLog("Leafing nodes for letters: [{0}]", HuffmanEncodingsAll.Join("],["));
		QuickLog("In Depth Huffman Key:");
		QuickLog(Enumerable.Range(0, keyGenerated.Length).Select(a => string.Format("[{0}: {1}]", HuffmanEncodingsAll[a], keyGenerated[a])).Join(", "));
		var selectedKeyword = CipherMachineData._allWords[Random.Range(4, 9)].PickRandom();
		var validLetters = alphabet.Where(a => !(CipherMachineData._allWords.ContainsKey(selectedKeyword.Length + 1) && CipherMachineData._allWords[selectedKeyword.Length + 1].Contains(selectedKeyword + a)));
		var selectedDecoyLetter = validLetters.PickRandom();
		QuickLog("Generated Keyword: {0}, Decoy Letter: {1}", selectedKeyword, selectedDecoyLetter);
		var binaryEncodedLetters = selectedKeyword.Select(a => HuffmanEncodingsAll[keyGenerated.IndexOf(a)]).Join("");
		var repeatDecoyCount = Random.Range(3, 6);
		for (var x = 0; x < repeatDecoyCount; x++)
			binaryEncodedLetters += HuffmanEncodingsAll[keyGenerated.IndexOf(selectedDecoyLetter)];
		QuickLog("Encoded With Keystring and Huffman Tree: {0}", binaryEncodedLetters);
		outputEncodingAll += binaryEncodedLetters;
		QuickLog("Final binary string: {0}", outputEncodingAll);
		Dictionary<string, string> encodingSubstitutions = new Dictionary<string, string>
		{
			{ "0", "A" },
			{ "1", "B" },
			{ "AA", "C" },
			{ "BA", "D" },
			{ "AB", "E" },
			{ "BB", "F" },
			{ "CA", "G" },
			{ "CB", "H" },
			{ "DA", "I" },
			{ "DB", "J" },
			{ "EA", "K" },
			{ "EB", "L" },
			{ "FA", "M" },
			{ "FB", "N" },
			{ "AG", "O" },
			{ "BG", "P" },
			{ "AH", "Q" },
			{ "BH", "R" },
			{ "AI", "S" },
			{ "BI", "T" },
			{ "AJ", "U" },
			{ "BJ", "V" },
			{ "AK", "W" },
			{ "BK", "X" },
			{ "AL", "Y" },
			{ "BL", "Z" },
		}; // The giant substitution key used for encoding. 

		var finalEncodings = outputEncodingAll.ToString();
		foreach (var encodeSub in encodingSubstitutions)
		{
			finalEncodings = finalEncodings.Replace(encodeSub.Key, encodeSub.Value);
		}
		QuickLog("Substituted Encodings: {0}", finalEncodings);
		QuickLog("Begin RGBA Decomposition Cipher:");

		// Original order is RGB. Corresponding to 0, 1, 2
		var rgbOrderObtained = Enumerable.Range(0, 3).ToArray().Shuffle(); //new[] { 2, 1, 0 }; 
		var orderColorsObtained = new int[27];
		for (var x = 0; x < 27; x++)
        {
			var colorRefIdxGrouping = new[] { x % 3, x / 3 % 3, x / 9 % 3 };
            var rearrangedColors = Enumerable.Range(0, 3).OrderBy(a => rgbOrderObtained[a]).Select(a => colorRefIdxGrouping[a]);
			var factor = 1;
			var sum = 0;
			for (var y = 0; y < 3; y++)
            {
				sum += factor * rearrangedColors.ElementAt(y);
				factor *= 3;
            }
			orderColorsObtained[x] = sum;
			//Debug.Log(sum);
        }

		//Debug.Log(rgbOrderObtained.Join());
		//Debug.Log(orderColorsObtained.Join());

		// To debug the condensed order.
		var debugValues = new int[3];
		for (var y = 0; y < 3; y++)
		{
			var factor = 2;
			for (var a = 0; a < rgbOrderObtained[y]; a++)
				factor *= 3;
			debugValues[y] = factor;
		}
		QuickLog("Condensed order (Least to most significant): {0}", debugValues.Select(a => colorRefNames[a]).Join(", "));
		QuickLog("Full order obtained of arranged colors: {0}", orderColorsObtained.Select(a => colorRefNames[a]).Join(", "));

		displayModuleIDText = Random.value < 0.5f;
		QuickLog("{0} \"MODULE ID\" on the first page.", displayModuleIDText ? "Displaying" : "Not displaying");
		var keyStringRGBABase = selectedKeyword.Distinct().Join("");
		keyStringRGBABase = displayModuleIDText
            ? keyStringRGBABase + alphabet.Except(selectedKeyword).Join("")
            : alphabet.Except(selectedKeyword).Join("") + keyStringRGBABase;
		//keyStringRGBABase = keyStringRGBABase.Substring(0, idxVoid) + "-" + keyStringRGBABase.Substring(idxVoid);
        
		var finalKeystring = "";
		var idxCurKeystring = 0;
		var idxVoid = Random.Range(0, 27);
		QuickLog("Color to skip over when applying the alphabet: {0}", colorRefNames[idxVoid]);
        for (var x = 0; x < 27; x++)
        {
			if (orderColorsObtained[x] == idxVoid)
				finalKeystring += "-";
			else
            {
				finalKeystring += keyStringRGBABase[idxCurKeystring];
				idxCurKeystring++;
			}
        }
		QuickLog("In depth keystring to color: {0}", Enumerable.Range(0, 27).Select(a => string.Format("[{0}: {1}]", finalKeystring[a], colorRefNames[orderColorsObtained[a]])).Join(", "));

		var idxesOutputColors = resultEncryptingHill.Select(a => orderColorsObtained[finalKeystring.IndexOf(a)]).ToArray();
        var selectedOperatorsAll = operatorTableTernary.Keys.ToArray().Shuffle().Take(Random.Range(1, 4)).ToArray();

		attemptCount = 0;

		retryRGBA:
		attemptCount++;
		idx6ColorsDisplay = new int[6];
		var idx6ColorsToLetters = new int[6];
        for (var x = 0; x < idx6ColorsDisplay.Length; x++)
        {
			var twoInputIdxes = new int[2];
			var curDecomposedIdx = new[] { idxesOutputColors[x] % 3, idxesOutputColors[x] / 3 % 3, idxesOutputColors[x] / 9 % 3 };
			var curOperatorIdx = selectedOperatorsAll[x % selectedOperatorsAll.Length];
			for (var p = 0; p < 2; p++)
				twoInputIdxes[p] = 0;
			for (var chn = 0; chn < 3; chn++)
			{
				var possibleCombinationsForValueChannel = Enumerable.Range(0, 9).Select(a => new[] { a % 3, a / 3 })
					.Where(a => operatorTableTernary[curOperatorIdx][a.First(), a.Last()] == curDecomposedIdx[chn]);
				var randomlyPickedCombination = possibleCombinationsForValueChannel.PickRandom();
				var factor = Mathf.Pow(3, chn);
				twoInputIdxes[0] += (int)(randomlyPickedCombination.First() * factor);
				twoInputIdxes[1] += (int)(randomlyPickedCombination.Last() * factor);
			}
			twoInputIdxes.Shuffle();
			idx6ColorsDisplay[x] = twoInputIdxes.First();
			idx6ColorsToLetters[x] = twoInputIdxes.Last();
		}
		

		/*
		 * Base:
		 * 0 1 2
		 * 3 4
		 * 5
		 * 
		 * Flip Rows Vertically:
		 * 2 1 0
		 * 4 3
		 * 5
		 * 
		 * Flip Columns Horizontally:
		 * 5 4 2
		 * 3 1
		 * 0
		 * 
		 * Swap Rows and Columns:
		 * 0 3 5
		 * 1 4
		 * 2
		 */
		var diagonalFlipIdxes = new[] { 0, 3, 5, 1, 4, 2 };
		var horizontalFlipIdxes = new[] { 5, 4, 2, 3, 1, 0 };
		var verticalFlipIdxes = new[] { 2, 1, 0, 4, 3, 5 };
		var outputtingIdxesAfterTransform = new int[6];
		var transformsAllChannels = new List<List<int>>();
		foreach (var chn in transformsAllChannels)
			chn.Clear();
		transformsAllChannels.Clear();

		var encodingRChannels = idx6ColorsToLetters.Select(a => a % 3).ToArray();
		var encodingGChannels = idx6ColorsToLetters.Select(a => a / 3 % 3).ToArray();
		var encodingBChannels = idx6ColorsToLetters.Select(a => a / 9 % 3).ToArray();


        for (var x = 0; x < 3; x++)
        {
			var selectedTransforms = Enumerable.Range(0, 4).ToList();
			var pickedTransformations = Random.Range(0, 5);
			selectedTransforms.Shuffle();
			selectedTransforms = selectedTransforms.Take(pickedTransformations).ToList();
			transformsAllChannels.Add(selectedTransforms);
        }
		// Transform R channel
        for (var r = 0; r < transformsAllChannels[0].Count; r++)
        {
			switch (transformsAllChannels[0][transformsAllChannels[0].Count - 1 - r])
            {
				case 0:
					encodingRChannels = verticalFlipIdxes.Select(a => encodingRChannels[a]).ToArray();
					break;
				case 1:
					encodingRChannels = horizontalFlipIdxes.Select(a => encodingRChannels[a]).ToArray();
					break;
				case 2:
					encodingRChannels = diagonalFlipIdxes.Select(a => encodingRChannels[a]).ToArray();
					break;
				case 3:
					encodingRChannels = encodingRChannels.Select(a => 2 - a).ToArray();
					break;
				default:
					break;
            }
        }
		// Transform G channel
		for (var g = 0; g < transformsAllChannels[1].Count; g++)
        {
			switch (transformsAllChannels[1][transformsAllChannels[1].Count - 1 - g])
            {
				case 0:
					encodingGChannels = verticalFlipIdxes.Select(a => encodingGChannels[a]).ToArray();
					break;
				case 1:
					encodingGChannels = horizontalFlipIdxes.Select(a => encodingGChannels[a]).ToArray();
					break;
				case 2:
					encodingGChannels = diagonalFlipIdxes.Select(a => encodingGChannels[a]).ToArray();
					break;
				case 3:
					encodingGChannels = encodingGChannels.Select(a => 2 - a).ToArray();
					break;
				default:
					break;
            }
        }
		// Transform B channel
		for (var b = 0; b < transformsAllChannels[2].Count; b++)
        {
			switch (transformsAllChannels[2][transformsAllChannels[2].Count - 1 - b])
            {
				case 0:
					encodingBChannels = verticalFlipIdxes.Select(a => encodingBChannels[a]).ToArray();
					break;
				case 1:
					encodingBChannels = horizontalFlipIdxes.Select(a => encodingBChannels[a]).ToArray();
					break;
				case 2:
					encodingBChannels = diagonalFlipIdxes.Select(a => encodingBChannels[a]).ToArray();
					break;
				case 3:
					encodingBChannels = encodingBChannels.Select(a => 2 - a).ToArray();
					break;
				default:
					break;
            }
        }

		for (var x = 0; x < outputtingIdxesAfterTransform.Length; x++)
			outputtingIdxesAfterTransform[x] = encodingRChannels[x] + encodingGChannels[x] * 3 + encodingBChannels[x] * 9;

		if (outputtingIdxesAfterTransform.Contains(idxVoid))
			goto retryRGBA;

		var fullEncodedString = outputtingIdxesAfterTransform.Select(a => finalKeystring[orderColorsObtained.IndexOf(b => b == a)]).Join("");
		var transformationIds = new[] { "TL", "TR", "BL", "BR" };

		QuickLog("{0} attempt{1} taken to attempt to finalize encoding RGBA Decomposition Cipher.", attemptCount, attemptCount == 1 ? "" : "s");
		QuickLog("Selected operators: {0}", selectedOperatorsAll.Join(", "));
		QuickLog("Selected colors to display on the module: {0}", idx6ColorsDisplay.Select(a => colorRefNames[a]).Join(", "));
		QuickLog("Selected colors to convert in order: {0}", idx6ColorsToLetters.Select(a => colorRefNames[a]).Join(", "));

		QuickLog("R Channel Transformations: {0}", transformsAllChannels[0].Any() ? transformsAllChannels[0].Select(a => transformationIds[a]).Join(", ") : "NONE");
		QuickLog("G Channel Transformations: {0}", transformsAllChannels[1].Any() ? transformsAllChannels[1].Select(a => transformationIds[a]).Join(", ") : "NONE");
		QuickLog("B Channel Transformations: {0}", transformsAllChannels[2].Any() ? transformsAllChannels[2].Select(a => transformationIds[a]).Join(", ") : "NONE");

		QuickLog("Final colors: {0}", outputtingIdxesAfterTransform.Select(a => colorRefNames[a]).Join(", "));

		QuickLog("Full Encryption: {0}", fullEncodedString);


		// Section where you fit stuff onto the module for all of the pages, and display everything.
		// Page 1
		encodingsPagesTop.Add(fullEncodedString);
		encodingsPagesSide.Add("");
		// Page 2
		var portionsEncodingString = new List<string>();
		var encodingStringToDisplay = finalEncodings.ToString();
		while (encodingStringToDisplay.Length > 0)
        {
			var snippet = encodingStringToDisplay.Substring(0, Mathf.Min(encodingStringToDisplay.Length, 17));
			portionsEncodingString.Add(snippet);
			encodingStringToDisplay = encodingStringToDisplay.Length > 17 ? encodingStringToDisplay.Substring(17) : "";
        }
		encodingsPagesTop.Add(portionsEncodingString.Take(3).Join("\n"));
		encodingsPagesSide.Add(portionsEncodingString.Skip(3).Join("\n"));
		// Page 3
		if (allWordsSelected.Count > 6)
		{
			var combinedKeywords = new List<string>();
			var x = 0;
			while (x < allWordsSelected.Count - 1)
			{
				if (allWordsSelected[x].Length + 1 + allWordsSelected[x + 1].Length < 17)
				{
					combinedKeywords.Add(string.Format("{0} {1}", allWordsSelected[x], allWordsSelected[x + 1]));
					x++;
				}
				else
					combinedKeywords.Add(allWordsSelected[x]);
				x++;
			}
			if (x < allWordsSelected.Count)
				combinedKeywords.Add(allWordsSelected[allWordsSelected.Count - 1]);
			encodingsPagesTop.Add(combinedKeywords.Take(3).Join("\n"));
			encodingsPagesSide.Add(combinedKeywords.Skip(3).Join("\n"));
		}
		else
        {
			encodingsPagesTop.Add(allWordsSelected.Take(3).Join("\n"));
			encodingsPagesSide.Add(allWordsSelected.Skip(3).Join("\n"));
		}
		// Page 4
		idx3ColorsDisplay = new int[3];
		var decomposedIdxVoid = new[] { idxVoid % 3, idxVoid / 3 % 3, idxVoid / 9 % 3 };
        for (var x = 0; x < 3; x++)
        {
			var curChnIdx = rgbOrderObtained[x];
			var chnValueIdx = decomposedIdxVoid[curChnIdx];
			var remainingRGBValues = decomposedIdxVoid[curChnIdx] == 2 ? 1 : 0;
			var focusedRGBValue = decomposedIdxVoid[curChnIdx] == 0 ? 1 : 2;

			for (int c = 1, p = 0 ; c < 27; c *= 3, p++)
				idx3ColorsDisplay[x] += p == curChnIdx ? c * focusedRGBValue : c * remainingRGBValues;
			
		}
		var possibleDescriptions = new Dictionary<char, string[]> {
			{ '+', new[] { "Add Symbol", "Plus Sign", "Plus Symbol", "Symbol For Adding", } },
			{ '×', new[] { "Multiply Symbol", "x, literally", "Times Symbol", "Multiply Sign", } },
			{ '○', new[] { "Circle", "o, literally", "Unfilled Circle", "Lowercase O", } },
			{ 'm', new[] { "Lowercase M", "M but Lowercase", "Lowercase Mike", } },
			{ 'M', new[] { "Uppercase M", "M but Uppercase", "Uppercase Mike", } },
			{ '∅', new[] { "Slashed Circle", "/ Through O", "Empty Set Symbol", "O and /", } },
		};
		var portCountMod3 = bombInfo.GetPortCount() % 3;

		var labeledTransforms = transformsAllChannels.Select(a => a.Any() ? a.Select(b => transformationIds[b]).Join(" ") : "NONE");

		encodingsPagesTop.Add(labeledTransforms.Skip(portCountMod3).Concat(labeledTransforms.Take(portCountMod3)).Join("\n"));
		encodingsPagesSide.Add(selectedOperatorsAll.Select(a => possibleDescriptions[a].PickRandom()).Join("\n"));


		// Section where you calculate the time to safely enter submission.
		QuickLog("Safe Digit Calculations:");
		expectedSafeDigit = bombInfo.GetSerialNumberNumbers().LastOrDefault() + moduleID;
		QuickLog("The last digit of the serial number is {0}, adding {1} to the module ID", bombInfo.GetSerialNumberNumbers().LastOrDefault(), moduleID);
		var allUnlitIndicatorsAsString = bombInfo.GetOffIndicators().Join("").ToUpperInvariant();
		var alphabetBeforeN = "ABCDEFGHIJKLM";
		expectedSafeDigit += allUnlitIndicatorsAsString.Count(a => alphabetBeforeN.Contains(a));
		QuickLog("From all of the letters in the unlit indicators, {0} of those come before the letter \"N.\"", allUnlitIndicatorsAsString.Count(a => alphabetBeforeN.Contains(a)));
		idxStrikeDisplay = Random.Range(0, 3);
		switch (idxStrikeDisplay)
        {
			case 0: // #
			default:
				expectedSafeDigit += bombInfo.GetBatteryHolderCount() + bombInfo.GetPortPlateCount();
				QuickLog("\"STRIKE(S)\" is not present, adding {0} and {1}.", bombInfo.GetBatteryHolderCount(), bombInfo.GetPortPlateCount());
				break;
			case 1: // # STRIKES
				var allLitIndicatorsAsString = bombInfo.GetOnIndicators().Join("").ToUpperInvariant();
				expectedSafeDigit += allLitIndicatorsAsString.Count(a => alphabetBeforeN.Contains(a));
				QuickLog("\"STRIKE(S)\" is present and appended after the digit, adding {0}.", allLitIndicatorsAsString.Count(a => alphabetBeforeN.Contains(a)));
				break;
			case 2: // STRIKES: #
				expectedSafeDigit += bombInfo.GetBatteryCount();
				QuickLog("\"STRIKE(S)\" is present and prepended before the digit, adding {0}.", bombInfo.GetBatteryCount());
				break;
        }
		QuickLog("Detected this many modules with \"FAIR\" in its name: {0}.", bombInfo.GetModuleNames().Count(a => a.ToUpperInvariant().Contains("FAIR")));
		expectedSafeDigit -= 3 * bombInfo.GetModuleNames().Count(a => a.ToUpperInvariant().Contains("FAIR"));
		expectedSafeDigit = PMod(expectedSafeDigit + 3, 10);
		QuickLog("Final Safe Digit: {0}", expectedSafeDigit);
		ledCountQuery.Add(1);
		isReady = true;
		StartCoroutine(HandleStartUpAnim());
	}
	int PMod(int dividend, int divisor)
    {
		return ((dividend % divisor) + divisor) % divisor;
    }
	void DisplayCurrentPage()
    {
		DisplayPage(curPageIdx, swapTopAndRightScreens);
    }
	IEnumerator HandleWaitLEDTransfer(int ledCount = 1)
    {
		yield return null;
		yield return new WaitWhile(indicatorCore.GetPlayingAnimBool);
		for (var x = 0; x < ledRenderers3.Length; x++)
			ledRenderers3[x].material.color = Color.black;
		for (var x = 0; x < ledRenderers6.Length; x++)
			ledRenderers6[x].material.color = Color.black;
		StartCoroutine(indicatorCore.HandleIndicatorModification(ledCount));
		yield return null;
		yield return new WaitWhile(indicatorCore.GetPlayingAnimBool);
		for (var x = 0; x < ledRenderers3.Length && ledCount == 3; x++)
			ledRenderers3[x].material.color = colorRefListsAll[idx3ColorsDisplay[x]];
		for (var x = 0; x < ledRenderers6.Length && ledCount == 6; x++)
			ledRenderers6[x].material.color = colorRefListsAll[idx6ColorsDisplay[x]];

	}


	void DisplayPage(int pageIdx, bool swapDisplays)
    {
		var displayTop = normalTextMeshes[swapDisplays ? 1 : 0];
		var displayRight = normalTextMeshes[swapDisplays ? 0 : 1];
		var displayStrikeTextTop = strikeTextMeshes[swapDisplays ? 1 : 0];
		var displayStrikeTextSide = strikeTextMeshes[swapDisplays ? 0 : 1];
		var BigDisplayTop = bigTextMeshes[swapDisplays ? 1 : 0];
		var BigDisplayRight = bigTextMeshes[swapDisplays ? 0 : 1];

		displayTop.text = "";
		displayRight.text = "";
		displayStrikeTextTop.text = "";
		displayStrikeTextSide.text = "";
		BigDisplayTop.text = "";
		BigDisplayRight.text = "";

		switch (pageIdx)
        {
			case 0:
                {
					BigDisplayTop.text = encodingsPagesTop[0];
					displayRight.text = string.Format("{0}{1}\n", displayModuleIDText ? "MODULE ID: " : "", moduleID);
					displayStrikeTextSide.text = string.Format("\n{0}{1}{2}", idxStrikeDisplay == 2 ? "STRIKE(S): " : "", bombInfo.GetStrikes(),idxStrikeDisplay == 1 ? " STRIKE(S)" : "");
				}
				break;
            default:
                {
					displayTop.text = encodingsPagesTop[pageIdx];
					displayRight.text = encodingsPagesSide[pageIdx];
				}
				break;
        }

	}


	int ObtainGCM(int firstValue, int secondValue)
    {
		var maxVal = firstValue < secondValue ? secondValue : firstValue;
		var minVal = firstValue < secondValue ? firstValue : secondValue;
		while (minVal > 0)
        {
			var newMin = maxVal % minVal;
			maxVal = minVal;
			minVal = newMin;
        }
		return maxVal;
    }

	float curDelay = 1f;
	// Update is called once per frame
	void Update () {
		if (isReady && !isSolved)
        {
			if (!indicatorCore.GetPlayingAnimBool())
			{
				if (curDelay > 0f)
					curDelay -= Time.deltaTime;
				else
				{
					if (ledCountQuery.Count > 1)
                    {
						ledCountQuery.RemoveAt(0);
						StartCoroutine(HandleWaitLEDTransfer(ledCountQuery.First()));
						curDelay = 1f;
                    }
				}
			}
			else
				curDelay = 1f;
			if (curPageIdx == 0 && !solving)
            {
				var displayStrikeTextTop = strikeTextMeshes[swapTopAndRightScreens ? 1 : 0];
				var displayStrikeTextSide = strikeTextMeshes[swapTopAndRightScreens ? 0 : 1];
				displayStrikeTextTop.text = "";
				displayStrikeTextSide.text = string.Format("\n{0}{1}{2}", idxStrikeDisplay == 2 ? "STRIKE(S): " : "", bombInfo.GetStrikes(), idxStrikeDisplay == 1 ? " STRIKE(S)" : "");
			}
        }
	}
	string FormatSecondsToTime(long num)
	{
		return string.Format("{0}:{1}", num / 60, (num % 60).ToString("00"));
	}
	bool TimeModeActive;
#pragma warning disable IDE0051 // Remove unused private members
	bool ZenModeActive;
	readonly string TwitchHelpMessage = "Select the given button with \"!{0} press L(eft);U(p);R(ight);D(own);Inner;Outer\" " +
		"To time a specific press, specify based only on seconds digits (##), full time stamp (DD:HH:MM:SS), or MM:SS where MM exceeds 99 min. " +
		"To press the idx/strike screen \"!{0} screen\" Semicolons can be used to combine presses, both untimed and timed. Inspect the displayed LED colors with \"!{0} inspect\"";
#pragma warning restore IDE0051 // Remove unused private members
	IEnumerator ProcessTwitchCommand(string command)
	{
		if (!isReady)
		{
			yield return "sendtochaterror The module has not activated yet. Wait for a bit until the module has started.";
			yield break;
		}
		string[] intereptedParts = command.ToLower().Split(';');
		int[] multiplierTimes = { 1, 60, 3600, 86400 }; // To denote seconds, minutes, hours, days in seconds.
		List<KMSelectable> selectedCommands = new List<KMSelectable>();
		List<List<long>> timeThresholds = new List<List<long>>();
		Match inspectCommand = Regex.Match(command, @"^inspect$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (inspectCommand.Success)
        {
			if (solving)
            {
				yield return "sendtochaterror You cannot inspect while the module in submission. Exit out of submission first before inspecting.";
				yield break;
            }
			yield return null;
			innerSelectable.OnInteract();
			yield return new WaitForSeconds(3f);
			innerSelectable.OnInteractEnded();
			yield break;
        }
		
		foreach (string commandPart in intereptedParts)
		{
			string partTrimmed = commandPart.Trim();
			if (partTrimmed.RegexMatch(@"^press "))
				partTrimmed = partTrimmed.Substring(5).Trim();

			string[] partOfPartTrimmed = partTrimmed.Split();
			if (partTrimmed.RegexMatch(@"^(u(p)?|r(ight)?|d(own)?|l(eft)?|inner|outer|screen)( (at|on))?( [0-9]+:([0-5][0-9]:){0,2}[0-5][0-9])+$"))
			{
				List<long> possibleTimes = new List<long>();
				for (int x = partOfPartTrimmed.Length - 1; x > 0; x--)
				{
					if (!partOfPartTrimmed[x].RegexMatch(@"^[0-9]+:([0-5][0-9]:){0,2}[0-5][0-9]$")) break;
					string[] curTimePart = partOfPartTrimmed[x].Split(':').Reverse().ToArray();
					long curTime = 0;
					for (int idx = 0; idx < curTimePart.Length; idx++)
					{
						long possibleModifier;
						if (!long.TryParse(curTimePart[idx], out possibleModifier))
						{
							yield return string.Format("sendtochaterror The command part \"{0}\" contains an uncalculatable time. The full command has been voided.", partTrimmed);
							yield break;
						}
						curTime += multiplierTimes[idx] * possibleModifier;
					}
					possibleTimes.Add(curTime);
				}

				possibleTimes = possibleTimes.Where(a => ZenModeActive ? a > bombInfo.GetTime() : a < bombInfo.GetTime()).ToList(); // Filter out all possible times by checking if the time stamp is possible

				if (possibleTimes.Any())
				{
					timeThresholds.Add(possibleTimes);
				}
				else
				{
					yield return string.Format("sendtochaterror The command part \"{0}\" gave no accessible times for this module. The full command has been voided.", partTrimmed);
					yield break;
				}
			}
			else if (partTrimmed.RegexMatch(@"^(u(p)?|r(ight)?|d(own)?|l(eft)?|inner|outer|screen)( (at|on))?( [0-5][0-9])+$"))
			{

				List<long> possibleTimes = new List<long>();
				for (int idx = partOfPartTrimmed.Length - 1; idx > 0; idx--)
				{
					if (!partOfPartTrimmed[idx].RegexMatch(@"^[0-5][0-9]$")) break;
					int secondsTime = int.Parse(partOfPartTrimmed[idx]);
					long curMinRemaining = (long)bombInfo.GetTime() / 60;
					for (long x = curMinRemaining - (ZenModeActive ? 0 : 2); x <= curMinRemaining + (ZenModeActive ? 2 : 0); x++)
					{
						if (x * 60 + secondsTime > bombInfo.GetTime() && ZenModeActive)
						{
							possibleTimes.Add(x * 60 + secondsTime);
						}
						else if (x * 60 + secondsTime < bombInfo.GetTime() && !ZenModeActive)
						{
							possibleTimes.Add(x * 60 + secondsTime);
						}
					}

				}
				if (possibleTimes.Any())
				{
					timeThresholds.Add(possibleTimes);
				}
				else
				{
					yield return string.Format("sendtochaterror The command part \"{0}\" gave no accessible times for this module. The full command has been voided.", partTrimmed);
					yield break;
				}
			}
			else if (partTrimmed.RegexMatch(@"^(u(p)?|r(ight)?|d(own)?|l(eft)?|inner|outer|screen)$"))
			{
				timeThresholds.Add(new List<long>());
			}
			else
			{
				yield return string.Format("sendtochaterror \"{0}\" is not a valid sub command, check your command for typos.", partTrimmed);
				yield break;
			}
			switch (partOfPartTrimmed[0])
			{
				case "l":
				case "left":
					selectedCommands.Add(coloredButtonSelectables[0]);
					break;
				case "u":
				case "up":
					selectedCommands.Add(coloredButtonSelectables[1]);
					break;
				case "r":
				case "right":
					selectedCommands.Add(coloredButtonSelectables[2]);
					break;
				case "d":
				case "down":
					selectedCommands.Add(coloredButtonSelectables[3]);
					break;
				case "inner":
					selectedCommands.Add(innerSelectable);
					break;
				case "outer":
					selectedCommands.Add(outerSelectable);
					break;
				case "screen":
					selectedCommands.Add(screenSelectable);
					break;
				default:
					yield return "sendtochaterror You aren't supposed to get this error. If you did, it's a bug, so please contact the developer about this.";
					yield break;
			}
		}
		if (selectedCommands.Any())
		{
			yield return "multiple strikes";
			var breakLoop = false;
			for (int x = 0; x < selectedCommands.Count && !breakLoop; x++)
			{
				yield return null;
				while (!interactable)
                {
					yield return string.Format("trycancel The press for button #{0} in the command was canceled!", x + 1);
                }
				if (timeThresholds[x].Any())
				{

					List<long> currentTimeThresholds = timeThresholds[x].Where(a => ZenModeActive ? a > bombInfo.GetTime() : a < bombInfo.GetTime()).ToList();
					if (!currentTimeThresholds.Any())
					{
						yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0} in the command that was sent.", x + 1);
						yield break;
					}
					long targetTime = ZenModeActive ? currentTimeThresholds.Min() : currentTimeThresholds.Max();
					yield return string.Format("sendtochat Target time for press #{0} in command: {1}", x + 1, FormatSecondsToTime(targetTime));
					bool canPlayWaitingMusic = Mathf.Abs(targetTime - bombInfo.GetTime()) >= 25;
					if (canPlayWaitingMusic)
					{
						yield return "waiting music";
						yield return "sendtochat This press will take a while, if you wish to cancel this command, do \"!cancel\" now.";
					}
					do
					{
						yield return string.Format("trycancel Your timed interation has been canceled after a total of {0}/{1} presses in the command.", x + 1, selectedCommands.Count);
						if ((long)bombInfo.GetTime() > targetTime && ZenModeActive)
						{
							currentTimeThresholds = currentTimeThresholds.Where(a => a > bombInfo.GetTime()).ToList();
							if (!currentTimeThresholds.Any())
							{
								yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0} in the command that was sent.", x + 1);
								yield break;
							}
							targetTime = currentTimeThresholds.Min();
							yield return string.Format("sendtochat Your timed interation has been altered. The new time is now {1} for press #{0} in the command that was sent.", x + 1, FormatSecondsToTime(targetTime));
						}
						else if ((long)bombInfo.GetTime() < targetTime && !ZenModeActive)
						{
							currentTimeThresholds = currentTimeThresholds.Where(a => a < bombInfo.GetTime()).ToList();
							if (!currentTimeThresholds.Any())
							{
								yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0} in the command that was sent.", x + 1);
								yield break;
							}
							targetTime = currentTimeThresholds.Max();
							yield return string.Format("sendtochat Your timed interation has been altered. The new time is now {1} for press #{0} in the command that was sent.", x + 1, FormatSecondsToTime(targetTime));
						}
					}
					while ((long)bombInfo.GetTime() != targetTime);
					if (canPlayWaitingMusic)
						yield return "end waiting music";
				}

				var buttonPressed = selectedCommands[x] == innerSelectable ? "Inner" :
					selectedCommands[x] == outerSelectable ? "Outer" :
					coloredButtonSelectables.Contains(selectedCommands[x]) ? new[] { "Left", "Up", "Right", "Down" }[coloredButtonSelectables.IndexOf(a => a == selectedCommands[x])] : "???";
				if (solving && ((!unlockedSubmission && (int)bombInfo.GetTime() % 10 != expectedSafeDigit) || (buttonPressed == "Inner" && decodedWords != inputtedWord)) && selectedCommands.Count > 1 && buttonPressed != "???")
				{
					yield return string.Format("strikemessage incorrectly pressing {0} on {1} after {2} press{3} in the TP command specified!", buttonPressed == "Inner" ? "Inner Center" : buttonPressed == "Outer" ? "Outer Center" : buttonPressed, bombInfo.GetFormattedTime(), x + 1, x == 0 ? "" : "es");
					breakLoop = true;
				}
				selectedCommands[x].OnInteract();
				if (selectedCommands[x] != outerSelectable && selectedCommands[x] != screenSelectable)
					selectedCommands[x].OnInteractEnded();
				yield return new WaitForSeconds(0.1f);
			}
			yield return "end multiple strikes";
		}
		yield break;
	}
	IEnumerator TwitchHandleForcedSolve()
    {
		disableStrikeOnAutosolve = true;
		QuickLog("Enabling autosolve. Disabling strike mechanics.");
		while (!isSolved)
        {
			while (!interactable)
				yield return true;
			yield return null;
			if (!solving)
				outerSelectable.OnInteract();
			else
            {
				if (unlockedSubmission)
                {
					var idxesLetters = decodedWords.Select(a => inputStringRef.IndexOf(a)).ToArray();
					foreach (var idxL in idxesLetters)
                    {
						var curVal = idxL;
						var idxes3 = new List<int>();
                        for (var x = 0; x < 3; x++)
                        {
							idxes3.Add(curVal % 3);
							curVal /= 3;
                        }
						idxes3.Reverse();
						foreach (int value in idxes3)
						{
							yield return new WaitForSeconds(0.1f);
							coloredButtonSelectables[value].OnInteract();
							coloredButtonSelectables[value].OnInteractEnded();
						}
					}
					yield return new WaitForSeconds(0.1f);
					innerSelectable.OnInteract();
					innerSelectable.OnInteractEnded();
				}
				else
                {
					while (Mathf.FloorToInt(bombInfo.GetTime() % 10) != expectedSafeDigit)
						yield return true;
					innerSelectable.OnInteract();
					innerSelectable.OnInteractEnded();
                }
            }
			yield return new WaitForSeconds(0.1f);
		}

		yield break;
    }

}
