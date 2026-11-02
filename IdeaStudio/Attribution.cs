namespace IdeaStudio;
public static class Attribution
{
    public const string Credit="created by matthew";
    public static string Stamp(string text)=>text.Contains(Credit,StringComparison.Ordinal)?text:text.TrimEnd()+"\n\n---\n"+Credit+"\n";
    public static void Apply(Project p){p.Prompt=Stamp(p.Prompt);p.Prd=Stamp(p.Prd);}
}
