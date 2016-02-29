<#
Copyright 2014 Cloudbase Solutions Srl

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
#>

$psutils = [System.Reflection.Assembly]::Load("Cloudbase.PSUtils, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7a69609ad6300e9f")
$api = [Activator]::CreateInstance($psutils.GetType("Cloudbase.PSUtils.Win32IniApi"))

function Get-IniFileValue
{
    [CmdletBinding()]
    param
    (
        [parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$Key,

        [parameter()]
        [string]$Section = "DEFAULT",

        [parameter()]
        [string]$Default = $null,

        [parameter(Mandatory=$true)]
        [string]$Path
    )
    process
    {
        return $api.GetIniValue($Section, $Key, $Default, $Path)
    }
}

function Set-IniFileValue
{
    [CmdletBinding()]
    param
    (
        [parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$Key,

        [parameter()]
        [string]$Section = "DEFAULT",

        [parameter(Mandatory=$true)]
        [string]$Value,

        [parameter(Mandatory=$true)]
        [string]$Path
    )
    process
    {
        $api.SetIniValue($Section, $Key, $Value, $Path)
    }
}

function Remove-IniFileValue
{
    [CmdletBinding()]
    param
    (
        [parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$Key,

        [parameter()]
        [string]$Section = "DEFAULT",

        [parameter(Mandatory=$true)]
        [string]$Path
    )
    process
    {
        $api.SetIniValue($Section, $Key, $null, $Path)
    }
}


function Start-ProcessAsUser
{
    <#
    .SYNOPSIS

    Starts a process under a user defined by the credentials given as a parameter.
    This command is similar to Linux "su", making possible to run a command under
    different Windows users, for example a user which is a domain administrator.

    .DESCRIPTION

    It uses a wrapper of advapi32.dll functionality,
    [PSCloudbase.ProcessManager]::RunProcess, which is defined as native C++ code
    in the same file.

    .PARAMETER Command

    The executable file path.

    .PARAMETER Arguments

    The arguments that will be sent to the process.

    .PARAMETER Credential

    The credential under which the newly spawned process will run. A credential can
    be created by instantiating System.Management.Automation.PSCredential class.

    .PARAMETER LoadUserProfile

    Whether to load the user profile in case the process needs it.

    .EXAMPLE

    $exitCode = Start-ProcessAsUser -Command "$PShome\powershell.exe" -Arguments @("script.ps1", "arg1", "arg2") -Credential $credential

    .Notes

    The user under which this command is run must have the appropriate privilleges
    and to be a local administrator in order to be able to execute the command
    successfully.
    #>
    [CmdletBinding()]
    Param
    (
        [parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [String]
        $Command,
        [parameter()]
        [String]
        $Arguments,
        [parameter(Mandatory=$true)]
        [PSCredential]
        $Credential,
        [parameter()]
        [bool]$LoadUserProfile = $true,
        [ValidateSet("InteractiveLogon", "NetworkLogon", "BatchLogon", "ServiceLogon")]
        [string]$LogonType="BatchLogon"
    )

    Process
    {
        $nc = $Credential.GetNetworkCredential()
        $domain = "."
        if($nc.Domain)
        {
            $domain = $nc.Domain
        }
        $logonTypes = @{
            "InteractiveLogon" = 2;
            "NetworkLogon" = 3;
            "BatchLogon" = 4;
            "ServiceLogon" = 5;
        }

        $l = $logonTypes[$LogonType]

        return [Cloudbase.PSUtils.ProcessManager]::RunProcess(
            $nc.UserName, $nc.Password, $domain, $Command, $Arguments,
            $LoadUserProfile, $l)
    }
}