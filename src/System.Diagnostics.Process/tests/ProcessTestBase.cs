﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;

namespace System.Diagnostics.Tests
{
    public class ProcessTestBase : RemoteExecutorTestBase
    {
        protected const int WaitInMS = 100 * 1000;
        protected readonly Process _process;
        protected readonly List<Process> _processes = new List<Process>();

        public ProcessTestBase()
        {
            _process = CreateProcessLong();
            _process.Start();
        }

        protected override void Dispose(bool disposing)
        {
            // Wait for all started processes to complete
            foreach (Process p in _processes)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        Assert.True(p.WaitForExit(WaitInMS));
                    }
                }
                catch (InvalidOperationException) { } // in case it was never started
            }

            base.Dispose(disposing);
        }

        protected Process CreateProcess(Func<int> method = null)
        {
            Process p = RemoteInvoke(method ?? (() => SuccessExitCode), start: false).Process;
            lock (_processes)
            {
                _processes.Add(p);
            }
            return p;
        }

        protected Process CreateProcess(Func<string, int> method, string arg)
        {
            Process p = RemoteInvoke(method, arg, start: false).Process;
            lock (_processes)
            {
                _processes.Add(p);
            }
            return p;
        }

        protected Process CreateProcessLong()
        {
            return CreateProcess(() =>
            {
                Thread.Sleep(WaitInMS);
                return SuccessExitCode;
            });
        }

        protected Process CreateProcessInfinite()
        {
            return CreateProcess(() =>
            {
                while (true)
                    Thread.Sleep(WaitInMS);
            });
        }

        protected void StartSleepKillWait(Process p)
        {
            p.Start();
            Thread.Sleep(50);
            p.Kill();
            Assert.True(p.WaitForExit(WaitInMS));
        }

    }
}
