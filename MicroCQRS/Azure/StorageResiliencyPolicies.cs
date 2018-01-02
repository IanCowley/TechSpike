using System;
using System.Net;
using System.Net.Http;
using log4net;
using Microsoft.WindowsAzure.Storage;
using Polly;

namespace MicroCQRS.Azure
{
    public class StorageResiliencyPolicies
    {
        public static Policy GetAsyncAndWaitPolicy(ILog logger)
        {
            return Policy.WrapAsync(
                Policy
                    .Handle<StorageException>()
                    .Or<HttpRequestException>()
                    .Or<WebException>()
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            logger.Warn($"Concurrency error executing command {exception}, retry count {retryCount}");
                        }),
                Policy
                    .Handle<StorageException>()
                    .Or<HttpRequestException>()
                    .Or<WebException>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: 5,
                        // time circuit opened before retry
                        durationOfBreak: TimeSpan.FromSeconds(10),
                        onBreak: (exception, duration) =>
                        {
                            logger.Info("Storage Provider circuit breaker opened");
                        },
                        onReset: () =>
                        {
                            logger.Info("Storage Provider circuit breaker closed");
                        }));
        }
    }
}
