﻿# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

########################################################################### General Websites Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests any cloud based cmdlet with invalid credentials and expect it'll throw an exception.
#>
function Test-WithInvalidCredentials
{
	param([ScriptBlock] $cloudCmdlet)
	
	# Setup
	Remove-AllSubscriptions

	# Test
	Assert-Throws $cloudCmdlet "Call Set-AzureSubscription and Select-AzureSubscription first."
}

########################################################################### Remove-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Remove-AzureWebsite with existing name
#>
function Test-RemoveAzureServiceWithValidName
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	$expected = "The website $name was not found. Please specify a valid website name."

	# Test
	Remove-AzureWebsite $name -Force

	# Assert
	Assert-Throws { Get-AzureWebsite $name } $expected
}

<#
.SYNOPSIS
Tests Remove-AzureWebsite with non existing name
#>
function Test-RemoveAzureServiceWithNonExistingName
{
	Assert-Throws { Remove-AzureWebsite "OneSDKNotExisting" -Force } "The website OneSDKNotExisting was not found. Please specify a valid website name."
}

<#
.SYNOPSIS
Tests Remove-AzureWebsite with WhatIf
#>
function Test-RemoveAzureServiceWithWhatIf
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	$expected = "The website $name was not found. Please specify a valid website name."

	# Test
	Remove-AzureWebsite $name -Force -WhatIf
	Remove-AzureWebsite $name -Force

	# Assert
	Assert-Throws { Get-AzureWebsite $name } $expected
}

########################################################################### Get-AzureWebsiteLog Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -Tail
#>
function Test-GetAzureWebsiteLogTail
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	$client = New-Object System.Net.WebClient
	$uri = "http://" + $website.HostNames[0]
	$client.BaseAddress = $uri
	$count = 0

	#Test
	Get-AzureWebsiteLog -Tail -Message "㯑䲘䄂㮉" | % {
		if ($_ -like "*㯑䲘䄂㮉*") { cd ..; exit; }
		Retry-DownloadString $client $uri
		$count++
		if ($count -gt 50) { cd ..; throw "Logs were not found"; }
	}
}

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -Tail with special characters in uri.
#>
function Test-GetAzureWebsiteLogTailUriEncoding
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	$client = New-Object System.Net.WebClient
	$uri = "http://" + $website.HostNames[0]
	$client.BaseAddress = $uri
	$count = 0

	#Test
	Get-AzureWebsiteLog -Tail -Message "mes/a:q;" | % {
		if ($_ -like "*mes/a:q;*") { cd ..; exit; }
		Retry-DownloadString $client $uri
		$count++
		if ($count -gt 50) { cd ..; throw "Logs were not found"; }
	}
}

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -Tail
#>
function Test-GetAzureWebsiteLogTailPath
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	$client = New-Object System.Net.WebClient
	$uri = "http://" + $website.HostNames[0]
	$client.BaseAddress = $uri
	Set-AzureWebsite -RequestTracingEnabled $true -HttpLoggingEnabled $true -DetailedErrorLoggingEnabled $true
	1..10 | % { Retry-DownloadString $client $uri }
	Start-Sleep -Seconds 30

	#Test
	$retry = $false
	do
	{
		try
		{
			Get-AzureWebsiteLog -Tail -Path http | % {
				if ($_ -like "*")
				{
					cd ..
					exit
				}
				throw "HTTP path is not reached"
			}
		}
		catch
		{
			if ($_.Exception.Message -eq "One or more errors occurred.")
			{
				$retry = $true;
				Write-Warning "Retry Test-GetAzureWebsiteLogTailPath"
				continue;
			}

			throw $_.Exception
		}
	} while ($retry)
}

<#
.SYNOPSIS
Tests Get-AzureWebsiteLog with -ListPath
#>
function Test-GetAzureWebsiteLogListPath
{
	# Setup
	New-BasicLogWebsite

	#Test
	$retry = $false
	do
	{
		try
		{
			$actual = Get-AzureWebsiteLog -ListPath;
			$retry = $false
		}
		catch
		{
			if ($_.Exception.Message -like "For security reasons DTD is prohibited in this XML document.*")
			{
				$retry = $true;
				Write-Warning "Retry Test-GetAzureWebsiteLogListPath"
				continue;
			}
			cd ..
			throw $_.Exception
		}
	} while ($retry)

	# Assert
	Assert-AreEqual 1 $actual.Count
	Assert-AreEqual "Git" $actual
	cd ..
}

########################################################################### Get-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Get-AzureWebsite
#>
function Test-GetAzureWebsite
{
	# Setup
	New-BasicLogWebsite
	$website = $global:currentWebsite
	Set-AzureWebsite $website.Name -AzureDriveTraceEnabled $true

	#Test
	$config = Get-AzureWebsite -Name $website.Name

	# Assert
	Assert-AreEqual $true $config.AzureDriveTraceEnabled
}

########################################################################### Start-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Start-AzureWebsite happy path.
#>
function Test-StartAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	Stop-AzureWebsite $name
	Assert-Throws { Get-AzureWebsite $name }

	# Test
	Start-AzureWebsite $name

	# Assert
	$website = Get-AzureWebsite $name
	Assert-AreEqual "Running" $website.State
}

########################################################################### Stop-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Stop-AzureWebsite happy path.
#>
function Test-StopAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name

	# Test
	Stop-AzureWebsite $name

	# Assert
	Assert-Throws { Get-AzureWebsite $name }
}

########################################################################### Restart-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Restart-AzureWebsite happy path.
#>
function Test-RestartAzureWebsite
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name

	# Test
	Restart-AzureWebsite $name

	# Assert
	$website = Get-AzureWebsite $name
	Assert-AreEqual "Running" $website.State
}