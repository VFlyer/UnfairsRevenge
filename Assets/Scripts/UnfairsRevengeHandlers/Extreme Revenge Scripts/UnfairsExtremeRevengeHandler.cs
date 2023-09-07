using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnfairsExtremeRevengeHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		for (var x = 0; x < 26; x++)
        {
			Debug.Log("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(a => alphabet[(alphabet.IndexOf(a) + x) % alphabet.Length]).Join(""));
        }
		Debug.Log("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(a => alphabet[(alphabet.IndexOf(a)  * 5) % alphabet.Length]).Join(""));
	}

	// Update is called once per frame
	void Update () {

	}
}
