using System.Collections.Generic;
public class CipherCaesar : AnyCipherScript {

    private int encodeKey = 0;

    public CipherCaesar(int key)
    {
        encodeKey = key;
        usedAlphabets = new string[0];
    }
    public override IEnumerable<string> Encode(params string[] value)
    {
        return value;
    }
}
