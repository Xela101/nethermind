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
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethermind.BeaconNode.OApiClient;
using Nethermind.HonestValidator.Configuration;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Nethermind.HonestValidator.Services
{
    public class BeaconNodeOApiClientFactory
    {
        private readonly ILogger<BeaconNodeOApiClientFactory> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public BeaconNodeOApiClientFactory(ILogger<BeaconNodeOApiClientFactory> logger, 
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        
        public BeaconNodeOApiClient CreateClient(string baseUrl)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();

            BeaconNodeOApiClient beaconNodeOApiClient = new BeaconNodeOApiClient(baseUrl, httpClient);
            
            return beaconNodeOApiClient;
        }
    }
}