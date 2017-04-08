public class Pair<TA, TB>
{
    public TA A { get; private set; }
    public TB B { get; private set; }

    public Pair(TA a, TB b)
    {
        A = a;
        B = b;
    }
}