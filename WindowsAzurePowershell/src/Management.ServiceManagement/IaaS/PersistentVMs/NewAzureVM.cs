﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.ServiceModel;
    using Utilities.Common;
    using Model;
    using Storage;
    using Helpers;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.New, "AzureVM", DefaultParameterSetName = "ExistingService"), OutputType(typeof(ManagementOperationContext))]
    public class NewAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        private bool createdDeployment = false;

        public NewAzureVMCommand()
        {
        }

        public NewAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Mandatory = true, ParameterSetName = "CreateService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name")]
        [Parameter(Mandatory = true, ParameterSetName = "ExistingService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "Required if AffinityGroup is not specified. The data center region where the cloud service will be created.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "Required if Location is not specified. The name of an existing affinity group associated with this subscription.")]
        [ValidateNotNullOrEmpty]
        public string AffinityGroup
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "The label may be up to 100 characters in length. Defaults to Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceLabel
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, ParameterSetName = "CreateService", HelpMessage = "A description for the cloud service. The description may be up to 1024 characters in length.")]
        [ValidateNotNullOrEmpty]
        public string ServiceDescription
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Label. Will default to service name if not specified.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Label. Will default to service name if not specified.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentLabel
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Name. Will default to service name if not specified.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Name. Will default to service name if not specified.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", HelpMessage = "Virtual network name.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", HelpMessage = "Virtual network name.")]
        public string VNetName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "CreateService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "DNS Settings for Deployment.")]
        [Parameter(Mandatory = false, ParameterSetName = "ExistingService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "DNS Settings for Deployment.")]
        [ValidateNotNullOrEmpty]
        public DnsServer[] DnsSettings
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "CreateService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List of VMs to Deploy.")]
        [Parameter(Mandatory = true, ParameterSetName = "ExistingService", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "List of VMs to Deploy.")]
        [ValidateNotNullOrEmpty]
        public PersistentVM[] VMs
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Waits for VM to boot")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter WaitForBoot
        {
            get;
            set;
        }

        public void NewAzureVMProcess()
        {
            SubscriptionData currentSubscription = this.GetCurrentSubscription();

            CloudStorageAccount currentStorage = null;
            try
            {
                currentStorage = CloudStorageAccountFactory.GetCurrentCloudStorageAccount(Channel, currentSubscription);
            }
            catch (ServiceManagementClientException) // couldn't access
            {
                throw new ArgumentException("CurrentStorageAccount is not accessible. Ensure the current storage account is accessible and in the same location or affinity group as your cloud service.");
            }
            if (currentStorage == null) // not set
            {
                throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storage account to set it.");
            }


            Operation lastOperation = null;

            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                try
                {
                    if (this.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var chsi = new CreateHostedServiceInput
                        {
                            AffinityGroup = this.AffinityGroup,
                            Location = this.Location,
                            ServiceName = this.ServiceName,
                            Description = this.ServiceDescription ??
                                            String.Format("Implicitly created hosted service{0}",DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm")),
                            Label = this.ServiceLabel ?? this.ServiceName
                        };

                        ExecuteClientAction(chsi, CommandRuntime + " - Create Cloud Service", s => this.Channel.CreateHostedService(s, chsi));
                    }
                }
                catch (ServiceManagementClientException ex)
                {
                    this.WriteErrorDetails(ex);
                    return;
                }
            }

            if (lastOperation != null && string.Compare(lastOperation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return;
            }

            foreach (var vm in VMs)
            {
                var configuration = vm.ConfigurationSets.OfType<WindowsProvisioningConfigurationSet>().FirstOrDefault();
                if (configuration != null)
                {
                    if (vm.WinRMCertificate != null)
                    {
                        if(!CertUtils.HasExportablePrivateKey(vm.WinRMCertificate))
                        {
                            throw new ArgumentException("WinRMCertificate needs to have an exportable private key.");
                        }
                        var operationDescription = string.Format("{0} - Uploading WinRMCertificate: {1}", CommandRuntime, vm.WinRMCertificate.Thumbprint);
                        var certificateFile = CertUtils.Create(vm.WinRMCertificate);
                        ExecuteClientActionInOCS(null, operationDescription, s => this.Channel.AddCertificates(s, this.ServiceName, certificateFile));
                    }
                    var certificateFilesWithThumbprint = from c in vm.X509Certificates
                                                         select new
                                                                {
                                                                    c.Thumbprint,
                                                                    CertificateFile = CertUtils.Create(c, vm.NoExportPrivateKey)
                                                                };
                    foreach (var current in certificateFilesWithThumbprint.ToList())
                    {
                        var operationDescription = string.Format("{0} - Uploading Certificate: {1}", CommandRuntime, current.Thumbprint);
                        ExecuteClientActionInOCS(null, operationDescription, s => this.Channel.AddCertificates(s, this.ServiceName, current.CertificateFile));
                    }
                }
            }

            var persistentVMs = this.VMs.Select(vm => CreatePersistenVMRole(vm, currentStorage)).ToList();

            // If the current deployment doesn't exist set it create it
            if (CurrentDeployment == null)
            {
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    try
                    {
                        var deployment = new Deployment
                        {
                            DeploymentSlot = "Production",
                            Name = this.DeploymentName ?? this.ServiceName,
                            Label = this.DeploymentLabel ?? this.ServiceName,
                            RoleList = new RoleList(new List<Role> { persistentVMs[0] }),
                            VirtualNetworkName = this.VNetName
                        };

                        if (this.DnsSettings != null)
                        {
                            deployment.Dns = new DnsSettings {DnsServers = new DnsServerList()};
                            foreach (var dns in this.DnsSettings)
                            {
                                deployment.Dns.DnsServers.Add(dns);
                            }
                        }

                        var operationDescription = string.Format("{0} - Create Deployment with VM {1}", CommandRuntime, persistentVMs[0].RoleName);
                        ExecuteClientAction(deployment, operationDescription, s => this.Channel.CreateDeployment(s, this.ServiceName, deployment));

                        if(this.WaitForBoot.IsPresent)
                        {
                            WaitForRoleToBoot(persistentVMs[0].RoleName);
                        }
                    }
                    catch (ServiceManagementClientException ex)
                    {
                        if (ex.HttpStatus == HttpStatusCode.NotFound)
                        {
                            throw new Exception("Cloud Service does not exist. Specify -Location or -AffinityGroup to create one.");
                        }
                        else
                        {
                            this.WriteErrorDetails(ex);
                        }
                        return;
                    }

                    this.createdDeployment = true;
                }
            }
            else
            {
                if (this.VNetName != null || this.DnsSettings != null || !string.IsNullOrEmpty(this.DeploymentLabel) || !string.IsNullOrEmpty(this.DeploymentName))
                {
                    WriteWarning("VNetName, DnsSettings, DeploymentLabel or DeploymentName Name can only be specified on new deployments.");
                }
            }

            if (this.createdDeployment == false && CurrentDeployment != null)
            {
                this.DeploymentName = CurrentDeployment.Name;
            }

            int startingVM = (this.createdDeployment == true) ? 1 : 0;

            for (int i = startingVM; i < persistentVMs.Count; i++)
            {
                var operationDescription = string.Format("{0} - Create VM {1}", CommandRuntime, persistentVMs[i].RoleName);
                ExecuteClientActionInOCS(persistentVMs[i],operationDescription, s => this.Channel.AddRole(s, this.ServiceName, this.DeploymentName ?? this.ServiceName, persistentVMs[i]));
            }

            if(this.WaitForBoot.IsPresent)
            {
                for (int i = startingVM; i < persistentVMs.Count; i++)
                {
                    WaitForRoleToBoot(persistentVMs[i].RoleName);
                }
            }
        }

        private PersistentVMRole CreatePersistenVMRole(PersistentVM persistentVM, CloudStorageAccount currentStorage)
        {
            if (persistentVM.OSVirtualHardDisk.MediaLink == null && string.IsNullOrEmpty(persistentVM.OSVirtualHardDisk.DiskName))
            {
                DateTime dateTimeCreated = DateTime.Now;
                string diskPartName = persistentVM.RoleName;

                if (persistentVM.OSVirtualHardDisk.DiskLabel != null)
                {
                    diskPartName += "-" + persistentVM.OSVirtualHardDisk.DiskLabel;
                }

                string vhdname = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", this.ServiceName, diskPartName,
                                               dateTimeCreated.Year, dateTimeCreated.Month, dateTimeCreated.Day,
                                               dateTimeCreated.Millisecond);
                string blobEndpoint = currentStorage.BlobEndpoint.AbsoluteUri;
                if (blobEndpoint.EndsWith("/") == false)
                {
                    blobEndpoint += "/";
                }

                persistentVM.OSVirtualHardDisk.MediaLink = new Uri(blobEndpoint + "vhds/" + vhdname);
            }

            foreach (DataVirtualHardDisk datadisk in persistentVM.DataVirtualHardDisks)
            {
                if (datadisk.MediaLink == null && string.IsNullOrEmpty(datadisk.DiskName))
                {
                    if (currentStorage == null)
                    {
                        throw new ArgumentException(
                            "CurrentStorageAccount is not set or not accessible. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
                    }

                    DateTime dateTimeCreated = DateTime.Now;
                    string diskPartName = persistentVM.RoleName;

                    if (datadisk.DiskLabel != null)
                    {
                        diskPartName += "-" + datadisk.DiskLabel;
                    }

                    string vhdname = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", this.ServiceName, diskPartName,
                                                   dateTimeCreated.Year, dateTimeCreated.Month, dateTimeCreated.Day,
                                                   dateTimeCreated.Millisecond);
                    string blobEndpoint = currentStorage.BlobEndpoint.AbsoluteUri;

                    if (blobEndpoint.EndsWith("/") == false)
                    {
                        blobEndpoint += "/";
                    }

                    datadisk.MediaLink = new Uri(blobEndpoint + "vhds/" + vhdname);
                }

                if (persistentVM.DataVirtualHardDisks.Count() > 1)
                {
                    // To avoid duplicate disk names
                    System.Threading.Thread.Sleep(1);
                }
            }

            return new PersistentVMRole
            {
                AvailabilitySetName = persistentVM.AvailabilitySetName,
                ConfigurationSets = persistentVM.ConfigurationSets,
                DataVirtualHardDisks = persistentVM.DataVirtualHardDisks,
                OSVirtualHardDisk = persistentVM.OSVirtualHardDisk,
                RoleName = persistentVM.RoleName,
                RoleSize = persistentVM.RoleSize,
                RoleType = persistentVM.RoleType,
                Label = persistentVM.Label
            };
        }

        protected override void ProcessRecord()
        {
            try
            {
                this.ValidateParameters();
                base.ProcessRecord();
                this.NewAzureVMProcess();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected void ValidateParameters()
        {
            if (ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(AffinityGroup))
                {
                    throw new ArgumentException("Location or AffinityGroup is required when creating a new Cloud Service.");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(AffinityGroup))
                {
                    throw new ArgumentException("Location or AffinityGroup can only be specified when creating a new cloud service.");
                }
            }

            if (this.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (!string.IsNullOrEmpty(this.VNetName) && string.IsNullOrEmpty(this.AffinityGroup))
                {
                    throw new ArgumentException("Must specify the same affinity group as the virtual network is deployed to.");
                }
            }

            if (this.ParameterSetName.Equals("CreateService", StringComparison.OrdinalIgnoreCase) == true || this.ParameterSetName.Equals("CreateDeployment", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (this.DnsSettings != null && string.IsNullOrEmpty(this.VNetName))
                {
                    throw new ArgumentException("VNetName is required when specifying DNS Settings.");
                }
            }

            foreach (PersistentVM pVM in this.VMs)
            {
                var provisioningConfiguration = pVM.ConfigurationSets
                                    .OfType<ProvisioningConfigurationSet>()
                                    .SingleOrDefault();

                if (provisioningConfiguration == null && pVM.OSVirtualHardDisk.SourceImageName != null)
                {
                    throw new ArgumentException(string.Format("Virtual Machine {0} is missing provisioning configuration", pVM.RoleName));
                }
            }
        }

        protected bool DoesCloudServiceExist(string serviceName)
        {
            bool isPresent = false;
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                try
                {
                    WriteVerboseWithTimestamp(string.Format("Begin Operation: {0}", CommandRuntime.ToString()));
                    AvailabilityResponse response = this.RetryCall(s => this.Channel.IsDNSAvailable(s, serviceName));
                    WriteVerboseWithTimestamp(string.Format("Completed Operation: {0}", CommandRuntime.ToString()));
                    isPresent = !response.Result;
                }
                catch (ServiceManagementClientException ex)
                {
                    if (ex.HttpStatus == HttpStatusCode.NotFound)
                    {
                        isPresent = false;
                    }
                    else
                    {
                        this.WriteErrorDetails(ex);
                    }
                }
            }

            return isPresent;
        }
    }
}