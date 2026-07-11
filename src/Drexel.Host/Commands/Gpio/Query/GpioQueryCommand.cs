using System;
using System.Device;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Gpio.Query;

/// <summary>
/// Performs a device query.
/// </summary>
internal sealed class GpioQueryCommand : Command<GpioQueryCommand.Options, GpioQueryCommand.Handler>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GpioQueryCommand"/> class.
    /// </summary>
    public GpioQueryCommand()
        : base("query", "Performs a device query.")
    {
    }

    /// <summary>
    /// The options associated with performing the command.
    /// </summary>
    public new sealed class Options
    {
    }

    /// <summary>
    /// The command implementation.
    /// </summary>
    /// <param name="console">
    /// The console to use.
    /// </param>
    public new sealed class Handler(IAnsiConsole console) : ICommandHandler<Options, Handler>
    {
        /// <inheritdoc/>
        public async Task<int> HandleAsync(Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using GpioController controller = new();

            ComponentInformation root = controller.QueryComponentInformation();
            console.WriteLine(root);

            return ExitCode.Success;
        }
    }
}

file static class ConsoleExtensions
{
    public static void Write(this IAnsiConsole console, Indentation indentation, string text)
    {
        console.Write(indentation.Value);
        console.Write(text);
    }

    public static void WriteLine(this IAnsiConsole console, Indentation indentation, string text)
    {
        console.Write(indentation.Value);
        console.WriteLine(text);
    }

    public static void WriteJsonPropertyName(this IAnsiConsole console, Indentation indentation, string text)
    {
        console.Write(indentation.Value);
        console.WriteJsonStringValue(text);
        console.Write(": ");
    }

    public static void WriteJsonStringValue(this IAnsiConsole console, string text)
    {
        console.Write("\"");
        console.Write(text.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\""));
        console.Write("\"");
    }

    public static void WriteLine(this IAnsiConsole console, ComponentInformation component)
        => console.WriteLine(component, new Indentation(2));

    public static void WriteLine(this IAnsiConsole console, ComponentInformation component, Indentation indent)
    {
        console.WriteLine(indent, "{");
        using (var token1 = indent.Indent())
        {
            console.WriteJsonPropertyName(indent, "Name");
            console.WriteJsonStringValue(component.Name);
            console.WriteLine(",");

            console.WriteJsonPropertyName(indent, "Description");
            console.WriteJsonStringValue(component.Description);
            console.WriteLine(",");

            console.WriteJsonPropertyName(indent, "Properties");
            console.WriteLine("[");
            using (var token2 = indent.Indent())
            {
                foreach (var kvp in component.Properties)
                {
                    console.WriteJsonPropertyName(indent, kvp.Key);
                    console.WriteJsonStringValue(kvp.Value);
                    console.WriteLine(",");
                }
            }
            console.WriteLine("]");

            console.WriteJsonPropertyName(indent, "SubComponents");
            console.WriteLine("[");
            using (var token2 = indent.Indent())
            {
                foreach (ComponentInformation subComponent in component.SubComponents)
                {
                    console.WriteLine(subComponent, indent);
                }
            }
            console.WriteLine("]");
        }

        console.WriteLine(indent, "}");
    }
}

file sealed class Indentation
{
    public Indentation(int size)
    {
        Size = size;
        Value = new(' ', 0);
    }

    public string Value { get; private set; }

    private int Size { get; }

    public IDisposable Indent()
    {
        Value = new(' ', Value.Length + Size);
        return new Token(Unindent);
    }

    private void Unindent()
    {
        int newLength = Value.Length - Size;
        if (newLength < 0)
        {
            newLength = 0;
        }

        Value = new(' ', newLength);
    }

    private sealed class Token : IDisposable
    {
        private Action? _callback;

        public Token(Action callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            Action? callback = Interlocked.Exchange(ref _callback, null);
            callback?.Invoke();
        }
    }
}