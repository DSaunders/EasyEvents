# EasyEvents

#### Making Event Sourcing easy for small applications

- Strongly typed events
- Async everywhere
- Multiple event storage options (even SQL Server, because why wouldn't you want to store all your events in SQL Server!?)

Raise events, then create handlers that respond to the events and modify state _in memory_. 
When your application re-starts, replay all of the events to restore your state with one line of code.

EasyEvents makes is easy to persist events - not state. 

## Events

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

The `stream` property is used to logically segregate your events. For example, you might have a stream for user account events (password changed, logged in), another for shopping cart events (item added, item removed) etc.

## Event Handlers

Event handlers subscribe to and process a single event, encouraging terse code that follows the single responsibility principle. 

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

As a general rule, application state should be kept in memory. State is then mutated only by your event handlers responding to an event, and will be recreated by replaying the same events whenever your application starts.

## Raising events

Raise events with one line:

```csharp
await easyEvents.RaiseEventAsync(new UserCreated("Bob"));
```

When your application starts, replay _all_ events to recreate your state:

```csharp
await easyEvents.ReplayAllEventsAsync();
```

## Processors

Add processors to your streams that run for each event put on a stream.

Use processors to raise new events, project data out into a read model used for reporting, or just to write to a log.

```csharp
easyEvents.AddProcessorForStream("subscriptions", async (context, evt) =>
{
    if (evt is UserCreated)
    {
        await events.RaiseEventAsync(new HaveAParty());
    }
});
```

We also have access to the the streams 'context' here. This is a `Dictionary<string,object>` that can be used to store anything you like between events. This example uses the stream context to track the number of users, raising a new event every 10 users:

```csharp
easyEvents.AddProcessorForStream("subscriptions", async (context, evt) =>
{
    if (!(evt is UserCreated))
        return;
        
    // Get number of users from context
    var numberOfUsers = context.ContainsKey("numberOfUsers")
        ? (int) context["numberOfUsers"]
        : 0;

    // Increment the number of users
    numberOfUsers++;
    
    // Number of users is a multiple of 10
    if (numberOfUsers % 10 == 0)
        await events.RaiseEventAsync(new EveryTenUsersEvent());
        
    // Update context for the next event    
    context["numberOfUsers"] = numberOfUsers;
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

- `IEasyEvents` should be a singleton, and resolve to `EasyEvents`. This is how you'll raise events.
- Register all `IEventHandler<T>`s. For example `IEventHandler<CustomerCreated>` might resolve to your `CustomerCreatedHandler` class

## Available stores

### SQL Server

Currently stores all events in a single table, but will soon seperate them by stream. 

The database must exist, but the tables will be created for you if they do not exist.

The only parameter to the `SqlEventStore` constructor is the connection string for the database where you wish to store your events.

```csharp
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new SqlEventStore("server=.;database=test;")
});
```

Internally, the `SqlEventStore` uses all Async methods when writing to the database. 

### In Memory

As you would guess, stores events in memory only.

You'll only use this for unit testing, and perhaps when testing your application locally.

```csharp
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new InMemoryEventStore()
});
```


## Replaying events

To re-create your events, you'll want to do this on startup:

```csharp
await eventDb. events.ReplayAllEventsAsync();
```

This will replay all events to your handlers.

If you wish to hook into this mechanism to avoid repeating sections of your own code (e.g. to avoid sending a welcome email again when your application restarts and re-plays the 'new customer' event), take a dependency on `IEasyEvents` and check the `IsReplayingEvents` property.

NB: Another pattern to avoid duplicating commands like sending emails, is to have your event handler populate a queue. When the email is sent, another event removes the email from the queue. Once the app is started and all events are replayed, process the queue and send only the emails that are still in the queue. 

## General guidlines

### Immutable events
Your events should be immutable and represent the past-tense (e.g. UserCreated).
Once and event is raised, it is saved immediately. Changing the properties of that object will cause issues when re-playing events, as we can't keep track of those changes.

TODO
- aggregates same stream base class
- replay events on start

## Currently working on:
- SQL storage - Option to use a table per stream
- EventStore storage
- File storage
