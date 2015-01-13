﻿using NuGet.PackageManagement;
using NuGet.PackagingCore;
using NuGet.ProjectManagement;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Utility;
using Xunit;

namespace NuGet.Test
{
    public class PackageDownloaderTests
    {
        [Fact]
        public async Task TestDownloadPackage()
        {
            Uri downloadUrl = new Uri(@"http://nuget.org/api/v2/Package/JQuery/1.8.2");
            using(var targetStream = new MemoryStream())
            {
                await PackageDownloader.GetPackageStream(downloadUrl, targetStream);
                // jQuery.1.8.2 is of size 185476 bytes. Make sure the download is successful
                Assert.Equal(185476, targetStream.Length);
            }
        }

        [Fact]
        public async Task TestDownloadAndInstallPackage()
        {
            var randomTestFolder = TestFilesystemUtility.CreateRandomTestFolder();
            Uri downloadUrl = new Uri(@"http://nuget.org/api/v2/Package/EntityFramework/5.0.0");
            var folderNuGetProject = new FolderNuGetProject(randomTestFolder);
            var packageIdentity = new PackageIdentity("EntityFramework", new NuGetVersion("5.0.0"));
            var packageInstallPath = folderNuGetProject.PackagePathResolver.GetInstallPath(packageIdentity);
            var nupkgFilePath = Path.Combine(packageInstallPath, folderNuGetProject.PackagePathResolver.GetPackageFileName(packageIdentity));
            var testNuGetProjectContext = new TestNuGetProjectContext();
            using (var targetStream = new MemoryStream())
            {
                await PackageDownloader.GetPackageStream(downloadUrl, targetStream);
                folderNuGetProject.InstallPackage(packageIdentity, targetStream, testNuGetProjectContext);
            }

            // Assert
            Assert.True(File.Exists(nupkgFilePath));
            using (var packageStream = File.OpenRead(nupkgFilePath))
            {
                var zipArchive = new ZipArchive(packageStream);
                Assert.Equal(5, zipArchive.Entries.Count);
            }

            // Clean-up
            TestFilesystemUtility.DeleteRandomTestFolders(randomTestFolder);
        }
    }
}
