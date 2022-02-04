using System.Collections.Generic;
using UnityEngine;

namespace CustomModManager
{
    public class ModManifest
    {
        public ModManifest RemoteManifest;

        public string Version;
        public VersionInformation GameVersionInformation;
        public string UpdateUrl;
        public Dictionary<string, List<PatchNote>> PatchNotes;

        public readonly ModEntry entry;

        public ModManifest(ModEntry entry)
        {
            this.entry = entry;
        }

        public class PatchNote
        {
            public string Text;
            public Color Color;
        }

        public bool NewVersionAvailable()
        {
            return RemoteManifest != null && Version != null && RemoteManifest.Version != Version;
        }

        public bool UpToDate()
        {
            return RemoteManifest != null && Version != null && RemoteManifest.Version == Version;
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
    }
}
