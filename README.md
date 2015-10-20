# Nano Server CoreCLR PowerShell Utilities

Due to lack of Add-Type, C# code needs to be included in a precompiled DLL.

### Usage

On Nano Server, copy Cloudbase.PSUtils.dll in
C:\Windows\System32\WindowsPowerShell\v1.0.

And then proceed as usual:

    Import-Module <module.ps1>

### Build

The Cloudbase.PSUtils.dll assembly can be rebuilt using the provided build script:

    .\build.ps1

The script needs a copy of Nano Server's CoreCLR and PowerShell assemblies,
which can be obtained from a Nano instance:

    C:\Windows\System32\WindowsPowerShell\v1.0

and from the CoreCLR location, e.g.:

    C:\Windows\WinSxS\amd64_microsoft-onecore-coreclr-powershell-ext_31bf3856ad364e35_10.0.10514.0_none_3bd975248393ca54
