﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel;

    /// <summary>
    /// The service bus-related part of the API
    /// </summary>
    public partial interface IServiceManagement
    {
        /// <summary>
        /// Gets a service bus namespace.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [GetServiceBusNamespaceBehavior]
        [WebGet(UriTemplate = @"{subscriptionId}/services/servicebus/namespaces/{name}")]
        IAsyncResult BeginGetServiceBusNamespace(string subscriptionId, string name, AsyncCallback callback, object state);

        ServiceBusNamespace EndGetServiceBusNamespace(IAsyncResult asyncResult);

        /// <summary>
        /// Gets service bus namespaces associated with a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [ListServiceBusNamespacesBehavior]
        [WebGet(UriTemplate = @"{subscriptionId}/services/servicebus/namespaces")]
        IAsyncResult BeginListServiceBusNamespaces(string subscriptionId, AsyncCallback callback, object state);

        ServiceBusNamespaceList EndListServiceBusNamespaces(IAsyncResult asyncResult);

        /// <summary>
        /// Gets service bus namespaces associated with a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [ListServiceBusRegionsBehavior]
        [WebGet(UriTemplate = @"{subscriptionId}/services/servicebus/regions")]
        IAsyncResult BeginListServiceBusRegions(string subscriptionId, AsyncCallback callback, object state);

        ServiceBusRegionList EndListServiceBusRegions(IAsyncResult asyncResult);

        /// <summary>
        /// Creates a new service bus namespace.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [CreateServiceBusNamespaceBehavior]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/servicebus/namespaces/{name}")]
        IAsyncResult BeginCreateServiceBusNamespace(string subscriptionId, ServiceBusNamespace namespaceDescription, string name, AsyncCallback callback, object state);

        ServiceBusNamespace EndCreateServiceBusNamespace(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static ServiceBusNamespace GetServiceBusNamespace(this IServiceManagement proxy, string subscriptionId, string name)
        {
            return proxy.EndGetServiceBusNamespace(proxy.BeginGetServiceBusNamespace(subscriptionId, name, null, null));
        }

        public static ServiceBusNamespaceList ListServiceBusNamespaces(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListServiceBusNamespaces(proxy.BeginListServiceBusNamespaces(subscriptionId, null, null));
        }

        public static ServiceBusRegionList ListServiceBusRegions(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListServiceBusRegions(proxy.BeginListServiceBusRegions(subscriptionId, null, null));
        }

        public static ServiceBusNamespace CreateServiceBusNamespace(this IServiceManagement proxy, string subscriptionId, ServiceBusNamespace namespaceDescription, string name)
        {
            return proxy.EndCreateServiceBusNamespace(proxy.BeginCreateServiceBusNamespace(subscriptionId, namespaceDescription, name, null, null));
        }
    }
}