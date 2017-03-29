# EasyEvents

Making Event Sourcing easy

- Strongly typed events
- Async everywhere
- Multiple event storage options (even SQL, because why wouldn't you want to store all your events in a SQL server!?)

EasyEvent encourages you to persist events, not state. Raise events, then create handlers that respond to the events and modify state _in memory_. 
When your application re-starts, replay all of the events to restore your state with one line of code.

## Show me some code ..

Create an event:

```csharp
public class UserCreated : IEvent
{
    public string Stream => "user-events";
    public string Name { get; }
    
    public UserCreated(string name) {
        Name = name;
    }
}
```

Raise the event:

```csharp
await easyEvents.RaiseEventAsync(new UserCreated("Bob"));
```

Handle the event:

```csharp
public class UserCreatedHandler : IEventHandler<UserCreated>
{
    public async Task HandleEvent(UserCreated @event)
    {
        await _myUsersDatabase.Add(@event.Name);
        Console.WriteLine("Say hello to " + @event.Name);
    }
}
```

When your application restarts, replay all events to recreate your state:

```csharp
await easyEvents.ReplayAllEventsAsync();
```

Add listeners to your streams that re-raise events, project data out into a read model etc..

```csharp
events.AddProcessorForStream("subscriptions", async (context, event) =>
{
    if (event is UserCreated)
    {
        await events.RaiseEventAsync(new HaveAParty());
    }
    
    // We also have access to the stream's 'context' here, which is a simple Dictionary<string,object> we 
    //  can use to store state between these subscriptions. For example, we could count the number of 
    //  UserCreated events before a UserDeleted event
});
```

## Configuration

Configuring EasyEvents is, well.. easy!

Incude something like this in your applications startup/bootsrapper class:

```csharp
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new SqlEventStore("server=.;database=test;"),
    HandlerFactory = type => { return container.Resolve(type); }
});
```

Here are the setting you'll need:

### Store

Tells EasyEvents where to store the event streams. See the 'Available Stores' section (you can also write your own store).

### HandlerFactory

This is a function that tells EasyEvents how to create handlers. We'll pass you a `Type` (that will be an `IEventHandler<>`), and you return the implementation.

This allows you to use your own container, just hook up the resolve method here.

## IoC Setup

If your IoC container auto-registers everything, you're probably fine.

If not, here a quick summary:

- `IEasyEvents` should be a singleton, and resolve to `EasyEvents'. This is how you'll raise events.
- Register all `IEventHandler<T>`s. For example `IEventHandler<CustomerCreated>` might resolve to `CustomerCreatedHandler`


## Replaying events

To re-create your events, you'll want to do this on startup:

```csharp
await eventDb. events.ReplayAllEventsAsync();
```

This will replay all events to your handlers.

If you wish to hook into this mechanism to avoid repeating sections of your own code (e.g. to avoid sending a welcome email again when your application restarts and re-plays the 'new customer' event), take a dependency on `IEasyEvents` and check the `IsReplayingEvents` property.

NB: Another pattern to avoid duplicating commands like sending emails, is to have your event handler populate a queue. When the email is sent, another event removes the email from the queue. Once the app is started and all events are replayed, process the queue and send only the emails that are still in the queue. 

## General guidlines

## Immutable events
Your events should be immutable and represent the past-tense (e.g. UserCreated).
Once and event is raised, it is saved immediately. Changing the properties of that object will cause issues when re-playing events, as we can't keep track of those changes.

TODO
- aggregates same stream base class
- replay events on start

### Currently working on:
- SQL storage - Option to use a table per stream
- EventStore storage
- File storage
