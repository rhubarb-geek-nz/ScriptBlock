// Copyright (c) 2024 Roger Brown.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace RhubarbGeekNz.ScriptBlock
{
    [TestClass]
    public class ScriptBlockTests
    {
        readonly InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
        PowerShell powerShell;

        public ScriptBlockTests()
        {
            foreach (Type t in new Type[] {
                typeof(InvokeScriptBlock),
                typeof(NewScriptBlock)
            })
            {
                CmdletAttribute ca = t.GetCustomAttribute<CmdletAttribute>();

                initialSessionState.Commands.Add(new SessionStateCmdletEntry($"{ca.VerbName}-{ca.NounName}", t, ca.HelpUri));
            }

            initialSessionState.Variables.Add(new SessionStateVariableEntry("ErrorActionPreference", ActionPreference.Stop, "Stop"));
        }

        [TestInitialize]
        public void Initialize()
        {
            powerShell = PowerShell.Create(initialSessionState);
        }

        [TestCleanup]
        public void Cleanup()
        {
            powerShell.Dispose();
            powerShell = null;
        }

        [TestMethod]
        public void TestInvokeScriptBlock()
        {
            var result = InvokeScriptBlock("'& {','Write-Output -InputObject \"foo\"','}' | Invoke-ScriptBlock");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("foo", result[0].BaseObject.ToString());
        }

        [TestMethod]
        public void TestNewScriptBlock()
        {
            var result = InvokeScriptBlock("'& {','Write-Output -InputObject \"Hello World\"','}' | New-ScriptBlock");
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].BaseObject is System.Management.Automation.ScriptBlock);
        }

        [TestMethod]
        public void TestInvokeNewScriptBlock()
        {
            var result = InvokeScriptBlock("'& {','Write-Output -InputObject \"bar\" ','}' | New-ScriptBlock | Invoke-ScriptBlock");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bar", result[0].BaseObject.ToString());
        }

        [TestMethod]
        public void TestInvokeScriptBlockArguments()
        {
            var result = InvokeScriptBlock("'param($foo)','Write-Output -InputObject $foo' | Invoke-ScriptBlock -ArgumentList 'bar'");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bar", result[0].BaseObject.ToString());
        }

        [TestMethod]
        public void TestInvokeError()
        {
            bool caught = false;
            try
            {
                InvokeScriptBlock("'& {','Write-Error \"bar\" ','}' | Invoke-ScriptBlock");
            }
            catch (ActionPreferenceStopException)
            {
                caught = true;
            }
            Assert.IsTrue(caught);
        }

        [TestMethod]
        public void TestInvokeNoNewScope()
        {
            var result = InvokeScriptBlock("'& Set-Variable -Name \"foo\" -Value \"bar\"' | Invoke-ScriptBlock -NoNewScope; $foo");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bar", result[0].BaseObject.ToString());
        }

        [TestMethod]
        public void TestInvokeNewScope()
        {
            var result = InvokeScriptBlock("'& Set-Variable -Name \"foo\" -Value \"bar\"' | Invoke-ScriptBlock; $foo");
            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result[0]);
        }

        [TestMethod]
        public void TestInvokeEmpty()
        {
            var result = InvokeScriptBlock("'','',''| Invoke-ScriptBlock");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TestNewEmpty()
        {
            var result = InvokeScriptBlock("'','',''| New-ScriptBlock");
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].BaseObject is System.Management.Automation.ScriptBlock);
        }


        private Collection<PSObject> InvokeScriptBlock(string cmd)
        {
            powerShell.AddScript(cmd);

            return powerShell.Invoke();
        }
    }
}
