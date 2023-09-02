namespace CustomModManager.Mod.Version
{
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

    public interface IModVersion 
    {
        string ToString();

        bool TryGetBuildMetadata(out string buildMetadata);

        EVersionUpdateComparisonResult GetVersionUpdateComparisonResult();
        EVersionComparisonResult GetVersionComparisonResult();
    }
}
