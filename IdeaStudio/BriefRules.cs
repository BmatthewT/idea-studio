using System.Text.RegularExpressions;

namespace IdeaStudio;
public static class BriefRules
{
    public static readonly string[] Platforms = ["Website", "Android APK", "Windows application", "Other software"];
    public static string[] DetectPlatforms(string idea)
    {
        var matches = new List<string>();
        if (Regex.IsMatch(idea, @"\b(android|apk)\b", RegexOptions.IgnoreCase)) matches.Add("Android APK");
        if (Regex.IsMatch(idea, @"\b(website|web\s*app(?:lication)?|situs\s*web|halaman\s*web)\b", RegexOptions.IgnoreCase)) matches.Add("Website");
        if (Regex.IsMatch(idea, @"\b(windows|winui|wpf|\.exe)\b", RegexOptions.IgnoreCase)) matches.Add("Windows application");
        return matches.ToArray();
    }
    public static string RecommendedDesign(string platform) => platform switch { "Android APK" => "Material Android", "Windows application" => "Fluent workspace", "Website" => "Radix essentials", _ => "Editorial studio" };
    public static bool Compatible(string platform, string design) => platform switch {
        "Android APK" => design is "Material Android" or "Editorial studio",
        "Windows application" => design is "Fluent workspace" or "Editorial studio",
        "Website" => design != "Material Android", _ => true
    };
    public static List<string> Issues(Project p)
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(p.Idea)) issues.Add("Describe the idea / Tuliskan ide.");
        if (string.IsNullOrWhiteSpace(p.Audience)) issues.Add("Audience or learning level is required / Isi pengguna atau tingkat belajar.");
        if (string.IsNullOrWhiteSpace(p.Outcome)) issues.Add("A success criterion is required / Isi hasil yang ingin dicapai.");
        if (Composer.Features(p).Length == 0) issues.Add("Add at least one requirement / Isi minimal satu kebutuhan.");
        if (Composer.Features(p).Length > 30) issues.Add("Use at most 30 requirements / Maksimal 30 kebutuhan.");
        if (new[] { p.Idea, p.Audience, p.Outcome, p.Features, p.Constraints, p.DesignNotes }.Any(s => s.Contains("[To confirm:", StringComparison.OrdinalIgnoreCase))) issues.Add("Replace leftover [To confirm: ...] placeholders with actual details.");
        if (p.Purpose == "Coding")
        {
            var detected = DetectPlatforms(p.Idea);
            if (!Platforms.Contains(p.Platform)) issues.Add("Choose a target platform / Pilih platform tujuan.");
            if (detected.Length > 1) issues.Add("The idea mentions multiple platforms. This brief supports one target: edit the idea to name only the intended platform.");
            else if (detected.Length == 1 && detected[0] != p.Platform) issues.Add($"Platform conflict: the idea names {detected[0]}, but the selected target is {p.Platform}. Change the target or edit the idea.");
            if (!Compatible(p.Platform, p.Design)) issues.Add($"{p.Design} is not a native {p.Platform} preset. Choose {RecommendedDesign(p.Platform)} or Editorial studio.");
        }
        return issues;
    }
    public static void EnsureReady(Project p)
    {
        var issues = Issues(p);
        if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues));
    }
}
