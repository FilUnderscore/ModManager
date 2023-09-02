namespace CustomModManager.Mod.Version
{
    public sealed class GameVersion : IModVersion
    {
        private readonly VersionInformation versionInformation;

        public GameVersion(VersionInformation versionInformation)
        {
            this.versionInformation = versionInformation;
        }

        public EVersionComparisonResult GetVersionComparisonResult()
        {
            return EVersionComparisonResult.Compatible;
        }

        public EVersionUpdateComparisonResult GetVersionUpdateComparisonResult()
        {
            return EVersionUpdateComparisonResult.Compatible;
        }

        public override string ToString()
        {
            return this.versionInformation.LongString;
        }

        public bool TryGetBuildMetadata(out string buildMetadata)
        {
            buildMetadata = null;
            return false;
        }
    }
}
