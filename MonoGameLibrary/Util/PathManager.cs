using System.IO;

namespace MonoGameLibrary.Util;

public static class PathManager
{
    public static readonly string RootDir = "../../../../";
    public static readonly string LibDir = Path.Join(RootDir, Constants.Util.PathManager.LibName + "/");
    public static readonly string LibDataDir = Path.Join(LibDir, "Data/");
    public static readonly string ProjectDir = Path.Join(RootDir, Constants.Util.PathManager.ProjectName + "/");
    public static readonly string ProjectDataDir = Path.Join(ProjectDir, "Data/");
}