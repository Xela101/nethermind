﻿//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System;
using Microsoft.Extensions.Logging;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Types;

namespace Nethermind.BeaconNode.Eth1Bridge
{
    internal static class LogDebug
    { 
        // 6bxx debug

        // 635x debug - eth1 (Eth1Bridge)
        public static readonly Action<ILogger, Exception?> PeeringWorkerExecute =
            LoggerMessage.Define(LogLevel.Debug,
                new EventId(6350, nameof(PeeringWorkerExecute)),
                "Eth1 bridge worker execute running.");
        
        public static readonly Action<ILogger, Exception?> PeeringWorkerStopping =
            LoggerMessage.Define(LogLevel.Debug,
                new EventId(6352, nameof(PeeringWorkerStopping)),
                "Eth1 bridge worker stopping.");

        // 7bxx - mock

        public static readonly Action<ILogger, ulong, Exception?> QuickStartStoreCreated =
            LoggerMessage.Define<ulong>(LogLevel.Debug,
                new EventId(7300, nameof(QuickStartStoreCreated)),
                "Quick start genesis store created with genesis time {GenesisTime:n0}.");
        public static readonly Action<ILogger, ValidatorIndex, string, Exception?> QuickStartAddValidator =
            LoggerMessage.Define<ValidatorIndex, string>(LogLevel.Debug,
                new EventId(7301, nameof(QuickStartAddValidator)),
                "Quick start adding deposit for mocked validator {ValidatorIndex} with public key {PublicKey}.");

    }
}