
namespace CDP4PluginPackager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    using CDP4PluginPackager.Models;

    using Newtonsoft.Json;

    public class App
    {
        private readonly bool shouldPluginGetPacked;
        private readonly string path;

        public string OutputPath { get; private set; }

        public CsprojectFile Csproj { get; private set; }

        public AssemblyName AssemblyInfo { get; private set; }

        public Manifest Manifest { get; set; }

        public App(string[] args = null)
        {
            if (args == null || !args.Any(Directory.Exists))
            {
                this.path = Directory.GetCurrentDirectory();
            }
            else
            {
                this.path = args.FirstOrDefault(Directory.Exists);
            }

            this.shouldPluginGetPacked = args?.Any(a => a.ToLower() == "pack") == true;
        }

        public void Start()
        {
            this.Deserialize();
            this.GetAssemblyInfo();
            this.BuildManifest();
            this.Serialize();

            if (this.shouldPluginGetPacked)
            {
                Console.WriteLine("---- Packing starting ----");
                this.Pack();
                Console.WriteLine("---- Packing done ----");
            }
        }
        
        private void BuildManifest()
        {
            this.Manifest = new Manifest
            {
                Name = this.AssemblyInfo.Name,
                Version = this.AssemblyInfo.Version.ToString(),
                ProjectGuid = this.Csproj.PropertyGroup.First(p => p.ProjectGuid != Guid.Empty).ProjectGuid,
                TargetFramework = this.Csproj.PropertyGroup.First(p => !string.IsNullOrWhiteSpace(p.TargetFrameworkVersion)).TargetFrameworkVersion,
                License = this.GetLicense(),
                Author = "RHEA System S.A.",
                References = this.ComputeReferences().ToList(),
                Website = "https://store.cdp4.com",
                Description = this.AssemblyInfo.FullName,
                ReleaseNote = this.GetReleaseNote()
            };
        }

        public string GetLicense()
        {
            var licensePath = Directory.GetParent(this.path).EnumerateFiles().FirstOrDefault(f => f.Name == "PluginLicense.txt")?.FullName;

            if (string.IsNullOrWhiteSpace(licensePath))
            {
                Console.WriteLine("---- Warning no license file has been found ----");
                return null;
            }

            return  File.ReadAllText(licensePath).
                Replace("$PLUGIN_NAME", this.AssemblyInfo.Name).
                Replace("$YEAR", DateTime.Now.Year.ToString());
        }

        private string GetReleaseNote()
        {
            var releaseNotePath = Directory.EnumerateFiles(this.path).FirstOrDefault(f => f.ToLower() == "releasenote.md");

            if (string.IsNullOrWhiteSpace(releaseNotePath))
            {
                Console.WriteLine("---- Warning no release note has been found ----");
                return null;
            }
            return File.ReadAllText(releaseNotePath);
        }

        private IEnumerable<Reference> ComputeReferences()
        {
            return this.Csproj.ItemGroup.SelectMany(r => r.Reference);
        }

        public void GetAssemblyInfo()
        {
            this.OutputPath = this.Csproj.PropertyGroup.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.OutputPath))?.OutputPath;
            var assemblyName = this.Csproj.PropertyGroup.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.AssemblyName))?.AssemblyName;
            var dllpath = $"{this.OutputPath}{assemblyName}.dll";
            this.AssemblyInfo = Assembly.LoadFrom(dllpath).GetName();
        }
        
        public void Serialize()
        {
            var output = JsonConvert.SerializeObject(this.Manifest);
            File.WriteAllText($"{Path.Join(this.OutputPath, this.Manifest.Name)}.plugin.manifest",output);
        }

        public void Deserialize()
        {
            var csprojPath = Directory.EnumerateFiles(this.path).FirstOrDefault(f => f.EndsWith(".csproj"));

            using var stream = File.OpenText(csprojPath);
            var serializer = new XmlSerializer(typeof(CsprojectFile));
            this.Csproj = (CsprojectFile)serializer.Deserialize(stream);
        }

        private void Pack()
        {
            var zipPath = Path.Combine(this.OutputPath, $"{this.Manifest.Name}.cdp4ck");
            var temporaryZipPath = Path.Combine(this.path, $"{this.Manifest.Name}.cdp4ck");
            
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(this.OutputPath, temporaryZipPath, CompressionLevel.Optimal, false);

            File.Move(temporaryZipPath, zipPath);
        }
    }
}
