using System.Collections.Concurrent;
using System.Net;
using CrushBot.Core.Helpers;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.StateMachine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CrushBot.Application.Services;

public class PollingService(
    IServiceProvider provider,
    ILogger<PollingService> logger)
    : BackgroundService
{
    private static readonly ConcurrentDictionary<long, Task> MessageTasks = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting polling service.");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.ChatMember],
            DropPendingUpdates = true
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = provider.CreateAsyncScope();
                var client = scope.ServiceProvider.GetRequiredService<ITelegramClient>();
                var updateReceiver = new QueuedUpdateReceiver(client.GetBaseClient(), receiverOptions);

                var bot = await client.GetMeAsync(cancellationToken);
                logger.LogInformation($"Start listening for @{bot!.Username}");

                await foreach (var update in updateReceiver.WithCancellation(cancellationToken))
                {
                    HandleUpdate(update, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Polling failed with error: {ex.Message}");
            }
        }
    }

    private void HandleUpdate(Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
        {
            var userId = update.Message!.From!.Id;

            if (!MessageTasks.TryAdd(userId, Task.CompletedTask))
            {
                return;
            }

            try
            {
                MessageTasks[userId] = RunHandleTask(async scope => 
                {
                    var stateMachine = scope.ServiceProvider.GetRequiredService<IStateMachineService>();
                    await stateMachine.ProcessMessageAsync(update.Message, cancellationToken);
                },
                cancellationToken);
            }
            finally
            {
                MessageTasks[userId]
                    .ContinueWith(x => MessageTasks.TryRemove(userId, out _), cancellationToken);
            }
        }

        if (update.Type == UpdateType.ChatMember)
        {
            RunHandleTask(async scope =>
            {
                var subService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                await subService.ProcessMemberAsync(update.ChatMember!);
            },
            cancellationToken);
        }
    }

    private Task RunHandleTask(Func<IServiceScope, Task> function, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            try
            {
                await using var scope = provider.CreateAsyncScope();
                await function(scope);
            }
            catch (Exception ex)
            {
                HandleUpdateError(ex);
            }
        },
        cancellationToken);
    }

    private void HandleUpdateError(Exception exception)
    {
        if (!HandleException(exception))
        {
            logger.LogError(exception, $"Unhandled exception. Error: {exception.Message}");
        }
    }

    private bool HandleException(Exception exception)
    {
        const HttpStatusCode code = HttpStatusCode.TooManyRequests;

        if (ExceptionHelper.IsApiException(exception, out var apiEx, code))
        {
            LogTooManyRequests(apiEx);
            return true;
        }

        if (ExceptionHelper.IsRequestException(exception, out var requestEx, code))
        {
            LogTooManyRequests(requestEx);
            return true;
        }

        return false;
    }

    private void LogTooManyRequests(Exception exception)
    {
        logger.LogWarning($"Too many requests. Error: {exception.Message}");
    }
}