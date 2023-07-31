using Nuke.Common.CI.GitHubActions;

public class CustomBuildCmdPathGitHubActionsAttribute : GitHubActionsAttribute
{
    public CustomBuildCmdPathGitHubActionsAttribute(
        string name,
        GitHubActionsImage image,
        params GitHubActionsImage[] images) : base(name, image, images)
    {
    }

    protected override string BuildCmdPath => Build.RootDirectory / "Tools" / "NukeBuild" / "build.cmd";
}