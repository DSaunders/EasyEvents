# EasyEvents [![NuGet Version](https://img.shields.io/nuget/v/EasyEvents.Core.svg?style=flat)](https://www.nuget.org/packages/EasyEvents.Core/)

#### Making Event Sourcing easy for small applications

- .NET Core
- Strongly typed events
- Async everywhere
- Multiple event storage options; SqlServer, file system and in-memory (EventStore coming soon)

Raise events, then create handlers that respond to the events and modify state _in memory_. 
When your application re-starts, replay all of the events to restore your state with one line of code.

EasyEvents makes is easy to persist events - not state. 

## Installation

Install via NuGet:

```
PM > Install-Package EasyEvents.Core
```

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

If your Event contains a property named `DateTime` (that is a `System.DateTime`), it will be auto-populated for you with the current date and time in UTC.

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

Here are the settings you'll need:

### Store

Tells EasyEvents where to store the event streams. See the 'Available Stores' section (you can also write your own store).

### HandlerFactory

This is a function that tells EasyEvents how to create handlers. We'll pass you a `Type` (that will be an `IEventHandler<IEvent>`), and you return the implementation.

This allows you to use your own container, just hook up the resolve method here.

## IoC Setup

If your IoC container auto-registers everything, you're probably fine.

If not, here a quick summary:

- `IEasyEvents` should be a singleton, and resolve to `EasyEvents`. This is how you'll raise events.
- Register all `IEventHandler<T>`s. For example `IEventHandler<CustomerCreated>` might resolve to your `CustomerCreatedHandler` class

## Available stores

### SQL Server

The database must exist, but an `events` table will be created for you with the correct schema.

The only parameter to the `SqlEventStore` constructor is the connection string for the database where you wish to store your events.

```csharp
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new SqlEventStore("server=.;database=test;")
});
```

Internally, the `SqlEventStore` uses all Async methods when writing to the database. 

The SQL store currently only support a single consumer. When an event is raised, it is persisted and them replayed back to any handlers.
Support for multiple applictions subscribing to the same event streams is listed in the 'Currently working on' section below.

### In Memory

As you would guess, stores events in memory only.

You'll only use this for unit testing, and perhaps when testing your application locally.

```csharp
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new InMemoryEventStore()
});
```

### File system

Stores all events into files named after your stream, suffixed with `.stream.txt`. For example `app-events.stream.txt`.

_The implementation of this store is still a little rough, so it's really only safe to use for local testing/development - which it's perfectly fine for. 
See the 'Currently working on' section for how this is being improved_

By default, events are stored in a `\_events` folder under the current directory. You can override this by providing an absolute path in the constructor.

```csharp
// Using the default path
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new FileSystemEventStore()
});

// Using a custom path
easyEvents.Configure(new EasyEventsConfiguration
{
    EventStore = new FileSystemEventStore("c:\events")
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

### Streams
It's logical to use the 'aggregate roots' in your application as your event streams. For example, you might have a 'Users' stream, that contains all user related events.

A neat way to do this in EasyEvents is to have a base class `IEvent` for each aggregate root, this will contain the stream name. Then, just derive each event from the base class, ensuring they always remain on the same stream.

```csharp
abstract class ApplicationEvents : IEvent
{
    public string Stream => "AppEvents";
}

class AppStartedEvent : ApplicationEvents
{
}
```

## Currently working on:
- Stores
    - All
        - Stop having to store full Assembly name for events, find a .NET Core replacement for enumerating all types and finding them by their Type's 'Name' property only (then cache the result).
    - SQL
        - Support multiple consumers using `SqlDependency` (not yet in .NET Core)
        - Versioning per-stream/Optimistic locking
    - EventStore
        - Build it
        - Versioning/Optimistic locking
    - File
        - Split a stream over multiple files when it gets too large to be managable
    - In Memory
        - Thread-safety
- Querying a stream's history using LINQ
    - Ideally by streaming, rather than loading the entire stream into memory
- StreamState
    - Improve API (instead of exposing IDictionary)
    - Thread-safety
