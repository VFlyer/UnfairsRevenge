public class CipherCaesar : AnyCipherScript {

    private int encodeKey = 0;

    public CipherCaesar(int key)
    {
        encodeKey = key;
        alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    }
    public override string Encode(string value)
    {
        return base.Encode(value);
    }
}
