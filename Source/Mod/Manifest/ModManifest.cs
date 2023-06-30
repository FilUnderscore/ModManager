using CustomModManager.Mod.Version;
using System.Collections.Generic;

namespace CustomModManager.Mod.Manifest
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
    }
}
