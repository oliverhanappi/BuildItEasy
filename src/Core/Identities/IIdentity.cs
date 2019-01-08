namespace BuildItEasy.Identities
{
    public interface IIdentity<out T>
    {
        T GetNextValue();
    }
}