using CustomModManager.Mod.Manifest;
using System;
using System.Text.RegularExpressions;

namespace CustomModManager.Mod.Version
{
    public sealed class SemVer : IComparable, IModVersion
    {
        private const string SEM_VER_REGEX = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

        private readonly ModManifest manifest;
        public readonly int Major, Minor, Patch;
        public readonly string Prerelease, BuildMetadata;

        private SemVer(ModManifest manifest, int major, int minor, int patch, string prerelease, string buildMetadata)
        {
            this.manifest = manifest;
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Prerelease = prerelease;
            this.BuildMetadata = buildMetadata;
        }

        public static SemVer Parse(string version, ModManifest manifest = null)
        {
            Regex regex = new Regex(SEM_VER_REGEX);
            Match match = regex.Match(version);

            if (!match.Success)
                return null;

            string major = match.Groups["major"].Value;
            string minor = match.Groups["minor"].Value;
            string patch = match.Groups["patch"].Value;
            string prerelease = "";
            string buildMetadata = "";

            if (match.Groups.Count > 3)
                prerelease = match.Groups["prerelease"].Value;

            if (match.Groups.Count > 4)
                buildMetadata = match.Groups["buildmetadata"].Value;

            if (!int.TryParse(major, out int majorVer) ||
                !int.TryParse(minor, out int minorVer) ||
                !int.TryParse(patch, out int patchVer))
                return null;

            return new SemVer(manifest, majorVer, minorVer, patchVer, prerelease, buildMetadata);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is SemVer))
                return 1;

            SemVer other = (SemVer)obj;

            int comparison = this.Major.CompareTo(other.Major);

            if (comparison != 0)
                return comparison;

            comparison = this.Minor.CompareTo(other.Minor);

            if (comparison != 0)
                return comparison;

            comparison = this.Patch.CompareTo(other.Patch);

            if (comparison != 0)
                return comparison;

            string[] prereleaseSplit = Prerelease.Split('.');
            string[] otherPrereleaseSplit = other.Prerelease.Split('.');

            for (int index = 0; index < prereleaseSplit.Length; index++)
            {
                string prereleaseCut = prereleaseSplit[index];
                string otherPrereleaseCut = otherPrereleaseSplit[index];

                for (int cutIndex = 0; cutIndex < prereleaseCut.Length; cutIndex++)
                {
                    comparison = prereleaseCut[cutIndex].CompareTo(otherPrereleaseCut[cutIndex]);

                    if (comparison != 0)
                        return comparison;
                }
            }

            return 0;
        }

        public EVersionComparisonResult GetVersionComparisonResult()
        {
            return this.manifest != null ? this.manifest.CurrentVersionCompatibleWithGameVersion() : EVersionComparisonResult.Not_Specified;
        }

        public EVersionUpdateComparisonResult GetVersionUpdateComparisonResult()
        {
            return this.manifest != null ? this.manifest.CurrentVersionCompatibleWithDifferentGameVersion() : EVersionUpdateComparisonResult.Not_Specified;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}" + (Prerelease.Length > 0 ? $"-{Prerelease}" : "") + (BuildMetadata.Length > 0 ? $"+{BuildMetadata}" : "");
        }
    }
}
