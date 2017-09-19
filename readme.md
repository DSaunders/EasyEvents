# EasyEvents [![NuGet Version](https://img.shields.io/nuget/v/EasyEvents.Core.svg?style=flat)](https://www.nuget.org/packages/EasyEvents.Core/)

#### Making Event Sourcing easy for small applications

- .NET Core
- Strongly typed events and handlers
- Async everywhere
- Add processors that run for every event on a stream (to project into reporting, raise new events or just for logging)
- Multiple event storage options; SqlServer, file system and in-memory (EventStore coming soon)

Raise events, then create handlers that respond to the events and modify state. 
When your application re-starts, replay all of the events to restore your state with one line of code.

EasyEvents makes is easy to persist events - not state.

```
PM > Install-Package EasyEvents.Core
```


```csharp
public class UserCreated : IEvent
{
    public string Stream => "user-events";
    public string UserName { get; }
    
    public UserCreated(string username) {
        UserName = username;
    }
}
```

```csharp
public class UserCreatedHandler : IEventHandler<UserCreated>
{
    public async Task HandleEvent(UserCreated @event)
    {
        Console.WriteLine("Hello, " + @event.UserName);
    }
}
```

```csharp
await easyEvents.RaiseEventAsync(new UserCreated("Jane"));
```

See the [Getting Started](https://github.com/DSaunders/EasyEvents/wiki/Getting-Started) to find out more.
