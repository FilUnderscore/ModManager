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
            return this.versionInformation.LongStringNoBuild;
        }

        public bool TryGetBuildMetadata(out string buildMetadata)
        {
            buildMetadata = $"b{this.versionInformation.Build}";
            return true;
        }
    }
}
