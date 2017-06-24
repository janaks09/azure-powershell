using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Microsoft.Azure.Build.Tasks.Properties;
using System.Collections.Generic;
using TestMapper;

namespace SmartTesterTask
{
    public class SmartTestingTask : Task
    {
        [Required]
        public string RepositoryOwner { get; set; }
        [Required]
        public string RepositoryName { get; set; }
        [Required]
        public string PullRequestNumber { get; set; }
        [Required]
        public string MapFilePath { get; set; }
        [Output]
        public string[] TestAssemblies { get; set; }
        public override bool Execute()
        {
           // Call the PowerShell.Create() method to create an 
           // empty pipeline.
            PowerShell powerShell = PowerShell.Create();

            powerShell.AddScript(Resources.GetFilesScript);
            powerShell.AddScript($"Get-PullRequestFileChanges -RepositoryOwner {RepositoryOwner} " +
                                         $"-RepositoryName {RepositoryName} -PullRequestNumber {PullRequestNumber}");

            // invoke execution on the pipeline (collecting output)
            Collection<PSObject> psOutput = powerShell.Invoke();
            List<string> filesChanged = new List<string>();

            if (psOutput == null)
            {
                return false;
            }

            foreach (var element in psOutput)
            {
                filesChanged.Add(element.ToString());
            }

            TestAssemblies = new List<string>(TestSetGenerator.GetTests(filesChanged, MapFilePath)).ToArray();

            return true;
        }
    }
}
