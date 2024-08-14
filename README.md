# Share state between blazor server and blazor web assembly(client)

This repo was inspired by this article - https://www.telerik.com/blogs/fetching-sharing-data-between-components-blazor-auto-render-mode

**Problem:** component that has auto render mode should be able to read/write the state from the server and from the client.
Since its first render occurs on the server and next renders occur on the client. 

How to make this process seamleassly?

**Solution:** store the state on the server. Create interface and register its implementation on the client and on the server.
The server's implementation will write/read the state directly from the server and client's one will read/write state via http requests.

## Step-by-step

**Description -** in this step-by-step tutorial we will create two Counter components and share value of counter between this components

1. Creat class that will store the state. In the server project create folder `State` and create there static class `CounterStateHolder`.
My `CounterStateHolder` is like this:
```
public class CounterStateHolder
{
    private int _counter;

    public void SetCounter(int counter)
    {
        _counter = counter;
    }

    public int GetCounter()
    {
        return _counter;
    }
}
```
2. Register this class as singleton on the server
3. On the client side create folder `Services` and create there service `ICommonStateResolver`. In my case interface looks like this:
```
public interface ICounterStateResolver
{
    public Task SetCounter(int counter);

    public Task<int> GetCounter();
}
```
4. Now let's comeback to the server project and create endpoints for reading/writing the state. I create static class `CounterStateEndpoint` in the `Endpoints` folder 
```
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
```
Don't forget to add `MapCounterStateEndpoints` to the server's `Program.cs`. You can add it right before `app.Run()`

5. Let's add server's implementation of the `ICounterStateResolver`. Create `Services` folder on the server and place there `ServerCounterStateResolver`
```
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
```
You can see that in server implementation we just call service that is placed on the server itself. You will feel the diference when in this client's implementation we will 
use http requests

Register it in the DI:
```
builder.Services.AddSingleton<ICounterStateResolver, ServerCounterStateResolver>();
```

6. You can plase client side registration in the `Services` folder of the client project. You can implement it like this:
```
public class ClientCounterStateResolver : ICounterStateResolver
{
    private readonly HttpClient _httpClient;

    public ClientCounterStateResolver(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<int> GetCounter()
    {
        return _httpClient.GetFromJsonAsync<int>("api/counterState");
    }

    public Task SetCounter(int counter)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                counterValue = counter
            }),
            Encoding.UTF8,
            "application/json");

        return _httpClient.PostAsync("api/counterState", jsonContent);
    }
}
```

Register it in the DI:

```
builder.Services.AddSingleton<ICounterStateResolver, ClientCounterStateResolver>();
```

7. First of all rename `Counter.razor` to `FirstCounter` and then edit it a little bit: 
```
@page "/firstCounter"
@using ShareStateInAutoRender.Client.Services
@rendermode InteractiveAuto
@implements IDisposable
@inject ICounterStateResolver CounterStateResolver
@inject PersistentComponentState ApplicationState

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    private PersistingComponentStateSubscription persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        persistingSubscription = ApplicationState.RegisterOnPersisting(PersistCounterValue);

        if (!ApplicationState.TryTakeFromJson<int>("CounterValue", out var counterValue))
        {
            currentCount = await CounterStateResolver.GetCounter();
        }
        else
        {
            currentCount = counterValue;
        }

        await base.OnInitializedAsync();
    }

    private Task PersistCounterValue()
    {
        ApplicationState.PersistAsJson("CounterValue", currentCount);

        return Task.CompletedTask;
    }

    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }

    public void Dispose()
    {
        persistingSubscription.Dispose();
    }
}
```

Changes made: 
- Edit navigation path from "counter" to "firstCounter" so we can have two counters which will share the state
- Inject `ICounterStateResolver` with which we will read/write state
- Inject `PersistentComponentState` so we can persist pre-render state otherwise we will see flickering
- Implement `IDisposable` so we can dispose subscription which will be fired when component will persist state after pre-rendering
- Add `OnInitializedAsync` where we either retrive persisted state or retrive state from the `ICounterStateResolver`
- Add `PersistCounterValue` method that persist state after pre-rendering
- Add `Dispose` method where we dispose `persistingSubscription` variable

8. Add razor component `SecondCounter.razor`. Copy-paste content of the `FirstCounter` and just edit its `@page` attribute to `secondCounter`

9. Add nav link to the `SecondCounter`, as a result you will have this `NavMenu.razor`:
```
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">ShareStateInAutoRender</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="firstCounter">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> First Counter
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="secondCounter">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> Second Counter
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="weather">
                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Weather
            </NavLink>
        </div>

    </nav>
</div>
```

10. Now you can run your application and see that "Current count" value is sync on both pages