del TsSoft.Dapper.QueryBuilder.*.nupkg
del *.nuspec
del .\TsSoft.Dapper.QueryBuilder\bin\Release\*.nuspec

function GetNodeValue([xml]$xml, [string]$xpath)
{
	return $xml.SelectSingleNode($xpath).'#text'
}

function SetNodeValue([xml]$xml, [string]$xpath, [string]$value)
{
	$node = $xml.SelectSingleNode($xpath)
	if ($node) {
		$node.'#text' = $value
	}
}

Remove-Item .\TsSoft.Dapper.QueryBuilder\bin -Recurse 
Remove-Item .\TsSoft.Dapper.QueryBuilder\obj -Recurse

$build = "c:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe ""TsSoft.Dapper.QueryBuilder\TsSoft.Dapper.QueryBuilder.csproj"" /p:Configuration=Release" 
Invoke-Expression $build

$Artifact = (resolve-path ".\TsSoft.Dapper.QueryBuilder\bin\Release\TsSoft.Dapper.QueryBuilder.dll").path

nuget spec -F -A $Artifact

Copy-Item .\TsSoft.Dapper.QueryBuilder.nuspec.xml .\TsSoft.Dapper.QueryBuilder\bin\Release\TsSoft.Dapper.QueryBuilder.nuspec

$GeneratedSpecification = (resolve-path ".\TsSoft.Dapper.QueryBuilder.nuspec").path
$TargetSpecification = (resolve-path ".\TsSoft.Dapper.QueryBuilder\bin\Release\TsSoft.Dapper.QueryBuilder.nuspec").path

[xml]$srcxml = Get-Content $GeneratedSpecification
[xml]$destxml = Get-Content $TargetSpecification
$value = GetNodeValue $srcxml "//version"
SetNodeValue $destxml "//version" $value;
$value = GetNodeValue $srcxml "//description"
SetNodeValue $destxml "//description" $value;
$value = GetNodeValue $srcxml "//copyright"
SetNodeValue $destxml "//copyright" $value;
$destxml.Save($TargetSpecification)

nuget pack $TargetSpecification

del *.nuspec
del .\TsSoft.Dapper.QueryBuilder\bin\Release\*.nuspec

exit
