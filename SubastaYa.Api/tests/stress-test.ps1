param(
    [Parameter(Mandatory = $true)]
    [int]$AuctionId,

    [Parameter(Mandatory = $true)]
    [int]$BuyerId,

    [Parameter(Mandatory = $true)]
    [decimal]$Amount,

    [string]$BaseUrl = "https://localhost:7090"
)

$endpoint = "$BaseUrl/api/v1/auctions/$AuctionId/bids"

$payload = @{
    buyerId = $BuyerId
    amount = $Amount
} | ConvertTo-Json -Compress

$gateName = "SubastaYaStressGate-$([Guid]::NewGuid())"

$gate = New-Object System.Threading.EventWaitHandle(
    $false,
    [System.Threading.EventResetMode]::ManualReset,
    $gateName)

$jobScript = {
    param(
        $gateName,
        $endpoint,
        $payload)

    $localGate =
        [System.Threading.EventWaitHandle]::OpenExisting($gateName)

    $localGate.WaitOne()

    $rawResponse = (
        & curl.exe `
            -k `
            -sS `
            -X POST `
            $endpoint `
            -H "Content-Type: application/json" `
            --data $payload `
            -w "`n%{http_code}"
    ) -join "`n"

    $lastBreak = $rawResponse.LastIndexOf("`n")

    [pscustomobject]@{
        StatusCode = [int]$rawResponse.Substring($lastBreak + 1)
        Body = $rawResponse.Substring(0, $lastBreak)
    }
}

$firstJob = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList $gateName, $endpoint, $payload

$secondJob = Start-Job `
    -ScriptBlock $jobScript `
    -ArgumentList $gateName, $endpoint, $payload

Start-Sleep -Milliseconds 750

$gate.Set()

$results = @(
    Receive-Job -Job $firstJob -Wait
    Receive-Job -Job $secondJob -Wait
)

Remove-Job -Job $firstJob
Remove-Job -Job $secondJob

$results | Format-Table -AutoSize

$codes = @($results.StatusCode | Sort-Object)

if (($codes -join ",") -ne "201,409")
{
    throw "[CODE-ERROR] - el stress test debía devolver exactamente 201 y 409."
}

Write-Host "Stress test aprobado: una puja fue aceptada y la otra rechazada con 409."