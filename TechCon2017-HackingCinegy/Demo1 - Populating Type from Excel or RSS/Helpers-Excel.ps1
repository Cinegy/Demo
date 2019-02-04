#
# Helper functions to load data from Excel file
#
# Excel data processing is based on sample code from:
# https://www.simple-talk.com/sysadmin/powershell/powershell-data-basics-file-based-data/
#

function Excel-ImportData([string]$FilePath, [string]$SheetName = "")
{
    $csvFile = Join-Path $env:temp ("{0}.csv" -f (Get-Item -path $FilePath).BaseName)
    if (Test-Path -path $csvFile) { Remove-Item -path $csvFile }
 
    # convert Excel file to CSV file
    $xlCSVType = 62 # better unicode support xlCSVUTF8 
    $excelObject = New-Object -ComObject Excel.Application  
    $excelObject.Visible = $false 
    $workbookObject = $excelObject.Workbooks.Open($FilePath)
    Excel-SetActiveSheet $workbookObject $SheetName | Out-Null
    $workbookObject.SaveAs($csvFile,$xlCSVType) 
    $workbookObject.Saved = $true
    $workbookObject.Close()
 
     # cleanup 
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbookObject) | Out-Null
    $excelObject.Quit()
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excelObject) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
 
    # now import and return the data 
    Import-Csv -path $csvFile
}

function Excel-FindSheet([Object]$workbook, [string]$name)
{
    $sheetNumber = 0
    for ($i=1; $i -le $workbook.Sheets.Count; $i++) {
        if ($name -eq $workbook.Sheets.Item($i).Name) { $sheetNumber = $i; break }
    }
    return $sheetNumber
}
 
function Excel-SetActiveSheet([Object]$workbook, [string]$name)
{
    if (!$name) { return }
    $sheetNumber = Excel-FindSheet $workbook $name
    if ($sheetNumber -gt 0) { $workbook.Worksheets.Item($sheetNumber).Activate() }
    return ($sheetNumber -gt 0)
}
