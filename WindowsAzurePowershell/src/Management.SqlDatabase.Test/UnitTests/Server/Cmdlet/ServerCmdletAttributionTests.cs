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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Server.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using SqlDatabase.Server.Cmdlet;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// These tests prevent regression in parameter validation attributes.
    /// </summary>
    [TestClass]
    public class ServerCmdletAttributionTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
        }

        [TestMethod]
        public void GetAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(GetAzureSqlDatabaseServer);
            UnitTestHelper.CheckConfirmImpact(cmdlet, ConfirmImpact.None);
            UnitTestHelper.CheckCmdletModifiesData(cmdlet, false);
        }

        [TestMethod]
        public void NewAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(NewAzureSqlDatabaseServer);
            UnitTestHelper.CheckConfirmImpact(cmdlet, ConfirmImpact.Low);
            UnitTestHelper.CheckCmdletModifiesData(cmdlet, true);
        }

        [TestMethod]
        public void RemoveAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(RemoveAzureSqlDatabaseServer);
            UnitTestHelper.CheckConfirmImpact(cmdlet, ConfirmImpact.High);
            UnitTestHelper.CheckCmdletModifiesData(cmdlet, true);
        }

        [TestMethod]
        public void SetAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(SetAzureSqlDatabaseServer);
            UnitTestHelper.CheckConfirmImpact(cmdlet, ConfirmImpact.Medium);
            UnitTestHelper.CheckCmdletModifiesData(cmdlet, true);
        }
    }
}
