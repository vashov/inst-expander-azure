using InstExpander.BusinessLogic.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Net;

namespace InstExpanderFunctions.FollowersWorker.Middlewares
{
    /// <summary>
    /// This middleware catches any exceptions during function invocations and
    /// returns a response with 500 status code for http invocations.
    /// </summary>
    /// Source: https://github.com/Azure/azure-functions-dotnet-worker/blob/main/samples/CustomMiddleware/ExceptionHandlingMiddleware.cs
    internal sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            // All exceptions caught in middleware are AggregateException
            //https://github.com/Azure/azure-functions-dotnet-worker/issues/993
            catch (AggregateException ex)
            {
                var specEx = GetSpecificException(ex);
                if (specEx is ChallengeRequiredException challengeEx)
                {
                    // Fail function execution to retry execution later.
                    throw challengeEx; // Clear stack trace of exception.
                }
                else if (specEx is WaitBeforeTryAgainException waitEx)
                {
                    // Fail function execution to retry execution later.
                    throw waitEx; // Clear stack trace of exception.
                }
                else if (specEx is CancellationException)
                {
                    logger.LogWarning("Operation was cancelled: {reason}", ex.Message);
                    return;
                }
                else if (specEx is ConfigurationErrorsException configEx)
                {
                    logger.LogError(configEx.Message);
                    if (!await WriteErrorResponse(context))
                        throw configEx; // Clear stack trace of exception.
                }
                else
                {
                    logger.LogError(ex, "Error processing invocation");
                    if (!await WriteErrorResponse(context))
                        throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing invocation");
                if (!await WriteErrorResponse(context))
                    throw;
            }
        }

        private static Exception GetSpecificException(AggregateException exception)
        {
            var e = exception.InnerExceptions.FirstOrDefault();
            return e ?? exception;
        }

        private async Task<bool> WriteErrorResponse(FunctionContext context)
        {
            var httpReqData = await context.GetHttpRequestDataAsync();
            if (httpReqData != null)
            {
                // Create an instance of HttpResponseData with 500 status code.
                var newHttpResponse = httpReqData.CreateResponse(HttpStatusCode.InternalServerError);
                // You need to explicitly pass the status code in WriteAsJsonAsync method.
                // https://github.com/Azure/azure-functions-dotnet-worker/issues/776
                await newHttpResponse.WriteAsJsonAsync(new { FooStatus = "Invocation failed!" }, newHttpResponse.StatusCode);

                var invocationResult = context.GetInvocationResult();

                var httpOutputBindingFromMultipleOutputBindings = GetHttpOutputBindingFromMultipleOutputBinding(context);
                if (httpOutputBindingFromMultipleOutputBindings is not null)
                {
                    httpOutputBindingFromMultipleOutputBindings.Value = newHttpResponse;
                }
                else
                {
                    invocationResult.Value = newHttpResponse;
                }
                return true;
            }
            return false;
        }

        private OutputBindingData<HttpResponseData> GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
        {
            // The output binding entry name will be "$return" only when the function return type is HttpResponseData
            var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
                .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

            return httpOutputBinding;
        }
    }
}
