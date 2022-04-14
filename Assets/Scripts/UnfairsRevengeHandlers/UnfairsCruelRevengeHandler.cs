using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using uernd = UnityEngine.Random;
using KeepCoding;
public class UnfairsCruelRevengeHandler : MonoBehaviour {

	public KMBombInfo bombInfo;
	public KMAudio mAudio;
	public KMBombModule modSelf;
	public KMSelectable[] colorButtonSelectables;
	public KMSelectable innerSelectable, outerSelectable, idxStrikeSelectableT, idxStrikeSelectableB;
	public GameObject[] colorButtonObjects;
	public GameObject innerRing, entireCircle, animBar;
	public MeshRenderer[] colorButtonRenderers, statusIndicators, statusIndicatorsExtra;
	public TextMesh pigpenDisplay, strikeIDDisplay, mainDisplay, pigpenSecondary;
	public Light[] colorLights;
	public Light centerLight;
	public ParticleSystem particles;
	public IndicatorCoreHandlerEX indicatorCoreHandlerEX, IndicatorCoreHandlerExtraScreen;
	public KMColorblindMode colorblindMode;
	public Material[] switchableMats = new Material[2];
	public KMGameInfo gameInfo;
	public ProgressBarHandler progressHandler;
	public StringListEditable uCipherWordBank;
	private string[]
		hardModeInstructions = { "PCR", "PCG", "PCB", "SCC", "SCM", "SCY", "SUB", "PVP", "NXP", "PVS", "NXS", "REP", "EAT", "STR", "IKE", "PRN", "CHK", "MOT", "OPP", "SKP", "INV", "ERT", "SWP", "AGN", "SCN" },
		legacyInstructions = { "PCR", "PCG", "PCB", "SCC", "SCM", "SCY", "SUB", "PVP", "NXP", "PVS", "NXS", "REP", "EAT", "STR", "IKE", "PRN", "CHK", "MOT", "OPP", "SKP", },
		baseColorList = new[] { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta" },
		lastCommands = { "FIN", "ISH", "ALE" },
		extraCruelInstructions = { },
		primaryList = { "Red", "Green", "Blue", };
	private string baseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", // Base alphabet for code assumes A=1,B=2,...,Y=25,Z=26
		displayPigpenText = "", fourSquareKey = "", selectedWord = "", encodingDisplay = "", uCipherWord = "", keyABaseKey = "";
	private string[,] keyBTable = {
			{ "ALPH", "ONE", "ABCD", "AEI", "PLAY", "JAKK", "FRLA", "ZEKN", "FIZZ", "HEND", "CLUT", "SCG" },
			{ "BETA", "TWO", "EFGH", "OUY", "HIDE", "MCDU", "VIRE", "ELIA", "TIMW", "ACRY", "MAGE", "BASH" },
			{ "CHAR", "THRE", "IJKL", "WBC", "SECR", "EOTA", "IONL", "REXK", "MOON", "ONYX", "SPAR", "MOCK" },
			{ "DELT", "FOUR", "MNOP", "DFG", "CIPH", "CAIT", "LEGN", "RIVE", "TAOO", "SAMD", "KONQ", "BRIN" },
			{ "ECHO", "FIVE", "QRST", "HJK", "FAIL", "MARA", "WILL", "TRAI", "LUPO", "ELUM", "FLAM", "KANE" },
			{ "FOXT", "SIX", "UVWX", "LMN", "PART", "WARI", "SKIP", "NANT", "LUMB", "FLUS", "MOMO", "HEXI" },
			{ "GOLF", "SEVN", "YZAB", "PQR", "BECO", "PIGD", "ETRS", "GRYB", "CATN", "ASIM", "MITT", "PERK" },
	};
	private string[] myszkowskiKeywords = {
		"ARCHER", "ATTACK", "BANANA", "BLASTS", "BURSTS", "BUTTON", "CANNON",
		"CALLER", "CELLAR", "DAMAGE", "DEFUSE", "DEVICE", "KABOOM", "LETTER",
		"LOOPED", "MORTAR", "NAPALM", "OTTAWA", "PAPERS", "POODLE", "POOLED",
		"RASHES", "RECALL", "ROBOTS", "SAPPER", "SHARES", "SHEARS", "WIRING",
	}, tableRoman = {
		"Fixed Roman",
		"Broken Roman",
		"Arabic",
	}, wordSearchWordsEven = {
		"HOTEL", "SEARCH", "ADD", "SIERRA", "FINISH",
		"PORT", "BOOM", "LINE", "KABOOM", "PANIC", "MANUAL", "DECOY",
		"SEE", "INDIA", "NUMBER", "ZULU","VICTOR", "DELTA", "HELP",
		"ROMEO", "TRUE","MIKE", "FOUND","BOMBS","WORK", "TEST",
		"GOLF", "TALK","BRAVO", "SEVEN", "MODULE", "LIST", "YANKEE",
		"CHART", "MATH", "READ", "LIMA", "COUNT",
	}, wordSearchWordsOdd = {
		"DONE", "QUEBEC", "CHECK", "FIND", "EAST",
		"COLOR", "SUBMIT", "BLUE", "ECHO", "FALSE", "ALARM", "CALL",
		"TWENTY", "NORTH", "LOOK", "GREEN", "XRAY", "YES", "LOCATE",
		"BEEP", "EXPERT", "EDGE", "RED", "WORD", "UNIQUE", "JINX",
		"LETTER", "SIX", "SERIAL", "TIMER", "SPELL", "TANGO", "SOLVE",
		"OSCAR", "NEXT", "LISTEN", "FOUR", "OFFICE",
	};
	private string[][] anagramValues = new string[][]
	{
		new string[] { "TAMERS", "STREAM", "MASTER", "ARM SET", "MRS TEA", "MR SEAT" },
		new string[] { "BARELY", "BARLEY", "BLEARY", "LAB RYE", "A BERYL", "ALB RYE" },
		new string[] { "RUDEST", "DUSTER", "RUSTED", "ED RUST", "EDS RUT", "DUST RE" },
		new string[] { "IDEALS", "SAILED", "LADIES", "A SLIDE", "DEAL IS", "SEA LID" },
	};

	DayOfWeek[] possibleDays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, };
	private static int[] modIDList;
	private static int lastModIDCnt;
	private static int modIDCnt;
	private int loggingModID, selectedModID, currentInputPos = 0, localStrikeCount = 0, currentScreenVal = 0, idxCurModIDDisplay = 0, idxCurStrikeDisplay = 0;
	IEnumerator currentlyRunning;
	IEnumerator[] colorsFlashing = new IEnumerator[6];
	bool isplayingSolveAnim, hasStarted, colorblindDetected, isAnimatingStart, isFinished, hasStruck = false, autoCycleEnabled = false, swapPigpenAndStandard = false, swapStandardKeys = false, inverseAutoCycle, legacyUCR, harderUCR, isChangingColors, noTPCruelCruelRevenge, tpPrepCruelRevenge, settingsOverriden, forceSolveRequested, allowDebugCiphers;
	private MeshRenderer[] usedRenderers;
	private Color[] colorWheel = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };
    private int[] idxColorList = Enumerable.Range(0, 6).ToArray(), initialIdxColorList, columnalTranspositionLst, debugCipherIdxes;
	List<string> lastCorrectInputs = new List<string>(), splittedInstructions = new List<string>(), displaySubstutionLettersAll = new List<string>();
	UnfairsCruelRevengeSettings ucrSettings = new UnfairsCruelRevengeSettings();
	void Awake()
	{
		try
		{
			ModConfig<UnfairsCruelRevengeSettings> fileSettings = new ModConfig<UnfairsCruelRevengeSettings>("UnfairsCruelRevengeSettings");
			if (ucrSettings.version != fileSettings.Settings.version)
			{
				fileSettings.Settings = ucrSettings;
			}
			else
			{
				ucrSettings = fileSettings.Settings;
				fileSettings.Settings = ucrSettings;
			}
			
			legacyUCR = ucrSettings.enableLegacyUCR;
			harderUCR = ucrSettings.cruelerRevenge;
			noTPCruelCruelRevenge = ucrSettings.noTPCruelerRevenge;
			allowDebugCiphers = ucrSettings.debugCiphers;
			debugCipherIdxes = ucrSettings.debugCiphersIdxes;
		}
		catch
		{
			Debug.LogWarningFormat("<Unfair's Cruel Revenge>: Settings do not work as intended! Using default settings!", loggingModID);
			legacyUCR = false;
			harderUCR = false;
		}
		finally
		{
			try
			{
				colorblindDetected = colorblindMode.ColorblindModeActive;
			}
			catch
			{
				colorblindDetected = false;
			}
		}
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
			modIDList.Shuffle();
		}
		selectedModID = modIDList[loggingModID - lastModIDCnt];
		//selectedModID = 38;

		modSelf.OnActivate += delegate
		{
			StopCoroutine(currentlyRunning);
			if (legacyUCR)
				PrepModuleLegacy();
			else
				PrepModule();

			hasStarted = true;
			LogCurrentInstruction();
			UpdateSecondaryScreen();
			UpdateStatusIndc();
		};
		for (int x = 0; x < colorButtonSelectables.Length; x++)
		{
			int y = x;
			colorButtonSelectables[x].OnInteract += delegate
			{
				colorButtonSelectables[y].AddInteractionPunch(0.1f);
				if (!isFinished && hasStarted && !isChangingColors)
				{
					if (colorsFlashing[y] != null)
					StopCoroutine(colorsFlashing[y]);
					colorsFlashing[y] = HandleFlashingAnim(y);
					if (!harderUCR || currentInputPos + 1 >= splittedInstructions.Count)
						StartCoroutine(colorsFlashing[y]);
					ProcessInstruction(baseColorList[idxColorList[y]]);
				}
				return false;
			};

			colorButtonSelectables[x].OnHighlight += delegate
			{
				string[] directionSamples = { "NW", "N", "NE", "SE", "S", "SW" };
				if (!isAnimatingStart && colorblindDetected && hasStarted && !isFinished && !isChangingColors)
				{
					mainDisplay.text = string.Format("{0} Button:\n{1}", directionSamples[y], baseColorList[idxColorList[y]]);
					mainDisplay.color = Color.white;
					pigpenDisplay.text = "";
				}
			};
			colorButtonSelectables[x].OnHighlightEnded += delegate
			{
				if (!isAnimatingStart && colorblindDetected && hasStarted && !isFinished && !isChangingColors)
				{
					mainDisplay.text = "";
					pigpenDisplay.text = displayPigpenText;
				}
			};

			//colorsFlashing[x] = HandleFlashingAnim(y);
		}
		innerSelectable.OnInteract += delegate
		{
			innerSelectable.AddInteractionPunch(0.1f);
			if (!isChangingColors)
				ProcessInstruction("Inner");
			StartCoroutine(HandlePressAnim(innerSelectable.gameObject));
			return false;
		};
		outerSelectable.OnInteract += delegate
		{
			outerSelectable.AddInteractionPunch(0.1f);
			if (!isChangingColors)
				ProcessInstruction("Outer");
			return false;
		};
		idxStrikeSelectableT.OnInteract += delegate
		{
			if (!isFinished && hasStarted)
			{
				currentScreenVal = (currentScreenVal + 3) % 4;
				UpdateSecondaryScreen();
			}
			return false;
		};
		idxStrikeSelectableB.OnInteract += delegate
		{
			if (!isFinished && hasStarted)
			{
				currentScreenVal = (currentScreenVal + 1) % 4;
				UpdateSecondaryScreen();
			}
			return false;
		};
		OverrideSettings();
		currentlyRunning = SampleStandardText();
		StartCoroutine(currentlyRunning);
		entireCircle.SetActive(false);
		pigpenDisplay.text = "";
		strikeIDDisplay.text = "";
		mainDisplay.text = "";
		pigpenSecondary.text = "";
		float rangeModifier = modSelf.gameObject.transform.lossyScale.x;
		centerLight.range *= rangeModifier;
		for (int x = 0; x < colorLights.Length; x++)
		{
			colorLights[x].range *= rangeModifier;
		}
		gameInfo.OnLightsChange += delegate(bool turnedOn)
		{
			if (isFinished) return;
			for (int i = 0; i < colorButtonRenderers.Length; i++)
			{
				colorButtonRenderers[i].material = turnedOn ? switchableMats[0] : switchableMats[1] ;
				colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.75f;
			}
		};
		bombInfo.OnBombExploded += delegate {
			if (harderUCR && !legacyUCR)
				mAudio.PlaySoundAtTransform("7_youdied", transform);
		};

		if (Application.isEditor)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Unity Editor Mode is active, if TP is enabled, you may use \"!# simulate on/off to simulate lights turning on or off.\"", loggingModID);
		}
		StartCoroutine(HandleAutoCycleAnim(false));
		usedRenderers = new[] { statusIndicators.First() }.Concat(statusIndicators.Skip(1).Take(harderUCR && !legacyUCR ? 8 : 4)).Concat(new[] { statusIndicators.Last() }).ToArray();

	}

	void UpdateSecondaryScreen()
	{
		string toDisplay = "";
		switch (currentScreenVal)
		{
			case 0:
				{
					switch (idxCurModIDDisplay)
					{
						case 0:
							toDisplay = ValueToFixedRoman(selectedModID);
							break;
						case 1:
							toDisplay = ValueToBrokenRoman(selectedModID);
							break;
						case 2:
							toDisplay = selectedModID.ToString();
							break;
					}
					strikeIDDisplay.text = string.Format("Module ID:\n{0}", toDisplay);
					strikeIDDisplay.color = Color.white;
					pigpenSecondary.text = "";
					break;
				}
			case 1:
				{
					strikeIDDisplay.color = TimeModeActive ? new Color(1, 0.5f, 0) : ZenModeActive ? Color.cyan : Color.red;
					pigpenSecondary.text = "";
					string strikeCntToDisplay = "";
					switch (idxCurStrikeDisplay)
					{
						case 0:
							strikeCntToDisplay = ValueToFixedRoman(TimeModeActive ? localStrikeCount : bombInfo.GetStrikes());
							break;
						case 1:
							strikeCntToDisplay = ValueToBrokenRoman(TimeModeActive ? localStrikeCount : bombInfo.GetStrikes());
							break;
						case 2:
							strikeCntToDisplay = (TimeModeActive ? localStrikeCount : bombInfo.GetStrikes()).ToString();
							break;
					}
					strikeIDDisplay.text = string.Format("Strikes Detected:\n{0}", strikeCntToDisplay);
					break;
				}
			case 2:
				{
					strikeIDDisplay.color = Color.white;
					if (swapPigpenAndStandard)
					{
						strikeIDDisplay.text = swapStandardKeys
                            ? string.Format("\n{1} | {0}\n{2} | {3}", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord, keyABaseKey, uCipherWord)
                            : string.Format("\n{0} | {1}\n{2} | {3}", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord, keyABaseKey, uCipherWord);
                        pigpenSecondary.text = string.Format("{0}\n\n", fourSquareKey);
					}
					else
					{
						strikeIDDisplay.text = swapStandardKeys
                            ? string.Format("{1} | {0}\n\n{2} | {3}", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord, keyABaseKey, uCipherWord)
                            : string.Format("{0} | {1}\n\n{2} | {3}", columnalTranspositionLst.Select(a => a + 1).Join(""), selectedWord, keyABaseKey, uCipherWord);
                        pigpenSecondary.text = string.Format("\n{0}\n", fourSquareKey);
					}
					break;
				}
			case 3:
                {
					strikeIDDisplay.color = Color.white;
					if ((harderUCR && !legacyUCR) || splittedInstructions.Count > 8)
					{
						if (bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1) >= displaySubstutionLettersAll.Count)
						{
							pigpenSecondary.text = "";
							strikeIDDisplay.text = string.Format("{1}{0}{1}", encodingDisplay, allowDebugCiphers ? "" : "=");
						}
						else
						{
							pigpenSecondary.text = FitToScreen(displaySubstutionLettersAll.ElementAtOrDefault((int)(bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1))), 13);
							strikeIDDisplay.text = "";
						}

					}
					else
					{
						pigpenSecondary.text = (legacyUCR || bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1) >= displaySubstutionLettersAll.Count) ? "" :
						FitToScreen(displaySubstutionLettersAll.ElementAtOrDefault((int)(bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1))), 13) + "\n";
						strikeIDDisplay.text = string.Format("\n\n{1}{0}{1}", encodingDisplay, allowDebugCiphers ? "" : "=");
					}
					break;
                }
			default:
				{
					strikeIDDisplay.color = Color.white;
					pigpenSecondary.text = "";
					strikeIDDisplay.text = "";
					break;
				}
		}
        for (var x = 0; x < statusIndicatorsExtra.Length; x++)
        {
			statusIndicatorsExtra[x].material.color = x == currentScreenVal ? Color.white : Color.black;
        }
	}
	List<string> GrabNonOverlappingInstructions(IEnumerable<string> instructionSets)
    {
		List<int> allIdxes = new List<int>();
        for (int x = 0; x < instructionSets.Count(); x++)
        {
			bool isUnique = true;
            string curInstruction = instructionSets.ElementAtOrDefault(x).Replace(baseAlphabet[8],baseAlphabet[9]);

			for (int y = 0; y < allIdxes.Count && isUnique; y++)
            {
				string curScanInstruction = instructionSets.ElementAtOrDefault(y).Replace(baseAlphabet[8], baseAlphabet[9]);
				if (curInstruction == curScanInstruction)
                {
					allIdxes.RemoveAt(y);
					isUnique = false;
					break;
                }
			}
			if (isUnique)
				allIdxes.Add(x);
        }

		return allIdxes.Select(a => instructionSets.ElementAtOrDefault(a)).ToList();
    }
	// Legacy Unfair's Cruel Revenge Section Begins Here
	void PrepModuleLegacy()
	{
		StartCoroutine(IndicatorCoreHandlerExtraScreen.HandleIndicatorModification(4));
		idxColorList.Shuffle();
		List<string> curColorList = idxColorList.Select(a => baseColorList[a]).ToList();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: How in the world did he forget about this module!? At least it's updated. You are currently using the legacy ruleset for Unfair's Cruel Revenge.", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Button colors in clockwise order (starting on the NW button): {1}", loggingModID, curColorList.Join(", "));
		StartCoroutine(HandleStartUpAnim());
		//StartCoroutine(TypePigpenText(FitToScreen("ABCDEFGHIJKLMNOPQRSTVUWXYZABCDEFGHIJKLM",13)));
		mainDisplay.text = "";
		// Basic Columnar Transposition Set
		int[] possibleSizes = { 2, 3, 6, 9 };
		columnalTranspositionLst = new int[possibleSizes.PickRandom()];
		for (int x = 0; x < columnalTranspositionLst.Length; x++)
		{
			columnalTranspositionLst[x] = x;
		}

		columnalTranspositionLst.Shuffle();
		// Extra Pigpen Key
		for (int x = 0; x < 12; x++)
		{
			fourSquareKey += baseAlphabet.PickRandom();
		}

		// Autokey Keyword Mislead
		int randomIdx = uernd.Range(0, wordSearchWordsEven.Length);
		bool useEven = uernd.Range(0, 2) == 1;
		selectedWord = useEven ? wordSearchWordsOdd[randomIdx] : wordSearchWordsEven[randomIdx];

		idxCurModIDDisplay = uernd.Range(0, 3);
		idxCurStrikeDisplay = uernd.Range(0, 3);
		swapPigpenAndStandard = uernd.Range(0, 2) == 1;
		swapStandardKeys = uernd.Range(0, 2) == 1;

		keyABaseKey = "-------";
		uCipherWord = "------";
		encodingDisplay = "LEGACY";

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Mod ID grabbed: {1} Keep in mind this can differ from the ID used for logging!", loggingModID, selectedModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Mod ID is in {1} Numerals", loggingModID, tableRoman[idxCurModIDDisplay]);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The strike counter is in {1} Numerals", loggingModID, tableRoman[idxCurStrikeDisplay]);
		// Value A Calculations
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -------------Value A Calculations-------------", loggingModID);
		int valueA = 0;
		char[] vowelList = { 'A', 'E', 'I', 'O', 'U' };
		int portTypeCount = bombInfo.GetPorts().Distinct().Count(),
			portPlateCount = bombInfo.GetPortPlateCount(),
			consonantCount = bombInfo.GetSerialNumberLetters().Where(a => !vowelList.Contains(a)).Count(),
			vowelCount = bombInfo.GetSerialNumberLetters().Where(a => vowelList.Contains(a)).Count(),
			litCount = bombInfo.GetOnIndicators().Count(),
			unlitCount = bombInfo.GetOffIndicators().Count(),
			batteryCount = bombInfo.GetBatteryCount();
		// For every port type
		valueA -= 2 * portTypeCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many distant port types: {1}, Value A logged at {2}", loggingModID, portTypeCount, valueA);
		// For every port plate
		valueA += 1 * portPlateCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many port plates: {1}, Value A logged at {2}", loggingModID, portPlateCount, valueA);
		// For every consonant in the serial number
		valueA += 1 * consonantCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many consonants in the serial number: {1}, Value A logged at {2}", loggingModID, consonantCount, valueA);
		// For every vowel in the serial number
		valueA -= 2 * vowelCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many vowels in the serial number: {1}, Value A logged at {2}", loggingModID, vowelCount, valueA);
		// For every lit indicator
		valueA += 2 * litCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many lit indicators: {1}, Value A logged at {2}", loggingModID, litCount, valueA);
		// For every unlit indicator
		valueA -= 2 * unlitCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many unlit indicators: {1}, Value A logged at {2}", loggingModID, unlitCount, valueA);
		if (batteryCount == 0)
			valueA += 10;
		else
			valueA -= 1 * batteryCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many batteries: {1}, Value A logged at {2}", loggingModID, batteryCount, valueA);
		if (bombInfo.GetPortCount() == 0)
		{
			valueA *= 2;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are no ports. Value A logged at {1}", loggingModID, valueA);
		}
		if (bombInfo.GetSolvableModuleIDs().Count() >= 31)
		{
			valueA /= 2;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are 31 or more modules on the bomb, including itself. Value A logged at {1}", loggingModID, valueA);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		// Value X calculations
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -------------Value X Calculations-------------", loggingModID);
		int valueX = 0;
		Dictionary<string, int> indicatorMultipler = new Dictionary<string, int> {
			{"BOB", 1 },{"CAR", 1 },{"CLR", 1 },
			{"FRK", 2 },{"FRQ", 2 },{"MSA", 2 },{"NSA", 2 },
			{"SIG", 3 },{"SND", 3 },{"TRN", 3 },
		};
		foreach (string ind in bombInfo.GetIndicators())
		{
			if (indicatorMultipler.ContainsKey(ind))
			{
				valueX += indicatorMultipler[ind] * (bombInfo.IsIndicatorOff(ind) ? -1 : bombInfo.IsIndicatorOn(ind) ? 1 : 0);
			}
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After indicators: X = {1}", loggingModID, valueX);
		valueX += 4 * (bombInfo.GetBatteryCount() % 2 == 1 ? 1 : -1);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After battery count: X = {1}", loggingModID, valueX);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("Parallel")))
		{
			//Debug.Log(currentPlate.Join());
			valueX += currentPlate.Contains("Serial") ? -4 : 5;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After port plates with parallel ports: X = {1}", loggingModID, valueX);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("DVI")))
		{
			//Debug.Log(currentPlate.Join());
			valueX += currentPlate.Contains("StereoRCA") ? 4 : -5;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After port plates with DVI-D ports: X = {1}", loggingModID, valueX);
		valueX = Mathf.Abs(valueX);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After absolute value: X = {1}", loggingModID, valueX);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		int monthOfStart = DateTime.Now.Month;
		int idxStartDOW = Array.IndexOf(possibleDays, DateTime.Now.DayOfWeek);
		string keyAString = ObtainKeyALegacy();
		string keyBString = keyBTable[idxStartDOW, monthOfStart - 1];
		string keyCString = EncryptUsingPlayfair(keyAString, keyBString);
		string keyDString = ObtainKeyCNew();

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key A: {1}", loggingModID, keyAString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key B: {1}", loggingModID, keyBString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key C: {1}", loggingModID, keyCString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key D: {1}", loggingModID, keyDString);

		ModifyBaseAlphabetLegacy();

		// Distinguishing Ciphers
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Which Ciphers Are Used------------", loggingModID);
		string[] baseCipherList = {
			"Playfair Cipher (Key A)",
			"Playfair Cipher (Key B)",
			"Playfair Cipher (Key C)",
			"Playfair Cipher (Key D)",
			"Caesar Cipher (Value A)",
			"ROT13 Cipher",
			"Affine Cipher (Value X)",
			"Atbash Cipher",
			"Basic Columnar Transposition",
			"Myszkowski Transposition",
			"Anagram Shuffler",
			"Scytale Transposition",
			"Autokey Cipher",
			"Four Square Cipher",
			"Redefence Transposition"
		};
		int[] idxCipherList = new int[] { 2, 6, 4, 0, 1, 3, 13, 5, 7, 9, 12, 11, 8, 10, 14 };
		/* The Ciphers are given based on a given set of instructions. Indexes are defined by the following:
		 * 0, 1, 2, 3 are Playfair Ciphers with keys A, B, C, D respectively.
		 * 4, 5 are Caesar Cipher with values A and 13 respectively.
		 * 6 is Affine Cipher with value x.
		 * 7 is Atbash Cipher
		 * 8 is Basic Columnar Transposition
		 * 9 is Myszkowski Transposition
		 * 10 is Anagram Shuffler
		 * 11 is Scytale Transposition
		 * 12 is Autokey Cipher
		 * 13 is Four Square Cipher
		 * 14 is Redefence Transposition
		 */
		List<string> allModIDs = bombInfo.GetModuleIDs();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Conditions Taken:", loggingModID);
		if (idxCurModIDDisplay != 2)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Module ID is displayed in Broken or Fixed Roman Numerals.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 3 ? 0 : 1).ToArray();
		}
		if (idxCurStrikeDisplay == 2)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Strike Counter is displayed in Arabic Numerals.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 5 ? 0 : a == 11 ? 2 : 1).ToArray();
		}
		if (!allModIDs.Contains("unfairCipher"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Unfair Cipher is not present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => new int[] { 0, 2, 4 }.Contains(a) ? 1 : 0).ToArray();
		}
		if (allModIDs.Contains("orangeCipher"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Orange Cipher is present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 13 ? 0 : 1).ToArray();
		}
		if (!allModIDs.Contains("Alphabetize") && allModIDs.Contains("ReverseAlphabetize"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Alphabetize is not present but Reverse Alphabetize is.", loggingModID);
			int idxRT = Array.IndexOf(idxCipherList, 12), idxAB = Array.IndexOf(idxCipherList, 7);
			int temp = idxCipherList[idxAB];
			idxCipherList[idxAB] = idxCipherList[idxRT];
			idxCipherList[idxRT] = temp;
		}
		if (allModIDs.Contains("CryptModule"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Cryptography is present.", loggingModID);
			List<int> ciphersEvenPos = new List<int>(), ciphersOddPos = new List<int>();
			for (int x = 0; x < idxCipherList.Length; x++)
			{
				if ((x + 1) % 2 == 0)
				{
					ciphersEvenPos.Add(idxCipherList[x]);
				}
				else
				{
					ciphersOddPos.Add(idxCipherList[x]);
				}
			}
			ciphersEvenPos.Reverse();
			int curPos = 0;
			for (int x = 0; x < ciphersEvenPos.Count; x++)
			{
				idxCipherList[curPos] = ciphersEvenPos[x];
				curPos++;
			}
			for (int x = 0; x < ciphersOddPos.Count; x++)
			{
				idxCipherList[curPos] = ciphersOddPos[x];
				curPos++;
			}
		}
		if (allModIDs.Contains("AnagramsModule"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Anagrams is present.", loggingModID);
			idxCipherList = idxCipherList.OrderBy(a => a == 10 ? 0 : 1).ToArray();
		}
		if (allModIDs.Contains("WordScrambleModule"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Word Scramble is present.", loggingModID);
			int temp = idxCipherList[2];
			idxCipherList[2] = idxCipherList[idxCipherList.Length - 3];
			idxCipherList[idxCipherList.Length - 3] = temp;
		}
		if (allModIDs.Contains("blackCipher"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Black Cipher is present.", loggingModID);
			int idxRT = Array.IndexOf(idxCipherList, 14), firstId = idxCipherList.FirstOrDefault();
			idxCipherList[idxRT] = firstId;
			idxCipherList[0] = 14;

			if (columnalTranspositionLst.Length <= 3)
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Columnar Transposition key has 3 or fewer numbers.", loggingModID);
				int lastId = idxCipherList.LastOrDefault(), idxBCT = Array.IndexOf(idxCipherList, 8);
				idxCipherList[idxBCT] = lastId;
				idxCipherList[idxCipherList.Length - 1] = 8;
			}
		}
		if (curColorList[2] == "Yellow")
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The NE colored button is Yellow.", loggingModID);
			IEnumerable<int> firstFour = idxCipherList.Take(4);
			idxCipherList = idxCipherList.OrderBy(a => firstFour.Contains(a) ? 1 : 0).ToArray();
		}
		if (Mathf.Abs(curColorList.IndexOf("Red") - curColorList.IndexOf("Cyan")) == 3)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Red is diametrically opposite to Cyan.", loggingModID);
			int idxR13 = Array.IndexOf(idxCipherList, 5), idxAS = Array.IndexOf(idxCipherList, 10), idxBCT = Array.IndexOf(idxCipherList, 8), idxMT = Array.IndexOf(idxCipherList, 9);
			int temp = idxCipherList[idxMT];
			idxCipherList[idxMT] = idxCipherList[idxR13];
			idxCipherList[idxR13] = temp;
			temp = idxCipherList[idxBCT];
			idxCipherList[idxBCT] = idxCipherList[idxAS];
			idxCipherList[idxAS] = temp;
		}
		if (curColorList.IndexOf("Yellow") < 3 && curColorList.IndexOf("Blue") < 3)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Yellow and Blue are both on the upper half.", loggingModID);
			int lastOne = idxCipherList.Last();
			idxCipherList = idxCipherList.OrderBy(a => a == lastOne ? 0 : 1).ToArray();
		}
		if (curColorList.IndexOf("Yellow") >= 3 && curColorList.IndexOf("Blue") >= 3)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Yellow and Blue are both on the lower half.", loggingModID);
			int firstOne = idxCipherList.First();
			idxCipherList = idxCipherList.OrderBy(a => a == firstOne ? 1 : 0).ToArray();
		}
		if (allModIDs.Contains("unfairsRevenge"))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Unfair's Revenge is present.", loggingModID);
			int stepsToCyan = 0;
			while (curColorList[stepsToCyan] != "Cyan" && stepsToCyan < curColorList.Count)
				stepsToCyan++;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: It takes {1} steps to reach Cyan, starting from the NW colored button and going CW.", loggingModID, stepsToCyan);
			for (int x = 0; x < stepsToCyan; x++)
			{
				int firstOne = idxCipherList.First();
				idxCipherList = idxCipherList.OrderBy(a => a == firstOne ? 1 : 0).ToArray();
			}
		}
		if (valueX % 13 == 6)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Value X is 13n + 6.", loggingModID);
			int idxAff = Array.IndexOf(idxCipherList, 6), idxAB = Array.IndexOf(idxCipherList, 7);
			int temp = idxCipherList[idxAff];
			idxCipherList[idxAff] = idxCipherList[idxAB];
			idxCipherList[idxAB] = temp;
			idxCipherList = idxCipherList.Where(a => a != 6).ToArray();
		}

		if (valueA % 26 == 0)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Value A is a multiple of 26.", loggingModID);

			int idxR13 = Array.IndexOf(idxCipherList, 5), idxCC = Array.IndexOf(idxCipherList, 4);
			int temp = idxCipherList[idxR13];
			idxCipherList[idxR13] = idxCipherList[idxCC];
			idxCipherList[idxCC] = temp;
			idxCipherList = idxCipherList.Where(a => a != 4).ToArray();
		}


		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The final order of the Cipher List is the following:", loggingModID);
		for (int x = 0; x < idxCipherList.Length; x++)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: {2}: {1}", loggingModID, baseCipherList[idxCipherList[x]], x + 1);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);

		// Generate non-conflicting instructions.
		do
		{
			splittedInstructions.Clear();
			GenerateInstructions();
		}
		while (splittedInstructions.Select(a => a.Replace(baseAlphabet[9], baseAlphabet[8])).Distinct().Count() != 6);
		// For each splitted instruction, replace any (10th letters) with (9th letters) and check if they are distinct to each other to have a length of 6.


		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -----------------------Encrypting-----------------------", loggingModID);
		// The Encryption Format
		List<int> firstCiphersIdx = idxCipherList.Take(5).ToList();
		List<string> encryptedResults = new List<string>();
		string[] directionSamples = { "NW", "N", "NE", "SE", "S", "SW" };
		string baseString = splittedInstructions.Join("");
		for (int x = 0; x < firstCiphersIdx.Count; x++)
		{
			string currentString = x == 0 ? baseString : encryptedResults.Last();
			switch (firstCiphersIdx[x])
			{
				case 0:
					{// Playfair Cipher with Key A
						encryptedResults.Add(EncryptUsingPlayfair(currentString.Replace(baseAlphabet[9], baseAlphabet[8]), keyAString.Replace(baseAlphabet[9], baseAlphabet[8]), true));
						break;
					}
				case 1:
					{// Playfair Cipher with Key B
						encryptedResults.Add(EncryptUsingPlayfair(currentString.Replace(baseAlphabet[9], baseAlphabet[8]), keyBString.Replace(baseAlphabet[9], baseAlphabet[8]), true));
						break;
					}
				case 2:
					{// Playfair Cipher with Key C
						encryptedResults.Add(EncryptUsingPlayfair(currentString.Replace(baseAlphabet[9], baseAlphabet[8]), keyCString.Replace(baseAlphabet[9], baseAlphabet[8]), true));
						break;
					}
				case 3:
					{// Playfair Cipher with Key D
						encryptedResults.Add(EncryptUsingPlayfair(currentString.Replace(baseAlphabet[9], baseAlphabet[8]), keyDString.Replace(baseAlphabet[9], baseAlphabet[8]), true));
						break;
					}
				case 4:
					{// Caesar Cipher with Value A
						encryptedResults.Add(EncryptUsingCaesar(currentString, valueA));
						break;
					}
				case 5:
					{// ROT 13 Cipher
						encryptedResults.Add(EncryptUsingCaesar(currentString, 13));
						break;
					}
				case 6:
					{// Affine Cipher with Value X
						encryptedResults.Add(EncryptUsingAffine(currentString, valueX));
						break;
					}
				case 7:
					{// Atbash Cipher
						encryptedResults.Add(EncryptUsingAtbash(currentString));
						break;
					}
				case 8:
					{// Basic Columnar Transposition
						encryptedResults.Add(EncryptUsingBasicColumnar(currentString, columnalTranspositionLst));
						break;
					}
				case 9:
					{// Myszkowski Transposition
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+----Mysckowski Transposition Preparations----+-", loggingModID);
						int sumSerNumDigits = bombInfo.GetSerialNumberNumbers().Sum();
						string selectedKey = myszkowskiKeywords[sumSerNumDigits % 28];
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon using Myszkowski Transposition, the sum of the serial number digits is {1}, which lands on the keyword: \"{2}\"", loggingModID, sumSerNumDigits, selectedKey);
						encryptedResults.Add(EncryptUsingMyszkowskiTransposition(currentString, selectedKey));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
					}
				case 10:
					{// Anagram Shuffler
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--------Anagram Shuffler Preparations--------+-", loggingModID);
						int selectedRow = (swapPigpenAndStandard ? 1 : 0) + (swapStandardKeys ? 2 : 0);
						int baseColIdx = curColorList.IndexOf("Green"), encryptColIdx = curColorList.IndexOf("Magenta");

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Row Used: {1} ({2}, {3})", loggingModID, selectedRow + 1, swapPigpenAndStandard ? "Pigpen Set at the top" : "Pigpen Set at the bottom", swapStandardKeys ? "Columnar Transposition key is to the right of the Autokey Cipher false keyword" : "Columnar Transposition key is to the left of the Autokey Cipher false keyword");
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Green button is on the {1} which corresponds to base set \"{2}.\"", loggingModID, directionSamples[baseColIdx], anagramValues[selectedRow][baseColIdx]);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Magenta button is on the {1} which corresponds to base set \"{2}.\"", loggingModID, directionSamples[encryptColIdx], anagramValues[selectedRow][encryptColIdx]);

						string[] baseWord = anagramValues[selectedRow][baseColIdx].Split(), encryptWord = anagramValues[selectedRow][encryptColIdx].Split();

						if (baseWord.Length == 2 && !bombInfo.GetSerialNumberLetters().Any(a => "AEIOU".Contains(a)))
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The base key consists of 2 words and there is no vowel in the serial number.", loggingModID);
							baseWord = baseWord.Reverse().ToArray();
						}
						if (encryptWord.Length == 2 && bombInfo.GetBatteryHolderCount() % 2 == 1)
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The encryption key consists of 2 words and there is an odd number of battery holders.", loggingModID);
							encryptWord = encryptWord.Reverse().ToArray();
						}
						string baseWordFinal = baseWord.Join(""), encryptWordFinal = encryptWord.Join("");
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon using Anagram Shuffler, the base key used is {1} and the encryption key used is {2}", loggingModID, baseWordFinal, encryptWordFinal);
						encryptedResults.Add(EncryptUsingAnagramShuffler(currentString, baseWordFinal, encryptWordFinal));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
					}
				case 11:
					{// Scytale Transposition
						int portCount = bombInfo.GetPortCount();
						encryptedResults.Add(EncryptUsingScytaleTransposition(currentString, portCount % 4 + 2));
						break;
					}
				case 12:
					{// Autokey Cipher
						string encryptionKey = useEven ? wordSearchWordsEven[randomIdx] : wordSearchWordsOdd[randomIdx];
						encryptedResults.Add(EncryptUsingAutoKeyCaesarCipher(currentString, encryptionKey, true));
						break;
					}
				case 13:
					{// Four Square Cipher
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--------Four Square Cipher Preparations--------+-", loggingModID);

						bool[] trueRules =
						{
							bombInfo.GetBatteryCount() == 3,
							bombInfo.GetPortCount() == 2,
							bombInfo.IsIndicatorPresent(Indicator.BOB),
							false,
							bombInfo.GetIndicators().Count() % 2 == 1,
							bombInfo.IsIndicatorPresent(Indicator.FRK),
							bombInfo.GetSerialNumberNumbers().FirstOrDefault() % 2 == 1,
							bombInfo.GetIndicators().Count() == 2,
							true,
							bombInfo.GetSerialNumberLetters().Count() >= 3,
							bombInfo.GetIndicators().Count() < 2,
							true,
							!bombInfo.GetSerialNumberNumbers().Any(a => new int[] { 0, 2, 4, 6, 8 }.Contains(a)),
							bombInfo.GetModuleIDs().Count() > 30,
							bombInfo.GetBatteryHolderCount() < 3,
						};
						string[] possibleStrings = {
							"NZYIFSUJWBDGVCAHMXTKLQEPOR",
							"AOXBRYGHWFNLDMJQVZSKCTUPEI",
							"ZPDYVKAUQWMCTLXJNHSGOFEIRB",
							"RYCBENFZVQTSLWPXMKAGIHJUDO",
							"ALDNUBSTVRXZOWFCIHEJGPQYKM",
							"OMRSNCGTZYDFQAVPIBXHELKUJW",
							"UHKTLEPQNJMIZOCDRWVSXFBAYG",
							"EBUYZLRCDXWOKQIGTAMSNPHVFJ",
							"YQMGRPFHSUNCEZTABVWKLDJIOX",
							"XBOJNYQUZFVALTKPGCWESRHIMD",
							"TWGCYNBXQKAUDZEJIMROSLHFVP",
							"KTPQBJCEISAYZNOUXGMRDWLHVF",
							"DXUAGEHMCJTOQSLRWPFVZBINKY",
							"CBDJUHOVLFIKSXPZRWQGETYAMN",
							"JIYEPUCAFKGNOQBWZDVLXMRTSH",
						};
						int[] trueIdxs = Enumerable.Range(0, 15).Where(a => trueRules[a]).ToArray(),
							falseIdxs = Enumerable.Range(0, 15).Where(a => !trueRules[a]).ToArray();
						int idxFirstTrue = trueIdxs.FirstOrDefault(), idxLastFalse = falseIdxs.LastOrDefault();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The following rules from Reverse Alphabetize (At 0 solves, 0 strikes) are true: [ {1} ]", loggingModID, trueIdxs.Select(a => a + 1).Join(", "));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The following rules from Reverse Alphabetize (At 0 solves, 0 strikes) are false: [ {1} ]", loggingModID, falseIdxs.Select(a => a + 1).Join(", "));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The first true rule is in row {1}", loggingModID, idxFirstTrue + 1);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The last false rule is in row {1}", loggingModID, idxLastFalse + 1);
						List<int> modifiedTrueInts = trueIdxs.ToList();
						while (modifiedTrueInts.Count > 2)
						{
							modifiedTrueInts.Remove(modifiedTrueInts.Max());
							modifiedTrueInts.Remove(modifiedTrueInts.Min());
						}
						string encryptionStringA = "";
						if (modifiedTrueInts.Count == 1)
						{
							int medianVal = modifiedTrueInts.Single();
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon reaching a single number, the median row used is row {1}", loggingModID, medianVal + 1);
							encryptionStringA = possibleStrings[medianVal];
						}
						else if (modifiedTrueInts.Sum() % 2 == 0)
						{
							int medianVal = modifiedTrueInts.Sum() / 2;
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon reaching to a pair of numbers, the median row used is row {1}", loggingModID, medianVal + 1);
							encryptionStringA = possibleStrings[medianVal];
						}
						else
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon reaching to a pair of numbers, the median row does not exist.", loggingModID);
							encryptionStringA = baseAlphabet;
						}
						encryptionStringA = encryptionStringA.Replace(baseAlphabet[9], baseAlphabet[8]);
						encryptedResults.Add(EncryptUsingFourSquare(currentString.Replace(baseAlphabet[9], baseAlphabet[8]), encryptionStringA, possibleStrings[idxFirstTrue].Replace(baseAlphabet[9], baseAlphabet[8]), possibleStrings[idxLastFalse].Replace(baseAlphabet[9], baseAlphabet[8]), fourSquareKey.Replace(baseAlphabet[9], baseAlphabet[8]), true));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+-----------------------------------------------+-", loggingModID);
						break;
					}
				case 14:
					{// Redefence Transposition
						encryptedResults.Add(EncryptUsingRedefenceTranspositon(currentString, columnalTranspositionLst));
						break;
					}
			}
		}
		for (int y = encryptedResults.Count - 1; y >= 0; y--)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After {2}: {1}", loggingModID, encryptedResults[y], baseCipherList[firstCiphersIdx[y]]);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Generated instructions: {1}", loggingModID, splittedInstructions.Join(", "));
		displayPigpenText = FitToScreen(encryptedResults.Any() ? encryptedResults.Last() : splittedInstructions.Join(""), 13);
		StartCoroutine(TypePigpenText(encryptedResults.Any() ? encryptedResults.Last() : splittedInstructions.Join("")));
		TwitchHelpMessage += " This is the legacy version of Unfair's Cruel Revenge. Use the legacy manual to assist you with this.";
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: --------------------------------------------------------", loggingModID);
	}
	// End Legacy Unfair's Cruel Revenge Handling
	void PrepModule()
	{
		StartCoroutine(IndicatorCoreHandlerExtraScreen.HandleIndicatorModification(4));
		idxColorList.Shuffle();
		initialIdxColorList = idxColorList.ToArray();
		List<string> curColorList = idxColorList.Select(a => baseColorList[a]).ToList();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: How in the world did he forget about this module!? At least it's updated. You are currently using the newer ruleset for Unfair's Cruel Revenge.", loggingModID);
		if (harderUCR)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Hard Mode Unfair's Cruel Revenge has been activated. I hope you are prepared.", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: {2} colors in clockwise order (starting on the NW button): {1}", loggingModID, curColorList.Join(", "), harderUCR ? "Initial button" : "Button");
		StartCoroutine(HandleStartUpAnim());
		//StartCoroutine(TypePigpenText(FitToScreen("ABCDEFGHIJKLMNOPQRSTVUWXYZABCDEFGHIJKLM",13)));
		mainDisplay.text = "";
		// Basic Columnar Transposition Set
		var possibleSizes = Enumerable.Range(2, 6);
		columnalTranspositionLst = Enumerable.Range(0, possibleSizes.PickRandom()).ToArray();
		columnalTranspositionLst.Shuffle();
		// Extra Pigpen Key
		for (int x = 0; x < 12; x++)
		{
			fourSquareKey += baseAlphabet.PickRandom();
		}

		// Autokey Keyword Mislead
		int randomIdx = uernd.Range(0, wordSearchWordsEven.Length);
		bool useEven = uernd.Range(0, 2) == 1;
		selectedWord = useEven ? wordSearchWordsOdd[randomIdx] : wordSearchWordsEven[randomIdx];
		uCipherWord = uCipherWordBank.allStrings.PickRandom();

		idxCurModIDDisplay = uernd.Range(0, 3);
		idxCurStrikeDisplay = uernd.Range(0, 3);
		swapPigpenAndStandard = uernd.Range(0, 2) == 1;
		swapStandardKeys = uernd.Range(0, 2) == 1;
		// Generate Key A
		var base24Keys = base36Reference.Keys.Take(24);
        for (var x = 0; x < 7; x++)
        {
			keyABaseKey += base24Keys.PickRandom();
		}



		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Mod ID grabbed: {1} Keep in mind this can differ from the ID used for logging!", loggingModID, selectedModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Mod ID is in {1} Numerals", loggingModID, tableRoman[idxCurModIDDisplay]);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The strike counter is in {1} Numerals", loggingModID, tableRoman[idxCurStrikeDisplay]);
		// Value A Calculations
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -------------Value A Calculations-------------", loggingModID);
		int valueA = 0;
		char[] vowelList = { 'A', 'E', 'I', 'O', 'U' };
		int portTypeCount = bombInfo.GetPorts().Distinct().Count(),
			portPlateCount = bombInfo.GetPortPlateCount(),
			consonantCount = bombInfo.GetSerialNumberLetters().Where(a => !vowelList.Contains(a)).Count(),
			vowelCount = bombInfo.GetSerialNumberLetters().Where(a => vowelList.Contains(a)).Count(),
			litCount = bombInfo.GetOnIndicators().Count(),
			unlitCount = bombInfo.GetOffIndicators().Count(),
			batteryCount = bombInfo.GetBatteryCount();
		// For every port type
		valueA -= 2 * portTypeCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many distant port types: {1}, Value A logged at {2}", loggingModID, portTypeCount, valueA);
		// For every port plate
		valueA += 1 * portPlateCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many port plates: {1}, Value A logged at {2}", loggingModID, portPlateCount, valueA);
		// For every consonant in the serial number
		valueA += 1 * consonantCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many consonants in the serial number: {1}, Value A logged at {2}", loggingModID, consonantCount, valueA);
		// For every vowel in the serial number
		valueA -= 2 * vowelCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many vowels in the serial number: {1}, Value A logged at {2}", loggingModID, vowelCount, valueA);
		// For every lit indicator
		valueA += 2 * litCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many lit indicators: {1}, Value A logged at {2}", loggingModID, litCount, valueA);
		// For every unlit indicator
		valueA -= 2 * unlitCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many unlit indicators: {1}, Value A logged at {2}", loggingModID, unlitCount, valueA);
		if (batteryCount == 0)
			valueA += 10;
		else
			valueA -= 1 * batteryCount;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are this many batteries: {1}, Value A logged at {2}", loggingModID, batteryCount, valueA);
		if (bombInfo.GetPortCount() == 0)
		{
			valueA *= 2;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are no ports. Value A logged at {1}", loggingModID, valueA);
		}
		if (bombInfo.GetSolvableModuleIDs().Count() >= 31)
		{
			valueA /= 2;
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are 31 or more modules on the bomb, including itself. Value A logged at {1}", loggingModID, valueA);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		// Value X calculations
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -------------Value X Calculations-------------", loggingModID);
		int valueX = 0;
		Dictionary<string, int> indicatorMultipler = new Dictionary<string, int> {
			{"BOB", 1 },{"CAR", 1 },{"CLR", 1 },
			{"FRK", 2 },{"FRQ", 2 },{"MSA", 2 },{"NSA", 2 },
			{"SIG", 3 },{"SND", 3 },{"TRN", 3 },
		};
		foreach (string ind in bombInfo.GetIndicators())
		{
			if (indicatorMultipler.ContainsKey(ind))
			{
				valueX += indicatorMultipler[ind] * (bombInfo.IsIndicatorOff(ind) ? -1 : bombInfo.IsIndicatorOn(ind) ? 1 : 0);
			}
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After indicators: X = {1}", loggingModID, valueX);
		valueX += 4 * (bombInfo.GetBatteryCount() % 2 == 1 ? 1 : -1);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After battery count: X = {1}", loggingModID, valueX);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("Parallel")))
		{
			//Debug.Log(currentPlate.Join());
			valueX += currentPlate.Contains("Serial") ? -4 : 5;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After port plates with parallel ports: X = {1}", loggingModID, valueX);
		foreach (IEnumerable<string> currentPlate in bombInfo.GetPortPlates().Where(a => a.Contains("DVI")))
		{
			//Debug.Log(currentPlate.Join());
			valueX += currentPlate.Contains("StereoRCA") ? 4 : -5;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After port plates with DVI-D ports: X = {1}", loggingModID, valueX);
		valueX = Mathf.Abs(valueX);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After absolute value: X = {1}", loggingModID, valueX);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		int monthOfStart = DateTime.Now.Month;
		int idxStartDOW = Array.IndexOf(possibleDays, DateTime.Now.DayOfWeek);
		string keyAString = ObtainKeyA();
		string keyBString = keyBTable[idxStartDOW, monthOfStart - 1];
		string keyCString = ObtainKeyCNew();

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key A: {1}", loggingModID, keyAString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key B: {1}", loggingModID, keyBString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Key C: {1}", loggingModID, keyCString);

		ModifyBaseAlphabet();



		// Distinguishing Ciphers
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Which Ciphers Are Used------------", loggingModID);
		Dictionary<int,string> baseCipherList = new Dictionary<int, string>() {
			{ 0, "Substitution Playfair Cipher (Key A)" },
			{ 1, "Substitution Playfair Cipher (Key B)" },
			{ 2, "Substitution Playfair Cipher (Key C)" },
			{ 3, "Caesar Cipher (Value A)" },
			{ 4, "ROT13 Cipher" },
			{ 5, "Affine Cipher (Value X)" },
			{ 6, "Atbash Cipher" },
			{ 7, "Basic Columnar Transposition" },
			{ 8, "Myszkowski Transposition" },
			{ 9, "Anagram Shuffler" },
			{ 10, "Scytale Transposition" },
			{ 11, "Autokey Mechanical Cipher" },
			{ 12, "Substitution Four Square Cipher" },
			{ 13, "Redefence Transposition" },
			{ 14, "Monoalphabetic Substitution" },
			{ 15, "Running Key Alberti Cipher" },
		};
		/* The Ciphers are given based on a given set of instructions. Indexes are defined by the following:
		 * 0, 1, 2 are Playfair Ciphers with keys A, B, C respectively.
		 * 3, 4 are Caesar Cipher with values A and 13 respectively.
		 * 5 is Affine Cipher with value X.
		 * 6 is Atbash Cipher
		 * 7 is Basic Columnar Transposition
		 * 8 is Myszkowski Transposition
		 * 9 is Anagram Shuffler
		 * 10 is Scytale Transposition
		 * 11 is Autokey Mechanical Cipher
		 * 12 is Four Square Cipher
		 * 13 is Redefence Transposition
		 * 14 is Monoalphabetic Substitution
		 * 15 is Modified Alberti Cipher
		 */
		int[] longestIdxCiphers = { 0, 1, 2, 11, 12, 15 },
			quickCipherIdxes = { 3, 4, 5, 6, 14 },
			transpositionCipherIdxes = { 7, 8, 9, 10, 13 };
		// Remove ambiguity decryption by removing ciphers that contradict with the base rule
		if (Mathf.Abs(valueX % 13) == 6)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Value X is 13n + 6. Affine Cipher will NOT show up as a possible cipher to prevent ambiguity.", loggingModID);
			quickCipherIdxes = quickCipherIdxes.Where(a => a != 5).ToArray();
		}

		// Generate non-conflicting instructions.
		var iterationCount = 0;
		do
		{
			splittedInstructions.Clear();
			GenerateInstructions(harderUCR ? 9 : 5);
			iterationCount++;
		}
		while (splittedInstructions.Distinct().Count() != (harderUCR ? 10 : 6) && iterationCount < 10000);
		// Decide what ciphers should be used and how the messages should be encrypted.
		string baseString = splittedInstructions.Join("");
		//Debug.Log(splittedMessages.Join(","));
		// Create a cipher code used to obtain the encryptions.
		int[] cipherIdxesAll = (harderUCR ?
			quickCipherIdxes.Shuffle().Take(3)
			.Concat(longestIdxCiphers.Shuffle().Take(3))
			.Concat(transpositionCipherIdxes.Shuffle().Take(3))
			: quickCipherIdxes.Shuffle().Take(2)
			.Concat(new[] { longestIdxCiphers.PickRandom() })
			.Concat(transpositionCipherIdxes.Shuffle().Take(2))).ToArray().Shuffle();
		if (allowDebugCiphers)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: DEBUG ENABLED.", loggingModID);
			cipherIdxesAll = debugCipherIdxes.Where(a => a >= 0 && a < 16).ToArray();
		}
		encodingDisplay = cipherIdxesAll.Select(b => (idxCurStrikeDisplay == 2 && idxCurModIDDisplay == 2 ? "FEDCBA9876543210" : idxCurStrikeDisplay == 2 || idxCurModIDDisplay == 2 ? "4A981D325E6C7FB0" : "0123456789ABCDEF").ElementAt(b)).Join("");
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Required Ciphers to Disarm: ", loggingModID);
		for (int x = 0; x < cipherIdxesAll.Length; x++)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: {2}: {1}", loggingModID, baseCipherList[cipherIdxesAll[x]], x + 1);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: This will be displayed as the following digits: {1}", loggingModID, encodingDisplay);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);


		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -----------------------Encrypting-----------------------", loggingModID);
		// The Encryption Format
		List<string> groupedEncryptedResults = new List<string>();
		List<int> preppedIdxes = new List<int>();
		string[] directionSamples = { "NW", "N", "NE", "SE", "S", "SW" };
		for (int x = 0; x < cipherIdxesAll.Length; x++)
        {
			var baseAlphabetNot10thLetters = baseAlphabet.Take(9).Concat(baseAlphabet.Skip(10));
			var resultingFinalSubstitutionString = "";
			var currentSubstituionString = "";
			string currentString = baseString;
			if (groupedEncryptedResults.Any())
			{
				currentString = groupedEncryptedResults.Last();
			}
			var lastEncryptedString = groupedEncryptedResults.Join("");
			if (string.IsNullOrEmpty(lastEncryptedString))
				lastEncryptedString = baseString;
			for (int y = 0; y < currentString.Length; y++)
			{
				char selectedLetter = baseAlphabetNot10thLetters.PickRandom();
				while (selectedLetter == lastEncryptedString[y]) // Prevent overlapping the substitution letters in that given position.
					selectedLetter = baseAlphabetNot10thLetters.PickRandom();
				currentSubstituionString += selectedLetter;
			}
		
				
			var curCipherIdx = cipherIdxesAll[x];
			switch (curCipherIdx)
			{
				case 0:
					{// Playfair Cipher with Key A
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------- Playfair Cipher (Key A) ----------+-", loggingModID);
						// Modify the string by substituting the 10th letters
						string modifiedString = "";
						var idx10thLetters = new List<int>();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution letters for this cipher: {1}", loggingModID, currentSubstituionString);
						for (var y = 0; y < currentString.Length; y++)
						{
							var curChar = currentString[y];
							if (curChar == baseAlphabet[9])
							{
								modifiedString += currentSubstituionString[y];
								idx10thLetters.Add(y);
							}
							else
								modifiedString += curChar;
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Message string after substituting 10th letters: {1}", loggingModID, modifiedString);
						var resultingEncryption = EncryptUsingPlayfair(modifiedString, keyAString, true);
						var resultToAdd = "";
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Encrypted string before resubstituting 10th letters: {1}", loggingModID, resultingEncryption);
						for (var y = 0; y < resultingEncryption.Length; y++) // Then resubstitute the 10th letters
						{
							if (idx10thLetters.Contains(y))
							{
								resultToAdd += baseAlphabet[9];
								resultingFinalSubstitutionString += resultingEncryption[y];
							}
							else
							{
								resultToAdd += resultingEncryption[y];
								resultingFinalSubstitutionString += currentSubstituionString[y];
							}
						}
						groupedEncryptedResults.Add(resultToAdd);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution Letters displayed for this cipher: {1}", loggingModID, resultingFinalSubstitutionString);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
					}
				case 1:
					{// Playfair Cipher with Key B
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------- Playfair Cipher (Key B) ----------+-", loggingModID);
						// Modify the string by substituting the 10th letters
						string modifiedString = "";
						var idx10thLetters = new List<int>();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution letters for this cipher: {1}", loggingModID, currentSubstituionString);
						for (var y = 0; y < currentString.Length; y++)
						{
							var curChar = currentString[y];
							if (curChar == baseAlphabet[9])
							{
								modifiedString += currentSubstituionString[y];
								idx10thLetters.Add(y);
							}
							else
								modifiedString += curChar;
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Message string after substituting 10th letters: {1}", loggingModID, modifiedString);
						var resultingEncryption = EncryptUsingPlayfair(modifiedString, keyBString, true);
						var resultToAdd = "";
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Encrypted string before resubstituting 10th letters: {1}", loggingModID, resultingEncryption);
						for (var y = 0; y < resultingEncryption.Length; y++) // Then resubstitute the 10th letters
						{
							if (idx10thLetters.Contains(y))
							{
								resultToAdd += baseAlphabet[9];
								resultingFinalSubstitutionString += resultingEncryption[y];
							}
							else
							{
								resultToAdd += resultingEncryption[y];
								resultingFinalSubstitutionString += currentSubstituionString[y];
							}
						}
						groupedEncryptedResults.Add(resultToAdd);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution Letters displayed for this cipher: {1}", loggingModID, resultingFinalSubstitutionString);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
					}
				case 2:
					{// Playfair Cipher with Key C
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------- Playfair Cipher (Key C) ----------+-", loggingModID);
						// Modify the string by substituting the 10th letters
						string modifiedString = "";
						var idx10thLetters = new List<int>();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution letters for this cipher: {1}", loggingModID, currentSubstituionString);
						for (var y = 0; y < currentString.Length; y++)
						{
							var curChar = currentString[y];
							if (curChar == baseAlphabet[9])
							{
								modifiedString += currentSubstituionString[y];
								idx10thLetters.Add(y);
							}
							else
								modifiedString += curChar;
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Message string after substituting 10th letters: {1}", loggingModID, modifiedString);
						var resultingEncryption = EncryptUsingPlayfair(modifiedString, keyCString, true);
						var resultToAdd = "";
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Encrypted string before resubstituting 10th letters: {1}", loggingModID, resultingEncryption);
						for (var y = 0; y < resultingEncryption.Length; y++) // Then resubstitute the 10th letters
						{
							if (idx10thLetters.Contains(y))
							{
								resultToAdd += baseAlphabet[9];
								resultingFinalSubstitutionString += resultingEncryption[y];
							}
							else
							{
								resultToAdd += resultingEncryption[y];
								resultingFinalSubstitutionString += currentSubstituionString[y];
							}
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution Letters displayed for this cipher: {1}", loggingModID, resultingFinalSubstitutionString);
						groupedEncryptedResults.Add(resultToAdd);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						break;
					}
				case 3:
					{// Caesar Cipher with Value A
						groupedEncryptedResults.Add(EncryptUsingCaesar(currentString, valueA));
						goto default;
					}
				case 4:
					{// ROT 13 Cipher
						groupedEncryptedResults.Add(EncryptUsingCaesar(currentString, 13));
						goto default;
					}
				case 5:
					{// Affine Cipher with Value X
						groupedEncryptedResults.Add(EncryptUsingAffine(currentString, valueX));
						goto default;
					}
				case 6:
					{// Atbash Cipher
						groupedEncryptedResults.Add(EncryptUsingAtbash(currentString));
						goto default;
					}
				case 7:
					{// Basic Columnar Transposition
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+-------- Basic Columnar Transposition --------+-", loggingModID);
						groupedEncryptedResults.Add(EncryptUsingBasicColumnar(currentString, columnalTranspositionLst, true));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+----------------------------------------------+-", loggingModID);
						goto default;
					}
				case 8:
					{// Myszkowski Transposition
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+----Mysckowski Transposition Preparations----+-", loggingModID);
						int sumSerNumDigits = bombInfo.GetSerialNumberNumbers().Sum();
						string selectedKey = myszkowskiKeywords[sumSerNumDigits % 28];
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon using Myszkowski Transposition, the sum of the serial number digits is {1}, which lands on the keyword: \"{2}\"", loggingModID, sumSerNumDigits, selectedKey);
						var keywordAlphabeticalOrder = baseAlphabet.Where(a => selectedKey.Contains(a)).Join("");
						var numberSet = selectedKey.Select(a => keywordAlphabeticalOrder.IndexOf(a) + 1);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Using the modified alphabet the number string obtained from this should be: \"{2}\"", loggingModID, sumSerNumDigits, numberSet.Join(""));
						groupedEncryptedResults.Add(EncryptUsingMyszkowskiTransposition(currentString, numberSet));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						goto default;
					}
				case 9:
					{// Anagram Shuffler
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--------Anagram Shuffler Preparations--------+-", loggingModID);
						int selectedRow = (swapPigpenAndStandard ? 1 : 0) + (swapStandardKeys ? 2 : 0);
						int baseColIdx = curColorList.IndexOf("Green"), encryptColIdx = curColorList.IndexOf("Magenta");
							
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Row Used: {1} ({2}, {3})", loggingModID, selectedRow + 1,
							swapPigpenAndStandard ? "Pigpen Set above the Autokey/Col Trans key" : "Pigpen Set below the Autokey/Col Trans key",
							swapStandardKeys ? "Columnar Transposition key is to the right of the Autokey Cipher false keyword" : "Columnar Transposition key is to the left of the Autokey Cipher false keyword");
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Green button is on the {1} which corresponds to base set \"{2}.\"", loggingModID, directionSamples[baseColIdx], anagramValues[selectedRow][baseColIdx]);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The Magenta button is on the {1} which corresponds to base set \"{2}.\"", loggingModID, directionSamples[encryptColIdx], anagramValues[selectedRow][encryptColIdx]);
							
						string[] baseWord = anagramValues[selectedRow][baseColIdx].Split(), encryptWord = anagramValues[selectedRow][encryptColIdx].Split();

						if (baseWord.Length == 2 && !bombInfo.GetSerialNumberLetters().Any(a => "AEIOU".Contains(a)))
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The base key consists of 2 words and there is no vowel in the serial number.", loggingModID);
							baseWord = baseWord.Reverse().ToArray();
						}
						if (encryptWord.Length == 2 && bombInfo.GetBatteryHolderCount() % 2 == 1)
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The encryption key consists of 2 words and there is an odd number of battery holders.", loggingModID);
							encryptWord = encryptWord.Reverse().ToArray();
						}
						string baseWordFinal = baseWord.Join(""), encryptWordFinal = encryptWord.Join("");

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Upon using Anagram Shuffler, the base key used is {1} and the encryption key used is {2}", loggingModID, baseWordFinal, encryptWordFinal);
						groupedEncryptedResults.Add(EncryptUsingAnagramShuffler(currentString, baseWordFinal, encryptWordFinal));

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+---------------------------------------------+-", loggingModID);
						goto default;
					}
				case 10:
					{// Scytale Transposition
						int portCount = bombInfo.GetPortCount();
						groupedEncryptedResults.Add(EncryptUsingScytaleTransposition(currentString, portCount % 4 + 2));
						goto default;
					}
				case 11:
					{// Autokey Mech Cipher
						string encryptionKey = useEven ? wordSearchWordsEven[randomIdx] : wordSearchWordsOdd[randomIdx];
						groupedEncryptedResults.Add(EncryptUsingAutoKeyMechCipher(currentString, encryptionKey, true));
						goto default;
					}
				case 12:
					{// Four Square Cipher
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+-------- Substitution Four Square Cipher --------+-", loggingModID);
						// Modify the string by substituting the 10th letters
						string modifiedString = "";
						var idx10thLetters = new List<int>();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution letters for this cipher: {1}", loggingModID, currentSubstituionString);
						for (var y = 0; y < currentString.Length; y++)
						{
							var curChar = currentString[y];
							if (curChar == baseAlphabet[9])
							{
								modifiedString += currentSubstituionString[y];
								idx10thLetters.Add(y);
							}
							else
								modifiedString += curChar;
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Message string after substituting 10th letters: {1}", loggingModID, modifiedString);
						var resultingEncryption = EncryptUsingFourSquare(modifiedString, keyAString, keyBString, keyCString, fourSquareKey, true);
						var resultToAdd = "";
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Encrypted string before resubstituting 10th letters: {1}", loggingModID, resultingEncryption);
						for (var y = 0; y < resultingEncryption.Length; y++) // Then resubstitute the 10th letters
						{
							if (idx10thLetters.Contains(y))
							{
								resultToAdd += baseAlphabet[9];
								resultingFinalSubstitutionString += resultingEncryption[y];
							}
							else
							{
								resultToAdd += resultingEncryption[y];
								resultingFinalSubstitutionString += currentSubstituionString[y];
							}
						}
						groupedEncryptedResults.Add(resultToAdd);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Substitution Letters displayed for this cipher: {1}", loggingModID, resultingFinalSubstitutionString);
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+-------------------------------------------------+-", loggingModID);
						break;
					}
				case 13:
					{// Redefence Transposition
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+------------ Redefence Transposition ------------+-", loggingModID);
						groupedEncryptedResults.Add(EncryptUsingRedefenceTranspositon(currentString, columnalTranspositionLst, true));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+-------------------------------------------------+-", loggingModID);
						goto default;
					}
				case 14:
					{// Monoalphabetic Substitution

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--+--+--+--Monoalphabetic Substitution Preparations--+--+--+--+-", loggingModID);
						var monoalphabeticEncryptString = uCipherWord.Distinct().Join("");

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Starting word without duplicates: {1}", loggingModID, monoalphabeticEncryptString);
						if (idxColorList[2] == 1)
						{
							monoalphabeticEncryptString = baseAlphabet.Where(a => !uCipherWord.Contains(a)).Join("") + monoalphabeticEncryptString;

							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The {1} button is Yellow.", loggingModID, directionSamples[2]);
						}
						else
						{
							monoalphabeticEncryptString += baseAlphabet.Where(a => !uCipherWord.Contains(a)).Join("");

							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The {1} button is not Yellow.", loggingModID, directionSamples[2]);

						}

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: As a result, you should get the key: {1}", loggingModID, monoalphabeticEncryptString);
						groupedEncryptedResults.Add(EncryptUsingMonoalphabeticSubstitution(currentString, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", monoalphabeticEncryptString));

						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+-", loggingModID);
						goto default;
					}
				case 15:
					{// Running Key Alberti Cipher
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--+--+--+-Running Key Alberti Cipher Preparations-+--+--+--+-", loggingModID);
						var passageWords = new string[][] {
						new[] { "THIS","CIPHER","MAY","BE","COMPLICATED","TO","MASTER","THE","FIRST","TIME","AROUND","YOU","WILL",
							"NEED","TO","USE","THESE","PARAGRAPHS","TO","OBTAIN","THE","KEY","THAT","ENCRYPTED","YOUR","MESSAGE" },
						new[] { "THE","ALBERTI","WHEEL","WHICH","YOU","WILL","CREATE","CONSISTS","OF","TWO","PARTS","THE","STATIONARY",
							"WHEEL","OR","STABILIS","AND","THE","MOVING","WHEEL","OR","MOBILIS","TO","CREATE","THE","WHEEL" },
						new[] { "MAKE","THE","STABILIS","WITH","THE","ENGLISH","ALPHABET","IN","ORDER","THEN","TO","MAKE","THE",
							"MOBILIS","SEPARATE","THE","MODIFIED","ALPHABET","BY","EVERY","OTHER","LETTER","TO","OBTAIN","TWO","HALVES" },
						new[] { "IF","RED","IS","DIAMETRICALLY","OPPOSITE","TO","CYAN","REVERSE","THE","FIRST","HALF","AND","SWAP",
							"THOSE","HALVES","OTHERWISE","SWAP","THE","SECOND","HALF","AND","THEN","COCATENATE","THE","HALVES","TOGETHER" },
						new[] { "MARK","YOUR","ANCHOR","LETTER","IN","YOUR","MOBILIS","THE","FIRST","LETTER","IN","THE","SERIAL",
							"NUMBER","IF","THERE","ARE","ANY","OTHERWISE","MARK","A","AS","YOUR","ANCHOR","LETTER","INSTEAD" },
						new[] { "NOW","TAKE","THE","FIRST","TWO","LETTERS","IN","THE","TWELVE","LETTER","PIGPEN","TEXT","AND",
							"CONVERT","THEM","INTO","THEIR","ENGLISH","LETTERS","USING","STEP","TWO","FROM","THIS","MANUAL","PROVIDED" },
						new[] { "CONVERT","THESE","INTO","THEIR","POSITIONS","IN","THE","MODIFIED","ALPHABET","START","ON","THE","PARAGRAPH",
							"FROM","THE","FIRST","NUMBER","OBTAINED","WHERE","ONE","OR","FOURTEEN","IS","THE","TOP","PARAGRAPH" },
						new[] { "USING","THE","SECOND","NUMBER","COUNT","THAT","MANY","WORDS","FROM","THE","PARAGRAPH","YOU","OBTAINED",
						"EARLIER","TO","OBTAIN","THE","START","OF","YOUR","RUNNING","KEY","ADD","THE","STARTING","WORD" },
						new[] { "IGNORE","PUNCTUATION","AND","FONT","STYLES","FOR","EACH","WORD","YOU","APPEND","ONTO","YOUR","KEY",
						"REPEAT","UNTIL","THE","KEY","IS","AS","LONG","OR","LONGER","THAN","YOUR","ENCRYPTED","MESSAGE" },
						new[] { "EACH","RELEVANT","PARAGRAPH","IN","THIS","PAGE","IS","EXACTLY","TWENTY","SIX","WORDS","LONG","CONTINUE",
						"TO","THE","FIRST","WORD","OF","THE","NEXT","PARAGRAPH","IF","YOU","REACH","THE","END" },
						new[] { "TO","DECRYPT","YOUR","MESSAGE","ROTATE","THE","MOBILIS","SO","THAT","THE","ANCHOR","LETTER","IS",
						"DIRECTLY","BELOW","THE","LETTER","IN","THE","STABILIS","FOR","EACH","LETTER","IN","THE","KEY" },
						new[] { "EXAMINE","THE","ENCRYPTED","LETTER","IN","YOUR","MOBILIS","TO","GET","YOUR","DECRYPTED","LETTER","IN",
						"YOUR","STABILIS","IN","THE","SAME","POSITION","REPEAT","UNTIL","YOU","HAVE","YOUR","DECRYPTED","STRING" },
						new[] { "WHEN","OBTAINING","YOUR","KEY","IF","YOU","REACH","THE","END","OF","THE","LAST","PARAGRAPH",
						"WRAP","AROUND","TO","THE","FIRST","WORD","OF","THE","FIRST","PARAGRAPH","UPON","REACHING","THIS" },
						}; // The entire page in the manual on how to decrypt Running Key Alberti Cipher, excluding titles and flavor text.
						int currentParaIdx = baseAlphabet.IndexOf(fourSquareKey[0]), currentWordIdx = baseAlphabet.IndexOf(fourSquareKey[1]);
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The first letter in the Four Square Key is {1} (Pos {2} in the modified alphabet)", loggingModID, fourSquareKey[0], currentParaIdx + 1);
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The second letter in the Four Square Key is {1} (Pos {2} in the modified alphabet)", loggingModID, fourSquareKey[1], currentWordIdx + 1);
						currentParaIdx %= passageWords.Length;
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Count {1} paragraph(s) and then {2} word(s) in that paragraph to get the start of the running key.", loggingModID, currentParaIdx + 1, currentWordIdx + 1);
						string encryptedMessage = "";
						while (encryptedMessage.Length < currentString.Length)
						{

							encryptedMessage += passageWords[currentParaIdx][currentWordIdx];
							currentWordIdx++;
							if (currentWordIdx >= passageWords[currentParaIdx].Length)
							{
								currentWordIdx = 0;
								currentParaIdx++;
								if (currentParaIdx >= passageWords.Length)
									currentParaIdx = 0;
							}
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: You should get \"{1}\" as the result of your Running Key.", loggingModID, encryptedMessage);
						var baseAlphabetHalves = new List<string>();
						for (var y = 0; y < 2; y++)
						{
							string givenHalf = "";
							var curIdx = y;
							while (curIdx < baseAlphabet.Length)
							{
								givenHalf += baseAlphabet[curIdx];
								curIdx += 2;
							}
							baseAlphabetHalves.Add(givenHalf);
						}
						if (Mathf.Abs(curColorList.IndexOf("Red") - curColorList.IndexOf("Cyan")) == 3)
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Red and Cyan are diametrically opposite to each other.", loggingModID);
							baseAlphabetHalves.Reverse();
						}
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Red and Cyan are not diametrically opposite to each other.", loggingModID);

						baseAlphabetHalves[1] = baseAlphabetHalves[1].Reverse().Join("");
						var combinedMobius = baseAlphabetHalves.Join("");
						var serialNoLetters = bombInfo.GetSerialNumberLetters();
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Your Mobilis wheel should be: \"{1}\"", loggingModID, combinedMobius);
						groupedEncryptedResults.Add(EncryptUsingAlbertiCipher(currentString, encryptedMessage, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", combinedMobius, combinedMobius.IndexOf(serialNoLetters.Any() ? serialNoLetters.First() : 'A')));
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: -+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+-", loggingModID);
						goto default;
					}
				default:
                    {
						//Debug.Log(curSubstitutionLetters);
						//Debug.Log(groupedEncryptedResults[curIdxSelected].LastOrDefault());
						for (var y = 0; y < currentString.Length; y++)
						{
							resultingFinalSubstitutionString += currentSubstituionString[y];
						}
						break;
                    }
			}
			if (!preppedIdxes.Contains(curCipherIdx))
				preppedIdxes.Add(curCipherIdx);
			displaySubstutionLettersAll.Add(resultingFinalSubstitutionString);
		}
		for (int y = groupedEncryptedResults.Count - 1; y >= 0; y--)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After {2}: {1}", loggingModID, groupedEncryptedResults[y], baseCipherList[cipherIdxesAll[y]]);
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Generated instructions: {1}", loggingModID, splittedInstructions.Join(", "));
		var displayResult = groupedEncryptedResults.LastOrDefault().Join("");

		displayPigpenText = FitToScreen(string.IsNullOrEmpty(displayResult) ? splittedInstructions.Join("") : displayResult, 13);
		StartCoroutine(TypePigpenText(string.IsNullOrEmpty(displayResult) ? splittedInstructions.Join("") : displayResult));
		TwitchHelpMessage += harderUCR ? " DO NOT chain colored button presses on this module. The colored buttons change after every press!" : "";
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: --------------------------------------------------------", loggingModID);
		// Section for testing purposes. To ensure ciphers and transpositions work as intended
		/*
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ---------------------Test Encryptions---------------------", loggingModID);
		string playfairEncryptedString = EncryptUsingPlayfair(baseString.Replace(baseAlphabet[9], baseAlphabet[8]), keyCString, true),
		  affineEncryptedString = EncryptUsingAffine(baseString, valueX),
		  caesarEncryptedString = EncryptUsingCaesar(baseString, valueA),
		  columnarTransposedString = EncryptUsingBasicColumnar(baseString, columnalTranspositionLst),
		  scytaleTransposedString = EncryptUsingScytaleTransposition(baseString, 6),
		  myszowkTransposedString = EncryptUsingMyszkowskiTransposition(baseString, "BANANA"),
		  myszowkNumberTransposedString = EncryptUsingMyszkowskiTransposition(baseString, new[] { 3, 2, 2, 1 }),
		  fourSquareString = EncryptUsingFourSquare(baseString.Replace(baseAlphabet[9], baseAlphabet[8]), "ALPHA", "BRAVO", "YANKEE", "ZULU", true),
		  anagramShuffledString = EncryptUsingAnagramShuffler(baseString, "EAT", "ATE"),
		  autoKeyEncryptedString = EncryptUsingAutoKeyMechCipher(baseString, "OMEGA", true),
		  atbashEncryptedString = EncryptUsingAtbash(baseString),
		  redefenceEncryptedString = EncryptUsingRedefenceTranspositon(baseString, columnalTranspositionLst),
		  monoalphabeticEncryptedString = EncryptUsingMonoalphabeticSubstitution(baseString, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "QAZWSXEDCRFVTGBYHNUJMIKOLP"),
		  albertiEncryptedString = EncryptUsingAlbertiCipher(baseString, "HELPMEIMLOSTINVOID", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ACEGIKMOQSUWYZXVTRPNLJHFDB", 10);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Myszkowski Transposed String: {1}", loggingModID, myszowkTransposedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Myszkowski Transposed String (With Numbers): {1}", loggingModID, myszowkNumberTransposedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Scytale Transposed String: {1}", loggingModID, scytaleTransposedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Columnar Transposed String: {1}", loggingModID, columnarTransposedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Caesar Encrypted String: {1}", loggingModID, caesarEncryptedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Affine Encrypted String: {1}", loggingModID, affineEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Atbash Encrypted String: {1}", loggingModID, atbashEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Playfair Encrypted String: {1}", loggingModID, playfairEncryptedString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Anagram Shuffled String: {1}", loggingModID, anagramShuffledString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Four Square Encrypted String: {1}", loggingModID, fourSquareString);
        Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Autokey Encrypted String: {1}", loggingModID, autoKeyEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Redefence Encrypted String: {1}", loggingModID, redefenceEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Monoalphabetic Substitution Encrypted String: {1}", loggingModID, monoalphabeticEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Alberti Cipher Encrypted String: {1}", loggingModID, albertiEncryptedString);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------------", loggingModID);
		*/
	}
	readonly int[] primeFactors = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997 };
	IEnumerable<int> GetDistinctFactors(int value)
	{
		var output = new List<int>();
		while (value > 1)
		{
			var primeDetected = true;
			if (primeFactors.Contains(value))
			{
				output.Add(value);
				break;
			}
			var inRangePrimes = primeFactors.Where(a => a < value);
			for (var x = 0; x < inRangePrimes.Count(); x++)
			{
				var curPrime = inRangePrimes.ElementAt(x);
				if (value % curPrime == 0)
				{
					value /= curPrime;
					primeDetected = false;
					output.Add(curPrime);
					break;
				}
			}
			if (primeDetected)
			{
				output.Add(value);
				break;
			}
		}

		return output.Distinct();
	}
	string EncryptUsingAlbertiCipher(string input, string key, string stationaryAlphabet, string movableAlphabet, int anchorIdx = 0)
    {
		/*
		 * Example:
		 *
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * ACEGIKMOQSUWYZXVTRPNLJHFDB
		 * ^
		 * Message: ALONEINTHEDARK
		 * Key:     NIGHTTIMEHOWLS
		 *
		 * Shift the bottom string until the anchor is lined up with the letter of reference above
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * ZXVTRPNLJHFDBACEGIKMOQSUWY
		 *              ^
		 * Alternatively, grab the index of the key letter and move X units left to the alphabet
		 * after obtaining the position of that letter
		 * where X is the distance between the anchor letter in the wheel in the initial state and the referenced letter in the stationary key
		 *
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * ACEGIKMOQSUWYZXVTRPNLJHFDB
		 * ^ - - - - - -|
		 * =            ! - - - - - -
		 * Result of first letter: Z
		 *
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * ACEGIKMOQSUWYZXVTRPNLJHFDB
		 * ^ - - - |
		 *    ! - - - =
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * PNLJHFDBACEGIKMOQSUWYZXVTR
		 *         ^
		 * Result of the second letter: G
		 *
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * ACEGIKMOQSUWYZXVTRPNLJHFDB
		 * ^ - - | ! - - =
		 *
		 * ABCDEFGHIJKLMNOPQRSTUVWXYZ
		 * LJHFDBACEGIKMOQSUWYZXVTRPN
		 *       ^
		 * Result of the third letter: Q
		 */
		if (stationaryAlphabet.Length != movableAlphabet.Length)
			throw new FormatException(string.Format("The unencrypted alphabet and the encrypted alphabet do not have the same length of characters!"));

		string modifiedKey = key.ToString();
		while (modifiedKey.Length < input.Length)
		{
			modifiedKey = (modifiedKey + key).Substring(0, input.Length);
		}
		string output = "";
		for (var x = 0; x < input.Length; x++)
        {

			output += movableAlphabet[
				(movableAlphabet.Length + stationaryAlphabet.IndexOf(input[x]) + anchorIdx - stationaryAlphabet.IndexOf(modifiedKey[x]))
				% movableAlphabet.Length];
        }

		return output;
	}
	string EncryptUsingMonoalphabeticSubstitution(string input, string unencryptedAlphabet, string encryptedAlphabet)
    {
		if (unencryptedAlphabet.Length != encryptedAlphabet.Length)
			throw new FormatException(string.Format("The unencrypted alphabet and the encrypted alphabet do not have the same length of characters!"));
        string output = "";
        for (var x = 0; x < input.Length; x++)
        {
			output += encryptedAlphabet[unencryptedAlphabet.IndexOf(input[x])];
        }
		return output;
    }
	string EncryptUsingCaesar(string input, int valueA = 0)
	{// Encrypt the string with the given valueA. Example: "ABCDEFG" + 7 -> "HIJKLMN"
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a)).ToArray();
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] += valueA;
			while (stringInputs[x] < 0)
				stringInputs[x] += baseAlphabet.Length;
			stringInputs[x] %= baseAlphabet.Length;
		}

		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingAffine(string input, int valueX = 0)
	{
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a) + 1).ToArray();
		int multiplier = valueX * 2 + 1;
		for (int x = 0; x < stringInputs.Length; x++)
		{
			stringInputs[x] *= multiplier;
			while (stringInputs[x] > baseAlphabet.Length)
				stringInputs[x] -= baseAlphabet.Length;
			stringInputs[x]--;
		}
		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string DecryptUsingAffine(string input, int valueX = 0)
	{
		int[] stringInputs = input.Select(a => baseAlphabet.IndexOf(a) + 1).ToArray();
		int multiplier = valueX * 2 + 1;
		if (!GetDistinctFactors(baseAlphabet.Length).Contains(multiplier))
			for (int x = 0; x < stringInputs.Length; x++)
			{
				while (stringInputs[x] % multiplier != 0 )
					stringInputs[x] += baseAlphabet.Length;
				stringInputs[x] /= multiplier;
				stringInputs[x]--;
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
				stringInputs[x] += baseAlphabet.Length;
			stringInputs[x]--;
			stringInputs[x] %= baseAlphabet.Length;
		}
		return stringInputs.Select(a => baseAlphabet[a]).Join("");
	}
	string EncryptUsingPlayfair(string input, string keyword = "", bool logSquares = false)
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
		 * To Encrypt:		BENT ON HER HEELS
		 * Key Pairs:		BE NT ON HE RH EE LS
		 * Expected Result:	LR IO WC ER CE EE KT
		 *
		 */

		string modifiedKeyword = keyword.Replace(baseAlphabet[9].ToString(), "").Distinct().Join(""),
			playfairGridBase = modifiedKeyword + baseAlphabet.Replace(baseAlphabet[9].ToString(), "").Distinct().Where(a => !modifiedKeyword.Distinct().Contains(a)).Join("");
		if (logSquares)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Given Playfair set: {1}", loggingModID, playfairGridBase);
		if (input.Length % 2 != 0) input += baseAlphabet[23];
		string output = "";
		for (int y = 0; y < input.Length; y += 2)
		{
			string currentSet = input.Substring(y, 2);
			if (currentSet.Contains(baseAlphabet[9]))
				throw new IndexOutOfRangeException(string.Format("The current set of letters {0} contains the 10th letter in the alphabet which cannot be used for encrypting!", currentSet));
			if (currentSet.Distinct().Count() == 1)
			{
				output += currentSet;
				continue;
			}
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
				//Debug.Log(string.Format("{0},{1}", rowIdxs[x], colIdxs[x]));

				output += playfairGridBase[5 * rowIdxs[x] + colIdxs[x]];
			}
		}
		return output;
	}
	string EncryptUsingBasicColumnar(string input, int[] key, bool logColumnarKey = false)
	{
		/* Example:
		 *
		 * To Encrypt:		GREATJOBMATE
		 * Key:				1342
		 * Process:			GREA TJOB MATE
		 *
		 * Expected Result:	GTM ABE RJA EOT
		 *
		 */
		if (logColumnarKey)
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Given Columnar Set: {1}", loggingModID, key.Select(a => a + 1).Join(""));
		List<string> splittedInput = new List<string>();
		for (int x = 0; x < key.Length; x++)
		{
			string curVal = "";
			int curIdx = x;
			while (curIdx < input.Length)
			{
				curVal += input[curIdx];
				curIdx += key.Length;
			}
			splittedInput.Add(curVal);
		}
		return splittedInput.OrderBy(a => key[splittedInput.IndexOf(a)]).Join("");
	}
	string EncryptUsingScytaleTransposition(string input, int rowCount = 2)
	{
		if (rowCount < 1)
			throw new FormatException(string.Format("{0} is not a valid length for encrypting with Scytale Transposition!",rowCount));
		string output = "";
		for (int x = 0; x < rowCount; x++)
		{
			int curPos = x;
			while (curPos < input.Length)
			{
				output += input[curPos];
				curPos += rowCount;
			}
		}
		return output;
	}
	string EncryptUsingMyszkowskiTransposition(string input, string keyword)
	{
		/* Example:
		 *
		 * To Encrypt:		GREATJOBMATE
		 * Keyword:			BOOT
		 * Alphabetical Order: A-Z standard
		 * Process:			GREA TJOB MATE
		 *
		 * Expected Result:	GTM RE JO AT ABE
		 *
		 */
		if (!keyword.Any(a => char.IsLetter(a)))
			throw new FormatException(string.Format("\"{0}\" is not a valid word for encrypting with Myszkowski Transposition!", keyword));
		char[] allChars = keyword.Where(a => char.IsLetter(a)).ToArray();
		List<string> separatedSets = new List<string>();
		for (int x = 0; x < keyword.Length; x++)
		{
			int curPos = x;
			string curVal = "";
			while (curPos < input.Length)
			{
				curVal += input[curPos];
				curPos += keyword.Length;
			}
			separatedSets.Add(curVal);
		}
		string output = "";
		List<char> sortedLetters = allChars.OrderBy(a => baseAlphabet.IndexOf(a)).Distinct().ToList();
		IEnumerable<int> intLists = sortedLetters.Select(a => sortedLetters.IndexOf(a));
		int[] keywordLists = allChars.Select(a => sortedLetters.IndexOf(a)).ToArray();
		for (int x = 0; x < intLists.Count(); x++)
		{
			List<string> currentSet = new List<string>();
			for (int y = 0; y < keywordLists.Length; y++)
			{
				if (keywordLists[y] == intLists.ElementAt(x))
					currentSet.Add(separatedSets[y]);
			}

			for (int pos = 0; pos < currentSet.Select(a => a.Length).Max(); pos++)
            {
				for (int y = 0; y < currentSet.Count(); y++)
                {
					if (pos < currentSet[y].Length)
						output += currentSet[y][pos];
                }
            }
		}
		return output;
	}
	string EncryptUsingMyszkowskiTransposition(string input, IEnumerable<int> key)
	{
		/* Example:
		 *
		 * To Encrypt:		GREATJOBMATE
		 * Keyword:			1223
		 * Process:			GREA TJOB MATE
		 *
		 * Expected Result:	GTM RE JO AT ABE
		 *
		 */
		if (!key.Any())
			throw new FormatException(string.Format("The key has no numbers for encrypting with Myszkowski Transposition!", key));
		List<string> separatedSets = new List<string>();
		for (int x = 0; x < key.Count(); x++)
		{
			int curPos = x;
			string curVal = "";
			while (curPos < input.Length)
			{
				curVal += input[curPos];
				curPos += key.Count();
			}
			separatedSets.Add(curVal);
		}
		string output = "";
		var sortedValues = key.OrderBy(a => a).Distinct().ToArray();
		var tempList = new List<int>();
        for (var x = 0; x < separatedSets.Count; x++) { tempList.Add(x); }
		foreach (var aValue in sortedValues)
        {
			List<string> currentSet = new List<string>();
			for (int y = 0; y < key.Count(); y++)
			{
				if (key.ElementAt(y) == aValue)
					currentSet.Add(separatedSets[y]);
			}

			for (int pos = 0; pos < currentSet.Select(a => a.Length).Max(); pos++)
			{
				for (int y = 0; y < currentSet.Count(); y++)
				{
					if (pos < currentSet[y].Length)
						output += currentSet[y][pos];
				}
			}
		}

		return output;
	}
	string EncryptUsingFourSquare(string input, string keywordA = "", string keywordB = "", string keywordC = "", string keywordD = "", bool logSquares = false)
	{
		string modifiedKeywordA = keywordA.Replace(baseAlphabet[9].ToString(), "").Distinct().Join(""),
			modifiedKeywordB = keywordB.Replace(baseAlphabet[9].ToString(), "").Distinct().Join(""),
			modifiedKeywordC = keywordC.Replace(baseAlphabet[9].ToString(), "").Distinct().Join(""),
			modifiedKeywordD = keywordD.Replace(baseAlphabet[9].ToString(), "").Distinct().Join("");
		string gridA = modifiedKeywordA + baseAlphabet.Replace(baseAlphabet[9].ToString(), "").Distinct().Where(a => !modifiedKeywordA.Distinct().Contains(a)).Join(""),
			gridB = modifiedKeywordB + baseAlphabet.Replace(baseAlphabet[9].ToString(), "").Distinct().Where(a => !modifiedKeywordB.Distinct().Contains(a)).Join(""),
			gridC = modifiedKeywordC + baseAlphabet.Replace(baseAlphabet[9].ToString(), "").Distinct().Where(a => !modifiedKeywordC.Distinct().Contains(a)).Join(""),
			gridD = modifiedKeywordD + baseAlphabet.Replace(baseAlphabet[9].ToString(),"").Distinct().Where(a => !modifiedKeywordD.Distinct().Contains(a)).Join("");
		if (logSquares)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid A: {1}", loggingModID, gridA);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid B: {1}", loggingModID, gridB);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid C: {1}", loggingModID, gridC);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Grid D: {1}", loggingModID, gridD);
		}
		if (input.Length % 2 != 0) input += baseAlphabet[23];
		string output = "";
		for (int y = 0; y < input.Length; y += 2)
		{
			string currentSet = input.Substring(y, 2);
			if (currentSet.Contains(baseAlphabet[9]))
				throw new IndexOutOfRangeException(string.Format("The current set of letters {0} contains the 10th letter in the alphabet which cannot be used for encrypting!",currentSet));
            int[] rowIdxs = new int[] { gridA.IndexOf(currentSet[0]) / 5, gridD.IndexOf(currentSet[1]) / 5 };
            int[] colIdxs = new int[] { gridD.IndexOf(currentSet[1]) % 5, gridA.IndexOf(currentSet[0]) % 5 };
			output += gridB[rowIdxs[0] * 5 + colIdxs[0]].ToString() + gridC[rowIdxs[1] * 5 + colIdxs[1]].ToString();
		}
		return output;
    }
	string EncryptUsingAnagramShuffler(string input, string keywordA, string keywordB)
    {
		if (!keywordA.Distinct().OrderBy(a => a).SequenceEqual(keywordB.Distinct().OrderBy(a => a)))
			throw new FormatException(string.Format("\"{0}\" and \"{1}\" are not anagrams for encrypting with Anagram Shuffler!", keywordA, keywordB));
		List<string> separatedSets = new List<string>();
		for (int x = 0; x < keywordA.Length; x++)
		{
			int curPos = x;
			string curVal = "";
			while (curPos < input.Length)
			{
				curVal += input[curPos];
				curPos += keywordA.Length;
			}
			separatedSets.Add(curVal);
		}
		int[] sortedList = keywordB.Select(x => keywordA.IndexOf(x)).ToArray();
		string output = "";
		for (int x=0;x<sortedList.Select(a => separatedSets[a].Length).Max();x++)
        {
			for (int y=0;y<sortedList.Length;y++)
            {
				if (separatedSets[sortedList[y]].Length > x)
					output += separatedSets[sortedList[y]][x];

			}
        }
		return output;
	}
	string alphabetColTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	string[] mechtable = new[] {
		"TJONXAKIPCZDWUSRQMYEBHFVGL",
		"NXAKIPCZDWUSRQMYEBHFVGLTJO",
		"VGLTJONXAKIPCZDWUSRQMYEBHF",
		"KIPCZDWUSRQMYEBHFVGLTJONXA",
		"BHFVGLTJONXAKIPCZDWUSRQMYE",
		"JONXAKIPCZDWUSRQMYEBHFVGLT",
		"LTJONXAKIPCZDWUSRQMYEBHFVG",
		"WUSRQMYEBHFVGLTJONXAKIPCZD",
		"FVGLTJONXAKIPCZDWUSRQMYEBH",
		"PCZDWUSRQMYEBHFVGLTJONXAKI",
		"YEBHFVGLTJONXAKIPCZDWUSRQM",
		"ONXAKIPCZDWUSRQMYEBHFVGLTJ",
		"EBHFVGLTJONXAKIPCZDWUSRQMY",
		"CZDWUSRQMYEBHFVGLTJONXAKIP",
		"XAKIPCZDWUSRQMYEBHFVGLTJON",
		"MYEBHFVGLTJONXAKIPCZDWUSRQ",
		"QMYEBHFVGLTJONXAKIPCZDWUSR",
		"USRQMYEBHFVGLTJONXAKIPCZDW",
		"GLTJONXAKIPCZDWUSRQMYEBHFV",
		"RQMYEBHFVGLTJONXAKIPCZDWUS",
		"AKIPCZDWUSRQMYEBHFVGLTJONX",
		"SRQMYEBHFVGLTJONXAKIPCZDWU",
		"ZDWUSRQMYEBHFVGLTJONXAKIPC",
		"DWUSRQMYEBHFVGLTJONXAKIPCZ",
		"HFVGLTJONXAKIPCZDWUSRQMYEB",
		"IPCZDWUSRQMYEBHFVGLTJONXAK",
	};
	string EncryptUsingAutoKeyCaesarCipher(string input, string keyword = "", bool logModifiedKeywordAndPositions = false)
	{
		string appendedKeyword = keyword + (keyword.Length >= input.Length ? "" : input.Substring(0, input.Length - keyword.Length));
		if (logModifiedKeywordAndPositions)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: AutoKey Cipher Keyword + PlainText: {1}", loggingModID, appendedKeyword);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Converting all of the letters in the key to their alphabetical positions results in this set of numbers: {1}", loggingModID, appendedKeyword.Select(a => baseAlphabet.IndexOf(a) + 1).Join(","));
		}
		string output = "";
		for (int x = 0; x < input.Length; x++)
		{
			int idxInput = baseAlphabet.IndexOf(input[x]), idxKey = baseAlphabet.IndexOf(appendedKeyword[x]);
			output += baseAlphabet[(idxInput + idxKey) % 26];
		}
		return output;
	}
	string EncryptUsingAutoKeyMechCipher(string input, string keyword = "", bool logModifiedKeywordAndPositions = false)
    {
		string appendedKeyword = keyword + (keyword.Length >= input.Length ? "" : input.Substring(0, input.Length - keyword.Length));
		if (logModifiedKeywordAndPositions)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: AutoKey Cipher Keyword + PlainText: {1}", loggingModID, appendedKeyword);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Converting all of the letters in the key to their alphabetical positions results in this set of numbers: {1}", loggingModID, appendedKeyword.Select(a => baseAlphabet.IndexOf(a) + 1).Join(","));
		}
		string output = "";
        for (int x = 0; x < input.Length; x++)
        {
			int idxInput = alphabetColTable.IndexOf(input[x]), idxKey = baseAlphabet.IndexOf(appendedKeyword[x]);
            output += mechtable[idxKey][idxInput];
        }
		return output;
    }
	string EncryptUsingRedefenceTranspositon(string input, int[] key, bool logValues = false)
    {
		string[] separtedSets = new string[key.Length];
		if (logValues) Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Redefence Transposition Key: {1}", loggingModID, key.Select(x => x + 1).Join(""));
		int curPos = 1;
		bool dirBack = true;
        for (int x = 0; x < input.Length; x++)
        {
			if (dirBack)
            {
				curPos--;
				if (curPos <= 0)
					dirBack = false;
            }
			else
            {
				curPos++;
				if (curPos >= key.Length - 1)
					dirBack = true;
            }
			separtedSets[curPos] += input[x];
		}
		return key.OrderBy(a => a).Select(a => separtedSets[Array.IndexOf(key, a)]).Join("");
    }

	Dictionary<char, int> base36Reference = new Dictionary<char, int>() {
		{'0', 0 }, {'1', 1 }, {'2', 2 }, {'3', 3 }, {'4', 4 }, {'5', 5 },
		{'6', 6 }, {'7', 7 }, {'8', 8 }, {'9', 9 }, {'A', 10 }, {'B', 11 },
		{'C', 12 }, {'D', 13 }, {'E', 14 }, {'F', 15 }, {'G', 16 }, {'H', 17 },
		{'I', 18 }, {'J', 19 }, {'K', 20 }, {'L', 21 }, {'M', 22 }, {'N', 23 },
		{'O', 24 }, {'P', 25 }, {'Q', 26 }, {'R', 27 }, {'S', 28 }, {'T', 29 },
		{'U', 30 }, {'V', 31 }, {'W', 32 }, {'X', 33 }, {'Y', 34 }, {'Z', 35 },
	}
	/*
	,charReference = new Dictionary<char, int>() {
		{'0', 0 }, {'1', 1 }, {'2', 2 }, {'3', 3 }, {'4', 4 }, {'5', 5 },
		{'6', 6 }, {'7', 7 }, {'8', 8 }, {'9', 9 }, {'A', 1 }, {'B', 2 },
		{'C', 3 }, {'D', 4 }, {'E', 5 }, {'F', 6 }, {'G', 7 }, {'H', 8 },
		{'I', 9 }, {'J', 10 }, {'K', 11 }, {'L', 12 }, {'M', 13 }, {'N', 14 },
		{'O', 15 }, {'P', 16 }, {'Q', 17 }, {'R', 18 }, {'S', 19 }, {'T', 20 },
		{'U', 21 }, {'V', 22 }, {'W', 23 }, {'X', 24 }, {'Y', 25 }, {'Z', 26 },
	}*/
	;
	string ObtainKeyALegacy()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Key A Calculations------------", loggingModID);
		string returningString = "";
		string hexDecimalString = "0123456789ABCDEF";
		string base36DigitFull = bombInfo.GetSerialNumber();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Starting Base-36 Value (Serial Number): {1}", loggingModID, base36DigitFull);
		long givenValue = 0;
		for (int x = 0; x < base36DigitFull.Length; x++)
		{
			givenValue *= 36;
			givenValue += base36Reference.ContainsKey(base36DigitFull[x]) ? base36Reference[base36DigitFull[x]] : 12;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Base-10 Value: {1}", loggingModID, givenValue);
		while (givenValue > 0)
		{
			returningString += hexDecimalString[(int)(givenValue % 16)];
			givenValue /= 16;
		}
		returningString = returningString.Reverse().Join("");
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Converting into Hexadecimal: {1}", loggingModID, returningString);
		string output = "";
		string[] listAllPossibilities = new string[] { returningString, selectedModID.ToString(), (bombInfo.GetPortPlateCount() + 1).ToString(), (2 + bombInfo.GetBatteryHolderCount()).ToString() };
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
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Intereperation + ModID, Port Plate, Battery Holder appending: {1}", loggingModID, output);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------------------------------------", loggingModID);
		return output;
	}
	string ObtainKeyA()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Key A Calculations------------", loggingModID);
		string returningString = "";
		string hexDecimalString = "0123456789ABCDEF";
		string base36DigitFull = keyABaseKey;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Starting Base-24 Value: {1}", loggingModID, base36DigitFull);
		long givenValue = 0;
		for (int x = 0; x < base36DigitFull.Length; x++)
		{
			givenValue *= 24;
			givenValue += base36Reference.ContainsKey(base36DigitFull[x]) ? base36Reference[base36DigitFull[x]] : 12;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Base-10 Value: {1}", loggingModID, givenValue);
		while (givenValue > 0)
		{
			returningString += hexDecimalString[(int)(givenValue % 16)];
			givenValue /= 16;
		}
		returningString = returningString.Reverse().Join("");
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Converting into Hexadecimal: {1}", loggingModID, returningString);
		string output = "";
		string[] listAllPossibilities = new string[] { returningString, selectedModID.ToString(), (bombInfo.GetPortPlateCount() + 1).ToString(), (2 + bombInfo.GetBatteryHolderCount()).ToString() };
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
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Intereperation + ModID, Port Plate, Battery Holder appending: {1}", loggingModID, output);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------", loggingModID);
		return output;
	}

	string ObtainKeyCNew()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------Key {1} Calculations------------", loggingModID, legacyUCR ? 'D' : 'C');
		string[] allPossibleStrings = {
			"WLUAZVHEJDNQSYFPGMBOIRCXTK",
			"MVFBXJQNHWZTAKPEOCURISDYLG",
			"DEYQLFZOURNPHVKIMSJACGXTWB",
			"LBMCHNAKVFJOSGDQEPUXRWIZTY",
			"BVUHFMALPINYSGJTRQKEOXWDCZ",
			"OXFGNSKUVPHQJIZWCTRYALEMDB",
			"ESFKUYZOPAWVRJBMIXGDQNTHCL",
			"KHLCISQNPOMBGJZWRAYVXTFDEU",
			"GWYUSDNZQFVALREPKMTOHXBCIJ",
			"JEXUSLCQYNOHKZADFWPRBITMVG",
			"IGYXKZMEULPNAQOVBJTWDSHFCR",
			"RZLAJBFVXTKYNODGWEHCQMPUIS",
			"HTGMSNPXCVYRWQZJFUODEIAKBL",
			"ZFXBJKRNOQCPUTVEHSMIADLYWG",
			"XSCOVHZQPYFABIETGKDJURMLWN"
				};
		bool[] trueConditions = new bool[] {
			bombInfo.GetIndicators().Count() == 2,
			bombInfo.GetPortCount() == 3,
			bombInfo.IsIndicatorPresent(Indicator.CAR),
			true,
			bombInfo.GetBatteryCount() % 2 == 0,
			bombInfo.IsIndicatorPresent(Indicator.MSA),
			"BCDFGHJKLMNPQRSTVWXYZ".Contains(bombInfo.GetSerialNumberLetters().ElementAtOrDefault(0)),
			bombInfo.GetPortPlateCount() == 2,
			bombInfo.GetBatteryCount() % 2 == 1,
			bombInfo.GetSerialNumberNumbers().Count() >= 3,
			bombInfo.GetIndicators().Count() > 2,
			false,
			!bombInfo.GetSerialNumber().Any(a => "AEIOU".Contains(a)),
			bombInfo.GetModuleIDs().Count() > 30,
			bombInfo.GetIndicators().Count() < 4,
		};
		int[] loggingSet = Enumerable.Range(0, 15).Where(a => trueConditions[a]).ToArray();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The following rules from Alphabetize (At 0 solves, 0 strikes) are true: [ {1} ]", loggingModID, loggingSet.Select(a => a + 1).Join(", "));
		int sum = 0;
		for (int x = 0;x<trueConditions.Length;x++)
		{
			sum += !trueConditions[x] ? (x + 1) : 0;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The sum of all of the false rules is {1}", loggingModID, sum);
		string output = sum % trueConditions.Where(a => !a).Count() == 0 ? allPossibleStrings[sum / trueConditions.Count(a => !a) - 1] : allPossibleStrings[14];
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Row used for Key {2}: {1}", loggingModID, sum % trueConditions.Count(a => !a) == 0 ? (sum / trueConditions.Count(a => !a)) : 15, legacyUCR ? 'D' : 'C' );
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ------------------------------------------", loggingModID);
		return output;
	}

	void ModifyBaseAlphabetLegacy()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ---------------Alphabet Modifications---------------", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Base Alphabet: {1}", loggingModID, baseAlphabet);
		string valueToChange = baseAlphabet.ToString();
		int valueModifier = bombInfo.GetSerialNumberNumbers().Any() ? 1 + bombInfo.GetSerialNumberNumbers().Last() : 11;
		valueToChange = valueToChange.Substring(26 - valueModifier) + valueToChange.Substring(0, 26 - valueModifier);

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Serial Number Digit Shift ({2}): {1}", loggingModID, valueToChange, valueModifier);
		if (bombInfo.GetSerialNumberLetters().Any())
		{
			char curLetter = bombInfo.GetSerialNumberLetters().Last();
			int idxCurLetter = valueToChange.IndexOf(curLetter);

			if (idxCurLetter == 0)
				valueToChange = valueToChange.Substring(1) + curLetter;
			else
			{
				valueToChange = curLetter + valueToChange.Substring(0, idxCurLetter) + (idxCurLetter + 1 >= 26 ? "" : valueToChange.Substring(idxCurLetter + 1));
			}
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Serial Number Letter Shifting: {1}", loggingModID, valueToChange);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Conditions Taken:", loggingModID, valueToChange);
		bool containsLitBOB = bombInfo.IsIndicatorOn(Indicator.BOB);
		if (containsLitBOB && bombInfo.GetBatteryCount() == 0 && bombInfo.GetPortPlateCount() == 0 && !bombInfo.GetOffIndicators().Any() && bombInfo.GetSerialNumberLetters().Where(a => "AEIOU".Contains(a)).Any())
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is exactly all of these: Lit BOB, no batteries, no port plates, no unlit indicators, at least a vowel in the serial number", loggingModID);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Threw away the modified alphabet to use the base alphabet instead.", loggingModID);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------", loggingModID);
			return;
		}
		if (containsLitBOB)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is a Lit BOB", loggingModID);
			valueToChange = valueToChange.Reverse().Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.GetBatteryHolderCount() % 2 == 1)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is an odd number of battery holders.", loggingModID);
			List<char> vowelList = new List<char>() { 'A', 'E', 'I', 'O', 'U' };
			if (bombInfo.GetSerialNumberLetters().Contains('W'))
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: 'W' is present in the serial number.", loggingModID);
				vowelList.Add('W');
			}
			valueToChange = valueToChange.OrderBy(a => vowelList.Contains(a) ? 1 : 0).Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.IsPortPresent(Port.DVI))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is a DVI port.", loggingModID);
			valueToChange = valueToChange.Substring(0,13).Reverse().Join("") + valueToChange.Substring(13);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (!bombInfo.IsPortPresent(Port.StereoRCA))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is not a Stereo RCA port.", loggingModID);
			List<char> modifiedList = new List<char>() { 'R','C','A' };
			valueToChange = valueToChange.OrderBy(a => modifiedList.IndexOf(a)).Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.GetBatteryCount() % 2 == 0)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is an even number of batteries.", loggingModID);
			List<int> usablePositions = new List<int> { 4, 6, 8, 9, 10, 14, 15, 21, 22, 25, 26 };
			string stationaryString = "", modifyingString = "";
			for (int x = 1; x <= 26; x++)
			{
				if (usablePositions.Contains(x))
					modifyingString += valueToChange[x - 1];
				else
					stationaryString += valueToChange[x - 1];
			}

			valueToChange = stationaryString + modifyingString.Reverse().Join("");

			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		List<string> detectableModIDs = new List<string>() { "sphere", "yellowArrowsModule", "greenArrowsModule", };
		if (bombInfo.GetModuleIDs().Any(a => detectableModIDs.Contains(a)))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At least 1 of the following modules are present: Green Arrows, Yellow Arrows, The Sphere", loggingModID);
			valueToChange = "LAZYDOG" + valueToChange;
			valueToChange = valueToChange.Distinct().Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		if (bombInfo.GetModuleIDs().Count(a => a.Equals(modSelf.ModuleType)) > 1)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There are duplicate Unfair's Cruel Revenge", loggingModID);
			valueToChange = valueToChange.Substring(0, 13) + valueToChange.Substring(13).Reverse().Join("");
			valueToChange = valueToChange.OrderBy(a => "THEQUICK".IndexOf(a)).Join("");
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
		}
		baseAlphabet = valueToChange;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Final Modified Alphabet String: {1}", loggingModID, baseAlphabet);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------", loggingModID);
	}
	void ModifyBaseAlphabet()
	{
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ---------------Alphabet Modifications---------------", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Base Alphabet: {1}", loggingModID, baseAlphabet);
		string valueToChange = baseAlphabet.ToString();
		int valueModifier = bombInfo.GetSerialNumberNumbers().Any() ? 1 + bombInfo.GetSerialNumberNumbers().Last() : 11;
		valueToChange = valueToChange.Substring(26 - valueModifier) + valueToChange.Substring(0, 26 - valueModifier);

		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Serial Number Digit Shift ({2}): {1}", loggingModID, valueToChange, valueModifier);
		if (bombInfo.GetSerialNumberLetters().Any())
		{
			char curLetter = bombInfo.GetSerialNumberLetters().Last();
			int idxCurLetter = valueToChange.IndexOf(curLetter);

			if (idxCurLetter == 0)
				valueToChange = valueToChange.Substring(1) + curLetter;
			else
			{
				valueToChange = curLetter + valueToChange.Substring(0, idxCurLetter) + (idxCurLetter + 1 >= 26 ? "" : valueToChange.Substring(idxCurLetter + 1));
			}
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: After Serial Number Letter Shifting: {1}", loggingModID, valueToChange);
		string lastEncryptedString = baseAlphabet;
		var appliedConditions = new List<int>();
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Conditions Taken:", loggingModID, valueToChange);
		bool containsLitBOB = bombInfo.IsIndicatorOn(Indicator.BOB);
		if (containsLitBOB && bombInfo.GetBatteryCount() == 0 && bombInfo.GetPortPlateCount() == 0 && !bombInfo.GetOffIndicators().Any() && bombInfo.GetSerialNumberLetters().Any(a => "AEIOU".Contains(a)))
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is exactly all of these: Lit BOB, no batteries, no port plates, no unlit indicators, at least a vowel in the serial number", loggingModID);
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Threw away the modified alphabet to use the base alphabet instead.", loggingModID);
			//Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------", loggingModID);

		}
		else
		{
			if (containsLitBOB)
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is a Lit BOB", loggingModID);
				valueToChange = valueToChange.Reverse().Join("");
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(2);
			}
			if (bombInfo.GetBatteryHolderCount() % 2 == 1)
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is an odd number of battery holders.", loggingModID);
				List<char> vowelList = new List<char>() { 'A', 'E', 'I', 'O', 'U' };
				if (bombInfo.GetSerialNumberLetters().Contains('W'))
				{
					Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: 'W' is present in the serial number.", loggingModID);
					vowelList.Add('W');
				}
				valueToChange = valueToChange.OrderBy(a => vowelList.Contains(a) ? 1 : 0).Join("");
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(3);
			}
			if (bombInfo.GetBatteryCount() % 3 == 0)
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The number of batteries is 3n.", loggingModID);
				List<int> usablePositions = new List<int> { 4, 6, 8, 9, 10, 14, 15, 21, 22, 25, 26 };
				string stationaryString = "", modifyingString = "";
				for (int x = 1; x <= 26; x++)
				{
					if (usablePositions.Contains(x))
						modifyingString += valueToChange[x - 1];
					else
						stationaryString += valueToChange[x - 1];
				}
				valueToChange = stationaryString + modifyingString.Reverse().Join("");
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(4);
			}
			List<string> detectableModIDs = new List<string>() { "sphere", "yellowArrowsModule", "greenArrowsModule", },
				allModIDs = bombInfo.GetModuleIDs();
			if (allModIDs.Any(a => detectableModIDs.Contains(a)))
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At least 1 of the following modules are present: Green Arrows, Yellow Arrows, The Sphere", loggingModID);
				valueToChange = "LAZYDOG" + valueToChange;
				valueToChange = valueToChange.Distinct().Join("");
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(5);
			}
			if (allModIDs.Any(a => a.Equals("unfairsRevenge")))
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Unfair's Revenge is present", loggingModID);
				valueToChange = valueToChange.Substring(0, 13) + valueToChange.Substring(13).Reverse().Join("");
				valueToChange = valueToChange.OrderBy(a => "THEQUICK".IndexOf(a)).Join("");
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(6);
			}
			if (bombInfo.IsPortPresent(Port.DVI))
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is a DVI port.", loggingModID);
				valueToChange = valueToChange.Substring(0, 13).Reverse().Join("") + valueToChange.Substring(13);
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(7);
			}
			if (!bombInfo.IsPortPresent(Port.StereoRCA))
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is not a Stereo RCA port.", loggingModID);
				List<char> modifiedList = new List<char>() { 'R', 'C', 'A' };
				valueToChange = valueToChange.OrderBy(a => modifiedList.IndexOf(a)).Join("");
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(8);
			}
			if (allModIDs.Count(a => a.Equals(modSelf.ModuleType)) > 1)
			{
				lastEncryptedString = valueToChange.ToString();
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Duplicate Unfair's Cruel Revenge are present.", loggingModID);
				var ucrCount = allModIDs.Count(a => a.Equals(modSelf.ModuleType));
				for (int x = 0; x < ucrCount - 1; x++)
				{
					valueToChange = valueToChange.Substring(valueToChange.Length - 10) + valueToChange.Substring(0, valueToChange.Length - 10);
				}
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(9);
			}
			if (appliedConditions.Count() % 2 == 1)
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Up to this condition, an odd number of conditions were met.", loggingModID);
				var temp = valueToChange;
				valueToChange = lastEncryptedString;
				lastEncryptedString = temp;
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(10);
			}
			var serialNoNumbers = bombInfo.GetSerialNumberNumbers();
			if (bombInfo.GetPortPlates().Any(a => a.Length == 0))
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: There is an empty port plate present.", loggingModID);
				lastEncryptedString = valueToChange.ToString();
				List<int> usablePositions = new List<int> { 6, 12, 18, 24 };
				string stationaryString = "", modifyingString = "";
				for (int x = 1; x <= 26; x++)
				{
					if (usablePositions.Contains(x))
						modifyingString += valueToChange[x - 1];
					else
						stationaryString += valueToChange[x - 1];
				}
				valueToChange = modifyingString + stationaryString.Substring(stationaryString.Length - 6) + stationaryString.Substring(0, stationaryString.Length - 6);
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(11);
			}
			else if (allModIDs.Any(a => new[] { "WhosOnFirst", "WhatsOnSecond" }.Contains(a)))
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Who's on First or What's on Second is present and the 11th condition is not met.", loggingModID);
				lastEncryptedString = valueToChange.ToString();
				var idxUH = new[] { valueToChange.IndexOf('U'), valueToChange.IndexOf('H'), };
				string stationaryString = "", modifyingString = "";
				if (Mathf.Abs(idxUH.Min() - idxUH.Max()) == 1)
				{
					Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: \"U\" and \"H\" are adjacent.", loggingModID);
					valueToChange = valueToChange.OrderBy(a => "UH".Contains(a) ? 0 : 1).Join("");
				}
				else
				{
					for (var x = 0; x < valueToChange.Length; x++)
					{
						if (x < idxUH.Max() && x > idxUH.Min())
							modifyingString += valueToChange[x];
						else
							stationaryString += valueToChange[x];
					}
					valueToChange = modifyingString + stationaryString;
				}
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(12);
			}
			if (allModIDs.Contains("blueArrowsModule") && appliedConditions.Any())
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Blue Arrows is present and there was at least 1 previously met condition.", loggingModID);
				if (appliedConditions.Last() == 10)
				{
					Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The last applied condition was the 10th condition. Applying the action from the 12th condition.", loggingModID);
					lastEncryptedString = valueToChange.ToString();
					var idxUH = new[] { valueToChange.IndexOf('U'), valueToChange.IndexOf('H'), };
					string stationaryString = "", modifyingString = "";
					if (Mathf.Abs(idxUH.Min() - idxUH.Max()) == 1)
					{
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: \"U\" and \"H\" are adjacent.", loggingModID);
						valueToChange = valueToChange.OrderBy(a => "UH".Contains(a) ? 0 : 1).Join("");
					}
					else
					{
						for (var x = 0; x < valueToChange.Length; x++)
						{
							if (x < idxUH.Max() && x > idxUH.Min())
								modifyingString += valueToChange[x];
							else
								stationaryString += valueToChange[x];
						}
						valueToChange = modifyingString + stationaryString;
					}
				}
				else
				{
					var temp = valueToChange;
					valueToChange = lastEncryptedString;
					lastEncryptedString = temp;
				}
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Modified Alphabet String after this condition: {1}", loggingModID, valueToChange);
				appliedConditions.Add(13);
			}
			else if (!appliedConditions.Any())
            {
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: No other conditions were met.", loggingModID);
				valueToChange = EncryptUsingCaesar(valueToChange, 13);
				var serialNoFirstDigit = serialNoNumbers.Any() ? serialNoNumbers.First() : 1;
                int[] conditionList = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 11, 12 };
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Applying the action from condition {1}", loggingModID, conditionList[serialNoFirstDigit]);
				switch (serialNoFirstDigit)
                    {
					case 0:
						{
							valueToChange = valueToChange.Reverse().Join("");
						}
						break;
					case 1:
						List<char> vowelList = new List<char>() { 'A', 'E', 'I', 'O', 'U' };
						if (bombInfo.GetSerialNumberLetters().Contains('W'))
						{
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: 'W' is present in the serial number.", loggingModID);
							vowelList.Add('W');
						}
						valueToChange = valueToChange.OrderBy(a => vowelList.Contains(a) ? 1 : 0).Join("");
						break;
					case 2:
						{
							List<int> usablePositions = new List<int> { 4, 6, 8, 9, 10, 14, 15, 21, 22, 25, 26 };
							string stationaryString = "", modifyingString = "";
							for (int x = 1; x <= 26; x++)
							{
								if (usablePositions.Contains(x))
									modifyingString += valueToChange[x - 1];
								else
									stationaryString += valueToChange[x - 1];
							}
							valueToChange = stationaryString + modifyingString.Reverse().Join("");
						}
						break;
					case 3:
						{
							valueToChange = "LAZYDOG" + valueToChange;
							valueToChange = valueToChange.Distinct().Join("");
						}
						break;
					case 4:
						{
							valueToChange = valueToChange.Substring(0, 13) + valueToChange.Substring(13).Reverse().Join("");
							valueToChange = valueToChange.OrderBy(a => "THEQUICK".IndexOf(a)).Join("");
						}
						break;
					case 5:
						{
							valueToChange = valueToChange.Substring(0, 13).Reverse().Join("") + valueToChange.Substring(13);
						}
						break;
					case 6:
						{
							List<char> modifiedList = new List<char>() { 'R', 'C', 'A' };
							valueToChange = valueToChange.OrderBy(a => modifiedList.IndexOf(a)).Join("");
						}
						break;
                    case 7:
						{
							var ucrCount = allModIDs.Count(a => a.Equals(modSelf.ModuleType));
							for (int x = 0; x < ucrCount - 1; x++)
							{
								valueToChange = valueToChange.Substring(10) + valueToChange.Substring(0, 10);
							}
						}
						break;
					case 8:
						{
							List<int> usablePositions = new List<int> { 6, 12, 18, 24 };
							string stationaryString = "", modifyingString = "";
							for (int x = 1; x <= 26; x++)
							{
								if (usablePositions.Contains(x))
									modifyingString += valueToChange[x - 1];
								else
									stationaryString += valueToChange[x - 1];
							}
							valueToChange = modifyingString + stationaryString.Substring(stationaryString.Length - 6) + stationaryString.Substring(0, stationaryString.Length - 6);
						}
						break;
					case 9:
						{
							var idxUH = new[] { valueToChange.IndexOf('U'), valueToChange.IndexOf('H'), };
							string stationaryString = "", modifyingString = "";
							if (Mathf.Abs(idxUH.Min() - idxUH.Max()) == 1)
							{
								Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: \"U\" and \"H\" are adjacent.", loggingModID);
								valueToChange = valueToChange.OrderBy(a => "UH".Contains(a) ? 0 : 1).Join("");
							}
							else
							{
								for (var x = 0; x < valueToChange.Length; x++)
								{
									if (x < idxUH.Max() && x > idxUH.Min())
										modifyingString += valueToChange[x];
									else
										stationaryString += valueToChange[x];
								}
								valueToChange = modifyingString + stationaryString;
							}
						}
						break;
					default:
						break;
					}
				appliedConditions.Add(14);
			}
			baseAlphabet = valueToChange;
		}
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Final Modified Alphabet String: {1}", loggingModID, baseAlphabet);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: From the following applied condition(s): {1}", loggingModID, appliedConditions.Join(", "));
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: ----------------------------------------------------", loggingModID);
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
	public string ValueToBrokenRoman(int value)
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
	public string ValueToFixedRoman(int value)
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
	public void GenerateInstructions(int instructionCount = 5)
	{
		
		List<string> instructionsToShuffle = legacyUCR ? GrabNonOverlappingInstructions(legacyInstructions) : harderUCR ? hardModeInstructions.Concat(extraCruelInstructions).ToList() : hardModeInstructions.ToList();
		instructionsToShuffle.Shuffle();
		splittedInstructions.AddRange(instructionsToShuffle.Take(instructionCount));
		splittedInstructions.Add(legacyUCR ? lastCommands.Take(2).PickRandom() : lastCommands.PickRandom());

	}
	IEnumerator HandleFlashingAnim(int btnIdx)
	{
		if (btnIdx < 0 || btnIdx >= 6) yield break;
		//colorButtonRenderers[btnIdx].material = switchableMats[1];
		var lastColor = isChangingColors ? colorButtonRenderers[btnIdx].material.color : colorWheel[idxColorList[btnIdx]];
		var speed = 2;
        for (float x = 0; x <= 1f; x += Time.deltaTime * speed)
		{
			colorLights[btnIdx].intensity = 1f + x;
			colorButtonRenderers[btnIdx].material.color = Color.white * (1f - x) + lastColor * .75f * x;
			yield return null;
		}
		//colorButtonRenderers[btnIdx].material = switchableMats[0];
		colorButtonRenderers[btnIdx].material.color = lastColor * .75f;
		colorLights[btnIdx].intensity = 1f;
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
			for (int a = 0; a < usedRenderers.Length; a += 1)
			{
				usedRenderers[a].material.color = Color.white;
			}
			yield return new WaitForSeconds(0.25f);
			for (int a = 0; a < usedRenderers.Length; a += 1)
			{
				usedRenderers[a].material.color = Color.black;
			}
			yield return new WaitForSeconds(1 / 60f);
		}
		StartCoroutine(indicatorCoreHandlerEX.HandleCollaspeAnim());
		yield return null;
	}
	IEnumerator HandleSolveAnim()
	{
		mAudio.PlaySoundAtTransform("submitstart", transform);
		isplayingSolveAnim = true;
		StartCoroutine(IndicatorCoreHandlerExtraScreen.HandleCollaspeAnim());
		for (var x = 0; x < statusIndicatorsExtra.Length; x++)
		{
			statusIndicatorsExtra[x].material.color = Color.black;
		}
		StartCoroutine(HandleFlickerSolveAnim());
		if (autoCycleEnabled)
			StartCoroutine(HandleAutoCycleAnim(false));
        var solveDelayArray = new[] { 9, 7, 5, 3, 1 }.Shuffle();
		foreach (int y in solveDelayArray)
		{
			for (int x = 0; x < y; x++)
			{
				pigpenDisplay.text = pigpenDisplay.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
				mainDisplay.text = mainDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : a : a).Join("");
				pigpenSecondary.text = pigpenSecondary.text.Select(a => !char.IsWhiteSpace(a) ? baseAlphabet.PickRandom() : a).Join("");
				strikeIDDisplay.text = strikeIDDisplay.text.Select(a => !char.IsWhiteSpace(a) ? char.IsLetter(a) ? baseAlphabet.PickRandom() : char.IsDigit(a) ? "0123456789".PickRandom() : a : a).Join("");
				mAudio.PlaySoundAtTransform("submiterate", transform);
				yield return new WaitForSeconds(0.2f);
			}
			mAudio.PlaySoundAtTransform("submiterate2", transform);
			yield return new WaitForSeconds(0.1f);
		}
		isplayingSolveAnim = false;
		mAudio.PlaySoundAtTransform("submitstop", transform);
		pigpenDisplay.text = "";
		mainDisplay.text = "";
		pigpenSecondary.text = "";
		strikeIDDisplay.text = "";
		foreach (Light singleLight in colorLights)
			singleLight.enabled = false;
		centerLight.enabled = false;
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.5f;
		}
		if (!legacyUCR && harderUCR)
        {
			mAudio.PlaySoundAtTransform("ForgetAnyColorFinalStage", transform);
			for (float x = 0; x <= 1f; x += Time.deltaTime / 4)
			{
				float curScale = 1f - x;
				float currentOffset = Easing.InCirc(x, 0, 1f, 1f);
				entireCircle.transform.localScale = new Vector3(curScale, curScale, curScale);
				entireCircle.transform.localEulerAngles = Vector3.up * 3600 * curScale;
				entireCircle.transform.localPosition = new Vector3(0, 5 * currentOffset, 0);
				yield return null;
			}
			entireCircle.SetActive(false);
		}

		yield return null;
	}
	IEnumerator TypePigpenText(string displayValue)
	{
		isAnimatingStart = true;
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
		StartCoroutine(indicatorCoreHandlerEX.HandleIndicatorModification(splittedInstructions.Count));
		yield return null;
		isAnimatingStart = false;
	}
	IEnumerator SampleStandardText()
	{
		Dictionary<string, string> sampleQuestionResponse = new Dictionary<string, string>()
		{
			{"It was too\nconsistent.", "So he did\nthis instead." },
			{"It was never fair", "in the first place." },
			{"Do defusers even\nread these?", "I guess not as much." },
			{"Landing Sequence...", "ERROR" },
			{"How fast can your", "team disarm it?" },
			{"Funny Text", "Side Text" },
			{"You have time...", "Right?" },
			{"Where did he go?", "Is it there?" },
			{"Who saw\nthis coming?", "Certainly not him." },
			{"Estimated\nSimulation", "About\nTwenty Minutes" },
		}, cruelModeQuestionResponse = new Dictionary<string, string>()
		{
			{"It was too\nconsistent.", "So he made this\nharder instead.\nWhy? Because-" },
			{"Nothing will ever\nbe the same...", "Ever again." },
			{"Why is it called\nUnfair's Crueler\nRevenge?", "Well... You'll\nabout to find\nout..." },
			{"Hard Mode Cruel\nRevenge Activated", "YOU ARE GOING\nTO REGRET THIS" },
			{"Wanna hear the\nmost annoying\nsound in the world?", "Cyan! Azure! Lapis\nLazuli! Celadon!\nCobalt!" },
			{"Estimated\nSimulation", "About\nFifty Minutes\nGive Or Take 1 Hour" },
		}, legacyModeQuestionResponse = new Dictionary<string, string>()
		{
			{ "Why is it called\nUnfair's Legacy\nRevenge?", "You wanna know?" },
			{ "The old days...", "It just keeps\ncoming back." },
			{"It was too\nconsistent.", "Why was it\nconsistent?" },
			{ "The old days\nof Cruel Revenge...", "Do you want\nto revisit it?" },
			{ "I mean it was\nborn like this.", "I assume you\nknew this." },
			{ "Estimated\nSimulation", "ERROR" },
		};
		//yield return null;
		//OverrideSettings();
		if (settingsOverriden)
        {
			var firstTextExpected = "CRUEL REVENGE\nHAS BEEN\nOVERRIDEN";
			var secondTextExpected = string.Format("MODE:\n{0}", legacyUCR ? "LEGACY" : harderUCR ? "CRUELER": "NORMAL");

			for (int x = 1; x <= Math.Max(firstTextExpected.Length, secondTextExpected.Length); x++)
			{
				mainDisplay.text = firstTextExpected.Substring(0, Math.Min(x, firstTextExpected.Length));
				strikeIDDisplay.text = secondTextExpected.Substring(0, Math.Min(x, secondTextExpected.Length));
				yield return null;
			}
			yield return new WaitForSecondsRealtime(1f);
		}
		KeyValuePair<string, string> selectedSample = legacyUCR ? legacyModeQuestionResponse.PickRandom() : harderUCR ? cruelModeQuestionResponse.PickRandom() : sampleQuestionResponse.PickRandom();
		mainDisplay.color = Color.red;
		strikeIDDisplay.color = Color.red;
		for (int x = 1; x <= Math.Max(selectedSample.Key.Length,selectedSample.Value.Length); x++)
		{
			mainDisplay.text = selectedSample.Key.Substring(0, Math.Min(x,selectedSample.Key.Length));
			strikeIDDisplay.text = selectedSample.Value.Substring(0, Math.Min(x, selectedSample.Value.Length));
			yield return null;
		}
		yield return new WaitForSeconds(3f);
		mainDisplay.text = "";
		strikeIDDisplay.text = "";
	}
	IEnumerator HandleStartUpAnim()
	{
		entireCircle.SetActive(true);
		for (int i = 0; i < colorButtonRenderers.Length; i++)
		{
			colorButtonRenderers[i].material.color = colorWheel[idxColorList[i]] * 0.75f;
		}
		entireCircle.transform.localScale = Vector3.zero;
		entireCircle.transform.localPosition = 5 * Vector3.up;
		yield return new WaitForSeconds(uernd.Range(0f, 2f));
		for (float x = 0; x <= 1f; x += Time.deltaTime)
		{
			float curScale = Mathf.Pow(x, 1);
			entireCircle.transform.localScale = new Vector3(curScale, curScale, curScale);
			entireCircle.transform.localEulerAngles = Vector3.up * 720 * (1f - x);
			float currentOffset = Easing.InOutQuad(1f - x, 0, 1f, 1f);
			entireCircle.transform.localPosition = new Vector3(0, 5 * currentOffset, 0);
			yield return null;
		}
		mAudio.PlaySoundAtTransform("werraMetallicTrimmed", entireCircle.transform);
		entireCircle.transform.localEulerAngles = Vector3.zero;
		entireCircle.transform.localPosition = Vector3.zero;
		entireCircle.transform.localScale = Vector3.one;
		outerSelectable.AddInteractionPunch(3f);
		for (int i = 0; i < colorLights.Length; i++)
		{
			colorLights[i].enabled = true;
			colorLights[i].color = colorWheel[idxColorList[i]];
		}
		centerLight.enabled = true;
		particles.Emit(90);
	}
	IEnumerator HandleChangingColorsAnim()
    {
		while (colorsFlashing.Any(a => a != null && a.MoveNext()))
        {
			yield return null;
        }
		var eachColor = colorButtonRenderers.Select(a => a.material.color);
        for (float t = 0; t < 1f; t += Time.deltaTime * 2)
        {
			yield return null;
			for (var a = 0; a < colorButtonRenderers.Length; a++)
				colorButtonRenderers[a].material.color = new Color(eachColor.ElementAt(a).r * (1f - t), eachColor.ElementAt(a).g * (1f - t), eachColor.ElementAt(a).b * (1f - t));
			for (int i = 0; i < colorLights.Length; i++)
			{
				colorLights[i].enabled = true;
				colorLights[i].color = new Color(eachColor.ElementAt(i).r * (1f - t), eachColor.ElementAt(i).g * (1f - t), eachColor.ElementAt(i).b * (1f - t));
			}
		}
		var colorsToChange = idxColorList.Select(a => colorWheel[a]);
		for (float t = 0; t < 1f; t += Time.deltaTime * 2)
		{
			yield return null;
			for (var a = 0; a < colorButtonRenderers.Length; a++)
				colorButtonRenderers[a].material.color = colorsToChange.ElementAt(a) * t;
			for (int i = 0; i < colorLights.Length; i++)
			{
				colorLights[i].enabled = true;
				colorLights[i].color = new Color(colorsToChange.ElementAt(i).r * t, colorsToChange.ElementAt(i).g * t, colorsToChange.ElementAt(i).b * t);
			}
		}
		for (var a = 0; a < colorButtonRenderers.Length; a++)
			colorButtonRenderers[a].material.color = colorsToChange.ElementAt(a);
		for (int i = 0; i < colorLights.Length; i++)
		{
			colorLights[i].enabled = true;
			colorLights[i].color = colorsToChange.ElementAt(i);
		}
		isChangingColors = false;
    }

	IEnumerator HandleStrikeAnim()
	{
		for (int x = 0; x < 5; x++)
		{
			pigpenDisplay.color = Color.red;
			mainDisplay.color = Color.red;
			for (int a = 0; a < usedRenderers.Length; a += 1)
			{
				usedRenderers[a].material.color = Color.red;
			}
			mAudio.PlaySoundAtTransform("wrong", transform);
			yield return new WaitForSeconds(0.1f);
			pigpenDisplay.color = Color.white;
			mainDisplay.color = Color.white;
			for (int a = 0; a < usedRenderers.Length; a += 1)
			{
				usedRenderers[a].material.color = Color.black;
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
				toLog = "Press Inner Center when the seconds digit match.";
				break;
			case "MOT":
				toLog = string.Format("Press Outer Center when the last seconds digit is {0}.", (selectedModID + (4 - currentInputPos + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10 + 10) % 10);
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
				if (!lastCorrectInputs.Any())
					toLog = "There were no previous inputs. Press Inner Center.";
				else
					toLog = string.Format("The last input was {0}, so press that.", lastCorrectInputs.Last());
				break;
			case "STR":
			case "IKE":
				{
                    string lastColor = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last() : "Red";
                    toLog = string.Format("Start on {0}. Count the number of colored buttons counter-clockwise as there are strikes obtained so far. Press the resulting button.", lastColor);
					break;
				}
			case "SKP":
				toLog = "Press Inner Center.";
				if (currentInputPos + 1 < splittedInstructions.Count && !lastCommands.Contains(splittedInstructions[currentInputPos + 1]))
					toLog += " The next instruction is skippable, so press Outer Center in replacement for the next instruction.";
				break;
			case "PVP":
				{
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
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
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
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
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
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
					toLog = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? string.Format("The last colored button you pressed is {0}.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : "You have not pressed a colored button yet. Start on the NW button.";
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
				if (!lastCorrectInputs.Any())
					toLog = "There were no previous inputs. Press Outer Center.";
				else
					toLog = string.Format("The last input was {0}, so press {1}.", lastCorrectInputs.Last(),
						lastCorrectInputs.Last() == "Outer" ? "Inner Center" :
						lastCorrectInputs.Last() == "Inner" ? "Outer Center" :
						rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last())) % 6]);
				break;
			case "FIN":
			case "ISH":
			case "ALE":
				toLog = "This instruction is complicated. Refer to the manual for how to press this last command.";
				break;
			case "INV":
			case "ERT":
				toLog = string.Format("There was an {0} number of previous inputs. Press {1}. Be prepared to swap the button presses for the next set of instructions.", lastCorrectInputs.Count % 2 == 0 ? "even" : "odd", lastCorrectInputs.Count % 2 == 0 ? "Inner Center" : "Outer Center");
				break;
			case "SWP":
				if (!lastCorrectInputs.Any(a => baseColorList.Contains(a)))
					toLog = "There were no previous colored inputs. Press the NW button.";
				else
					toLog = string.Format("The last colored button input was {0}, so press that.", lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last());
				toLog += " Be prepared to swap the button presses for the next set of instructions.";
				break;
			case "AGN":
				if (currentInputPos == 0 && splittedInstructions.Count > 1)
					toLog = string.Format("This is the first instruction. Perform the \"{0}\" instruction.", splittedInstructions[1]);
				else if (splittedInstructions.Count > 1)
					toLog = string.Format("Perform the \"{0}\" instruction, since it's the last instruction performed.", splittedInstructions[currentInputPos - 1]);
				break;
			case "SCN":
				var sumAlphabeticalPositions = bombInfo.GetSerialNumberLetters().Select(a => baseAlphabet.IndexOf(a) + 1).Sum();
				toLog = string.Format("The sum of the alphabetical positions in the serial number is {0}, go to page {1} on the screen and then press Inner Center.", sumAlphabeticalPositions, sumAlphabeticalPositions % 4 + 1);
				break;
		}
		if (swapInnerOuterPresses || invertColorButtonPresses && !lastCommands.Contains(splittedInstructions[currentInputPos]))
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Account for the modifiers that are currently active for this instruction.", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Instruction {2} (\"{3}\"): {1}", loggingModID, toLog, currentInputPos + 1, splittedInstructions[currentInputPos]);
	}
	bool canSkip = false, swapInnerOuterPresses = false, invertColorButtonPresses = false;
	Dictionary<string, string> complementaryCounterparts = new Dictionary<string, string>() {
		{ "Red", "Cyan" },
		{ "Yellow", "Blue" },
		{ "Green", "Magenta" },
		{ "Cyan", "Red" },
		{ "Blue", "Yellow" },
		{ "Magenta", "Green" },
	};
	bool IsCurInstructionCorrect(string input)
    {
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		bool isCorrect = true;
		//Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Pressing the {1} button at {2} on the countdown timer...", loggingModID, input, bombInfo.GetFormattedTime());
		int secondsTimer = (int)(bombInfo.GetTime() % 60);
		int solvedCount = bombInfo.GetSolvedModuleIDs().Count();
		int solvableCount = bombInfo.GetSolvableModuleIDs().Count();
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		if (canSkip)
		{
			isCorrect = input == (swapInnerOuterPresses ? "Inner" : "Outer");
		}
		else
			switch (splittedInstructions[currentInputPos])
			{
				case "PCR":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[0]] : baseColorList[0]);
					break;
				case "PCG":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[2]] : baseColorList[2]);
					break;
				case "PCB":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[4]] : baseColorList[4]);
					break;
				case "SCC":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[3]] : baseColorList[3]);
					break;
				case "SCM":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[5]] : baseColorList[5]);
					break;
				case "SCY":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[1]] : baseColorList[1]);
					break;
				case "SUB":
					isCorrect = input == (swapInnerOuterPresses ? "Outer" : "Inner") && secondsTimer % 11 == 0;
					break;
				case "MOT":
					isCorrect = input == (swapInnerOuterPresses ? "Inner" : "Outer") && secondsTimer % 10 == (selectedModID + (5 - (1 + currentInputPos)) + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10;
					break;
				case "PRN":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ^ swapInnerOuterPresses ? "Inner" : "Outer");
					break;
				case "CHK":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ^ swapInnerOuterPresses ? "Outer" : "Inner");
					break;
				case "REP":
				case "EAT":
					if (!lastCorrectInputs.Any())
						isCorrect = input == "Inner";
					else
					{
						var lastInput = lastCorrectInputs.Last();
						isCorrect = input == (baseColorList.Contains(lastInput) ?
							invertColorButtonPresses ? complementaryCounterparts[lastInput] : lastInput
							: swapInnerOuterPresses ^ lastInput == "Outer" ? "Outer" : "Inner");
					}
					break;
				case "STR":
				case "IKE":
					{
						int strikeCount = TimeModeActive ? localStrikeCount : bombInfo.GetStrikes();
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : Array.IndexOf(rearrangedColorList, baseColorList[0]);
						curIdx -= strikeCount % 6;
						string resultingButton = rearrangedColorList[(curIdx + 6) % 6];
						//Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} strike(s) the resulting button should be {2}.", loggingModID, strikeCount, resultingButton);
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[resultingButton] : resultingButton);
						break;
					}
				case "SKP":
					{
						isCorrect = input == (swapInnerOuterPresses ? "Outer" : "Inner");
						break;
					}
				case "PVP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "NXP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "PVS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "NXS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "OPP":
					{
						if (!lastCorrectInputs.Any() || lastCorrectInputs.Last() == "Inner")
							isCorrect = input == (swapInnerOuterPresses ? "Inner" : "Outer");
						else if (lastCorrectInputs.Last() == "Outer")
							isCorrect = input == (swapInnerOuterPresses ? "Outer" : "Inner");
						else
                        {
							var threeClockColor = rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs[currentInputPos - 1])) % 6];
							isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[threeClockColor] : threeClockColor);
                        }

                        break;
					}
				case "FIN":
					{
						if (legacyUCR) goto case "LEGACY";
						isCorrect = input == (solvedCount % 2 == 0 ? "Inner" : "Outer") && (solvableCount - solvedCount) % 10 == secondsTimer % 10;
					}
					break;
				case "ISH":
					{
						if (legacyUCR) goto case "LEGACY";
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						curIdx = (curIdx + lastCorrectInputs.Where(a => !baseColorList.Contains(a)).Count()) % 6;
						isCorrect = input == rearrangedColorList[curIdx] && (solvableCount - solvedCount) % 10 == secondsTimer % 10;
					}
					break;
				case "ALE":
					{

						var lastColoredButtonInputs = lastCorrectInputs.Where(a => baseColorList.Contains(a));
						int lastColoredIdx = lastColoredButtonInputs.Any() ? Array.IndexOf(rearrangedColorList, lastColoredButtonInputs.Last()) : 0;
						string goalButton = lastColoredButtonInputs.Count() % 2 == 0 ? rearrangedColorList[(lastColoredIdx + 3) % 6] : complementaryCounterparts[rearrangedColorList[lastColoredIdx]];
						isCorrect = secondsTimer % 10 == solvedCount % 10 && input == goalButton;

					}
					break;
				case "SWP":
					{
						var lastColoredButtonInputs = lastCorrectInputs.Where(a => baseColorList.Contains(a));
						var lastColoredPress = lastColoredButtonInputs.Any() ? lastColoredButtonInputs.Last() : baseColorList[0];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[lastColoredPress] : lastColoredPress);
					}
					break;
				case "INV":
				case "ERT":
					{
						isCorrect = input == (lastCorrectInputs.Count % 2 == 0 ^ swapInnerOuterPresses ? "Inner" : "Outer");
					}
					break;
				case "LEGACY":
                    {// The old UCR instruction set
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						curIdx = (curIdx + lastCorrectInputs.Where(a => !baseColorList.Contains(a)).Count()) % 6;
						curIdx -= solvedCount % 6;
						while (curIdx < 0)
							curIdx += 6;
						isCorrect = input == rearrangedColorList[curIdx] && (bombInfo.GetSolvableModuleIDs().Count() - solvedCount) % 10 == secondsTimer % 10;
						break;
					}
				case "SCN":
                    {
						var sumAlphabeticalPositions = bombInfo.GetSerialNumberLetters().Select(a => baseAlphabet.IndexOf(a) + 1).Sum();
						isCorrect = sumAlphabeticalPositions % 4 == currentScreenVal && input == (swapInnerOuterPresses ? "Outer" : "Inner");
							break;
					}
				case "AGN":
                    {
						if (splittedInstructions.Count > 0)
						{
							var lastInstruction = splittedInstructions[currentInputPos == 0 ? 1 : currentInputPos - 1];
							if (lastInstruction == "INV" || lastInstruction == "ERT")
								goto case "INV";
							else if (lastInstruction == "STR" || lastInstruction == "IKE")
								goto case "STR";
							else if (lastInstruction == "REP" || lastInstruction == "EAT")
								goto case "REP";
							else if (lastInstruction == "SCN")
								goto case "SCN";
							else if (lastInstruction == "OPP")
								goto case "OPP";
							else if (lastInstruction == "NXS")
								goto case "NXS";
							else if (lastInstruction == "PVS")
								goto case "PVS";
							else if (lastInstruction == "NXP")
								goto case "NXP";
							else if (lastInstruction == "PVP")
								goto case "PVP";
							else if (lastInstruction == "SKP")
								goto case "SKP";
							else if (lastInstruction == "MOT")
								goto case "MOT";
							else if (lastInstruction == "SUB")
								goto case "SUB";
							else if (lastInstruction == "PRN")
								goto case "PRN";
							else if (lastInstruction == "CHK")
								goto case "CHK";
							else if (lastInstruction == "PCR")
								goto case "PCR";
							else if (lastInstruction == "PCG")
								goto case "PCG";
							else if (lastInstruction == "PCB")
								goto case "PCB";
							else if (lastInstruction == "SCC")
								goto case "SCC";
							else if (lastInstruction == "SCY")
								goto case "SCY";
							else if (lastInstruction == "SCM")
								goto case "SCM";
						}
						break;
                    }

			}
		return isCorrect;
	}

	void ProcessInstruction(string input)
	{
		if (isFinished || !hasStarted) return;
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		bool isCorrect = true;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Pressing the {1} button at {2} on the countdown timer...", loggingModID, input, bombInfo.GetFormattedTime());
		int secondsTimer = (int)bombInfo.GetTime() % 60;
		int solvedCount = bombInfo.GetSolvedModuleIDs().Count();
		int solvableCount = bombInfo.GetSolvableModuleIDs().Count();
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		if (canSkip)
		{
			isCorrect = input == (swapInnerOuterPresses ? "Inner" : "Outer");
			canSkip = false;
		}
		else
			switch (splittedInstructions[currentInputPos])
			{
				case "PCR":
                    isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[0]] : baseColorList[0]);
					break;
				case "PCG":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[2]] : baseColorList[2]);
					break;
				case "PCB":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[4]] : baseColorList[4]);
					break;
				case "SCC":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[3]] : baseColorList[3]);
					break;
				case "SCM":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[5]] : baseColorList[5]);
					break;
				case "SCY":
					isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[baseColorList[1]] : baseColorList[1]);
					break;
				case "SUB":
					isCorrect = input == (swapInnerOuterPresses ? "Outer" : "Inner") && secondsTimer % 11 == 0;
					break;
				case "MOT":
					isCorrect = input == (swapInnerOuterPresses ? "Inner" : "Outer") && secondsTimer % 10 == ((selectedModID + 4 - currentInputPos + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10 + 10) % 10;
					break;
				case "PRN":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ^ swapInnerOuterPresses ? "Inner" : "Outer");
					break;
				case "CHK":
					isCorrect = input == (primesUnder20.Contains(selectedModID % 20) ^ swapInnerOuterPresses ? "Outer" : "Inner");
					break;
				case "BOB":
					isCorrect = input == "Inner";
					if (bombInfo.IsIndicatorOn(Indicator.BOB) && bombInfo.GetBatteryCount() == 4 && bombInfo.GetBatteryHolderCount() == 2 && bombInfo.GetIndicators().Count() == 1)
					{
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: BOB is nice today. He will make you skip the rest of the instructions.", loggingModID);
						currentInputPos = splittedInstructions.Count;
					}
					break;
				case "REP":
				case "EAT":
					if (!lastCorrectInputs.Any())
						isCorrect = input == "Inner";
					else
					{
						var lastInput = lastCorrectInputs.Last();
						isCorrect = input == (baseColorList.Contains(lastInput) ?
							invertColorButtonPresses ? complementaryCounterparts[lastInput] : lastInput
							: swapInnerOuterPresses ^ lastInput == "Outer" ? "Outer" : "Inner");
                    }
					break;
				case "STR":
				case "IKE":
					{
						int strikeCount = TimeModeActive ? localStrikeCount : bombInfo.GetStrikes();
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : Array.IndexOf(rearrangedColorList, baseColorList[0]);
						curIdx -= strikeCount % 6;
						string resultingButton = rearrangedColorList[(curIdx + 6) % 6];
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} strike(s) the resulting button should be {2}.", loggingModID, strikeCount, resultingButton);
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[resultingButton] : resultingButton);
						break;
					}
				case "SKP":
					{
						isCorrect = input == (swapInnerOuterPresses ? "Outer" : "Inner");
						if (currentInputPos + 1 < splittedInstructions.Count && !lastCommands.Contains(splittedInstructions[currentInputPos + 1]))
							canSkip = true;
						break;
					}
				case "PVP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "NXP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "PVS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "NXS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						var selectedColor = rearrangedColorList[curIdx];
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[selectedColor] : selectedColor);
						break;
					}
				case "OPP":
					{
						if (!lastCorrectInputs.Any() || lastCorrectInputs.Last() == "Inner")
							isCorrect = input == (swapInnerOuterPresses ? "Inner" : "Outer");
						else if (lastCorrectInputs.Last() == "Outer")
							isCorrect = input == (swapInnerOuterPresses ? "Outer" : "Inner");
						else
						{
							var threeClockColor = rearrangedColorList[(3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs[currentInputPos - 1])) % 6];
							isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[threeClockColor] : threeClockColor);
						}
						break;
					}
				case "FIN":
					{
						if (legacyUCR) goto case "LEGACY";
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} solved, {2} unsolved, {3} must be pressed when the last seconds digit is {4}.", loggingModID, solvedCount, solvableCount - solvedCount, solvedCount % 2 == 0 ? "Inner Center" : "Outer Center", (solvableCount - solvedCount) % 10);
						isCorrect = input == (solvedCount % 2 == 0 ? "Inner" : "Outer") && (solvableCount - solvedCount) % 10 == secondsTimer % 10;
					}
					break;
				case "ISH":
					{
						if (legacyUCR) goto case "LEGACY";
						else
						{
							int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
							curIdx = (curIdx + lastCorrectInputs.Where(a => !baseColorList.Contains(a)).Count()) % 6;
							Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} unsolved, the resulting button should be {2} which much be pressed when the last seconds digit is {3}.", loggingModID, solvableCount - solvedCount, rearrangedColorList[curIdx], (solvableCount - solvedCount) % 10);
							isCorrect = input == rearrangedColorList[curIdx] && (solvableCount - solvedCount) % 10 == secondsTimer % 10;
						}
					}
					break;
				case "ALE":
                    {

						var lastColoredButtonInputs = lastCorrectInputs.Where(a => baseColorList.Contains(a));
						int lastColoredIdx = lastColoredButtonInputs.Any() ? Array.IndexOf(rearrangedColorList, lastColoredButtonInputs.Last()) : 0;
						string goalButton = lastColoredButtonInputs.Count() % 2 == 0 ? rearrangedColorList[(lastColoredIdx + 3) % 6] : complementaryCounterparts[rearrangedColorList[lastColoredIdx]];
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} solved, the resulting button should be {2} which much be pressed when the last seconds digit is {3}.", loggingModID, solvedCount, goalButton, solvedCount % 10);
						isCorrect = secondsTimer % 10 == solvedCount % 10 && input == goalButton;

					}
					break;
				case "SWP":
                    {
						var lastColoredButtonInputs = lastCorrectInputs.Where(a => baseColorList.Contains(a));
						int lastColoredIdx = lastColoredButtonInputs.Any() ? Array.IndexOf(rearrangedColorList, lastColoredButtonInputs.Last()) : 0;
						isCorrect = input == (invertColorButtonPresses ? complementaryCounterparts[rearrangedColorList[lastColoredIdx]] : rearrangedColorList[lastColoredIdx]);
						swapInnerOuterPresses = !swapInnerOuterPresses;
                    }
					break;
				case "INV":
				case "ERT":
                    {
						isCorrect = input == (lastCorrectInputs.Count % 2 == 0 ^ swapInnerOuterPresses ? "Inner" : "Outer");

						invertColorButtonPresses = !invertColorButtonPresses;
                    }
					break;
				case "LEGACY":
					{// The old UCR finale instruction set
						int curIdx = lastCorrectInputs.Where(a => baseColorList.Contains(a)).Any() ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Where(a => baseColorList.Contains(a)).Last()) : 0;
						curIdx = (curIdx + lastCorrectInputs.Where(a => !baseColorList.Contains(a)).Count()) % 6;
						curIdx -= solvedCount % 6;
						while (curIdx < 0)
							curIdx += 6;
						isCorrect = input == rearrangedColorList[curIdx] && (bombInfo.GetSolvableModuleIDs().Count() - solvedCount) % 10 == secondsTimer % 10;
						Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: At {1} solved, {2} unsolved, the resulting button should be {3} which much be pressed when the last seconds digit is {4}.", loggingModID, solvedCount, bombInfo.GetSolvableModuleIDs().Count() - solvedCount, rearrangedColorList[curIdx], (bombInfo.GetSolvableModuleIDs().Count() - solvedCount) % 10);
						break;
					}
				case "SCN":
					{
						var sumAlphabeticalPositions = bombInfo.GetSerialNumberLetters().Select(a => baseAlphabet.IndexOf(a) + 1).Sum();
						isCorrect = sumAlphabeticalPositions % 4 == currentScreenVal && input == (swapInnerOuterPresses ? "Outer" : "Inner");
						break;
					}
				case "AGN":
					{
						if (splittedInstructions.Count > 0)
						{
							if (splittedInstructions.Count > 0)
							{
								var lastInstruction = splittedInstructions[currentInputPos == 0 ? 1 : currentInputPos - 1];
								if (lastInstruction == "INV" || lastInstruction == "ERT")
									goto case "INV";
								else if (lastInstruction == "STR" || lastInstruction == "IKE")
									goto case "STR";
								else if (lastInstruction == "REP" || lastInstruction == "EAT")
									goto case "REP";
								else if (lastInstruction == "SCN")
									goto case "SCN";
								else if (lastInstruction == "OPP")
									goto case "OPP";
								else if (lastInstruction == "NXS")
									goto case "NXS";
								else if (lastInstruction == "PVS")
									goto case "PVS";
								else if (lastInstruction == "NXP")
									goto case "NXP";
								else if (lastInstruction == "PVP")
									goto case "PVP";
								else if (lastInstruction == "SKP")
									goto case "SKP";
								else if (lastInstruction == "MOT")
									goto case "MOT";
								else if (lastInstruction == "SUB")
									goto case "SUB";
								else if (lastInstruction == "PRN")
									goto case "PRN";
								else if (lastInstruction == "CHK")
									goto case "CHK";
								else if (lastInstruction == "PCR")
									goto case "PCR";
								else if (lastInstruction == "PCG")
									goto case "PCG";
								else if (lastInstruction == "PCB")
									goto case "PCB";
								else if (lastInstruction == "SCC")
									goto case "SCC";
								else if (lastInstruction == "SCY")
									goto case "SCY";
								else if (lastInstruction == "SCM")
									goto case "SCM";
							}
						}
						break;
					}
			}
		if (isCorrect)
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The resulting press is correct.", loggingModID);
			string[] possibleSounds = { "button1", "button2", "button3", "button4" };
			lastCorrectInputs.Add(input);
			currentInputPos++;
			if (currentInputPos >= splittedInstructions.Count)
			{
				Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: All instructions are handled correctly. You're done.", loggingModID);
				isFinished = true;
				if (harderUCR && !legacyUCR && !forceSolveRequested)
				{
					mAudio.PlaySoundAtTransform("6_awesome", transform);
				}
				modSelf.HandlePass();
				StartCoroutine(HandleSolveAnim());
				return;
			}
			else
			{
				if (harderUCR && !legacyUCR)
				{
					isChangingColors = true;
					idxColorList.Shuffle();
					Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: Warning: Button colors have changed to the following in clockwise order (starting on the NW button): {1}", loggingModID, idxColorList.Select(a => baseColorList[a]).Join(", "));
					StartCoroutine(HandleChangingColorsAnim());
				}
				if (!canSkip)
					LogCurrentInstruction();
				else
				{
					Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The next instruction is getting skipped.", loggingModID);
				}
			}
			mAudio.PlaySoundAtTransform(possibleSounds.PickRandom(), transform);
			UpdateStatusIndc();
		}
		else
		{
			Debug.LogFormat("[Unfair's Cruel Revenge #{0}]: The resulting press is incorrect. Restarting from the first instruction...", loggingModID);
			if (currentInputPos + 1 >= splittedInstructions.Count)
				mAudio.PlaySoundAtTransform("Darkest Dungeon - OverconfidenceRant", transform);
			modSelf.HandleStrike();
			hasStruck = true;
			lastCorrectInputs.Clear();
			currentInputPos = 0;
			canSkip = false;
			swapInnerOuterPresses = false;
			invertColorButtonPresses = false;
			localStrikeCount += TimeModeActive ? 1 : 0;
			StartCoroutine(HandleStrikeAnim());
			if (harderUCR && !legacyUCR)
            {
				isChangingColors = true;
				idxColorList = initialIdxColorList.ToArray();
				StartCoroutine(HandleChangingColorsAnim());
            }
			LogCurrentInstruction();
			
		}
	}
	void UpdateStatusIndc()
	{
		for (int a = 0; a < usedRenderers.Length; a++)
		{
			usedRenderers[a].material.color = currentInputPos == a  ? Color.yellow : currentInputPos > a  ? Color.green : Color.black;
		}
	}
	// Update is called once per frame, may be scaled by other events
	void Update() {
		if (!isFinished)
		{
            switch (currentScreenVal)
            {
                case 1:
                    {
                        string toDisplay = "";
                        switch (idxCurStrikeDisplay)
                        {
                            case 0:
                                toDisplay = ValueToFixedRoman(TimeModeActive ? localStrikeCount : bombInfo.GetStrikes());
                                break;
                            case 1:
                                toDisplay = ValueToBrokenRoman(TimeModeActive ? localStrikeCount : bombInfo.GetStrikes());
                                break;
                            case 2:
                                toDisplay = (TimeModeActive ? localStrikeCount : bombInfo.GetStrikes()).ToString();
                                break;
                        }
                        strikeIDDisplay.text = string.Format("Strikes Detected:\n{0}", toDisplay);
                        break;
                    }

                case 3:
					{
						if (harderUCR && !legacyUCR)
						{
							if (bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1) >= displaySubstutionLettersAll.Count)
							{
								pigpenSecondary.text = "";
								strikeIDDisplay.text = string.Format("={0}=",encodingDisplay);
							}
							else
							{
								pigpenSecondary.text = FitToScreen(displaySubstutionLettersAll.ElementAtOrDefault((int)(bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1))), 13);
								strikeIDDisplay.text = "";
							}
						}
						else
						{
							pigpenSecondary.text = (legacyUCR || bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1) >= displaySubstutionLettersAll.Count) ? "" :
							FitToScreen(displaySubstutionLettersAll.ElementAtOrDefault((int)(bombInfo.GetTime() % (displaySubstutionLettersAll.Count + 1))), 13) + "\n";
						}
					}
                    break;
            }
        }
		if (autoCycleEnabled && !isFinished)
		{
			progressHandler.curProgress += Time.deltaTime;
			if (progressHandler.curProgress >= progressHandler.maxProgress)
			{
				progressHandler.curProgress = 0;
				currentScreenVal = (currentScreenVal + (inverseAutoCycle ? 3 : 1)) % 4;
				UpdateSecondaryScreen();
			}
		}
		else
		{
			progressHandler.curProgress = Mathf.Max(0, progressHandler.curProgress - Time.deltaTime);
		}
	}

	public class UnfairsCruelRevengeSettings
    {
		public bool enableLegacyUCR = false;
		public bool cruelerRevenge = false;
		public bool noTPCruelerRevenge = false;
		public bool debugCiphers;
		public int[] debugCiphersIdxes;
		public string version = "2.0";
    }
	string FormatSecondsToTime(long num)
	{
		return string.Format("{0}:{1}", num / 60, (num % 60).ToString("00"));
	}
	// Mission Detection Begins Here
	private void OverrideSettings()
    {
		try
		{
			var lastTPSettings = noTPCruelCruelRevenge;
			var missionDescription = Game.Mission.Description;
			var missionID = Game.Mission.ID;
			noTPCruelCruelRevenge = true;
			//Debug.LogFormat("<Unfair's Cruel Revenge #{0}> Detected Mission ID: {1}", loggingModID, missionID ?? "<unknown>");
			switch (missionID)
			{
				case "mod_missionpack_VFlyer_missionUCRLegacyPractice":
					settingsOverriden = true;
					legacyUCR = true;
					harderUCR = false;
					allowDebugCiphers = false;
					break;
				case "mod_missionpack_VFlyer_mission47thWrath":
					settingsOverriden = true;
					harderUCR = true;
					legacyUCR = false;
					allowDebugCiphers = false;
					break;
				case "mod_missionpack_VFlyer_mission47thProblem":
					settingsOverriden = true;
					harderUCR = false;
					legacyUCR = false;
					allowDebugCiphers = false;
					break;
				case "freeplay":
					noTPCruelCruelRevenge = lastTPSettings;
					Debug.LogFormat("<Unfair's Cruel Revenge #{0}> MISSION DETECTED AS FREEPLAY. CANNOT OVERRIDE SETTINGS.", loggingModID);
					allowDebugCiphers &= true;
					return;
				default:
					break;
			}
			if (settingsOverriden)
			{
				Debug.LogFormat("<Unfair's Cruel Revenge #{0}> Are the settings overriden? YES, BY MISSION ID", loggingModID);
				return;
			}
			allowDebugCiphers = false;
			var allPossibleOverrides = new[] { "Old", "Legacy", "Normal", "Standard", "Crueler", };
			Match UCRMatch = Regex.Match(missionDescription ?? "", string.Format(@"\[UCROverride\]\s({0})", allPossibleOverrides.Join("|")));
			if (UCRMatch.Success)
			{
				switch (UCRMatch.Value.Split().Last())
				{
					case "Old":
					case "Legacy":
						legacyUCR = true;
						harderUCR = false;
						settingsOverriden = true;
						break;
					case "Normal":
					case "Standard":
						legacyUCR = false;
						harderUCR = false;
						settingsOverriden = true;
						break;
					case "Crueler":
						legacyUCR = false;
						harderUCR = true;
						settingsOverriden = true;
						break;
				}
			}
			else
				noTPCruelCruelRevenge = lastTPSettings;
			Debug.LogFormat("<Unfair's Cruel Revenge #{0}> Are the settings overriden? {1}", loggingModID, settingsOverriden ? "YES BY MISSION DESCRIPTION" : "NO");
		}
		catch (Exception resultingError)
        {
			Debug.LogErrorFormat("<Unfair's Cruel Revenge #{0}> EXCEPTION THROWN. USING SETTINGS PROVIDED BY FILE INSTEAD.", loggingModID);
			Debug.LogException(resultingError);

			settingsOverriden = false;
			legacyUCR = ucrSettings.enableLegacyUCR;
			harderUCR = ucrSettings.cruelerRevenge;
            noTPCruelCruelRevenge = ucrSettings.noTPCruelerRevenge;
			allowDebugCiphers = ucrSettings.debugCiphers;
		}
	}

	// TP Handling Begins here
	IEnumerator TwitchHandleForcedSolve()
	{
		string[] rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
		int[] primesUnder20 = { 2, 3, 5, 7, 11, 13, 17, 19 };
		while (!hasStarted) yield return true;
		hasStruck = false;
		forceSolveRequested = true;
		Debug.LogFormat("<Unfair's Cruel Revenge #{0}> Force solve Requested by TP.", loggingModID);
		while (currentInputPos < splittedInstructions.Count)
		{
			yield return new WaitForSeconds(0.1f);
			if (harderUCR)
            {
				rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToArray();
				while (isChangingColors)
					yield return true;
            }
			if (canSkip)
			{
				(swapInnerOuterPresses ? innerSelectable : outerSelectable).OnInteract();
			}
			else
			switch (splittedInstructions[currentInputPos])
			{
				case "PCR":
					colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[baseColorList[0]] : baseColorList[0])].OnInteract();
					break;
				case "PCG":
					colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[baseColorList[2]] : baseColorList[2])].OnInteract();
					break;
				case "PCB":
					colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[baseColorList[4]] : baseColorList[4])].OnInteract();
					break;
				case "SCC":
					colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[baseColorList[3]] : baseColorList[3])].OnInteract();
					break;
				case "SCM":
					colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[baseColorList[5]] : baseColorList[5])].OnInteract();
					break;
				case "SCY":
					colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[baseColorList[1]] : baseColorList[1])].OnInteract();
					break;
				case "STR":
				case "IKE":
					{
						var strikeCount = bombInfo.GetStrikes();
						var expectedIdxBeforeInvert = (lastCorrectInputs.Any(a => rearrangedColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => rearrangedColorList.Contains(a))) : (Array.IndexOf(idxColorList, 0)) + strikeCount) % 6;
						colorButtonSelectables[invertColorButtonPresses ? Array.IndexOf(rearrangedColorList,complementaryCounterparts[rearrangedColorList[expectedIdxBeforeInvert]]) : expectedIdxBeforeInvert].OnInteract();
					}
					break;
				case "SKP":
					(swapInnerOuterPresses ? outerSelectable : innerSelectable).OnInteract();
					break;
				case "PRN":
					(primesUnder20.Contains(selectedModID % 20) ^ swapInnerOuterPresses ? innerSelectable : outerSelectable).OnInteract();
					break;
				case "CHK":
					(primesUnder20.Contains(selectedModID % 20) ^ swapInnerOuterPresses ? outerSelectable : innerSelectable).OnInteract();
					break;
				case "PVP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						colorButtonSelectables[invertColorButtonPresses ? Array.IndexOf(rearrangedColorList, complementaryCounterparts[rearrangedColorList[curIdx]]) : curIdx].OnInteract();
						break;
					}
				case "NXP":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (!primaryList.Contains(rearrangedColorList[curIdx]));
						colorButtonSelectables[invertColorButtonPresses ? Array.IndexOf(rearrangedColorList, complementaryCounterparts[rearrangedColorList[curIdx]]) : curIdx].OnInteract();
						break;
					}
				case "PVS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = curIdx - 1 < 0 ? 5 : curIdx - 1;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						colorButtonSelectables[invertColorButtonPresses ? Array.IndexOf(rearrangedColorList, complementaryCounterparts[rearrangedColorList[curIdx]]) : curIdx].OnInteract();
						break;
					}
				case "NXS":
					{
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						do
						{
							curIdx = (curIdx + 1) % 6;
						}
						while (primaryList.Contains(rearrangedColorList[curIdx]));
						colorButtonSelectables[invertColorButtonPresses ? Array.IndexOf(rearrangedColorList, complementaryCounterparts[rearrangedColorList[curIdx]]) : curIdx].OnInteract();
						break;
					}
				case "OPP":
					{
						if (!lastCorrectInputs.Any() || lastCorrectInputs[lastCorrectInputs.Count - 1] == "Inner")
							(swapInnerOuterPresses ? innerSelectable : outerSelectable).OnInteract();
						else if (lastCorrectInputs[lastCorrectInputs.Count - 1] == "Outer")
							(swapInnerOuterPresses ? outerSelectable : innerSelectable).OnInteract();
						else
						{
							var expectedIdx = (3 + Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last())) % 6;
							colorButtonSelectables[invertColorButtonPresses ? Array.IndexOf(rearrangedColorList, complementaryCounterparts[rearrangedColorList[expectedIdx]]) : expectedIdx].OnInteract();
						}
						break;
					}
				case "REP":
				case "EAT":
					{
						var lastInput = lastCorrectInputs.LastOrDefault();
						if (!lastCorrectInputs.Any() || lastInput == "Inner")
							(swapInnerOuterPresses ? outerSelectable : innerSelectable).OnInteract();
						else if (lastInput == "Outer")
							(swapInnerOuterPresses ? innerSelectable : outerSelectable).OnInteract();
						else
							colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[lastInput] : lastInput)].OnInteract();
					}
					break;
				case "INV":
				case "ERT":
                    {
						(swapInnerOuterPresses ^ lastCorrectInputs.Count % 2 == 0 ? innerSelectable : outerSelectable).OnInteract();
                    }
					break;
				case "SWP":
                    {
						var lastColorInput = lastCorrectInputs.Any(a => baseColorList.Contains(a))
							? lastCorrectInputs.Last(a => baseColorList.Contains(a))
							: rearrangedColorList.First();
						colorButtonSelectables[Array.IndexOf(rearrangedColorList, invertColorButtonPresses ? complementaryCounterparts[lastColorInput] : lastColorInput)].OnInteract();
					}
					break;
				case "SUB":
					{
						while ((int)(bombInfo.GetTime() % 60 % 11) != 0)
						{
							yield return true;
						}
						(swapInnerOuterPresses ? outerSelectable : innerSelectable).OnInteract();
					}
					break;
				case "MOT":
					{
						var calculatedExpectedDigit = ((selectedModID + (4 - currentInputPos) + lastCorrectInputs.Where(a => baseColorList.Contains(a)).Count()) % 10 + 10) % 10;
						while ((int)(bombInfo.GetTime() % 10) != calculatedExpectedDigit)
						{
							yield return true;
						}
						(swapInnerOuterPresses ? innerSelectable : outerSelectable).OnInteract();
					}
					break;
				case "FIN":
                    {
						if (legacyUCR) goto case "LEGACY";
						var solveCount = bombInfo.GetSolvedModuleIDs().Count();
						var allModCount = bombInfo.GetSolvableModuleIDs().Count;
						while ((int)(bombInfo.GetTime() % 10) != (allModCount - solveCount) % 10)
						{
							yield return true;
							solveCount = bombInfo.GetSolvedModuleIDs().Count();
						}
						(solveCount % 2 == 0 ? innerSelectable : outerSelectable).OnInteract();
					}
					break;
				case "ISH":
                    {
						if (legacyUCR) goto case "LEGACY";
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						curIdx = (curIdx + lastCorrectInputs.Count(a => !baseColorList.Contains(a))) % 6;
						var solveCount = bombInfo.GetSolvedModuleIDs().Count();
						var allModCount = bombInfo.GetSolvableModuleIDs().Count;
						while ((int)(bombInfo.GetTime() % 10) != (allModCount - solveCount) % 10)
						{
							yield return true;
							solveCount = bombInfo.GetSolvedModuleIDs().Count();
						}
						colorButtonSelectables[curIdx].OnInteract();
					}
					break;
				case "ALE":
                    {
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						var solveCount = bombInfo.GetSolvedModuleIDs().Count();
						while ((int)(bombInfo.GetTime() % 10) != solveCount % 10)
						{
							yield return true;
							solveCount = bombInfo.GetSolvedModuleIDs().Count();;
						}
						colorButtonSelectables[lastCorrectInputs.Count(a => baseColorList.Contains(a)) % 2 == 0 ? (curIdx + 3) % 6 :
							Array.IndexOf(rearrangedColorList, complementaryCounterparts[rearrangedColorList[curIdx]])].OnInteract();
					}
					break;
				case "LEGACY":
                    {
						int curIdx = lastCorrectInputs.Any(a => baseColorList.Contains(a)) ? Array.IndexOf(rearrangedColorList, lastCorrectInputs.Last(a => baseColorList.Contains(a))) : 0;
						curIdx += lastCorrectInputs.Count(a => !baseColorList.Contains(a));
						var solveCount = bombInfo.GetSolvedModuleIDs().Count();
						var allModCount = bombInfo.GetSolvableModuleIDs().Count;
						while ((int)(bombInfo.GetTime() % 10) != (allModCount - solveCount) % 10)
						{
							yield return true;
							solveCount = bombInfo.GetSolvedModuleIDs().Count();
						}
						colorButtonSelectables[(curIdx + 5 * solveCount) % 6].OnInteract();
					}
					break;
				case "SCN":
                    {
						var expectedScreenIdx = bombInfo.GetSerialNumberLetters().Select(a => baseAlphabet.IndexOf(a) + 1).Sum() % 4;
						while (currentScreenVal != expectedScreenIdx)
                        {
							idxStrikeSelectableB.OnInteract();
							yield return new WaitForSeconds(0.1f);
                        }
						(swapInnerOuterPresses ? outerSelectable : innerSelectable).OnInteract();
					}
					break;
				case "AGN":
					{
						if (splittedInstructions.Count > 0)
						{
								if (splittedInstructions.Count > 0)
								{
									var lastInstruction = splittedInstructions[currentInputPos == 0 ? 1 : currentInputPos - 1];
									if (lastInstruction == "INV" || lastInstruction == "ERT")
										goto case "INV";
									else if (lastInstruction == "STR" || lastInstruction == "IKE")
										goto case "STR";
									else if (lastInstruction == "REP" || lastInstruction == "EAT")
										goto case "REP";
									else if (lastInstruction == "SCN")
										goto case "SCN";
									else if (lastInstruction == "OPP")
										goto case "OPP";
									else if (lastInstruction == "NXS")
										goto case "NXS";
									else if (lastInstruction == "PVS")
										goto case "PVS";
									else if (lastInstruction == "NXP")
										goto case "NXP";
									else if (lastInstruction == "PVP")
										goto case "PVP";
									else if (lastInstruction == "SKP")
										goto case "SKP";
									else if (lastInstruction == "MOT")
										goto case "MOT";
									else if (lastInstruction == "SUB")
										goto case "SUB";
									else if (lastInstruction == "PRN")
										goto case "PRN";
									else if (lastInstruction == "CHK")
										goto case "CHK";
									else if (lastInstruction == "PCR")
										goto case "PCR";
									else if (lastInstruction == "PCG")
										goto case "PCG";
									else if (lastInstruction == "PCB")
										goto case "PCB";
									else if (lastInstruction == "SCC")
										goto case "SCC";
									else if (lastInstruction == "SCY")
										goto case "SCY";
									else if (lastInstruction == "SCM")
										goto case "SCM";
								}
							}
						break;
					}
				default:
					yield return true;
					break;
			}
			if (hasStruck)
			{
				isFinished = true;
				StartCoroutine(HandleSolveAnim());
				modSelf.HandlePass();
				yield break;
			}
		}
	}
	IEnumerator HandleAutoCycleAnim(bool enable)
	{

		if (enable)
		{
			animBar.SetActive(true);
			for (float x = 1; x >= 0; x -= Time.deltaTime)
			{
				animBar.transform.localPosition = new Vector3(0, 0, 2.5f * x);
				yield return null;
			}
			animBar.transform.localPosition = Vector3.zero;
			autoCycleEnabled = true;

		}
		else
		{
			autoCycleEnabled = false;
			for (float x = 0; x <= 1; x += Time.deltaTime)
			{
				animBar.transform.localPosition = new Vector3(0, 0, 2.5f * x);
				yield return null;
			}
			animBar.transform.localPosition = Vector3.forward * 2.5f;
			animBar.SetActive(false);
		}


		yield return null;

	}
	IEnumerator HandleDelay()
    {
		tpPrepCruelRevenge = true;
		yield return new WaitForSecondsRealtime(5);
		tpPrepCruelRevenge = false;
    }
	IEnumerator ActivateCruelerRevengeTP()
    {
		hasStarted = false;
		harderUCR = true;
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}] TP has activated Unfair's Crueler Revenge! Restarting from the beginning...", loggingModID);
		Debug.LogFormat("[Unfair's Cruel Revenge #{0}] -=--=--=--=--=--=--=--=--=--=--=--=--=--=--=--=--=--=--=--=--=-", loggingModID);
		currentScreenVal = 0;
		while (pigpenDisplay.text.Length > 0 || pigpenSecondary.text.Length > 0 || strikeIDDisplay.text.Length > 0 || mainDisplay.text.Length > 0)
        {
			yield return new WaitForSecondsRealtime(0.02f);
			if (pigpenDisplay.text.Length > 0)
				pigpenDisplay.text = pigpenDisplay.text.Substring(0, pigpenDisplay.text.Length - 1).Trim();
			if (pigpenSecondary.text.Length > 0)
				pigpenSecondary.text = pigpenSecondary.text.Substring(0, pigpenSecondary.text.Length - 1).Trim();
			if (strikeIDDisplay.text.Length > 0)
				strikeIDDisplay.text = strikeIDDisplay.text.Substring(0, strikeIDDisplay.text.Length - 1).Trim();
			if (mainDisplay.text.Length > 0)
				mainDisplay.text = mainDisplay.text.Substring(0, mainDisplay.text.Length - 1).Trim();
		}
		StartCoroutine(indicatorCoreHandlerEX.HandleCollaspeAnim());
		StartCoroutine(IndicatorCoreHandlerExtraScreen.HandleCollaspeAnim());
		var expectedTextToType = "YOU ARE GOING\nTO REGRET THIS.";
		mainDisplay.color = Color.red;
		for (var x = 1; x < expectedTextToType.Length; x++)
        {
			mainDisplay.text = expectedTextToType.Substring(0, x);
			yield return new WaitForSeconds(0.02f);
		}
		for (float x = 0; x <= 1f; x += Time.deltaTime / 2)
		{
			float curScale = 1f - x;
			entireCircle.transform.localScale = new Vector3(curScale, curScale, curScale);
			entireCircle.transform.localEulerAngles = Vector3.up * 720 * (1f - x);
			float currentOffset = Easing.InOutQuad(x, 0, 1f, 1f);
			entireCircle.transform.localPosition = new Vector3(0, 5 * currentOffset, 0);
			yield return null;
		}
		mainDisplay.text = "";
		entireCircle.SetActive(false);
		keyABaseKey = "";
		fourSquareKey = "";
		baseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		displaySubstutionLettersAll.Clear();
		usedRenderers = new[] { statusIndicators.First() }.Concat(statusIndicators.Skip(1).Take(8)).Concat(new[] { statusIndicators.Last() }).ToArray();
		PrepModule();
		UpdateSecondaryScreen();
		LogCurrentInstruction();
		hasStarted = true;
		yield return null;
    }
	bool TimeModeActive;
#pragma warning disable IDE0051 // Remove unused private members
	bool ZenModeActive;
	bool TwitchShouldCancelCommand;
	string TwitchHelpMessage =
		"Select the given button with \"!{0} press R(ed);G(reen);B(lue);C(yan);M(agenta);Y(ellow);Inner;Outer\" " +
		"To time a specific press, append based only on seconds digits (##), up to full time stamp (DD:HH:MM:SS), or MM:SS where MM exceeds 99 min. " +
		"To press the idx/strike screen \"!{0} screen\" Semicolons can be used to combine presses, both untimed and timed.\n"+
		"Enable autocycle on the screen by using \"!{0} autocycle ##.###\", turn autocycle off with \"!{0} autocycle off\", or make the autocycle cycle in the opposite direction with \"!{0} autocycle reverse\". Get the colors of the buttons around the module by using \"!{0} colorblind\" or \"!{0} cycle\"";
#pragma warning restore IDE0051 // Remove unused private members
	IEnumerator ProcessTwitchCommand(string command)
	{
		if (!hasStarted)
		{
			yield return "sendtochaterror The module has not activated yet. Wait for a bit until the module has started.";
			yield break;
		}
		if (isFinished)
		{
			yield return "sendtochaterror The module is already solved, why bother trying to interact with it? (This is an anarchy command prevention message.)";
			yield break;
		}
		string baseCommand = command.ToLower();
		string[] intereptedParts = command.ToLower().Split(';');
		List<KMSelectable> selectedCommands = new List<KMSelectable>();
		List<List<long>> timeThresholds = new List<List<long>>();
		List<string> rearrangedColorList = idxColorList.Select(a => baseColorList[a]).ToList();

        Match autocycleCommands = Regex.Match(command, @"^autocycle\s(\d+(\.\d+)?|off|disable|deactivate|reverse|flip)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
			colorblindCommands = Regex.Match(command, @"^colou?rblind|cycle$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
			cruelRevengeActivationCommands = Regex.Match(command, @"^gimmiecruelerrevenge$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		int[] multiplierTimes = { 1, 60, 3600, 86400 }; // To denote seconds, minutes, hours, days in seconds.
		if (Application.isEditor)
		{
			if (baseCommand.RegexMatch(@"^simulate (off|on)$"))
			{
				yield return null;
				string[] commandParts = command.Split();
				gameInfo.OnLightsChange(commandParts[1].EqualsIgnoreCase("off"));
				yield break;
			}
		}
		if (cruelRevengeActivationCommands.Success)
        {
			if (noTPCruelCruelRevenge)
			{
				yield return "sendtochat {0}, I'm afraid I can't let you do that.";
				yield break;
			}
			else if (legacyUCR)
            {
				yield return "sendtochat {0}, Crueler Revenge cannot be enabled while the module is showing the Legacy version.";
				yield break;
			}
			else if (harderUCR)
			{
				yield return "sendtochat {0}, Crueler Revenge is already enabled.";
				yield break;
			}
			if (!tpPrepCruelRevenge)
            {
				StartCoroutine(HandleDelay());
				yield return "sendtochat {0}, are you sure you want to enable Crueler Revenge? You will NOT be able to revert this back upon doing so! Type in the same command within 5 seconds to confirm.";
				yield break;
			}
			else
            {
				yield return null;
				StartCoroutine(ActivateCruelerRevengeTP());
				yield return "sendtochat {0}, you asked for this.";
				yield break;
			}
		}
		else if (autocycleCommands.Success)
		{
			string[] shutoffCommands = { "off", "disable", "deactivate" };
			string[] reverseCommands = { "reverse", "flip" };
			string curCommand = baseCommand.Split()[1];
			float cycleSpeed = 0;

			if (float.TryParse(curCommand, out cycleSpeed))
			{

				if (cycleSpeed < 0.5f || cycleSpeed > 10f)
				{
					yield return "sendtochaterror I am not setting Auto-Cycle for Unfair's Cruel Revenge (#{1}) at " + cycleSpeed.ToString("0.00") + " intervals.";
					yield break;
				}
				if (cycleSpeed == progressHandler.maxProgress && autoCycleEnabled)
				{
					yield return "sendtochaterror Auto-Cycle interval for Unfair's Cruel Revenge (#{1}) is already at " + cycleSpeed.ToString("0.00") + ".";
					yield break;
				}
				yield return null;
				progressHandler.maxProgress = cycleSpeed;
				progressHandler.curProgress = 0f;
				if (!autoCycleEnabled)
					StartCoroutine(HandleAutoCycleAnim(true));
				yield return "sendtochat {0}, Auto-Cycle has been enabled/adjusted for Unfair's Cruel Revenge (#{1}) at " + cycleSpeed.ToString("0.00") + " intervals.";

			}
			else if (shutoffCommands.Contains(curCommand))
			{
				if (!autoCycleEnabled)
				{
					yield return "sendtochaterror Auto-Cycle for Unfair's Cruel Revenge (#{1}) is already off.";
					yield break;
				}
				yield return null;
				StartCoroutine(HandleAutoCycleAnim(false));
				yield return "sendtochat {0}, autocycle has been disabled for Unfair's Cruel Revenge (#{1}).";
			}
			else if (reverseCommands.Contains(curCommand))
			{
				if (!autoCycleEnabled)
				{
					yield return "sendtochaterror Auto-Cycle for Unfair's Cruel Revenge (#{1}) is off.";
					yield break;
				}
				yield return null;
				inverseAutoCycle = !inverseAutoCycle;
				progressHandler.curProgress = 0f;
				if (!autoCycleEnabled)
					StartCoroutine(HandleAutoCycleAnim(true));
				yield return "sendtochat {0}, autocycle has been reversed for Unfair's Cruel Revenge (#{1}).";
			}
			else
			{
				yield return string.Format("sendtochaterror I don't know what autocycle subcommand \"{0}\" is.", curCommand);
				yield break;
			}
		}
		else if (colorblindCommands.Success)
		{
			bool lastColorblindState = colorblindDetected;
			colorblindDetected = true;
			for (int x = 0; x < 6 && !TwitchShouldCancelCommand; x++)
			{
				var curSelected = colorButtonSelectables[x].Highlight.gameObject;
				var highlight = curSelected.transform.Find("Highlight(Clone)");
				if (highlight != null)
					curSelected = highlight.gameObject ?? curSelected;
				yield return null;
				colorButtonSelectables[x].OnHighlight();
				curSelected.SetActive(true);
				yield return new WaitForSeconds(1.5f);
				if (TwitchShouldCancelCommand || x == 5)
					colorButtonSelectables[x].OnHighlightEnded();
				curSelected.SetActive(false);
				yield return new WaitForSeconds(0.1f);
			}
			if (TwitchShouldCancelCommand)
			{
				colorButtonSelectables[0].OnHighlightEnded();
				yield return "cancelled";
			}
			colorblindDetected = lastColorblindState;
			yield break;
		}
		else
		{
			foreach (string commandPart in intereptedParts)
			{
				string partTrimmed = commandPart.Trim();
				if (partTrimmed.RegexMatch(@"^press "))
				{
					partTrimmed = partTrimmed.Substring(5).Trim();
				}
				string[] partOfPartTrimmed = partTrimmed.Split();
				if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer)( (at|on))?( [0-9]+:([0-5][0-9]:){0,2}[0-5][0-9])+$"))
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
				else if (partTrimmed.RegexMatch(@"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer)( (at|on))?( [0-5][0-9])+$"))
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
				else if (Regex.IsMatch(partTrimmed, @"^(r(ed)?|g(reen)?|b(lue)?|c(yan)?|m(agenta)?|y(ellow)?|inner|outer|screen)$"))
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
					case "screen":
						selectedCommands.Add(idxStrikeSelectableB);
						break;
					default:
						yield return "sendtochaterror You aren't supposed to get this error. If you did, it's a bug, so please contact the developer about this.";
						yield break;
				}
			}
		}
		hasStruck = false;
		if (selectedCommands.Any())
		{
			yield return "multiple strikes";
			for (int x = 0; x < selectedCommands.Count && !hasStruck; x++)
			{
				yield return null;
				if (hasStruck) yield break;
				while (isChangingColors)
                {
					yield return string.Format("trycancel Your button press has been canceled after {0} press{1} in the command specified.", x + 1, x == 1 ? "" : "es");
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
						yield return string.Format("trycancel Your timed interation has been canceled after a total of {0}/{1} presses in the command that was sent.", x + 1, selectedCommands.Count);
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
				var buttonPressed = selectedCommands[x] == innerRing ? "Inner" :
					selectedCommands[x] == outerSelectable ? "Outer" :
					colorButtonSelectables.Contains(selectedCommands[x]) ? baseColorList[idxColorList[Array.IndexOf(colorButtonSelectables, selectedCommands[x])]] : "???";
				if (!IsCurInstructionCorrect(buttonPressed) && selectedCommands.Count > 1 && buttonPressed != "???")
                {
					yield return string.Format("strikemessage by incorrectly pressing {0} on {1} after {2} press{3} in the TP command specified!", buttonPressed == "Inner" ? "Inner Center" : buttonPressed == "Outer" ? "Outer Center" : buttonPressed, bombInfo.GetFormattedTime(), x + 1, x == 1 ? "" : "es");
				}
				else if (IsCurInstructionCorrect(buttonPressed) && harderUCR && currentInputPos + 1 >= splittedInstructions.Count)
					yield return "awardpointsonsolve 30";
				selectedCommands[x].OnInteract();
				if (x + 1 < selectedCommands.Count && colorButtonSelectables.Contains(selectedCommands[x + 1]) && harderUCR && !hasStruck)
				{
					yield return "sendtochat {0}, I'm not allowing you to press another colored button in the same command due to how Crueler Revenge changes the buttons around.";
					yield break;
				}
				yield return new WaitForSeconds(0.1f);
			}
			yield return "end multiple strikes";
		}
		yield break;
	}
}
