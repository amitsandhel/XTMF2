﻿/*
    Copyright 2017 University of Toronto

    This file is part of XTMF2.

    XTMF2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    XTMF2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with XTMF2.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XTMF2.RuntimeModules
{
    public sealed class BasicEvent : BaseEvent
    {
        private List<Action> ToInvoke = new List<Action>();

        public override void Invoke()
        {
            // make a copy in case the invocation causes an additional registration
            List<Action> copy;
            lock(ToInvoke)
            {
                copy = ToInvoke.ToList();
            }
            foreach(var registered in copy)
            {
                registered.Invoke();
            }
        }

        public override void Register(Action module)
        {
            lock(ToInvoke)
            {
                ToInvoke.Add(module);
            }
        }
    }

    public sealed class BasicEvent<Context> : BaseEvent<Context>
    {
        private List<Action<Context>> ToInvoke = new List<Action<Context>>();

        public override void Invoke(Context context)
        {
            // make a copy in case the invocation causes an additional registration
            List<Action<Context>> copy;
            lock (ToInvoke)
            {
                copy = ToInvoke.ToList();
            }
            foreach (var registered in copy)
            {
                registered.Invoke(context);
            }
        }

        public override void Register(Action<Context> module)
        {
            lock (ToInvoke)
            {
                ToInvoke.Add(module);
            }
        }
    }
}
