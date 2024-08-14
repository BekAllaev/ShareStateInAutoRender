namespace ShareStateInAutoRender.State
{
    public static class CounterStateHolder
    {
        private static int _counter;

        public static void SetCounter(int counter)
        {
            _counter = counter;
        }

        public static int GetCounter()
        {
            return _counter;
        }
    }
}
