using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using NuGet;
using Splat;

namespace Squirrel
{
    public sealed partial class UpdateManager : IUpdateManager, IEnableLogger
    {
        readonly string rootAppDirectory;
        readonly string applicationName;
        readonly IFileDownloader urlDownloader;
        readonly string updateUrlOrPath;
        readonly FrameworkVersion appFrameworkVersion;

        IDisposable updateLock;

        public UpdateManager(string urlOrPath, 
            string applicationName,
            FrameworkVersion appFrameworkVersion,
            string rootDirectory = null,
            IFileDownloader urlDownloader = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(urlOrPath));
            Contract.Requires(!String.IsNullOrEmpty(applicationName));

            updateUrlOrPath = urlOrPath;
            this.applicationName = applicationName;
            this.appFrameworkVersion = appFrameworkVersion;

            this.rootAppDirectory = Path.Combine(rootDirectory ?? getLocalAppDataDirectory(), applicationName);

            this.urlDownloader = urlDownloader ?? new FileDownloader();
        }

        public async Task<UpdateInfo> CheckForUpdate(bool ignoreDeltaUpdates = false, Action<int> progress = null)
        {
            var checkForUpdate = new CheckForUpdateImpl(rootAppDirectory);

            await acquireUpdateLock();
            return await checkForUpdate.CheckForUpdate(Utility.LocalReleaseFileForAppDir(rootAppDirectory), updateUrlOrPath, ignoreDeltaUpdates, progress, urlDownloader);
        }

        public async Task DownloadReleases(IEnumerable<ReleaseEntry> releasesToDownload, Action<int> progress = null)
        {
            var downloadReleases = new DownloadReleasesImpl(rootAppDirectory);
            await acquireUpdateLock();

            await downloadReleases.DownloadReleases(updateUrlOrPath, releasesToDownload, progress, urlDownloader);
        }

        public async Task ApplyReleases(UpdateInfo updateInfo, Action<int> progress = null)
        {
            var applyReleases = new ApplyReleasesImpl(rootAppDirectory);
            await acquireUpdateLock();

            await applyReleases.ApplyReleases(updateInfo, false, progress);
        }

        public async Task FullInstall(bool silentInstall = false)
        {
            var updateInfo = await CheckForUpdate();
            await DownloadReleases(updateInfo.ReleasesToApply);

            var applyReleases = new ApplyReleasesImpl(rootAppDirectory);
            await acquireUpdateLock();

            await applyReleases.ApplyReleases(updateInfo, silentInstall);
        }

        public async Task FullUninstall()
        {
            var applyReleases = new ApplyReleasesImpl(rootAppDirectory);
            await acquireUpdateLock();

            await applyReleases.FullUninstall();
        }

        const string uninstallRegSubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        public async Task<RegistryKey> CreateUninstallerRegistryEntry(string uninstallCmd, string quietSwitch)
        {
            var releases = ReleaseEntry.ParseReleaseFile(Path.Combine(rootAppDirectory, "packages", "RELEASES"));
            var latest = releases.OrderByDescending(x => x.Version).First();

            // Download the icon and PNG => ICO it. If this doesn't work, who cares
            var pkgPath = Path.Combine(rootAppDirectory, "packages", latest.Filename);
            var zp = new ZipPackage(pkgPath);
                
            var targetPng = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            var targetIco = Path.Combine(rootAppDirectory, "app.ico");
            try {
                var wc = new WebClient();

                await wc.DownloadFileTaskAsync(zp.IconUrl, targetPng);
                using (var fs = new FileStream(targetIco, FileMode.Create)) 
                using (var bmp = (Bitmap)Image.FromFile(targetPng))
                using (var ico = Icon.FromHandle(bmp.GetHicon())) {
                    ico.Save(fs);
                }
            } catch(Exception ex) {
                this.Log().InfoException("Couldn't write uninstall icon, don't care", ex);
            } finally {
                File.Delete(targetPng);
            }

            var stringsToWrite = new[] {
                new { Key = "DisplayIcon", Value = "targetIco" },
                new { Key = "DisplayName", Value = zp.Description ?? zp.Summary },
                new { Key = "DisplayVersion", Value = zp.Version.ToString() },
                new { Key = "InstallDate", Value = DateTime.Now.ToString("yyyymmdd") },
                new { Key = "InstallLocation", Value = rootAppDirectory },
                new { Key = "Publisher", Value = zp.Authors.First() },
                new { Key = "QuietUninstallString", Value = String.Format("{0} {1}", uninstallCmd, quietSwitch) },
                new { Key = "UninstallString", Value = uninstallCmd },
            };

            var dwordsToWrite = new[] {
                new { Key = "EstimatedSize", Value = (int)((new FileInfo(pkgPath)).Length / 1024) },
                new { Key = "NoModify", Value = 1 },
                new { Key = "NoRepair", Value = 1 },
                new { Key = "Language", Value = 0x0409 },
            };

            var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
                .CreateSubKey(uninstallRegSubKey + "\\" + applicationName, RegistryKeyPermissionCheck.ReadWriteSubTree);

            foreach (var kvp in stringsToWrite) {
                key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.String);
            }
            foreach (var kvp in dwordsToWrite) {
                key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.DWord);
            }

            return key;
        }

        public void Dispose()
        {
            var disp = Interlocked.Exchange(ref updateLock, null);
            if (disp != null) {
                disp.Dispose();
            }
        }

        ~UpdateManager()
        {
            if (updateLock != null) {
                throw new Exception("You must dispose UpdateManager!");
            }
        }

        Task<IDisposable> acquireUpdateLock()
        {
            if (updateLock != null) return Task.FromResult(updateLock);

            return Task.Run(() => {
                // TODO: We'll bring this back later
                var key = Utility.CalculateStreamSHA1(new MemoryStream(Encoding.UTF8.GetBytes(rootAppDirectory)));
                var theLock = Disposable.Create(() => { });

                /*
                IDisposable theLock;
                try {
                    theLock = RxApp.InUnitTestRunner() ?
                        Disposable.Empty : new SingleGlobalInstance(key, 2000);
                } catch (TimeoutException) {
                    throw new TimeoutException("Couldn't acquire update lock, another instance may be running updates");
                }
                */

                var ret = Disposable.Create(() => {
                    theLock.Dispose();
                    updateLock = null;
                });

                updateLock = ret;
                return ret;
            });
        }

        static string getLocalAppDataDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
    }
}