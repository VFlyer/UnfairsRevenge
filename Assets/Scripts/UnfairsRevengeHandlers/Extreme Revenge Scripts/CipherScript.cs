using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnyCipherScript
{
    protected string[] encodingStrings;
    protected List<string> keywords;
    protected string alphabet;

	public AnyCipherScript()
    {
        keywords = new List<string>();
        encodingStrings = new string[0];
        alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    }
    public virtual IEnumerable<string> GetEncodingString()
    {
        return encodingStrings;
    }
    public virtual void ChangeAlphabet(string newAlphabet)
    {
        alphabet = newAlphabet;
    }
    public virtual IEnumerable<string> GetKeywords()
    {
        return keywords;
    }
    public virtual string Encode(string value)
    {
        return value;
    }
}
