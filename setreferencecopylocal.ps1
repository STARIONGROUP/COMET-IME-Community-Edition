#THIS FILE IS OBSOLETE SINCE THE CSPROJ CONVERSION TO SDK STYLE PROJECTS


# Copyright RHEA System S.A.
# A powershell script that is used to make sure that on the plugin projects the CDP4Common nuget is not copied to local
# this script needs to be run after CDP4Common nuget has been updated

#dir -Recurse *.csproj | `
#    ForEach-Object { $projectpath = $_.FullName; 
#        if (($projectpath -notlike '*CDP4IME*') -and ($projectpath -notlike '*.Tests*'))
#        {
#            $xml = [xml](Get-Content $_)
#            $references = $xml.Project.ItemGroup | Where-Object { $_.Reference -ne $null }
#            foreach($reference in $references.ChildNodes)
#            {
#                if (($reference.Include -like '*CDP4RequirementsVerification*') -or ($reference.Include -like '*CDP4Common*') -or ($reference.Include -like '*CDP4JsonSerializer*') -or ($reference.Include -like '*CDP4Dal*') -or ($reference.Include -like '*Microsoft.Practices.ServiceLocation*') -or ($reference.Include -like '*NLog*') -or ($reference.Include -like '*Newtonsoft.Json*') -or ($reference.Include -like '*DevExpress*'))
#                {
#                    if($reference.Private -eq $null)
#                    {
#                        [System.Xml.XmlElement]$copyLocal = $xml.CreateElement("Private", "http://schemas.microsoft.com/developer/msbuild/2003")
#                        $copyLocal.InnerText = "False"
#                        [Void]$reference.AppendChild($copyLocal) 
#
#                        $xml.save($projectpath)
#                        write-Host $projectpath
#                    }
#                    else
#                    {
#                        if ($reference.Private -eq "True")
#                        {
#                            $reference.Private = "False"
#                            $xml.save($projectpath)
#                            write-Host $projectpath
#                        }
#                    }
#                }
#            }
#        }
#    }