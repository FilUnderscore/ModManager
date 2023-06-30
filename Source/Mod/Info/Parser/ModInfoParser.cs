namespace CustomModManager.Mod.Info.Parser
{
    public interface IModInfoParser
    {
        bool TryParse(out ModInfo modInfo);
    }
}
