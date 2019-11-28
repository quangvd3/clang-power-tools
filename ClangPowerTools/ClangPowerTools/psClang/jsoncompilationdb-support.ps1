# JSON Compilation Database project load
# ------------------------------------------------------------------------------------------------

Function GetJobs-CompilationDb([string] $projectPath
                              ,[Parameter(Mandatory=$true) ][WorkloadType] $workloadType)
{
    $projectData = LoadProject-CompilationDb $projectPath

    [string] $exeToCall = Get-ExeToCall -workloadType $workloadType

    $jobs = @()

    foreach ($projectItem in $projectData)
    {
        [string] $exeArgs = $projectItem.command

        $exeArgs = $exeArgs.Replace("clang-tool", "")
       
        $exeArgs = "-x c++ -fsyntax-only $aClangCompileFlags $exeArgs"
        Write-Verbose $exeArgs

        $newJob = New-Object PsObject -Prop @{ 'FilePath'        = $exeToCall
                                             ; 'WorkingDirectory'= $projectItem.directory
                                             ; 'ArgumentList'    = $exeArgs
                                             ; 'File'            = $projectItem.file
                                             ; 'JobCounter'      = 0 <# will be lazy initialized #>
                                             }
        $jobs += @($newJob)
    }

    return $jobs
}

Function LoadProject-CompilationDb([Parameter(Mandatory=$true)][string] $projectPath)
{
    return (Get-Content $projectPath | ConvertFrom-Json)
}
