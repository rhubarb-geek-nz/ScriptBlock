// Copyright (c) 2024 Roger Brown.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace RhubarbGeekNz.ScriptBlock
{
    [Cmdlet(VerbsCommon.New, "ScriptBlock")]
    [OutputType(typeof(System.Management.Automation.ScriptBlock))]
    public sealed class NewScriptBlock : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true)]
        public IEnumerable<char> InputString { get; set; }

        private List<string> ScriptBlockText;

        protected override void BeginProcessing()
        {
            ScriptBlockText = new List<string>();
        }

        protected override void ProcessRecord()
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

        protected override void EndProcessing()
        {
            WriteObject(
                System.Management.Automation.ScriptBlock.Create(
                    ScriptBlockText.Count == 1 ? ScriptBlockText[0] : string.Join(
                        System.Environment.NewLine,
                        ScriptBlockText.ToArray())));
        }
    }
}
