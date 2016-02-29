$coreclrdir="C:\Dev\coreclr_nano_tp3"

& csc.exe /nostdlib /noconfig /r:$coreclrdir\mscorlib.dll /r:$coreclrdir\System.Runtime.dll /target:library /out:Cloudbase.PSUtils.dll /keyfile:CoreClrPSUtils.snk ini.cs sudo.cs AssemblyInfo.cs
if($lastexiterror) { throw "csc failed"}
