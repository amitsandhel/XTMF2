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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using XTMF2.Editing;
using XTMF2.ModelSystemConstruct;

namespace XTMF2
{
    public sealed class Boundary : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        private object WriteLock = new object();
        private ObservableCollection<ModelSystemStructure> _Modules = new ObservableCollection<ModelSystemStructure>();
        private ObservableCollection<Start> _Starts = new ObservableCollection<Start>();
        private ObservableCollection<Boundary> _Boundaries = new ObservableCollection<Boundary>();

        public Boundary(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Called when loading a boundary
        /// </summary>
        internal Boundary()
        {
        }

        internal bool Contains(Boundary boundary)
        {
            foreach (var b in _Boundaries)
            {
                if (b == boundary || b.Contains(boundary))
                {
                    return true;
                }
            }
            return false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyObservableCollection<ModelSystemStructure> Modules
        {
            get
            {
                lock (WriteLock)
                {
                    return new ReadOnlyObservableCollection<ModelSystemStructure>(_Modules);
                }
            }
        }

        private ReadOnlyObservableCollection<Start> Starts
        {
            get
            {
                lock (WriteLock)
                {
                    return new ReadOnlyObservableCollection<Start>(_Starts);
                }
            }
        }

        internal Dictionary<Type, int> GetUsedTypes()
        {
            return GetUsedTypes(new List<Type>()).Select((type, index) => (type: type, index: index))
                .ToDictionary(e => e.type, e => e.index);
        }

        private List<Type> GetUsedTypes(List<Type> included)
        {
            foreach (var module in _Modules)
            {
                var t = module.Type;
                if (t != null)
                {
                    if (!included.Contains(t))
                    {
                        included.Add(t);
                    }
                }
            }
            foreach (var child in _Boundaries)
            {
                child.GetUsedTypes(included);
            }
            return included;
        }

        public ReadOnlyObservableCollection<Boundary> Boundaries
        {
            get
            {
                lock (WriteLock)
                {
                    return new ReadOnlyObservableCollection<Boundary>(_Boundaries);
                }
            }
        }

        internal void Save(ref int index, Dictionary<Type, int> typeDictionary, JsonTextWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Starts");
            writer.WriteStartArray();
            foreach (var start in _Starts)
            {
                start.Save(ref index, typeDictionary, writer);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("Modules");
            writer.WriteStartArray();
            foreach (var module in _Modules)
            {
                module.Save(ref index, typeDictionary, writer);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("Boundaries");
            writer.WriteStartArray();
            foreach (var child in _Boundaries)
            {
                child.Save(ref index, typeDictionary, writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static bool FailWith(ref string error, string message)
        {
            error = message;
            return false;
        }

        internal bool Load(Dictionary<int, Type> typeLookup, JsonTextReader reader, ref string error)
        {
            if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
            {
                return FailWith(ref error, "Unexpected token when reading boundary!");
            }
            while(reader.Read() && reader.TokenType == JsonToken.EndObject)
            {
                if(reader.TokenType != JsonToken.PropertyName || reader.TokenType != JsonToken.Comment)
                {
                    return FailWith(ref error, "Unexpected token when reading boundary!");
                }
                switch(reader.Value)
                {
                    case "Starts":
                        if(!reader.Read() || reader.TokenType != JsonToken.StartArray)
                        {
                            return FailWith(ref error, "Unexpected token when starting to read Starts for a boundary.");
                        }
                        while(reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            if(!Start.Load(reader, out Start start, ref error))
                            {
                                return false;
                            }
                        }
                        break;
                    case "Modules":
                        if (!reader.Read() || reader.TokenType != JsonToken.StartArray)
                        {
                            return FailWith(ref error, "Unexpected token when starting to read Modules for a boundary.");
                        }
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            if (reader.TokenType != JsonToken.Comment)
                            {
                                if (!ModelSystemStructure.Load(reader, out ModelSystemStructure mss, ref error))
                                {
                                    return false;
                                }
                            }
                        }
                        break;
                    case "Boundaries":
                        if (!reader.Read() || reader.TokenType != JsonToken.StartArray)
                        {
                            return FailWith(ref error, "Unexpected token when starting to read Modules for a boundary.");
                        }
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            if (reader.TokenType != JsonToken.Comment)
                            {
                                var boundary = new Boundary();
                                if(!boundary.Load(typeLookup, reader, ref error))
                                {
                                    return false;
                                }
                            }
                        }
                        break;
                    default:
                        return FailWith(ref error, $"Unexpected value when reading boundary {reader.Value}");
                }
            }
            return true;
        }

        public bool SetName(ModelSystemSession session, string name, ref string error)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                error = "A name cannot be whitespace.";
                return false;
            }
            Name = name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            return true;
        }

        public bool SetDescription(ModelSystemSession session, string description, ref string error)
        {
            Description = description;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            return true;
        }

        public Boundary Clone()
        {
            lock (WriteLock)
            {
                var ret = new Boundary(Name)
                {
                    _Modules = new ObservableCollection<ModelSystemStructure>(from mod in _Modules
                                                                              select mod.Clone()),
                    _Boundaries = new ObservableCollection<Boundary>(from bound in _Boundaries
                                                                     select bound.Clone())
                };
                return ret;
            }
        }

        internal bool RemoveStart(Start start, ref string error)
        {
            throw new NotImplementedException();
        }

        internal bool AddStart(string startName, out Start start, ref string error)
        {
            start = null;
            // ensure the name is unique between starting points
            foreach (var ms in _Starts)
            {
                if (ms.Name.Equals(startName, StringComparison.OrdinalIgnoreCase))
                {
                    error = "There already exists a start with the same name!";
                    return false;
                }
            }
            start = new Start(startName, this, null, new Point() { X = 0, Y = 0 });
            _Starts.Add(start);
            return true;
        }
    }
}
