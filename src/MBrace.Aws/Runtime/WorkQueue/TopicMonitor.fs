﻿namespace MBrace.AWS.Runtime

open System
open System.Threading

open MBrace.Core.Internals
open MBrace.Runtime
open MBrace.Runtime.Utils

/// TopicMonitor implements an agent which periodically checks all service bus topic subscriptions
/// for messages assigned to inactive workers. If found, it will push the messages back to the main
/// Queue, to be further processed by a different worker for fault handling.
[<Sealed; AutoSerializable(false)>]
type TopicMonitor private 
        (workerManager : WorkerManager, 
         currentWorker : IWorkerId option,
         topic  : Topic, 
         queue  : Queue, 
         logger : ISystemLogger) =

    // generates a pair of numbers indicating a position of the current worker in the cluster
    // used to organize a roundrobin topic monitoring sequence between workers.
    let workerPosition = 
        let getPos = async { 
            try 
                let! ws = workerManager.GetAvailableWorkers()
                let i = 
                    match currentWorker with
                    | None -> 0
                    | Some cw ->
                        ws 
                        |> Seq.sortBy (fun w -> w.Id)
                        |> Seq.tryFindIndex (fun w -> w.Id = cw)
                        |> fun r -> defaultArg r 0

                return int64 i, int64 ws.Length

            with _ -> return 0L, 2L
        }

        CacheAtom.Create(getPos, intervalMilliseconds = 30000)

    let cleanupWorkerQueue (worker : IWorkerId) = async {
        try
            let subscription = topic.GetSubscription(worker)
            let! allMessages = subscription.DequeueAllMessagesBatch()
            if not <| Array.isEmpty allMessages then
                logger.LogInfof 
                    "TopicMonitor : perfoming worker queue maintance for %A." 
                    worker.Id
                logger.LogInfof 
                    "TopicMonitor : moving %d messages to main queue for %A" 
                    allMessages.Length 
                    worker.Id

                let workItems = allMessages |> Seq.map WorkItemMessage.FromDequeuedSqsMessage
                do! queue.EnqueueMessagesBatch(workItems)
                do! subscription.Delete(allMessages)

        with e ->
            logger.Logf LogLevel.Error "Error cleaning up subscription '%s': %O" worker.Id e
    }

    // WorkItem queue maintenance : periodically check for non-responsive workers and cleanup their queue
    let rec loop (count : int64) = async {
        do! Async.Sleep 5000
        let! i,n = workerPosition.GetValueAsync()
        if count % n <> i then return! loop (count + 1L) else

        logger.LogInfo "TopicMonitor : starting topic maintenance."

        let! result = Async.Catch <| async {
            let! workersToCheck = workerManager.GetInactiveWorkers()
            do! workersToCheck 
                |> Seq.map (fun w -> cleanupWorkerQueue w.Id) 
                |> Async.Parallel 
                |> Async.Ignore
        }

        match result with
        | Choice1Of2 () -> 
            logger.LogInfo "TopicMonitor : maintenance complete."
        | Choice2Of2 ex -> 
            logger.Logf LogLevel.Error "TopicMonitor : maintenance error:  %A" ex

        return! loop (count + 1L)
    }

    let cts = new CancellationTokenSource()
    do Async.Start(loop 0L, cts.Token)

    interface IDisposable with
        member __.Dispose() = cts.Cancel()

    static member Create (workerManager : WorkerManager, workQueue : WorkItemQueue, logger : ISystemLogger, ?currentWorker: IWorkerId) = async {
        return new TopicMonitor(workerManager, currentWorker, workQueue.Topic, workQueue.Queue, logger)
    }