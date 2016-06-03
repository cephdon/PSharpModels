﻿using System;
using System.Collections.Generic;
using Microsoft.PSharp;

namespace Raft
{
    public class SafetyMonitor : Monitor
    {
        public class NotifyLeaderElected : Event
        {
            public int Term;

            public NotifyLeaderElected(int term)
                : base()
            {
                this.Term = term;
            }
        }

        private class LocalEvent : Event { }

        private int CurrentTerm;
        private HashSet<int> TermsWithLeader;

        [Start]
        [OnEntry(nameof(InitOnEntry))]
        [OnEventGotoState(typeof(LocalEvent), typeof(Monitoring))]
        class Init : MonitorState { }

        void InitOnEntry()
        {
            this.CurrentTerm = -1;
            this.TermsWithLeader = new HashSet<int>();
            this.Raise(new LocalEvent());
        }

        [OnEventDoAction(typeof(NotifyLeaderElected), nameof(ProcessLeaderElected))]
        class Monitoring : MonitorState { }

        void ProcessLeaderElected()
        {
            var term = (this.ReceivedEvent as NotifyLeaderElected).Term;

            this.Assert(!this.TermsWithLeader.Contains(term), "Detected more than one leader in term " + term);
            this.TermsWithLeader.Add(term);
        }
    }
}