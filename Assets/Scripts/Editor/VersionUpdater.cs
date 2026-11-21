namespace FrogGame.Editor
{
    using System;
    using System.Diagnostics;
    using UnityEditor;
    using Debug = UnityEngine.Debug;

    public static class VersionUpdater
    {
        private enum UpdateType
        {
            Major,
            Minor,
            Patch
        }

        private const string MENU_NAME = "Version Updater";
        private const string TARGET_BRANCH = "main";

        private static int[] GetCurrentVersionNumbers()
        {
            string currentVersion = PlayerSettings.bundleVersion;
            string[] versionParts = currentVersion.Split('.');

            if (versionParts.Length != 3)
            {
                throw new InvalidOperationException($"Current version '{currentVersion}' does not match 'major.minor.patch' format.");
            }

            return new int[]
            {
                Convert.ToInt32(versionParts[0]),
                Convert.ToInt32(versionParts[1]),
                Convert.ToInt32(versionParts[2])
            };
        }

        private static string GetIncreasedBundleVersion(UpdateType updateType)
        {
            int[] version = GetCurrentVersionNumbers();

            switch (updateType)
            {
                case UpdateType.Patch:
                    version[2]++;
                    break;

                case UpdateType.Minor:
                    version[1]++;
                    version[2] = 0;
                    break;

                case UpdateType.Major:
                    version[0]++;
                    version[1] = 0;
                    version[2] = 0;
                    break;
            }

            return string.Join('.', version);
        }

        private static void TrySetVersion(UpdateType updateType)
        {
            try
            {
                if (!ValidateCurrentBranch(TARGET_BRANCH))
                {
                    throw new InvalidOperationException($"Cannot change version on a branch other than '{TARGET_BRANCH}'.");
                }

                string currentVersion = PlayerSettings.bundleVersion;
                string newVersion = GetIncreasedBundleVersion(updateType);
                int currentBundleVersionCode = PlayerSettings.Android.bundleVersionCode;
                int newBundleVersionCode = currentBundleVersionCode + 1;

                string title = $"{updateType} update!";
                string message = $"Are you sure you want to increment the product version?{Environment.NewLine}" +
                                 $"WARNING: This action will commit, tag, and push the change to version control.{Environment.NewLine}" +
                                 $"{Environment.NewLine}" +
                                 $"Application version: {currentVersion} → {newVersion}{Environment.NewLine}" +
                                 $"Bundle version code: {currentBundleVersionCode} → {newBundleVersionCode}";

                if (EditorUtility.DisplayDialog(title, message, "Yes", "No"))
                {
                    UpdateAppVersion(newVersion, newBundleVersionCode);
                    CommitTagAndPushVersionChange(newVersion, newBundleVersionCode);

                    EditorUtility.DisplayDialog("Success!", "The product version has increased and is ready for a build.", "Ok");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[VersionUpdater] Failed: {exception.Message}");
                EditorUtility.DisplayDialog("Error!", $"The product version could not be changed: {exception.Message}", "Ok");
            }
        }

        private static bool ValidateCurrentBranch(string targetBranch)
        {
            string currentBranch = RunCmdCommand("git rev-parse --abbrev-ref HEAD")?.Trim();

            return string.Equals(currentBranch, targetBranch, StringComparison.OrdinalIgnoreCase);
        }

        private static void UpdateAppVersion(string newVersion, int newBundleVersionCode)
        {
            PlayerSettings.bundleVersion = newVersion;
            PlayerSettings.Android.bundleVersionCode = newBundleVersionCode;
            AssetDatabase.SaveAssets();
        }

        private static void CommitTagAndPushVersionChange(string newVersion, int newBundleVersionCode)
        {
            string gitCommands = $"git add \"ProjectSettings/ProjectSettings.asset\" && " +
                                 $"git commit -m \"Update version to {newVersion} (bundleVersionCode: {newBundleVersionCode})\" && " +
                                 $"git tag {newVersion} && " +
                                 $"git push && " +
                                 $"git push --tags";

            RunCmdCommand(gitCommands);
        }

        private static string RunCmdCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{command}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(processStartInfo);

            if (process == null) return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        [MenuItem("My tools/" + MENU_NAME + "/New version/New patch")]
        private static void SetNewPatch() => TrySetVersion(UpdateType.Patch);

        [MenuItem("My tools/" + MENU_NAME + "/New version/New minor")]
        private static void SetNewMinor() => TrySetVersion(UpdateType.Minor);

        [MenuItem("My tools/" + MENU_NAME + "/New version/New major")]
        private static void SetNewMajor() => TrySetVersion(UpdateType.Major);
    }
}