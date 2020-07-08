﻿using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uernd = UnityEngine.Random;
public class UnfairsRevengeHandler : MonoBehaviour {

	public KMBombInfo bombInfo;
	public KMAudio mAudio;
	public KMBombModule modSelf;
	public KMSelectable[] colorButtonSelectables;
	public KMSelectable innerSelectable, outerSelectable, idxStrikeSelectable;
	public GameObject[] colorButtonObjects;
	public GameObject innerRing, entireCircle;
	public MeshRenderer[] colorButtonRenderers, statusIndicators;
	public TextMesh pigpenDisplay, strikeIDDisplay, mainDisplay;
	public Light[] colorLights;
	public Light centerLight;
	public ParticleSystem particles;
	public IndicatorCoreHandler indicatorHandler;
	//public Material[] switchableMats = new Material[2];

	private string[]
		normalModeInstructions = { "PCR", "PCG", "PCB", "SCC", "SCM", "SCY", "SUB", "MIT", "PRN", "CHK", "BOB", "REP", "EAT", "STR", "IKE", "SIG", "OPP", "PVP", "NXP", "PVS", "NXS" },
		hardModeInstructions = { "PCR", "PCG", "PCB", "SCC", "SCM", "SCY", "SUB", "PVP", "NXP", "PVS", "NXS", "REP", "EAT", "STR", "IKE", "CHK", "MIT", "OPP", "SIG" },
		baseColorList = new[] { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta" },
		primaryList = { "Red", "Green", "Blue", };
	private string baseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Base alphabet for code assumes A=1,B=2,...,Y=25,Z=26
	private string[,] keyBTable = {
			{ "ABDA", "FEV", "DBHC", "BLD", "DBIE", "AFEF", "AFCG", "CQH", "DEAI", "FEAA", "EFAB", "DECC" },
			{ "ABDB", "FEW", "DBHD", "BLE", "DBIF", "AFEG", "AFCH", "CQI", "DEAA", "FEAB", "EFAC", "DECD" },
			{ "ABDC", "FEX", "DBHE", "BLF", "DBIG", "AFEH", "AFCI", "CQA", "DEAB", "FEAC", "EFAD", "DECE" },
			{ "ABDD", "FEY", "DBHF", "BLG", "DBIH", "AFEI", "AFCA", "CQB", "DEAC", "FEAD", "EFAE", "DECF" },
			{ "ABDE", "FEZ", "DBHG", "BLH", "DBII", "AFEA", "AFCB", "CQC", "DEAD", "FEAE", "EFAF", "DED" },
			{ "ABDF", "FEBG", "DBHH", "BLI", "DBIA", "AFEB", "AFCC", "CQD", "DEAE", "FEAF", "EFB", "DEDA" },
			{ "ABDG", "FEBH", "DBHI", "BLA", "DBIB", "AFEC", "AFCD", "CQE", "DEAF", "FET", "EFBA", "DEDB" },
	};
	DayOfWeek[] possibleDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, };
	private static int[] modIDList;
	private static int lastModIDCnt;
	private static int modIDCnt;
	private int loggingModID, selectedModID, currentInputPos = 0;
	IEnumerator currentlyRunning;
	IEnumerator[] colorsFlashing = new IEnumerator[6];
	bool hardModeEnabled, isplayingSolveAnim, hasStarted, isShowingStrikeCount, isFinished, hasStruck = false;

	private Color[] colorWheel = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
	private int[] idxColorList = { 0, 1, 2, 3, 4, 5 };
	List<string> lastCorrectInputs = new List<string>(), splittedInstructions = new List<string>();
	void Awake()
	{
		hardModeEnabled = false;
	}
	// Use this for initialization
	void Start() {
		loggingModID = ++modIDCnt;
		if (modIDList == null || loggingModID - lastModIDCnt >= modIDList.Length)
		{
			lastModIDCnt = modIDCnt;
			modIDList = new int[26];
			for (int x = 0; x < 26; x++)
			{
				modIDList[x] = x + lastModIDCnt;
			}
			modIDList = modIDList.OrderBy(x => uernd.Range(int.MinValue, int.MaxValue)).ToArray();
		}
		selectedModID = modIDList[loggingModID - lastModIDCnt];
		//selectedModID = 38;

		modSelf.OnActivate += delegate
		{
			StopCoroutine(currentlyRunning);

			PrepModule();
			
			hasStarted = true;
			LogCurrentInstruction();
			UpdateStatusIndc();
		};
		for (int x = 0; x < colorButtonSelectables.Length; x++)
		{
			int y = x;
			colorButtonSelectables[x].OnInteract += delegate
			{
				colorButtonSelectables[y].AddInteractionPunch(0.1f);
				if (!isFinished)
				{
					StopCoroutine(colorsFlashing[y]);
					colorsFlashing[y] = HandleFlashingAnim(y);
					StartCoroutine(colorsFlashing[y]);
					ProcessInstruction(baseColorList[idxColorList[y]]);
				}
				return false;
			};
			colorsFlashing[x] = HandleFlashingAnim(y);
		}
		innerSelectable.OnInteract += delegate
		{
			innerSelectable.AddInteractionPunch(0.1f);
			ProcessInstruction("Inner");
			StartCoroutine(HandlePressAnim(innerSelectable.gameObject));
			return false;
		};
		outerSelectable.OnInteract += delegate
		{
			outerSelectable.AddInteractionPunch(0.1f);
			ProcessInstruction("Outer");
			return false;
		};
		idxStrikeSelectable.OnInteract += delegate
		{
			isShowingStrikeCount = !isShowingStrikeCount;
			return false;
		};
		currentlyRunning = SampleStandardText();
		StartCoroutine(currentlyRunning);
		entireCircle.SetActive(false);
		pigpenDisplay.text = "";
		strikeIDDisplay.text = "";
		mainDisplay.text = "";
		float rangeModifier = modSelf.gameObject.transform.lossyScale.x;
		centerLight.range *= rangeModifier;
		for (int x = 0; x < colorLights.Length; x++)
		{
			colorLights[x].range *= rangeModifier;
		}

	}
	void PrepModule()
	{
		if (hardModeEnabled)
		{
			idxColorList.Shuffle();
		}
		Debug.LogFormat("[Unfair's Revenge #{0}] Button colors in clockwise order (starting on the NW button): {1}", loggingModID, idxColorList.Select(a => baseColorList[a]).Join(", "));
		StartCoroutine(HandleStartUpAnim());
		//StartCoroutine(TypePigpenText("ABCDEFGHIJKLMNOPQRSTVUWXYZABCDEFGHIJKLM"));
		GenerateInstructions();
		mainDisplay.text = "";
		string toDisplay = ValueToFixedRoman(selectedModID);
		strikeIDDisplay.text = FitToScreen(toDisplay, 5);
		Debug.LogFormat("[Unfair's Revenge #{0}] Mod ID grabbed: {1} Keep in mind this can differ from the ID used for logging!", loggingModID, selectedModID);

		string baseString = splittedInstructions.Join("");
		
		Debug.LogFormat("[Unfair's Revenge #{0}] ----------Caesar Offset Calculations----------", loggingModID);
		int offset = 0;
		char[] vowelList = { 'A', 'E', 'I', 'O', 'U' };
		int portTypeCount = bombInfo.GetPorts().Distinct().Count(),
			portPlateCount = bombInfo.GetPortPlateCount(),
			consonantCount = bombInfo.GetSerialNumberLetters().Where(a => !vowelList.Contains(a)).Count(),
			vowelCount = bombInfo.GetSerialNumberLetters().Where(a => vowelList.Contains(a)).Count(),
			litCount = bombInfo.GetOnIndicators().Count(),
			unlitCount = bombInfo.GetOffIndicators().Count(),
			batteryCount = bombInfo.GetBatteryCount();
		// For every port type
		offset -= 2 * portTypeCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many distant port types: {1}, Offset logged at {2}", loggingModID, portTypeCount, offset);
		// For every port plate
		offset += 1 * portPlateCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many port plates: {1}, Offset logged at {2}", loggingModID, portPlateCount, offset);
		// For every consonant in the serial number
		offset += 1 * consonantCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many consonants in the serial number: {1}, Offset logged at {2}", loggingModID, consonantCount, offset);
		// For every vowel in the serial number
		offset -= 2 * vowelCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many vowels in the serial number: {1}, Offset logged at {2}", loggingModID, vowelCount, offset);
		// For every lit indicator
		offset += 2 * litCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many lit indicators: {1}, Offset logged at {2}", loggingModID, litCount, offset);
		// For every unlit indicator
		offset -= 2 * unlitCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many unlit indicators: {1}, Offset logged at {2}", loggingModID, unlitCount, offset);
		if (batteryCount == 0)
			offset += 10;
		else
			offset -= 1 * batteryCount;
		Debug.LogFormat("[Unfair's Revenge #{0}] There are this many batteries: {1}, Offset logged at {2}", loggingModID, batteryCount, offset);
		if (bombInfo.GetPortCount() == 0)
		{
			offset *= 2;
			Debug.LogFormat("[Unfair's Revenge #{0}] There are no ports. Offset logged at {1}", loggingModID, offset);
		}
		if (bombInfo.GetSolvableModuleIDs().Count() >= 31)
		{
			offset /= 2;
			Debug.LogFormat("[Unfair's Revenge #{0}] There are 31 or more modules on the bomb, including itself. Offset logged at {1}", loggingModID, offset);
		}
		Debug.LogFormat("[Unfair's Revenge #{0}] ----------------------------------------------", loggingModID);
		Debug.LogFormat("[Unfair's Revenge #{0}] ----------Affine Offset Calculations----------", loggingModID);
		int multiplier = 0;
		Dictionary<string, int> indicatorMultipler = new Dictionary<string, int> {
			{"BOB", 1 },{"CAR", 1 },{"CLR", 1 },
			{"FRK", 2 },{"FRQ", 2 },{"MSA", 2 },{"NSA", 2 },
			{"SIG", 3 },{"SND", 3 },{"TRN", 3 },
		};
		foreach (string ind in bombInfo.GetIndicators())
		{
			if (indicatorMultipler.ContainsKey(ind))
			{
				multiplier += indicatorMultipler[ind] * (bombInfo.IsIndicatorOff(ind) ? -1 : bombInfo.IsIndicatorOn(ind) ? 1 : 0);
			}
		}
		Debug.LogFormat("[Unfair's Revenge #{0}] After indicators: X = {1}", loggingModID, multiplier);
		multiplier += 4 * (bombInfo.GetBatteryCount() % 2 == 1 ? 1 : -1);
		Debug.LogFormat("[Unfair's Revenge #{0}] After battery count: X = {1}", loggingModID, multiplier);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("Parallel")))
		{
			//Debug.Log(currentPlate.Join());
			multiplier += currentPlate.Contains("Serial") ? -4 : 5;
		}
		Debug.LogFormat("[Unfair's Revenge #{0}] After port plates with parallel ports: X = {1}", loggingModID, multiplier);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("DVI")))
		{
			//Debug.Log(currentPlate.Join());
			multiplier += currentPlate.Contains("StereoRCA") ? 4 : -5;
		}
		Debug.LogFormat("[Unfair's Revenge #{0}] After port plates with DVI-D ports: X = {1}", loggingModID, multiplier);
		multiplier = Mathf.Abs(multiplier);
		Debug.LogFormat("[Unfair's Revenge #{0}] After absolute value: X = {1}", loggingModID, multiplier);
		Debug.LogFormat("[Unfair's Revenge #{0}] ----------------------------------------------", loggingModID);
		int monthOfStart = DateTime.Now.Month;
		int idxStartDOW = Array.IndexOf(possibleDays, DateTime.Now.DayOfWeek);
		string keyAString = obtainKeyA();
		string keyBString = keyBTable[idxStartDOW, monthOfStart - 1];
		string keyCString = EncryptUsingPlayfair(keyAString, keyBString);

		Debug.LogFormat("[Unfair's Revenge #{0}] Key A: {1}", loggingModID, keyAString);
		Debug.LogFormat("[Unfair's Revenge #{0}] Key B: {1}", loggingModID, keyBString);
		Debug.LogFormat("[Unfair's Revenge #{0}] Key C: {1}", loggingModID, keyCString);

		string playfairEncryptedString = EncryptUsingPlayfair(splittedInstructions.Join(""), keyCString),
			step3EncryptedString = multiplier % 13 == 6 ? EncryptUsingAtbash(playfairEncryptedString) : EncryptUsingAffine(playfairEncryptedString, multiplier),
			caesarEncryptedString = EncryptUsingCaesar(step3EncryptedString,offset);

		Debug.LogFormat("[Unfair's Revenge #{0}] Caesar Encrypted String: {1}", loggingModID, caesarEncryptedString);
		Debug.LogFormat("[Unfair's Revenge #{0}] Affine/Atbash Encrypted String: {1}", loggingModID, step3EncryptedString);
		Debug.LogFormat("[Unfair's Revenge #{0}] Playfair Encrypted String: {1}", loggingModID, playfairEncryptedString);
		Debug.LogFormat("[Unfair's Revenge #{0}] Generated instructions: {1}", loggingModID, splittedInstructions.Join(", "));

		StartCoroutine(TypePigpenText(caesarEncryptedString));

	}

	string EncryptUsingCaesar(string input, int offset = 0)
	{// Encrypt the string with the given offset. Example: "ABCDEFG" + 7 -> "HIJKLMN"
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a)).ToArray();
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] += offset;
			while (stringInputs[x] < 0)
				stringInputs[x] += 26;
			stringInputs[x] %= 26;
		}

		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingAffine(string input, int multiplier = 0)
	{
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a)+1).ToArray();
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] *= (multiplier * 2 + 1);
			while (stringInputs[x] < 1)
				stringInputs[x] += 26;
			stringInputs[x]--;
			stringInputs[x] %= 26;
		}
		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingAtbash(string input)
	{
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a)).ToArray();
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] = baseAlphabet.Length - stringInputs[x];
			while (stringInputs[x] < 1)
				stringInputs[x] += 26;
			stringInputs[x]--;
			stringInputs[x] %= 26;
		}
		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingPlayfair(string input, string keyword = "")
	{

		/* Example:
		 * 
		 * Keyword: UNFAIRCIPHER
		 * Keyword after removing duplicate letters: UNFAIRCPHE
		 * 
		 * Grid when using keyword:
		 * U N F A I
		 * R C P H E
		 * B D G K L
		 * M O Q S T
		 * V W X Y Z
		 * 
		 * On a rectangular/square grid, this can be used to grab:
		 * - The row index of the given letter
		 * - The col index of the given letter
		 * 
		 * To Encrypt: BENT ON HER HEELS
		 * Key Pairs: BE NT ON HE RH EE LS
		 * Key Pairs after dupe filtering: BE NT ON HE RH EX LS
		 * Expected Result: LR IO WC ER PZ KT
		 * 
		 */
		string playfairGridBase = keyword.Replace('J','I').Distinct().Join("") + baseAlphabet.Replace('J', 'I').Distinct().Where(a => !keyword.Distinct().Contains(a)).Join("");
		Debug.LogFormat("[Unfair's Revenge #{0}] Given Playfair set: {1}", loggingModID, playfairGridBase);
		if (input.Length % 2 != 0) input += "X";
		string output = "";
		for (int y = 0; y < input.Length; y += 2)
		{
			string currentSet = input.Substring(y, 2);
			if (currentSet.Distinct().Count() == 1)
				currentSet = currentSet.Substring(0, 1) + "X";

			int[] rowIdxs = currentSet.Select(l => playfairGridBase.IndexOf(l) / 5).ToArray();
			int[] colIdxs = currentSet.Select(l => playfairGridBase.IndexOf(l) % 5).ToArray();

			if (rowIdxs.Distinct().Count() == 1)
			{
				colIdxs = colIdxs.Select(a => (a + 1) % 5).ToArray();
			}
			else if (colIdxs.Distinct().Count() == 1)
			{
				rowIdxs = rowIdxs.Select(a => (a + 1) % 5).ToArray();
			}
			else
			{
				colIdxs = colIdxs.Reverse().ToArray();
			}
			for (int x = 0; x < 2; x++)
			{
				output += playfairGridBase[5 * rowIdxs[x] + colIdxs[x]];
			}
		}

		return output;
	}
	Dictionary<char, int> charReference = new Dictionary<char, int>() {
		{'0', 0 }, {'1', 1 }, {'2', 2 }, {'3', 3 }, {'4', 4 }, {'5', 5 },
		{'6', 6 }, {'7', 7 }, {'8', 8 }, {'9', 9 }, {'A', 1 }, {'B', 2 },
		{'C', 3 }, {'D', 4 }, {'E', 5 }, {'F', 6 }, {'G', 7 }, {'H', 8 },
		{'I', 9 }, {'J', 10 }, {'K', 11 }, {'L', 12 }, {'M', 13 }, {'N', 14 },
		{'O', 15 }, {'P', 16 }, {'Q', 17 }, {'R', 18 }, {'S', 19 }, {'T', 20 },
		{'U', 21 }, {'V', 22 }, {'W', 23 }, {'X', 24 }, {'Y', 25 }, {'Z', 26 },
	};
	string obtainKeyA()
	{
		string returningString = "";
		string hexDecimalString = "0123456789ABCDEF";
		string curSerNo = bombInfo.GetSerialNumber();
		string curValue = charReference[curSerNo[0]] >= 20 ? "0" : charReference[curSerNo[0]].ToString();
		string remainingSerNo = curSerNo.Substring(1);
		for (int x = 0; x < remainingSerNo.Length; x++)
		{
			curValue += charReference[remainingSerNo[x]];
		}
		string vowelList = "AEIOU";
		if (vowelList.Contains(curSerNo[4]) || vowelList.Contains(curSerNo[3]))
			curValue = curValue.Substring(0,curValue.Length - 1);
		long givenValue = long.Parse(curValue);
		while (givenValue > 0)
		{
			returningString += hexDecimalString[(int)(givenValue % 16)];
			givenValue /= 16;
		}
		returningString = returningString.Reverse().Join("");
		string output = "";
		string[] listAllPossibilities = new string[] { returningString, selectedModID.ToString(), bombInfo.GetPortPlateCount().ToString(), bombInfo.GetBatteryHolderCount().ToString() };

		foreach (string selectedString in listAllPossibilities)
			for (int x = 0; x < selectedString.Length; x++)
			{
				if (x + 1 < selectedString.Length)
				{
					string intereptedString = selectedString.Substring(x, 2);
					if (intereptedString.RegexMatch(@"^(1\d|2[0123456])$"))
					{
						int intereptedValue = int.Parse(intereptedString);
						output += baseAlphabet[intereptedValue - 1];
						x++;
						continue;
					}
				}
				if (hexDecimalString.Substring(10).Contains(selectedString[x]))
				{
					output += selectedString[x];
				}
				else
				{
					int intereptedValue = int.Parse(selectedString[x].ToString());
					if (intereptedValue > 0)
						output += baseAlphabet[intereptedValue - 1];
				}
			}
		return output;
	}

	private Dictionary<int, string> romanValues = new Dictionary<int, string>() {
		{1,"I" },{5,"V" },{10,"X" },{50,"L" },{100,"C" },{500,"D" },{1000,"M" },{5000,"V_" },{10000,"X_" }
	};
	string FitToScreen(string value, int maxLength)
	{
		string output = "";
		if (maxLength <= 0)
			throw new ArgumentException(string.Format("{0} is not a valid length for the method FitToScreen!", maxLength));
		int a = 0;
		for (int x = 0; x < value.Length / maxLength; x++)
		{
			output += value.Substring(a, maxLength) + "\n";
			a += maxLength;
		}
		if (a < value.Length)
			output += value.Substring(a);
		return output.Trim();
	}
	string ValueToBrokenRoman(int value)
	{
		string output = "";
		int[] possibleKeys = romanValues.Keys.ToArray();
		while (value > 0)
		{
			int applier = 0;
			for (int x = 0; x < possibleKeys.Length; x++)
			{
				if (possibleKeys[x] > value)
					break;
				applier = possibleKeys[x];
			}
			output += romanValues[applier];
			value -= applier;

		}
		return output;
	}
	string ValueToFixedRoman(int value)
	{
		string output = "";
		int[] possibleKeys = romanValues.Keys.ToArray();
		string curValue = value.ToString();
		for (int x = 0; x < curValue.Length; x++)
		{
			int givenValue = int.Parse(curValue[curValue.Length - 1 - x].ToString());
			switch (givenValue)
			{
				case 0:
					break;
				case 1:
				case 2:
				case 3:
					for (int y = 0; y < givenValue; y++)
						output = romanValues[possibleKeys[2 * x]] + output;
					break;
				case 4:
					output = romanValues[possibleKeys[2 * x]] + romanValues[possibleKeys[2 * x + 1]] + output;
					break;
				case 5:
				case 6:
				case 7:
				case 8:
					string toAdd = "";
					toAdd += romanValues[possibleKeys[2 * x + 1]];
					for (int y = 0; y < givenValue - 5; y++)
						toAdd += romanValues[possibleKeys[2 * x]];
					output = toAdd + output;
					break;
				case 9:
					output = romanValues[possibleKeys[2 * x]] + romanValues[possibleKeys[2 * (x + 1)]] + output;
					break;
			}
		}
		return output;
	}
	void GenerateInstructions()
	{
		if (hardModeEnabled)
		{
			string[] lastCommand = { "FIN", "ISH" };
			List<string> instructionsToShuffle = hardModeInstructions.ToList();
			instructionsToShuffle.Shuffle();
			splittedInstructions.AddRange(instructionsToShuffle.Take(9));
			splittedInstructions.Add(lastCommand.PickRandom());
		}
		else
		{
			for (int x = 0; x < 4; x++)
			{
				string oneGiven = normalModeInstructions.PickRandom();
				splittedInstructions.Add(oneGiven);
			}
		}
		
	}
	IEnumerator HandleFlashingAnim(int btnIdx)
	{
		if (btnIdx < 0 || btnIdx >= 6) yield break;
		//colorButtonRenderers[btnIdx].material = switchableMats[1];
		for (int x = 5; x >= 0; x--)
		{
			colorLights[btnIdx].intensity = 4f + (x / 5f);
			colorButtonRenderers[btnIdx].material.color = (colorWheel[idxColorList[btnIdx]] * .75f * ((5-x) / 5f)) + Color.white * (x/5f);
			yield return new WaitForSeconds(0.05f);
		}
		//colorButtonRenderers[btnIdx].material = switchableMats[0];
		colorButtonRenderers[btnIdx].material.color = colorWheel[idxColorList[btnIdx]] * .75f;
		colorLights[btnIdx].intensity = 4f;
		yield return null;
	}
	IEnumerator HandlePressAnim(GameObject givenItem)
	{
		for (int x = 0; x < 2; x++)
		{
			givenItem.transform.localPosition += Vector3.down / 1000;
			yield return new WaitForSeconds(1 / 60f);
		}
		for (int x = 0; x < 2; x++)
		{
			givenItem.transform.localPosition += Vector3.up / 1000;
			yield return new WaitForSeconds(1 / 60f);
		}
		yield return null;
	}
	IEnumerator HandleFlickerSolveAnim()
	{
		while (isplayingSolveAnim)
		{
			for (int a = 0; a < 10; a += hardModeEnabled ? 1 : 3)
			{
				statusIndicators[a].material.color = Color.white;
			}
			yield return new WaitForSeconds(0.2f);
			for (int a = 0; a < 10; a += hardModeEnabled ? 1 : 3)
			{
				statusIndicators[a].material.color = Color.black;
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return null;
	}
	IEnumerator HandleSolveAnim()
	{

		mAudio.PlaySoundAtTransform("submitstart", transform);
		isplayingSolveAnim = true;
		StartCoroutine(HandleFlickerSolveAnim());
		strikeIDDisplay.text = "";
		for (int y = 12; y > 0; y -= 3)
		{
			for (int x = 0; x < y; x++)
			{
				pigpenDisplay.text = pigpenDisplay.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
				pigpenDisplay.gameObject.SetActive(true);
				mAudio.PlaySoundAtTransform("submiterate", transform);
				yield return new WaitForSeconds(0.1f);
				pigpenDisplay.gameObject.SetActive(false);
				yield return new WaitForSeconds(0.1f);
			}
			mAudio.PlaySoundAtTransform("submiterate2", transform);
			yield return new WaitForSeconds(0.1f);
		}
		mAudio.PlaySoundAtTransform("submitstop", transform);

		isplayingSolveAnim = false;
		foreach (Light singleLight in colorLights)
			singleLight.enabled = false;
		centerLight.enabled = false;
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.5f;
		}
		yield return null;
	}
	IEnumerator TypePigpenText(string displayValue)
	{
		string[] typeSoundList = { "type_1", "type_2", "type_3" };
		for (int x = 0; x < displayValue.Length; x++)
		{
			if (x > 0 && x % 13 == 0)
			{
				pigpenDisplay.text += "\n";
				mAudio.PlaySoundAtTransform("line", transform);
			}
			pigpenDisplay.text += displayValue[x];
			yield return new WaitForSeconds(0.2f);
			mAudio.PlaySoundAtTransform(typeSoundList.PickRandom(), transform);
		}
		mAudio.PlaySoundAtTransform("line", transform);
		yield return new WaitForSeconds(0.3f);
		mAudio.PlaySoundAtTransform("line_2", transform);
		StartCoroutine(indicatorHandler.HandleIndicatorModification(splittedInstructions.Count));
		yield return null;
	}
	IEnumerator SampleStandardText()
	{
		Dictionary<string, string> sampleQuestionResponse = new Dictionary<string, string>()
		{
			{"Meteor!", "Whooo-eeeeh!!" },
			{"Revenge...", "Of the Unfairs." },
			{"Raffina:", "Rainbow Deluxe!" },
			{"Landing Sequence...", "Initiated" },
			{"I'll tell you\nwhat you want", "What you really\nreally want" },
		};
		KeyValuePair<string, string> selectedSample = sampleQuestionResponse.PickRandom();
		mainDisplay.color = Color.red;
		for (int x = 1; x <= selectedSample.Key.Length; x++)
		{
			mainDisplay.text = selectedSample.Key.Substring(0, x);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return new WaitForSeconds(1f);
		for (int x = 1; x <= selectedSample.Value.Length; x++)
		{
			mainDisplay.text = selectedSample.Value.Substring(0, x);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return new WaitForSeconds(2f);
		mainDisplay.text = "";
	}
	IEnumerator HandleStartUpAnim()
	{
		entireCircle.SetActive(true);
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.75f;
		}
		int animLength = 60;
		for (float x = 0; x <= animLength; x++)
		{
			float curScale = Mathf.Pow(x / animLength, 1);
			entireCircle.transform.localScale = new Vector3(curScale, curScale, curScale);
			if (x != animLength)
				entireCircle.transform.Rotate(Vector3.up * (360 / Mathf.Max(animLength, 0.5f)));
			float currentOffset = Mathf.Pow((x - animLength) / animLength, 2f);
			entireCircle.transform.localPosition = new Vector3(0, 5 * currentOffset, 0);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		mAudio.PlaySoundAtTransform("werraMetallicTrimmed", entireCircle.transform);
		outerSelectable.AddInteractionPunch(3f);
		for (int i = 0; i < colorLights.Length; i++)
		{
			colorLights[i].enabled = true;
			colorLights[i].color = colorWheel[idxColorList[i]];
		}
		centerLight.enabled = true;
		particles.Emit(90);
	}
	IEnumerator HandleStrikeAnim()
	{
		for (int x = 0; x < 5; x++)
		{
			for (int a = 0; a < 10; a += hardModeEnabled ? 1 : 3)
			{
				statusIndicators[a].material.color = Color.red;
			}
			mAudio.PlaySoundAtTransform("wrong", transform);
			yield return new WaitForSeconds(0.1f);
			for (int a = 0; a < 10; a += hardModeEnabled ? 1 : 3)
			{
				statusIndicators[a].material.color = Color.black;
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		UpdateStatusIndc();
		yield return null;
	}
	void LogCurrentInstruction()
	{
		if (isFinished) return;
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		string toLog = "This is an example of logging a current instruction.";
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		switch (splittedInstructions[currentInputPos])
		{
			case "PCR":
				toLog = "Press Red.";
				break;
			case "PCG":
				toLog = "Press Green.";
				break;
			case "PCB":
				toLog = "Press Blue.";
				break;
			case "SCC":
				toLog = "Press Cyan.";
				break;
			case "SCM":
				toLog = "Press Magenta.";
				break;
			case "SCY":
				toLog = "Press Yellow.";
				break;
			case "SUB":
				toLog = "Press Outer Center when the seconds digit match.";
				break;
			case "MIT":
				toLog = string.Format("Press Inner Center when the last seconds digit is {0}.", (selectedModID + 1 + currentInputPos + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10);
				break;
			case "PRN":
				toLog = string.Format("Press {0} Center because {1} is {2}.", primesUnder20.Contains(selectedModID % 20) ? "Inner" : "Outer", selectedModID % 20, primesUnder20.Contains(selectedModID % 20) ? "prime" : "not prime");
				break;
			case "CHK":
				toLog = string.Format("Press {0} Center because {1} is {2}.", primesUnder20.Contains(selectedModID % 20) ? "Outer" : "Inner", selectedModID % 20, primesUnder20.Contains(selectedModID % 20) ? "prime" : "not prime");
				break;
			case "BOB":
				toLog = "Press Inner Center.";
				break;
			case "REP":
			case "EAT":
				if (lastCorrectInputs.Count == 0)
					toLog = "There were no previous inputs. Press Inner Center.";
				else
					toLog = string.Format("The last input was {0}, so press that.", lastCorrectInputs[currentInputPos - 1]);
				break;
			case "STR":
			case "IKE":
				toLog = "Start on Red. Count the number of colored buttons clockwise as there are strikes obtained so far. Press the resulting button.";
				break;
			case "SIG":
				string[] finaleInstructions = { "FIN", "ISH" };
				toLog = "Press Inner Center.";
				if (currentInputPos + 1 < splittedInstructions.Count && !finaleInstructions.Contains(splittedInstructions[currentInputPos + 1]))
					toLog += " The next instruction is skippable, so press Cyan in replacement for the next instruction.";
				break;
			case "PVP":
				{
					toLog = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
					}
					while (!primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CCW you should press is {0}.",rearrangedColorList[curIdx]);
					break;
				}
			case "NXP":
				{
					toLog = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = (curIdx + 1) % 6;
					}
					while (!primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CW you should press is {0}.", rearrangedColorList[curIdx]);
					break;
				}
			case "PVS":
				{
					toLog = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
					}
					while (primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CCW you should press is {0}.", rearrangedColorList[curIdx]);
					break;
				}
			case "NXS":
				{
					toLog = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
					int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
					do
					{
						curIdx = (curIdx + 1) % 6;
					}
					while (primaryList.Contains(rearrangedColorList[curIdx]));
					toLog += string.Format(" The resulting button when going CW you should press is {0}.", rearrangedColorList[curIdx]);
					break;
				}
			case "OPP":
				if (lastCorrectInputs.Count == 0)
					toLog = "There were no previous inputs. Press Outer Center.";
				else
					toLog = string.Format("The last input was {0}, so press {1}.", lastCorrectInputs[currentInputPos - 1],
						lastCorrectInputs[currentInputPos - 1] == "Outer" ? "Inner Center" :
						lastCorrectInputs[currentInputPos - 1] == "Inner" ? "Outer Center" :
						rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs[currentInputPos - 1])) % 6]);
				break;
			case "FIN":
			case "ISH":
				toLog = "This instruction is complicated. Refer to the manual for how to press this last command.";
				break;
		}
		Debug.LogFormat("[Unfair's Revenge #{0}] Instruction {2}: {1}", loggingModID, toLog, currentInputPos + 1);
	}
	bool canSkip = false;
	void ProcessInstruction(string input)
	{
		if (isFinished) return;
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		bool isCorrect = true;
		Debug.LogFormat("[Unfair's Revenge #{0}] Pressing the {1} button at {2} on the countdown timer...", loggingModID, input, bombInfo.GetFormattedTime());
		int secondsTimer = (int)bombInfo.GetTime() % 60;
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		string[] finaleInstructions = { "FIN", "ISH" };
		if (canSkip)
		{
			isCorrect = input == baseColorList[3];
			canSkip = false;
		}
		else
			switch (splittedInstructions[currentInputPos])
			{
				case "PCR":
					isCorrect = input == baseColorList[0];
					break;
				case "PCG":
					isCorrect = input == baseColorList[2];
					break;
				case "PCB":
					isCorrect = input == baseColorList[4];
					break;
				case "SCC":
					isCorrect = input == baseColorList[3];
					break;
				case "SCM":
					isCorrect = input == baseColorList[5];
					break;
				case "SCY":
					isCorrect = input == baseColorList[1];
					break;
				case "SUB":
					isCorrect = input == "Outer" && secondsTimer % 11 == 0;
					break;
				case "MIT":
					isCorrect = input == "Inner" && secondsTimer % 10 == (selectedModID + 1 + currentInputPos + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10;
					break;
				case "PRN":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ? "Inner" : "Outer");
					break;
				case "CHK":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ? "Outer" : "Inner");
					break;
				case "BOB":
					isCorrect = input == "Inner";
					if (bombInfo.IsIndicatorOn(Indicator.BOB) && bombInfo.GetBatteryCount() == 4 && bombInfo.GetBatteryHolderCount() == 2 && bombInfo.GetIndicators().Count() == 1)
					{
						Debug.LogFormat("[Unfair's Revenge #{0}] BOB is nice today. He will make you skip the rest of the instructions.", loggingModID);
						currentInputPos = splittedInstructions.Count;
					}
					break;
				case "REP":
				case "EAT":
					if (lastCorrectInputs.Count == 0)
						isCorrect = input == "Inner";
					else
						isCorrect = input == lastCorrectInputs[lastCorrectInputs.Count - 1];
					break;
				case "STR":
				case "IKE":
					{
						int strikeCount = bombInfo.GetStrikes();
						string resultingButton = rearrangedColorList[(strikeCount + Array.IndexOf(rearrangedColorList, baseColorList[0])) % 6];
						Debug.LogFormat("[Unfair's Revenge #{0}] At {1} strike(s) the resulting button should be {2}.", loggingModID, strikeCount, resultingButton);
						isCorrect = input == resultingButton;
						break;
					}
				case "SIG":
					{
						isCorrect = input == "Inner";
						if (currentInputPos + 1 < splittedInstructions.Count && !finaleInstructions.Contains(splittedInstructions[currentInputPos + 1]))
							canSkip = true;
						break;
					}
				case "PVP":
					{
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "NXP":
					{
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "PVS":
					{
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "NXS":
					{
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						isCorrect = input == rearrangedColorList[curIdx];
						break;
					}
				case "OPP":
					{
						if (lastCorrectInputs.Count == 0 || lastCorrectInputs[lastCorrectInputs.Count - 1] == "Inner")
							isCorrect = input == "Outer";
						else if (lastCorrectInputs[lastCorrectInputs.Count - 1] == "Outer")
							isCorrect = input == "Inner";
						else
							isCorrect = input == rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs[currentInputPos - 1])) % 6];
						break;
					}
				case "FIN":
				case "ISH":
					{
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						curIdx = (curIdx + lastCorrectInputs.Where(a => !baseColorList.Contains(a)).Count()) % 6;
						int solvedCount = bombInfo.GetSolvedModuleIDs().Count();
						curIdx -= solvedCount % 6;
						while (curIdx < 0)
							curIdx += 6;
						isCorrect = input == rearrangedColorList[curIdx] && (bombInfo.GetSolvableModuleIDs().Count() - solvedCount) % 10 == secondsTimer % 10;
						Debug.LogFormat("[Unfair's Revenge #{0}] At {1} solved, {2} unsolved, the resulting button should be {3} which much be pressed when the last seconds digit is {4}.", loggingModID, solvedCount, bombInfo.GetSolvableModuleIDs().Count() - solvedCount, rearrangedColorList[curIdx], (bombInfo.GetSolvableModuleIDs().Count() - solvedCount)%10);
					}
					break;
			}
		if (isCorrect)
		{
			Debug.LogFormat("[Unfair's Revenge #{0}] The resulting press is correct.", loggingModID);
			string[] possibleSounds = { "button1", "button2", "button3", "button4" };
			lastCorrectInputs.Add(input);
			currentInputPos++;
			if (currentInputPos >= splittedInstructions.Count)
			{
				Debug.LogFormat("[Unfair's Revenge #{0}] All instructions are handled correctly. You're done.", loggingModID);
				isFinished = true;
				modSelf.HandlePass();
				StartCoroutine(HandleSolveAnim());
				return;
			}
			else if (!canSkip)
				LogCurrentInstruction();
			else
			{
				Debug.LogFormat("[Unfair's Revenge #{0}] The next instruction is getting skipped.", loggingModID);
			}
			mAudio.PlaySoundAtTransform(possibleSounds.PickRandom(), transform);
			UpdateStatusIndc();
		}
		else
		{
			Debug.LogFormat("[Unfair's Revenge #{0}] The resulting press is incorrect. Restarting from the first instruction...", loggingModID);
			if (currentInputPos + 1 >= splittedInstructions.Count)
				mAudio.PlaySoundAtTransform("Darkest Dungeon - OverconfidenceRant", transform);
			modSelf.HandleStrike();
			hasStruck = true;
			lastCorrectInputs.Clear();
			currentInputPos = 0;
			canSkip = false;
			StartCoroutine(HandleStrikeAnim());
			LogCurrentInstruction();
		}
	}
	void UpdateStatusIndc()
	{
		for (int a = 0; a < 10; a += hardModeEnabled ? 1 : 3)
		{
			statusIndicators[a].material.color = currentInputPos * (hardModeEnabled ? 1 : 3) == a  ? Color.yellow : currentInputPos * (hardModeEnabled ? 1 : 3) > a  ? Color.green : Color.black;
		}
	}
	// Update is called once per frame, may be scaled by other events
	void Update () {
		if (hasStarted && !isFinished)
			if (isShowingStrikeCount)
			{
				strikeIDDisplay.text = ValueToFixedRoman(bombInfo.GetStrikes());
				strikeIDDisplay.color = Color.red;
			}
			else
			{
				strikeIDDisplay.text = ValueToFixedRoman(selectedModID);
				strikeIDDisplay.color = Color.white;
			}
	}

	string FormatSecondsToTime(int num)
	{
		return string.Format("{0}:{1}",num/60,num%60);
	}

	// TP Handling Begins here
	void TwitchHandleForcedSolve()
	{
		isFinished = true;
		StartCoroutine(HandleSolveAnim());
	}


	bool TimeModeActive;
#pragma warning disable IDE0051 // Remove unused private members
	bool ZenModeActive;
	readonly string TwitchHelpMessage = "Select the given button with \"!{0} press R(ed);G(reen);B(lue);C(yan);M(agenta);Y(ellow);Inner;Outer\" To time a specific press, append \"at\" onto the command and either specify based only on seconds digits (##), full time stamp (DD:HH:MM:SS), or MM:SS where MM exceeds 99 min. Semicolons can be used to combine presses, both untimed and timed.";
#pragma warning restore IDE0051 // Remove unused private members
	IEnumerator ProcessTwitchCommand(string command)
	{
		if (!hasStarted)
		{
			yield return "sendtochaterror The module has not activated yet. Wait for a bit until the module has started";
			yield break;
		}
		string[] intereptedParts = command.ToLower().Split(';');
		List<KMSelectable> selectedCommands = new List<KMSelectable>();
		List<List<int>> timeThresholds = new List<List<int>>();
		List<string> rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToList();

		int[] multiplierTimes = { 1, 60, 3600, 86400 }; // To denote seconds, minutes, hours, days in seconds.

		foreach (string commandPart in intereptedParts)
		{
			string partTrimmed = commandPart.Trim();
			if (partTrimmed.RegexMatch(@"^press "))
			{
				partTrimmed = partTrimmed.Substring(6);
			}
			string[] partOfPartTrimmed = partTrimmed.Split();
			if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer) at( (([0-9]+:)?([0-5][0-9]:){2}|[0-9]+)[0-5][0-9])+$"))
			{
				List<int> possibleTimes = new List<int>();
				for (int x = partOfPartTrimmed.Length - 1; x > 0; x--)
				{
					if (!partOfPartTrimmed[x].RegexMatch(@"^([0-9]+:)?([0-5][0-9]:){2}[0-5][0-9])$")) break;
					string[] curTimePart = partOfPartTrimmed[x].Split(':').Reverse().ToArray();
					int curTime = 0;
					for (int idx = 0; idx < curTimePart.Length; idx++)
					{
						curTime += multiplierTimes[idx] * int.Parse(curTimePart[idx]);
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
					yield return string.Format("sendtochaterror The command part \"{0}\" gave no accessible times for this module. The command has been voided.", partTrimmed);
					yield break;
				}
			}
			else if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer) at( [0-5][0-9])+$"))
			{
				
				List<int> possibleTimes = new List<int>();
				for (int idx = partOfPartTrimmed.Length - 1; idx > 0; idx--)
				{
					if (!partOfPartTrimmed[idx].RegexMatch(@"^[0-5][0-9]$")) break;
					int secondsTime = int.Parse(partOfPartTrimmed[idx]);
					int curMinRemaining = (int)bombInfo.GetTime()/60;
					for (int x = curMinRemaining - (ZenModeActive ? 0 : 2); x <= curMinRemaining + (ZenModeActive ? 2 : 0); x++)
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
					yield return string.Format("sendtochaterror The command part \"{0}\" gave no accessible times for this module. The command has been voided.", partTrimmed);
					yield break;
				}
			}
			else if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer)$"))
			{
				timeThresholds.Add(new List<int>());
			}
			else
			{
				yield return string.Format("sendtochaterror \"{0}\" is not a valid sub command, check your command for typos.",partTrimmed);
				yield break;
			}
			switch (partOfPartTrimmed[0])
			{
				case "r":
				case "red":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Red")]);
					break;
				case "g":
				case "green":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Green")]);
					break;
				case "b":
				case "blue":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Blue")]);
					break;
				case "c":
				case "cyan":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Cyan")]);
					break;
				case "m":
				case "magenta":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Magenta")]);
					break;
				case "y":
				case "yellow":
					selectedCommands.Add(colorButtonSelectables[rearrangedColorList.IndexOf("Yellow")]);
					break;
				case "inner":
					selectedCommands.Add(innerSelectable);
					break;
				case "outer":
					selectedCommands.Add(outerSelectable);
					break;
				default:
					yield return "sendtochaterror You aren't supposed to get this error.";
					yield break;
			}
		}
		hasStruck = false;
		if (selectedCommands.Any())
		{
			for (int x = 0; x < selectedCommands.Count; x++)
			{
				if (hasStruck) yield break;
				if (timeThresholds[x].Any())
				{
					int targetTime = ZenModeActive ? timeThresholds[x].Min() : timeThresholds[x].Max();
					yield return string.Format("sendtochat Target time for press #{0}: {1}", x + 1, FormatSecondsToTime(targetTime));
					do
					{
						yield return string.Format("trycancel Your timed interation has been canceled after a total of {0}/{1} presses.", x + 1, selectedCommands.Count);
						if ((int)bombInfo.GetTime() > targetTime && ZenModeActive)
						{
							timeThresholds[x] = timeThresholds[x].Where(a => a > bombInfo.GetTime()).ToList();
							if (!timeThresholds[x].Any())
							{
								yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0}", x + 1);
								yield break;
							}
							targetTime = timeThresholds[x].Min();
							yield return string.Format("sendtochat Your timed interation has been altered. The new time is now {1} for press #{0}", x + 1, FormatSecondsToTime(targetTime));
						}
						else if ((int)bombInfo.GetTime() < targetTime && !ZenModeActive)
						{
							timeThresholds[x] = timeThresholds[x].Where(a => a < bombInfo.GetTime()).ToList();
							if (!timeThresholds[x].Any())
							{
								yield return string.Format("sendtochaterror Your timed interation has been canceled. There are no remaining times left for press #{0}", x + 1);
								yield break;
							}
							targetTime = timeThresholds[x].Max();
							yield return string.Format("sendtochat Your timed interation has been altered. The new time is now {1} for press #{0}", x + 1, FormatSecondsToTime(targetTime));
						}
					}
					while ((int)bombInfo.GetTime() != targetTime);
				}
				yield return null;
				selectedCommands[x].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield break;
	}
}