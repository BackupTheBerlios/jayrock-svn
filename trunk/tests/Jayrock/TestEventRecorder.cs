#region License, Terms and Conditions
//
// Jayrock - A JSON-RPC implementation for the Microsoft .NET Framework
// Written by Atif Aziz (atif.aziz@skybow.com)
// Copyright (c) Atif Aziz. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 2.1 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA 
//
#endregion

namespace Jayrock
{
    #region Imports

    using System;
    using System.Collections;
    using Jayrock.Json;
    using NUnit.Framework;

    #endregion

    internal sealed class TestEventRecorder
    {
        private object _sender;
        private EventArgs _args;
        private IList _occurrenceList;
        
        public TestEventRecorder() {}

        public TestEventRecorder(IList orderList)
        {
            _occurrenceList = orderList;
        }

        public void ValueChanging(object sender, ValueChangingEventArgs args)
        {
            RecordEvent(sender, args);
        }
        
        private void RecordEvent(object sender, EventArgs args)
        {
            Assert.IsNull(_sender, "Recorder used twice!");
            
            _sender = sender;
            _args = args;
            
            if (_occurrenceList != null)
                _occurrenceList.Add(this);
        }

        public void AssertUsed()
        {
            Assert.IsNotNull(_sender, "Event expected.");
        }
        
        public EventArgs AssertRecording(object expectedSender)
        {
            AssertUsed();
            Assert.AreSame(expectedSender, _sender, "Event sender.");
            return _args;
        }

        public void AssertWasted()
        {
            Assert.IsNull(_sender);
        }
    }
}