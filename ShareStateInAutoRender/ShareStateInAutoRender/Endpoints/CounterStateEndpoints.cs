using ShareStateInAutoRender.State;

namespace ShareStateInAutoRender.Endpoints
{
    public static class CounterStateEndpoints
    {
        public static void MapCounterStateEndpoints(this WebApplication app)
        {
            app.MapGet("api/counterState", GetHandler);
            app.MapPost("api/counterState", PostHandler);
        }

        private static IResult GetHandler(HttpContext context)
        {
            var counter = CounterStateHolder.GetCounter();
            return Results.Ok(counter);
        }

        private static IResult PostHandler(HttpContext context, CounterValueJson counterValueJson)
        {
            CounterStateHolder.SetCounter(counterValueJson.Counter);
            return Results.Ok();
        }
    }

    public class CounterValueJson
    {
        public int Counter { get; set; }
    }
}
