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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    public sealed class ParserOutput : IParserOutput
    {
        private Stack _stack;
        private JObject _currentObject;
        private JArray _currentArray;

        //
        // The following two statics are only used as an optimization so that we
        // don't create a boxed Boolean each time the True and False properties
        // are evaluated. Instead we keep returning a reference to the same
        // immutable value. This should put much less pressure on the GC.
        //

        private readonly static object _trueObject = true;
        private readonly static object _falseObject = false;

        public object NullPrimitive
        {
            get { return JNull.Value; }
        }

        public object TruePrimitive
        {
            get { return _trueObject; }
        }

        public object FalsePrimitive
        {
            get { return _falseObject; }
        }

        public object ToStringPrimitive(string s)
        {
            return Mask.NullString(s);
        }

        public object ToNumberPrimitive(string s)
        {
            //
            // Try first parsing as a 32-bit integer. If that doesn't work
            // then just assume it is a double.
            //

            try 
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            } 
            catch (FormatException) {}
            catch (OverflowException) {}

            try 
            {
                return Convert.ToDouble(s, CultureInfo.InvariantCulture);
            } 
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }
        }

        public object ToBoolean(bool value)
        {
            return value;
        }

        public void StartObject()
        {
            OnStart();
            _currentObject = new JObject();
        }

        public void ObjectPut(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
                
            if (name.Length == 0)
                throw new ArgumentOutOfRangeException("name");

            if (_currentObject == null)
                throw new InvalidOperationException("BuildObject must be called before ObjectPut.");

            _currentObject[name] = value;
        }

        public object EndObject()
        {
            if (_currentObject == null)
                throw new InvalidOperationException("BuildObject must be called before EndObject.");

            return OnEnd(_currentObject);
        }

        public void StartArray()
        {
            OnStart();
            _currentArray = new JArray();
        }

        public void ArrayPut(object value)
        {
            if (_currentArray == null)
                throw new InvalidOperationException("BuildArray must be called before ArrayPut.");

            _currentArray.Add(value);
        }

        public object EndArray()
        {
            if (_currentArray == null)
                throw new InvalidOperationException("BuildArray must be called before EndArray.");

            return OnEnd(_currentArray);
        }

        private void OnStart()
        {
            //
            // Remember the current array or object, whichever happens to be
            // the one this instance is building right now, by pushing it on to
            // the stack.
            //

            if (_currentObject != null)            
            {
                Stack.Push(_currentObject);
                _currentObject = null;
            }
            else if (_currentArray != null)
            {
                Stack.Push(_currentArray);
                _currentArray = null;
            }

            //
            // Sanity check.
            //
 
            Debug.Assert(_currentArray == null);
            Debug.Assert(_currentObject == null);
        }

        private object OnEnd(object current)
        {
            Debug.Assert(current != null);

            _currentArray = null;
            _currentObject = null;

            if (Stack.Count != 0)
            {
                object o = Stack.Pop();

                //
                // Pop last object as the current object or array and
                // assert that we've poppoed one of the two as a sanity check.
                //

                _currentObject = o as JObject;

                if (_currentObject == null)
                    _currentArray = o as JArray;

                Debug.Assert(_currentObject != null || _currentArray != null);
            }

            return current;
        }

        private Stack Stack
        {
            get
            {
                if (_stack == null)
                    _stack = new Stack();

                return _stack;
            }
        }

        //
        // Test-only members
        //

        #if TEST

        internal object TestCurrentObject
        {
            get { return _currentObject; }
        }

        internal object TestCurrentArray
        {
            get { return _currentArray; }
        }

        internal Stack TestStack
        {
            get { return Stack; }
        }

        #endif
    }
}