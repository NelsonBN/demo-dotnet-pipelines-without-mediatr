using Microsoft.Extensions.DependencyInjection;
using static System.Console;

var services = new ServiceCollection();

services.AddTransient(typeof(IPipeline<>), typeof(Pipeline<>));

services
    .AddSingleton<UseCase1Command>()
    .AddSingleton<UseCase2Command>()
    .AddSingleton<UseCase1Query>()
    .AddSingleton<UseCase2Query>();

var serviceProvider = services.BuildServiceProvider();

var command1 = serviceProvider.GetRequiredService<IPipeline<UseCase1Command>>();
command1.Run(r => r.Hi());

var command2 = serviceProvider.GetRequiredService<IPipeline<UseCase2Command>>();
command2.Run(r => r.Bye());

var query1 = serviceProvider.GetRequiredService<IPipeline<UseCase1Query>>();
WriteLine(query1.Run(r => r.HowAreYou()));

var query2 = serviceProvider.GetRequiredService<IPipeline<UseCase2Query>>();
WriteLine(query2.Run(r => r.WhereAreYouFrom()));



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


interface IRunner { }

interface IPipeline<TRunner>
    //where TRunner : IRunner
{
    void Run(Action<TRunner> action);
    R Run<R>(Func<TRunner, R> action);
}

class Pipeline<TRunner>(TRunner runner) : IPipeline<TRunner>
    //where TRunner : IRunner
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
