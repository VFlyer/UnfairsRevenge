using System.Collections.Generic;

public class AnyCipherScript
{
    protected string[] encodingStrings;
    protected List<string> displayTexts;
    protected string[] usedAlphabets;
    int alphabetsRequired { get { return 0; } }

	public AnyCipherScript()
    {
        displayTexts = new List<string>();
        encodingStrings = new string[0];
        usedAlphabets = new string[0];
    }
    public virtual IEnumerable<string> GetEncodingString()
    {
        return encodingStrings;
    }
    public virtual void AssignAlphabets(params string[] newAlphabets)
    {
        usedAlphabets = newAlphabets;
    }
    public virtual IEnumerable<string> GetKeywords()
    {
        return displayTexts;
    }
    public virtual IEnumerable<string> Encode(params string[] valuesToEncrypt)
    {
        return valuesToEncrypt;
    }
}
