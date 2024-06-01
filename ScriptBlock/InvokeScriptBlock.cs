// Copyright (c) 2024 Roger Brown.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace RhubarbGeekNz.ScriptBlock
{
    [Cmdlet(VerbsLifecycle.Invoke, "ScriptBlock")]
    public class InvokeScriptBlock : PSCmdlet
    {
        [Parameter(ParameterSetName = "String", Mandatory = true, ValueFromPipeline = true)]
        public IEnumerable<char> InputString { get; set; }

        [Parameter(ParameterSetName = "ScriptBlock", Mandatory = true, ValueFromPipeline = true)]
        public System.Management.Automation.ScriptBlock ScriptBlock { get; set; }

        [Parameter]
        public object[] ArgumentList { get; set; }

        [Parameter]
        public SwitchParameter NoNewScope { get; set; }

        private List<string> ScriptBlockText = new List<string>();

        protected override void ProcessRecord()
        {
            if (InputString != null)
            {
                if (InputString is string s)
                {
                    ScriptBlockText.Add(s);
                }
                else
                {
                    ScriptBlockText.Add(new string(InputString is char[] c ? c : InputString.ToArray()));
                }
            }

            if (ScriptBlock != null)
            {
                if (ScriptBlockText.Count > 0)
                {
                    string script = ScriptBlockText.Count == 1 ? ScriptBlockText[0] : string.Join(Environment.NewLine, ScriptBlockText.ToArray());
                    ScriptBlockText.Clear();
                    InvokeScriptBlockWithPowerShell(System.Management.Automation.ScriptBlock.Create(script));
                }

                InvokeScriptBlockWithPowerShell(ScriptBlock);
            }
        }

        protected override void EndProcessing()
        {
            if (ScriptBlockText.Count > 0)
            {
                InvokeScriptBlockWithPowerShell(
                    System.Management.Automation.ScriptBlock.Create(
                        ScriptBlockText.Count == 1 ? ScriptBlockText[0] : string.Join(
                            Environment.NewLine,
                            ScriptBlockText.ToArray())));
            }
        }

        private ActionPreference? GetErrorActionPreference()
        {
            ActionPreference? result;
            PropertyInfo pi = CommandRuntime.GetType().GetProperty("ErrorAction", BindingFlags.Instance | BindingFlags.NonPublic);

            if ((pi != null) && typeof(ActionPreference).IsAssignableFrom(pi.PropertyType))
            {
                result = (ActionPreference?)pi.GetValue(CommandRuntime);
            }
            else
            {
                object ErrorActionPreference = GetVariableValue("ErrorActionPreference");

                if (ErrorActionPreference != null)
                {
                    if (ErrorActionPreference is ActionPreference actionPreference)
                    {
                        result = actionPreference;
                    }
                    else
                    {
                        if (ErrorActionPreference is string s)
                        {
                            result = (ActionPreference)Enum.Parse(typeof(ActionPreference), s);
                        }
                        else
                        {
                            throw new ArgumentException($"ErrorActionPreference {ErrorActionPreference}");
                        }
                    }
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }

        private void InvokeScriptBlockWithPowerShell(System.Management.Automation.ScriptBlock scriptBlock)
        {
            using (PowerShell powerShell = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                PowerShell cmd = powerShell.AddCommand("Invoke-Command", true);

                cmd = cmd.AddParameter("ScriptBlock", scriptBlock);

                if (NoNewScope)
                {
                    cmd.AddParameter("NoNewScope");
                }

                if (ArgumentList != null)
                {
                    cmd = cmd.AddParameter("ArgumentList", ArgumentList);
                }

                PSDataCollection<object> input = new PSDataCollection<object>();
                input.Complete();
                PSDataCollection<object> output = new PSDataCollection<object>();

                output.DataAdded += (s, e) =>
                {
                    object data = output[e.Index];
                    WriteObject(data);
                };

                PSInvocationSettings invocationSettings = new PSInvocationSettings
                {
                    Host = Host,
                    ExposeFlowControlExceptions = true,
                    ErrorActionPreference = GetErrorActionPreference()
                };

                powerShell.Invoke<object, object>(input, output, invocationSettings);
            }
        }
    }
}
