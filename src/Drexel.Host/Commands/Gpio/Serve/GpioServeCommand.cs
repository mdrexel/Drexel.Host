using System;
using System.CommandLine;
using System.Device.Gpio;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Gpio.Serve;

internal sealed class GpioServeCommand : Command<GpioServeCommand.Options, GpioServeCommand.Handler>
{
    private static Option<int> Port { get; } =
        new(["--port", "-p"], "The port to listen on.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioServeCommand"/> class.
    /// </summary>
    public GpioServeCommand()
        : base("serve", "Starts an HTTP server for interacting with GPIO pins.")
    {
        Add(Port);
    }

    /// <summary>
    /// The options associated with performing the command.
    /// </summary>
    public new sealed class Options
    {
        /// <summary>
        /// Gets the port to listen on.
        /// </summary>
        public int Port { get; init; }
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
            using HttpListener httpListener = new();
            httpListener.Prefixes.Add($"http://+:{options.Port}/");

            console.WriteLine("Listening on:");
            foreach (string prefix in httpListener.Prefixes)
            {
                console.WriteLine($"  {prefix}");
            }

            httpListener.Start();
            await using CancellationTokenRegistration registration = cancellationToken.Register(httpListener.Stop);

            while (httpListener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await httpListener.GetContextAsync().ConfigureAwait(false);
                    _ = Task.Run(
                        () => HandleRequestAsync(controller, context, cancellationToken),
                        cancellationToken);
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested)
                {
                    // Ignore exceptions that occur when we're shutting down.
                }
            }

            return ExitCode.Success;
        }

        private async Task HandleRequestAsync(
            GpioController controller,
            HttpListenerContext listenerContext,
            CancellationToken cancellationToken)
        {
            Guid traceId = Guid.NewGuid();
            HttpContext context =
                new()
                {
                    CancellationToken = cancellationToken,
                    Controller = controller,
                    Request = listenerContext.Request,
                    Response = listenerContext.Response,
                    TraceId = traceId,
                };
            console.WriteLine($"[{traceId}] Received request: {context.Request.RawUrl}");

            try
            {
                if (IsRead(context, out var read))
                {
                    await read.Invoke().ConfigureAwait(false);
                    return;
                }
                else if (IsWrite(context, out var write))
                {
                    await write.Invoke().ConfigureAwait(false);
                    return;
                }

                throw new NoHandlerException();
            }
            catch (Exception e)
            {
                console.WriteException(e);

                context.Response.StatusCode = (int)GetStatusCode();
                context.Response.SendChunked = true;

                ReadOnlyMemory<char> asString = e.ToString().AsMemory();
                await using StreamWriter writer = new(context.Response.OutputStream);
                await writer.WriteAsync(asString, cancellationToken);

                HttpStatusCode GetStatusCode() =>
                    e switch
                    {
                        NoHandlerException _ => HttpStatusCode.BadRequest,
                        MissingRequiredParameterException _ => HttpStatusCode.BadRequest,
                        CouldntParseParameterException _ => HttpStatusCode.BadRequest,
                        _ => HttpStatusCode.InternalServerError,
                    };
            }
            finally
            {
                context.Response.Close();
                console.WriteLine($"[{traceId}] Completed request");
            }
        }

        private bool IsRead(HttpContext context, [MaybeNullWhen(returnValue: false)] out Func<Task> callback)
        {
            if (!"GET".Equals(context.Request.HttpMethod, StringComparison.OrdinalIgnoreCase)
                || !"/".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                callback = null;
                return false;
            }

            callback = Callback;
            return true;

            async Task Callback()
            {
                int pin = GetPin();
                PinMode mode = GetPinMode();

                using GpioPin gpio = context.Controller.OpenPin(pin, mode);
                PinValue value = gpio.Read();
                string result =
                    value switch
                    {
                        var x when x == PinValue.High => "High",
                        var x when x == PinValue.Low => "Low",
                        _ => throw new NotImplementedException($"The pin value returned by the GPIO controller was not recognized. Value: {value.ToString()}"),
                    };

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.SendChunked = true;

                ReadOnlyMemory<char> asString = result.AsMemory();
                await using StreamWriter writer = new(context.Response.OutputStream);
                await writer.WriteAsync(asString, context.CancellationToken);
            }

            int GetPin()
            {
                string? rawPin = context.Request.QueryString["pin"];
                if (rawPin is null)
                {
                    throw new MissingRequiredParameterException("pin");
                }

                try
                {
                    return int.Parse(rawPin);
                }
                catch (Exception e)
                {
                    throw new CouldntParseParameterException("pin", e);
                }
            }

            PinMode GetPinMode()
            {
                string? rawMode = context.Request.QueryString["mode"];
                if (rawMode is null)
                {
                    return PinMode.Input;
                }

                try
                {
                    return Enum.Parse<PinMode>(rawMode);
                }
                catch (Exception e)
                {
                    throw new CouldntParseParameterException("mode", e);
                }
            }
        }

        private bool IsWrite(HttpContext context, [MaybeNullWhen(returnValue: false)] out Func<Task> callback)
        {
            if (!"POST".Equals(context.Request.HttpMethod, StringComparison.OrdinalIgnoreCase)
                || !"/".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                callback = null;
                return false;
            }

            callback = Callback;
            return true;

            async Task Callback()
            {
                int pin = GetPin();
                (PinValue value, PinValue inverted) = GetPinValue();
                int? duration = GetDuration();

                using GpioPin gpio = context.Controller.OpenPin(pin, PinMode.Output);
                gpio.Write(value);
                if (duration.HasValue)
                {
                    Thread.Sleep(duration.Value);
                    gpio.Write(inverted);
                }

                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }

            int GetPin()
            {
                string? rawPin = context.Request.QueryString["pin"];
                if (rawPin is null)
                {
                    throw new MissingRequiredParameterException("pin");
                }

                try
                {
                    return int.Parse(rawPin);
                }
                catch (Exception e)
                {
                    throw new CouldntParseParameterException("pin", e);
                }
            }

            (PinValue Value, PinValue Inverted) GetPinValue()
            {
                string? rawMode = context.Request.QueryString["value"];
                if (rawMode is null)
                {
                    throw new MissingRequiredParameterException("value");
                }

                try
                {
                    if ("HIGH".Equals(rawMode, StringComparison.OrdinalIgnoreCase))
                    {
                        return (PinValue.High, PinValue.Low);
                    }
                    else if ("LOW".Equals(rawMode, StringComparison.OrdinalIgnoreCase))
                    {
                        return (PinValue.Low, PinValue.High);
                    }
                    else
                    {
                        throw new FormatException($"The specified value is not a valid {nameof(PinValue)}. Value: {rawMode}");
                    }
                }
                catch (Exception e)
                {
                    throw new CouldntParseParameterException("mode", e);
                }
            }

            int? GetDuration()
            {
                string? rawDuration = context.Request.QueryString["duration"];
                if (rawDuration is null)
                {
                    return null;
                }

                try
                {
                    int duration = int.Parse(rawDuration);
                    if (duration < 0)
                    {
                        throw new ArgumentOutOfRangeException($"The duration must be non-negative. Duration: {duration}");
                    }

                    return duration;
                }
                catch (Exception e)
                {
                    throw new CouldntParseParameterException("duration", e);
                }
            }
        }

        private sealed class NoHandlerException : Exception
        {
            public NoHandlerException()
                : base("The specified request didn't match a request handler.")
            {
            }
        }

        private sealed class MissingRequiredParameterException : Exception
        {
            public MissingRequiredParameterException(string parameter)
                : base($"The request is missing a required parameter: {parameter}")
            {
            }
        }

        private sealed class CouldntParseParameterException : Exception
        {
            public CouldntParseParameterException(string parameter, Exception exception)
                : base($"The request specified a parameter that couldn't be parsed. Parameter: {parameter}, Exception: {exception}")
            {
            }
        }

        private sealed class HttpContext
        {
            public required Guid TraceId { get; init; }

            public required HttpListenerRequest Request { get; init; }

            public required HttpListenerResponse Response { get; init; }

            public required GpioController Controller { get; init; }

            public required CancellationToken CancellationToken { get; init; }
        }
    }
}