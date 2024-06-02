# Demo - .NET - Pipeline without MediatR

A simple example of how we can implement a pipeline to run a piece of code before and after the main logic of a method (Command/Query) without using MediatR.

> Note: This example is not a full implementation of the `Pipeline` feature of MediatR. It's just a simple example for we understand how we can implement a pipeline without using MediatR for some scenarios as logging, audit logs, validations, etc.


### Create a Pipeline

First, we need to create a pipeline that will be responsible for running the code before and after the main logic of a method.

* The pipeline will have a method `Run` that will receive an action to execute the use case.
* The pipeline will have a method `Run` that will receive a function to execute the use case and return a result.

```csharp
interface IPipeline<TRunner>
{
    void Run(Action<TRunner> action);
    R Run<R>(Func<TRunner, R> action);
}
```

If we prefer also can create a marker interface `IRunner` to ensure that the class that will be passed to the pipeline has the method `Run`.

```csharp
interface IRunner { }

interface IPipeline<TRunner>
    where TRunner : IRunner
{
    void Run(Action<TRunner> action);
    R Run<R>(Func<TRunner, R> action);
}
```

#### Concrete Pipeline implementation

```csharp

class Pipeline<TRunner>(TRunner runner) : IPipeline<TRunner>
    where TRunner : IRunner
{
    public void Run(Action<TRunner> action)
    {
        WriteLine($"##### Send -> Before {typeof(TRunner).Name} #####");
        action(runner);
        WriteLine($"##### Send -> After {typeof(TRunner).Name} #####");
    }

    public TResult Run<TResult>(Func<TRunner, TResult> action)
    {
        WriteLine($"##### Get -> Before {typeof(TRunner).Name} #####");
        TResult result = action(runner);
        WriteLine($"##### Get -> After {typeof(TRunner).Name} #####");

        return result;
    }
}
```

Like we can see, the pipeline can run a piece of code before and after the main logic of a method (`action(runner);`, `TResult result = action(runner);`).


### Dependency Injection

#### Inject the pipeline

In this case we will use the Open Generic Type to register the pipeline in the DI container.

```csharp
services.AddTransient(typeof(IPipeline<>), typeof(Pipeline<>));
```

#### Inject use cases

```csharp
services
    .AddSingleton<UseCase1Command>()
    .AddSingleton<UseCase2Command>()
    .AddSingleton<UseCase1Query>()
    .AddSingleton<UseCase2Query>();
```


### Use Case

Now we can create our use cases (Command/Query) reponsible for the main logic of the application.
If we create a marker interface `IRunner` we need to extend it in the use cases.

```csharp
class UseCase1Query : IRunner
{
    public string HowAreYou()
    {
        WriteLine("How are you?");
        return "I'm fine, thank you!";
    }
}

class UseCase2Query : IRunner
{
    public string WhereAreYouFrom()
    {
        WriteLine("Where are you from?");
        return "I'm from Portugal!";
    }
}

class UseCase1Command : IRunner
{
    public void Hi() => WriteLine("Hello World!!!");
}

class UseCase2Command : IRunner
{
    public void Bye() => WriteLine("Goodbye!!!");
}
```


### Usage

```csharp

var command1 = serviceProvider.GetRequiredService<IPipeline<UseCase1Command>>();
command1.Run(r => r.Hi());

var command2 = serviceProvider.GetRequiredService<IPipeline<UseCase2Command>>();
command2.Run(r => r.Bye());

var query1 = serviceProvider.GetRequiredService<IPipeline<UseCase1Query>>();
WriteLine(query1.Run(r => r.HowAreYou()));

var query2 = serviceProvider.GetRequiredService<IPipeline<UseCase2Query>>();
WriteLine(query2.Run(r => r.WhereAreYouFrom()));
```
