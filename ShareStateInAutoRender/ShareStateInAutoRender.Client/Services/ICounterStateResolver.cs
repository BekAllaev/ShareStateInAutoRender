namespace ShareStateInAutoRender.Client.Services
{
    public interface ICounterStateResolver
    {
        public Task SetCounter(int counter);

        public Task<int> GetCounter();
    }
}
