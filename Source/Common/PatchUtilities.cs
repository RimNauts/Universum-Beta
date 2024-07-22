namespace Universum.Common;

public static class PatchUtilities {
    public static bool Prepare(string methodName, object target, ref bool verboseError) {
        if (target is not null) return true;

        if (!verboseError) return false;
            
        Debugger.Log(
            key: "Universum.Error.FailedToPatch",
            prefix: $"{Mod.Manager.METADATA.Name}: ",
            args: [methodName],
            severity: Debugger.Severity.Error
        );
        verboseError = false;

        return false;
    }
}
