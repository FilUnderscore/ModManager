namespace CustomModManager.Mod.Version
{
    public sealed class GameVersion : IModVersion
    {
        private readonly VersionInformation versionInformation;

        public GameVersion(VersionInformation versionInformation)
        {
            this.versionInformation = versionInformation;
        }

        public override string ToString()
        {
            return this.versionInformation.LongString;
        }
    }
}
