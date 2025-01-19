namespace Services.Keys
{
    public interface IKeyGenerator
    {
        string Generate(int length);
    }
}