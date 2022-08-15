using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomModManager
{
    public class ModManifest
    {
        public ModManifest RemoteManifest;

        public SemVer Version;
        public List<string> Dependencies;
        public VersionInformation GameVersionInformation;
        public SortedDictionary<SemVer, string> PatchNotes;

        public EVersionStatus UpToDate()
        {
            if (Version == null || RemoteManifest == null || RemoteManifest.Version == null)
                return EVersionStatus.Not_Specified;

            return Version.CompareTo(RemoteManifest.Version) >= 0 ? EVersionStatus.Up_To_Date : EVersionStatus.Not_Up_To_Date;
        }

        public EVersionComparisonResult CurrentVersionCompatibleWithGameVersion()
        {
            if (GameVersionInformation == null)
                return EVersionComparisonResult.Not_Specified;

            VersionInformation currentVersion = Constants.cVersionInformation;

            return currentVersion.CompareTo(GameVersionInformation) == 0 ? EVersionComparisonResult.Compatible : EVersionComparisonResult.Not_Compatible;
        }

        public EVersionUpdateComparisonResult CurrentVersionCompatibleWithDifferentGameVersion()
        {
            if (GameVersionInformation == null)
                return EVersionUpdateComparisonResult.Not_Specified;

            VersionInformation currentVersion = Constants.cVersionInformation;

            if(currentVersion.CompareTo(GameVersionInformation) != 0)
            {
                if (RemoteManifest != null && RemoteManifest.GameVersionInformation.CompareTo(GameVersionInformation) > 0)
                    return EVersionUpdateComparisonResult.Not_Compatible;
                else if (GameVersionInformation.CompareToRunningBuild() != VersionInformation.EVersionComparisonResult.SameMinor)
                    return EVersionUpdateComparisonResult.May_Not_Be_Compatible;
            }

            return EVersionUpdateComparisonResult.Compatible;
        }

        public enum EVersionComparisonResult
        {
            Compatible,
            Not_Compatible,
            Not_Specified
        }

        public enum EVersionUpdateComparisonResult
        {
            Compatible,
            May_Not_Be_Compatible,
            Not_Compatible,
            Not_Specified
        }

        public enum EVersionStatus
        {
            Up_To_Date,
            Not_Up_To_Date,
            Not_Specified
        }

        public class SemVer : IComparable
        {
            private const string SEM_VER_REGEX = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
            public readonly int Major, Minor, Patch;
            public readonly string Prerelease, BuildMetadata;

            private SemVer(int major, int minor, int patch, string prerelease, string buildMetadata)
            {
                this.Major = major;
                this.Minor = minor;
                this.Patch = patch;
                this.Prerelease = prerelease;
                this.BuildMetadata = buildMetadata;
            }

            public static SemVer Parse(string version)
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

                return new SemVer(majorVer, minorVer, patchVer, prerelease, buildMetadata);
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

                for(int index = 0; index < prereleaseSplit.Length; index++)
                {
                    string prereleaseCut = prereleaseSplit[index];
                    string otherPrereleaseCut = otherPrereleaseSplit[index];

                    for(int cutIndex = 0; cutIndex < prereleaseCut.Length; cutIndex++)
                    {
                        comparison = prereleaseCut[cutIndex].CompareTo(otherPrereleaseCut[cutIndex]);

                        if (comparison != 0)
                            return comparison;
                    }
                }

                return 0;
            }

            public override string ToString()
            {
                return $"{Major}.{Minor}.{Patch}" + (Prerelease.Length > 0 ? $"-{Prerelease}" : "") + (BuildMetadata.Length > 0 ? $"+{BuildMetadata}" : "");
            }
        }
    }
}
