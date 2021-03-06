﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Sys.Metrics.ETW.TestProcess
{
    public class TestProcessHandle : IDisposable
    {
        public readonly Process Process;
        private readonly ProcessKillJob killJob;
        private bool disposed;
        
        public TestProcessHandle(bool gcOnStart=true)
        {
            var pathToAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var file = new FileInfo(Directory.GetFiles(pathToAssembly, "Vostok.Sys.Metrics.ETW.TestProcess.???").First());
            string executable, args = string.Empty;
            switch (file.Extension)
            {
                case ".exe":
                    executable = file.FullName;
                    break;
                case ".dll":
                    executable = "dotnet";
                    args = file.Name;
                    break;
                default:
                    throw new Exception("Unknown TestProcess file extension");
            }

            Process = Process.Start(
                new ProcessStartInfo(executable, args)
                {
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                });

            killJob = new ProcessKillJob();
            killJob.AddProcess(Process);            
            Warmup();
            if (gcOnStart)
                MakeGC(0);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            
            Process.StandardInput.WriteLine("exit");
            if (!Process.WaitForExit(5000))
            {
                Process.Kill();
            }
            Process.Dispose();
            killJob.Dispose();

            disposed = true;
        }
        
        public void EatCpu(int cores)
        {
            var command = $"eat_cpu {cores}";
            Process.StandardInput.WriteLine(command);
            WaitForCompletion(command);
        }

        public void EatMemory(long sizeBytes)
        {
            var command = $"eat_mem {sizeBytes}";
            Process.StandardInput.WriteLine(command);
            WaitForCompletion(command);
        }

        public void EatPrivateMemory(long sizeBytes)
        {
            var command = $"eat_private_mem {sizeBytes}";
            Process.StandardInput.WriteLine(command);
            WaitForCompletion(command);
        }

        public void MakeGC(int depth)
        {
            var command = $"gc {depth}";
            Process.StandardInput.WriteLine(command);
            WaitForCompletion(command);
        }

        public void MakeThreads(int count)
        {
            var command = $"threads {count}";
            Process.StandardInput.WriteLine(command);
            WaitForCompletion(command);
        }

        private void Warmup()
        {
            var command = "warmup";
            Process.StandardInput.WriteLine(command);
            WaitForCompletion(command);
        }

        private void WaitForCompletion(string command)
        {
            do
            {
                var readTask = Process.StandardOutput.ReadLineAsync();
                var timeout = Task.Delay(5000);
                var completed = Task.WhenAny(readTask, timeout);
                if (completed.Result == readTask)
                {
                    Console.WriteLine(readTask.Result);
                    if (readTask.Result == command)
                    {
                        return;
                    }
                }
                else
                {
                    throw new TimeoutException($"Completion of command '{command}' timed out");
                }
            } while (true);
        }
    }
}