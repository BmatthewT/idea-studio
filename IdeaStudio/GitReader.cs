using System.Diagnostics;
using System.IO;
namespace IdeaStudio;
public static class GitReader
{
    public static async Task<string> Run(string executable,string? directory,params string[] arguments)
    {
        var start=new ProcessStartInfo(executable){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        if(directory!=null)start.WorkingDirectory=directory;
        foreach(var arg in arguments)start.ArgumentList.Add(arg);
        start.Environment["GIT_TERMINAL_PROMPT"]="0";start.Environment["GH_PROMPT_DISABLED"]="1";
        using var process=Process.Start(start)??throw new IOException("Could not start "+executable);
        var output=process.StandardOutput.ReadToEndAsync();var error=process.StandardError.ReadToEndAsync();
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try{await process.WaitForExitAsync(timeout.Token);}catch{try{process.Kill(true);}catch{}throw new IOException("Command timed out.");}
        var result=await output;var failure=await error;
        if(process.ExitCode!=0)throw new IOException(executable=="gh"?"GitHub login is unavailable or expired. Sign in and check again.":failure);
        if(result.Length>2_000_000)throw new IOException("Diff is too large. Stage a smaller set of changes.");
        return result;
    }
    public static async Task<(string Summary,string Patch,string Message)> Read(string folder,bool staged)
    {
        var root=(await Run("git",folder,"rev-parse","--show-toplevel")).Trim();
        var prefix=new List<string>{"-c","core.quotePath=true","diff","--no-ext-diff","--no-textconv"};if(staged)prefix.Add("--cached");
        var names=await Run("git",root,prefix.Concat(new[]{"--name-status","--"}).ToArray());
        var stats=await Run("git",root,prefix.Concat(new[]{"--stat","--"}).ToArray());
        var patch=await Run("git",root,prefix.Concat(new[]{"--unified=3","--"}).ToArray());
        var untracked=await Run("git",root,"ls-files","--others","--exclude-standard");
        var message=Compose(names,stats);
        return ($"{root}\nScope: {(staged?"Staged changes":"Unstaged tracked changes")}\n\n{names}\n{stats}\nUntracked files (not included in message or patch):\n{untracked}",patch,message);
    }
    public static string Compose(string names,string stats)
    {
        var lines=names.Split('\n',StringSplitOptions.RemoveEmptyEntries);
        if(lines.Length==0)return "";
        var details=lines.Select(line=>{var parts=line.TrimEnd('\r').Split('\t');var verb=parts[0][0] switch{'A'=>"Add",'D'=>"Remove",'R'=>"Rename",'C'=>"Copy",_=>"Update"};return "- "+verb+" "+string.Join(" → ",parts.Skip(1));});
        return $"chore: update {lines.Length} {(lines.Length==1?"file":"files")}\n\n"+string.Join("\n",details)+"\n\n"+stats.Trim();
    }
}
