@echo off
setlocal

:: Define paths using environment variables
set "ILMERGE_EXE=%USERPROFILE%\.nuget\packages\ilmerge\3.0.41\tools\net452\ILMerge.exe"
set "OUTPUT_EXE=%TARGETDIR%M&B-Bannerlord-ModPackTool-Final.exe"
set "TARGET_DLL=%TARGETPATH%"
set "INPUT_FILE=%TARGETDIR%M&B-Bannerlord-ModPackTool.exe"

set "ASPOSE=%TARGETDIR%Aspose.Zip.dll"
set "Microsoft.Bcl.AsyncInterfaces=%TARGETDIR%Microsoft.Bcl.AsyncInterfaces.dll"
set "SevenZipSharp=%TARGETDIR%SevenZipSharp.dll"
set "System.Buffers=%TARGETDIR%System.Buffers.dll"
set "System.Configuration.ConfigurationManager=%TARGETDIR%System.Configuration.ConfigurationManager.dll"
set "System.Memory=%TARGETDIR%System.Memory.dll"
set "System.Numerics.Vectors=%TARGETDIR%System.Numerics.Vectors.dll"
set "System.Runtime.CompilerServices.Unsafe=%TARGETDIR%System.Runtime.CompilerServices.Unsafe.dll"
set "System.Security.AccessControl=%TARGETDIR%System.Security.AccessControl.dll"
set "System.Security.Cryptography.ProtectedData=%TARGETDIR%System.Security.Cryptography.ProtectedData.dll"
set "System.Security.Permissions=%TARGETDIR%System.Security.Permissions.dll"
set "System.Security.Principal.Windows=%TARGETDIR%System.Security.Principal.Windows.dll"
set "System.Text.Encoding.CodePages=%TARGETDIR%System.Text.Encoding.CodePages.dll"
set "System.Text.Encodings.Web=%TARGETDIR%System.Text.Encodings.Web.dll"
set "System.Text.Json=%TARGETDIR%System.Text.Json.dll"
set "System.Threading.Tasks.Extensions=%TARGETDIR%System.Threading.Tasks.Extensions.dll"
set "System.ValueTuple=%TARGETDIR%System.ValueTuple.dll"

:: Run ILMerge with specified DLLs
"%ILMERGE_EXE%" "%INPUT_FILE%" /out:"%OUTPUT_EXE%" "%ASPOSE%" "%Microsoft.Bcl.AsyncInterfaces%" "%SevenZipSharp%" "%System.Buffers%" "%System.Configuration.ConfigurationManager%" "%System.Memory%" "%System.Numerics.Vectors%" "%System.Runtime.CompilerServices.Unsafe%" "%System.Security.AccessControl%" "%System.Security.Cryptography.ProtectedData%" "%System.Security.Permissions%" "%System.Security.Principal.Windows%" "%System.Text.Encoding.CodePages%" "%System.Text.Encodings.Web%" "%System.Text.Json%" "%System.Threading.Tasks.Extensions%" "%System.ValueTuple%"

endlocal