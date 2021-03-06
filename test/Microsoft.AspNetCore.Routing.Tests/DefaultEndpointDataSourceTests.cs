﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultEndpointDataSourceTests
    {
        [Fact]
        public void Constructor_Params_EndpointsInitialized()
        {
            // Arrange & Act
            var dataSource = new DefaultEndpointDataSource(
                new Endpoint(TestConstants.EmptyRequestDelegate, EndpointMetadataCollection.Empty, "1"),
                new Endpoint(TestConstants.EmptyRequestDelegate, EndpointMetadataCollection.Empty, "2")
                );

            // Assert
            Assert.Collection(dataSource.Endpoints,
                endpoint => Assert.Equal("1", endpoint.DisplayName),
                endpoint => Assert.Equal("2", endpoint.DisplayName));
        }

        [Fact]
        public void Constructor_Enumerable_EndpointsInitialized()
        {
            // Arrange & Act
            var dataSource = new DefaultEndpointDataSource(new List<Endpoint>
            {
                new Endpoint(TestConstants.EmptyRequestDelegate, EndpointMetadataCollection.Empty, "1"),
                new Endpoint(TestConstants.EmptyRequestDelegate, EndpointMetadataCollection.Empty, "2")
            });

            // Assert
            Assert.Collection(dataSource.Endpoints,
                endpoint => Assert.Equal("1", endpoint.DisplayName),
                endpoint => Assert.Equal("2", endpoint.DisplayName));
        }
    }
}