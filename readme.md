# eventDb 

A library designed to make event sourcing simple in small projects.

- Strongly typed events
- Async everywhere
- Handle events by implementing an interface
- Bring your own IoC container, just tell eventDb how to get to it
- Various storage options (including SQL, because why wouldn't you want to store all your events in a single SQL table?)



TODO: brain dump - write about all this stuff
```csharp
public class UserCreated : IEvent
{
    public string Stream => "user-events";
    public string Name { get; set; }
}
```

```csharp
await eventDb.RaiseEventAsync(new UserCreated { Name = "Bob" });
```

```csharp
public class UserCreatedHandler : IEventHandler<UserCreated>
{
    public async Task HandleEvent(UserCreated @event)
    {
        Console.WriteLine("Say hello to " + @event.Name);
    }
}
```

```csharp
eventDb.Configure(new EventDbConfiguration
{
    EventStore = new SqlEventStore("server=.;database=test;"),
    HandlerFactory = type => { return container.Resolve(type); }
});
```

```csharp
await eventDb.ReplayAllEvents();
```




TODO: general guidlines

- events idempotent
- aggregates same stream base class
- replay events on start