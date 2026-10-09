function Get-CandidateSource([string]$Root) {
    $candidateFiles = foreach ($candidateDir in @('Assets','Packages','ProjectSettings')) {
        Get-ChildItem -LiteralPath (Join-Path $Root "UnityProject/$candidateDir") -Recurse -File |
            Where-Object { $_.Name -notin @('CandidateIdentity.json','CandidateIdentity.json.meta','PerformanceTestRunInfo.json','PerformanceTestRunInfo.json.meta','PerformanceTestRunSettings.json','PerformanceTestRunSettings.json.meta') } |
            ForEach-Object {
                [ordered]@{ path=$_.FullName.Substring($Root.Length+1).Replace('\','/'); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
            }
    }
    $candidateOrdered = [System.Collections.Generic.SortedDictionary[string,string]]::new([StringComparer]::Ordinal)
    foreach ($candidateEntry in $candidateFiles) { $candidateOrdered.Add($candidateEntry.path, $candidateEntry.sha256) }
    $candidateText = (($candidateOrdered.GetEnumerator() | ForEach-Object { $_.Key + ':' + $_.Value }) -join "`n") + "`n"
    $candidateHasher = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($candidateHasher.ComputeHash([Text.Encoding]::UTF8.GetBytes($candidateText)))).Replace('-','').ToLowerInvariant() }
    finally { $candidateHasher.Dispose() }
}

function Invoke-CandidateChecked([string]$Executable, [string]$Arguments, [string]$Log, [string]$Marker, [int]$Minutes=20) {
    $candidateProcess = Start-Process -FilePath $Executable -ArgumentList ($Arguments + ' -logFile "' + $Log + '"') -WindowStyle Hidden -PassThru
    $candidateDeadline = [DateTime]::UtcNow.AddMinutes($Minutes)
    while (-not $candidateProcess.WaitForExit(1000)) {
        if ([DateTime]::UtcNow -gt $candidateDeadline) { $candidateProcess.Kill(); throw "Timeout. See $Log" }
    }
    $candidateLog = [IO.File]::ReadAllText($Log)
    if ($candidateProcess.ExitCode -ne 0 -or -not $candidateLog.Contains($Marker) -or
        $candidateLog -match 'error CS\d+|Unhandled Exception|SHIFTBOUND .*FAILED:') {
        throw "Failed (exit $($candidateProcess.ExitCode)): $Arguments. See $Log"
    }
    Write-Output "PASS: $Marker -> $Log"
}
