using ShareStateInAutoRender.Client.Services;
using ShareStateInAutoRender.State;

namespace ShareStateInAutoRender.Services
{
    public class ServerCounterStateResolver : ICounterStateResolver
    {
        public async Task<int> GetCounter()
        {
            await Task.CompletedTask;

            return CounterStateHolder.GetCounter();
        }

        public async Task SetCounter(int counter)
        {
            await Task.CompletedTask;

            CounterStateHolder.SetCounter(counter);
        }
    }
}
