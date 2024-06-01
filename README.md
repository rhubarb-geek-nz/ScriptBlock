# ScriptBlock

This [PowerShell](https://en.wikipedia.org/wiki/PowerShell) module creates and runs scripts from the input pipeline.

## Command summary

```
New-ScriptBlock [-InputString] <string>

Invoke-ScriptBlock -InputString <string> [-ArgumentList <Object[]>] [-NoNewScope]

Invoke-ScriptBlock -ScriptBlock <scriptblock> [-ArgumentList <Object[]>] [-NoNewScope]
```

The instantiation cmdlet uses [ScriptBlock.Create](https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.scriptblock.create?view=powershellsdk-7.4.0#system-management-automation-scriptblock-create(system-string)) to create the script.

The invocation cmdlet uses [Invoke-Command](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/invoke-command?view=powershell-7.4) to run the script.

## Example
Pipe a script and invoke with given arguments.

```
@'
param($Message)
Write-Output -InputObject $Message
'@ | Invoke-ScriptBlock -ArgumentList 'Hello World'
```

## Build

Build in the `ScriptBlock` subdirectory with 

```
$ dotnet publish --configuration Release
```

Install by copying the module into a directory on the [PSModulePath](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_psmodulepath)

## Example simulating POSIX shell behaviour for piped scripts

This uses [rhubarb-geek-nz.Console](https://www.powershellgallery.com/packages/rhubarb-geek-nz.Console/1.0.4) to provide POSIX like `stdin`, `stdout` and `stderr` access.

* Script is read from `stdin` using `Read-Console`
* It is invoked by `Invoke-ScriptBlock`.
* Arguments are passed via `$MyInvocation`
* Output is written through `Write-Console` to split to `stdout` and `stderr`.
* try/catch is used to set `LastExitCode` on `Write-Error`

```
#!/usr/bin/env pwsh

$ErrorActionPreference = [System.Management.Automation.ActionPreference]::Stop

& {
	try
	{
		Read-Console | Invoke-ScriptBlock -ArgumentList $global:MyInvocation.UnboundArguments

		Set-Variable -Name LastExitCode -Scope Global -Value 0
	}
	catch
	{
		Set-Variable -Name LastExitCode -Scope Global -Value 1

		$PSItem
	}
} *>&1 | Write-Console

Exit $LastExitCode
```
