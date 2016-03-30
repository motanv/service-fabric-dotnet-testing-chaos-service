// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Fabric;
    using System.Fabric.Testability.Scenario;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using ChaosTest.Common;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// This stateful service keeps inducing failovers and faults - chosen probabilistically - to the cluster,
    /// using ChaosTestScenario from Microsoft.ServiceFabric.Testability under the hood, hiding the fact
    /// that ChaosTestScenario runs in unit of iteration, giving the impression that the fault inducing is perennial.
    /// </summary>
    public class ChaosService : StatefulService
    {
        private CancellationTokenSource stopEventTokenSource;

        public ChaosService(StatefulServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Starts the test.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            IReliableDictionary<string, CurrentState> chaosServiceState =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>(StringResource.ChaosServiceStateKey);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<CurrentState> currentStateResult =
                    await chaosServiceState.TryGetValueAsync(tx, StringResource.ChaosServiceStateKey, LockMode.Update);

                if (currentStateResult.HasValue &&
                    (currentStateResult.Value == CurrentState.Stopped ||
                     currentStateResult.Value == CurrentState.None))
                {
                    await
                        chaosServiceState.AddOrUpdateAsync(
                            tx,
                            StringResource.ChaosServiceStateKey,
                            CurrentState.Running,
                            (key, existingValue) => CurrentState.Running);
                }

                await tx.CommitAsync();
            }
        }

        public async Task StopAsync()
        {
            IReliableDictionary<string, CurrentState> chaosServiceState =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>(StringResource.ChaosServiceStateKey);

            bool shouldCancel = false;

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<CurrentState> currentStateResult =
                    await chaosServiceState.TryGetValueAsync(tx, StringResource.ChaosServiceStateKey, LockMode.Update);

                if (currentStateResult.HasValue &&
                    currentStateResult.Value == CurrentState.Running)
                {
                    await
                        chaosServiceState.AddOrUpdateAsync(
                            tx,
                            StringResource.ChaosServiceStateKey,
                            CurrentState.Stopped,
                            (key, existingValue) => CurrentState.Stopped);
                    shouldCancel = true;
                }

                await tx.CommitAsync();
            }

            if (shouldCancel && this.stopEventTokenSource != null && !this.stopEventTokenSource.IsCancellationRequested)
            {
                this.stopEventTokenSource.Cancel();
            }
        }

        public async Task<Result> GetEventsAsync()
        {
            Result results = new Result
            {
                ChaosLog = new SortedList<long, ChaosEntry>(),
                CurrentState = StringResource.NotAvailable,
                TotalRuntime = StringResource.NotAvailable
            };

            IReliableDictionary<long, ChaosEntry> savedEvents =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<long, ChaosEntry>>(StringResource.SavedEventsKey);

            IReliableDictionary<string, DateTime> startTime =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, DateTime>>(StringResource.StartTimeKey);

            IReliableDictionary<string, CurrentState> currentState =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>(StringResource.ChaosServiceStateKey);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                IAsyncEnumerable<KeyValuePair<long, ChaosEntry>> enumerable = await savedEvents.CreateEnumerableAsync(tx);

                await enumerable.ForeachAsync(CancellationToken.None, item => { results.ChaosLog.Add(item.Key, item.Value); });

                ConditionalValue<DateTime> result = await startTime.TryGetValueAsync(tx, StringResource.StartTimeKey);

                if (result.HasValue)
                {
                    results.TotalRuntime = (DateTime.UtcNow - result.Value).ToString();
                }

                ConditionalValue<CurrentState> currentStateResult = await currentState.TryGetValueAsync(tx, StringResource.ChaosServiceStateKey);

                if (currentStateResult.HasValue)
                {
                    results.CurrentState = currentStateResult.Value.ToString();
                }

                await tx.CommitAsync();
            }

            return results;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(
                    context =>
                        new OwinCommunicationListener("", new Startup(this), context, ServiceEventSource.Current))
            };
        }

        protected override async Task RunAsync(CancellationToken runAsyncCancellationToken)
        {
            // This is to keep track of exceptions in the validation step at the end of
            // each iteration of the ChaosTestScenario that is being used under the cover 
            //
            bool validationExceptionCaught = false;

            IReliableDictionary<string, CurrentState> chaosServiceState =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>(StringResource.ChaosServiceStateKey);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                if (!await chaosServiceState.ContainsKeyAsync(tx, StringResource.ChaosServiceStateKey, LockMode.Update))
                {
                    await chaosServiceState.AddAsync(tx, StringResource.ChaosServiceStateKey, CurrentState.Stopped);
                }
                await tx.CommitAsync();
            }


            while (!runAsyncCancellationToken.IsCancellationRequested)
            {
                try
                {
                    // check to see if we're in a "stop" or "start" state.
                    // this continues to poll until we're in a "start" state.
                    // a ReliableDictionary is used to store this information so that if the service
                    //   fails over to another node, the state is preserved and the chaos test will continue to execute.
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        ConditionalValue<CurrentState> currentStateResult =
                            await chaosServiceState.TryGetValueAsync(tx, StringResource.ChaosServiceStateKey);

                        if (currentStateResult.HasValue &&
                            (currentStateResult.Value == CurrentState.Stopped ||
                             currentStateResult.Value == CurrentState.None))
                        {
                            await Task.Delay(Constants.IntervalBetweenLoopIteration, runAsyncCancellationToken);
                            continue;
                        }
                    }

                    // this section runs the actual chaos test.
                    // the cancellation token source is linked to the token provided to RunAsync so that we
                    //   can stop the test if the service needs to shut down.
                    using (FabricClient fabricClient = new FabricClient())
                    {
                        using (this.stopEventTokenSource = CancellationTokenSource.CreateLinkedTokenSource(runAsyncCancellationToken))
                        {
                            // when a validation exception is caught, this waits for a while to let the cluster stabilize before continuing.
                            if (validationExceptionCaught)
                            {
                                await Task.Delay(ChaosTestConfigSettings.MaxClusterStabilizationTimeout, this.stopEventTokenSource.Token);
                                validationExceptionCaught = false;
                            }

                            ChaosTestScenarioParameters chaosScenarioParameters =
                                new ChaosTestScenarioParameters(
                                    ChaosTestConfigSettings.MaxClusterStabilizationTimeout,
                                    ChaosTestConfigSettings.MaxConcurrentFaults,
                                    ChaosTestConfigSettings.EnableMoveReplicaFaults,
                                    TimeSpan.MaxValue)
                                {
                                    WaitTimeBetweenFaults =
                                        ChaosTestConfigSettings.WaitTimeBetweenFaults,
                                    WaitTimeBetweenIterations =
                                        ChaosTestConfigSettings.WaitTimeBetweenIterations
                                };

                            ChaosTestScenario chaosTestScenario = new ChaosTestScenario(fabricClient, chaosScenarioParameters);

                            // capture progress events so we can report them back
                            chaosTestScenario.ProgressChanged += this.TestScenarioProgressChanged;

                            // this continuously runs the chaos test until the CancellationToken is signaled.
                            await chaosTestScenario.ExecuteAsync(this.stopEventTokenSource.Token);
                        }
                    }
                }
                catch (TimeoutException e)
                {
                    string message = $"Caught TimeoutException '{e.Message}'. Will wait for cluster to stabilize before continuing test";
                    ServiceEventSource.Current.ServiceMessage(this, message);
                    validationExceptionCaught = true;
                    await this.StoreEventAsync(message);
                }
                catch (FabricValidationException e)
                {
                    string message = $"Caught FabricValidationException '{e.Message}'. Will wait for cluster to stabilize before continuing test";
                    ServiceEventSource.Current.ServiceMessage(this, message);
                    validationExceptionCaught = true;
                    await this.StoreEventAsync(message);
                }
                catch (OperationCanceledException)
                {
                    if (runAsyncCancellationToken.IsCancellationRequested)
                    {
                        // if RunAsync is canceled then we need to quit.
                        throw;
                    }

                    ServiceEventSource.Current.ServiceMessage(
                        this,
                        "Caught OperationCanceledException Exception during test execution. This is expected if test was stopped");
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is OperationCanceledException)
                    {
                        if (runAsyncCancellationToken.IsCancellationRequested)
                        {
                            // if RunAsync is canceled then we need to quit.
                            throw;
                        }

                        ServiceEventSource.Current.ServiceMessage(
                            this,
                            "Caught OperationCanceledException Exception during test execution. This is expected if test was stopped");
                    }
                    else
                    {
                        string message = $"Caught unexpected Exception during test excecution {e.InnerException}";
                        ServiceEventSource.Current.ServiceMessage(this, message);
                        await this.StoreEventAsync(message);
                    }
                }
                catch (Exception e)
                {
                    string message = $"Caught unexpected Exception during test excecution {e}";
                    ServiceEventSource.Current.ServiceMessage(this, message);
                    await this.StoreEventAsync(message);
                }
            }
        }

        /// <summary>
        /// This is the event-handler for the events coming into from ChaosTestScenario
        /// </summary>
        /// <param name="sender">ChaosTestScenario</param>
        /// <param name="e">Event</param>
        private void TestScenarioProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string eventString = e.UserState.ToString();

            // Chaos Test Scenario runs in units of iterations; but from this sample's 
            // Chaos service point of view, iteration does not make much sense; hence, 
            // getting rid of pre- and post-, ambles for iterations.
            if (eventString.StartsWith("Running iteration"))
            {
                return;
            }

            if (eventString.StartsWith("Scenario complete"))
            {
                return;
            }

            this.StoreEventAsync(eventString).GetAwaiter().GetResult();
        }

        /// <summary>
        /// This method saves the description (eventString) and timestamp of the recentmost induced fault as
        /// a ChaosEntry in a Reliable Dictionary
        /// </summary>
        /// <param name="eventString"></param>
        /// <returns>A task to await on</returns>
        private async Task StoreEventAsync(string eventString)
        {
            ServiceEventSource.Current.ServiceMessage(this, "ChaosTest: {0}", eventString);

            IReliableDictionary<string, long> eventCount =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>(StringResource.EventCountKey);

            IReliableDictionary<string, DateTime> startTime =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, DateTime>>(StringResource.StartTimeKey);

            IReliableDictionary<long, ChaosEntry> savedEvents =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<long, ChaosEntry>>(StringResource.SavedEventsKey);


            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                if (!await startTime.ContainsKeyAsync(tx, StringResource.StartTimeKey))
                {
                    await startTime.AddAsync(tx, StringResource.StartTimeKey, DateTime.UtcNow);
                }

                if (!await eventCount.ContainsKeyAsync(tx, StringResource.EventCountKey))
                {
                    await eventCount.AddAsync(tx, StringResource.EventCountKey, 0);
                }

                ConditionalValue<long> result =
                    await eventCount.TryGetValueAsync(tx, StringResource.EventCountKey, LockMode.Update);

                if (result.HasValue)
                {
                    long currentCount = result.Value;

                    // If we have HistoryLength number of events, we make room for new events by removing oldest ones,
                    // always keeping HistoryLength number of recentmost events on the show on the webpage.
                    if (currentCount > Constants.HistoryLength - 1)
                    {
                        await savedEvents.TryRemoveAsync(tx, currentCount - Constants.HistoryLength + 1);
                    }

                    ChaosEntry chaosEntry = new ChaosEntry
                    {
                        Record = eventString,
                        TimeStamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                    };

                    await savedEvents.AddAsync(tx, ++currentCount, chaosEntry);
                    await eventCount.SetAsync(tx, StringResource.EventCountKey, currentCount);

                    await tx.CommitAsync();
                }
            }
        }
    }
}