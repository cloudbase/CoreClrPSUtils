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
