﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

namespace Nevermind.JsonRpc.DataModel
{
    public class Log : IJsonRpcResult
    {
        public bool Removed { get; set; }
        public Quantity LogIndex { get; set; }
        public Quantity TransactionIndex { get; set; }
        public Data TransactionHash { get; set; }
        public Data BlockHash { get; set; }
        public Quantity BlockNumber { get; set; }
        public Data Address { get; set; }
        public Data Data { get; set; }
        public IEnumerable<Data> Topics { get; set; }

        public object ToJson()
        {
            throw new NotImplementedException();
        }
    }
}