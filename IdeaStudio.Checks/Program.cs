using IdeaStudio;
using System.Text.Json;
using System.Xml.Linq;
int count = 0;
void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); count++; }
foreach (var platform in new[] { "Website", "Android APK", "Windows application" })
{
    var p = new Project { Idea = "Plan revision around exams", Platform = platform, Audience = "University students", Outcome = "Build a weekly plan in 2 minutes", Features = "Add exams\nSchedule revision\nTrack progress", Constraints = "Offline only", Design = "Carbon console", DesignNotes = "Readable compact layout" };
    p.Design = BriefRules.RecommendedDesign(p.Platform); Composer.Generate(p);
    Check(p.Prompt.Contains(platform) && p.Prompt.Contains(p.Audience) && p.Prompt.Contains(p.Constraints), platform + " context propagated");
    Check(p.Prd.Contains("AC-03") && p.Prd.Contains("Track progress") && p.Prompt.Contains(p.Design), platform + " requirements and design");
    Check(p.Branches.SelectMany(b => b.Items).Contains("Track progress"), platform + " map scope");
    var copy = JsonSerializer.Deserialize<Project>(JsonSerializer.Serialize(p, Composer.Json), Composer.Json)!;
    Composer.Validate(copy); Check(copy.Prompt == p.Prompt && copy.Branches.Count == 6, platform + " project round trip");
}
foreach (var purpose in new[] { "Homework", "Straightforward task", "Something else" })
{
    var p = new Project { Idea = "Explain photosynthesis", Purpose = purpose, Audience = "Students", Outcome = "Explain three stages", Features = "Explain the process" }; p.Design = BriefRules.RecommendedDesign(p.Platform); Composer.Generate(p);
    Check(!p.Prompt.Contains("## Design direction") && !p.Prd.Contains("Platform:"), purpose + " omits software design");
    Check(!p.Prompt.Contains("To confirm"), purpose + " flags missing context");
    if (purpose == "Homework") Check(p.Prompt.Contains("patient tutor") && p.Prompt.Contains("practice exercise"), "Homework learning support");
}
var special = new Project { Idea = "<script>alert('x')</script> & 中文 café " + new string('X', 250), Features = "A & B\n<feature>", DesignNotes = "Long notes " + new string('Z', 300) }; special.Audience="Students";special.Outcome="Understand the topic";Composer.Generate(special);
var svg = Composer.Svg(special); var xml = XDocument.Parse(svg);
Check(!xml.Descendants().Any(e => e.Name.LocalName == "script"), "SVG safely escapes markup");
Check(xml.Descendants().Any(e => e.Value.Contains("中文")), "SVG preserves Unicode");
Check(Composer.MindMarkdown(special).Contains("<feature>"), "Map outline retains scope");
bool invalid = false; try { Composer.Validate(new Project { Branches = null! }); } catch (InvalidDataException) { invalid = true; }
Check(invalid, "Invalid project rejected");
invalid = false; try { Composer.Validate(new Project { Branches = [new Branch { Items = [null!] }] }); } catch (InvalidDataException) { invalid = true; }
Check(invalid, "Invalid map node rejected");
Console.WriteLine($"{count} checks passed.");
var conflict = new Project { Idea="An Android app for revision", Platform="Website", Audience="Students", Outcome="Create a plan in 2 minutes", Features="Add exams", Design="Fluent workspace" };
Check(BriefRules.Issues(conflict).Any(s=>s.Contains("Platform conflict")), "Android versus Website conflict blocked");
conflict.Platform="Android APK";
Check(BriefRules.Issues(conflict).Any(s=>s.Contains("not a native")), "Fluent Android preset blocked");
conflict.Design="Material Android";Composer.Generate(conflict);
Check(conflict.Prompt.Contains("Target: Android APK")&&!conflict.Prompt.Contains("Target: Website")&&!conflict.Prompt.Contains("[To confirm:"),"Corrected Android prompt has consistent target and no placeholders");
Check(BriefRules.Issues(new Project()).Count>=4,"Incomplete brief blocked");
Check(BriefRules.DetectPlatforms("Build an Android app and a website").Length==2,"Multiple targets detected");
Console.WriteLine($"FINAL: {count} checks passed.");
Check(conflict.Prompt.Contains("ask at most three") && conflict.Prompt.Contains("STOP"),"Clarification phase stops before implementation");
Check(conflict.Prompt.Contains("Never claim successful builds"),"Execution honesty rule included");
conflict.UserJourney="Open planner, add exam, view saved exam";conflict.AcceptanceMeasures="Start on home screen; stop when saved exam appears";conflict.TechnicalDecisions="Local storage, no accounts";Composer.Generate(conflict);
Check(conflict.Prompt.Contains(conflict.UserJourney)&&conflict.Prd.Contains(conflict.AcceptanceMeasures)&&conflict.Prompt.Contains(conflict.TechnicalDecisions),"Authored workflow, checks, and decisions propagate");
var restored=JsonSerializer.Deserialize<Project>(JsonSerializer.Serialize(conflict))!;
Check(restored.UserJourney==conflict.UserJourney && restored.AcceptanceMeasures==conflict.AcceptanceMeasures,"New brief details survive project round trip");
Console.WriteLine($"FINAL REVIEW CHECKS: {count} passed.");

if(args.Contains("--sample")) File.WriteAllText("work/claude-sample.md", conflict.Prompt);
foreach(var idea in new[]{"Write a polite meeting invitation", "Summarize a comparison of Android and Windows", "Plan a weekly study timetable"})
{
    var task=new Project {Idea=idea,Purpose="Straightforward task",Platform="Android APK",Design="Fluent workspace",Audience="Classmates",Outcome="A concise usable result",Features="Include the important points\nUse clear language"};
    Check(BriefRules.Issues(task).Count==0,"Ordinary task ignores software platform mismatch: "+idea);
    Composer.Generate(task);
    Check(new[]{"implementation","builds","Kotlin","deployment","## Design direction","## Technical decisions"}.All(s=>!task.Prompt.Contains(s,StringComparison.OrdinalIgnoreCase)),"No software instructions in ordinary task: "+idea);
    Check(task.Prd.StartsWith("# Task brief") && task.Prompt.Contains("finished deliverable"),"Task brief and completion instruction generated");
    var saved=JsonSerializer.Deserialize<Project>(JsonSerializer.Serialize(task,Composer.Json),Composer.Json)!;Composer.Validate(saved);
    Check(saved.Prompt==task.Prompt && XDocument.Parse(Composer.Svg(saved)).Root!=null && Composer.MindMarkdown(saved).Contains("Classmates"),"Task project and map export round trip");
}
Console.WriteLine($"TOTAL: {count} passed.");
Check(conflict.Prompt.Contains("## Logic and data specification")&&conflict.Prompt.Contains("pseudocode"),"Coding prompt requires algorithm and data specification");
Check(conflict.Prompt.Contains(Attribution.Credit)&&conflict.Prd.Contains(Attribution.Credit),"Prompt and PRD attribution");
Check(Composer.MindMarkdown(conflict).Contains(Attribution.Credit)&&Composer.Svg(conflict).Contains(Attribution.Credit),"Mind map export attribution");
Check(Attribution.Stamp(Attribution.Stamp("Document"))==Attribution.Stamp("Document"),"Attribution is idempotent");
Console.WriteLine($"FINAL TOTAL: {count} passed.");
var fixture=Path.GetFullPath(Path.Combine("work","git-check-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(fixture);
await GitReader.Run("git",fixture,"init");
File.WriteAllText(Path.Combine(fixture,"hello world.txt"),"first line\n");
await GitReader.Run("git",fixture,"add","--","hello world.txt");
var statusBefore=await GitReader.Run("git",fixture,"status","--porcelain");
var stagedRead=await GitReader.Read(fixture,true);
Check(stagedRead.Message.Contains("Add hello world.txt")&&stagedRead.Patch.Contains("+first line"),"Staged new file and path with spaces read correctly");
Check(await GitReader.Run("git",fixture,"status","--porcelain")==statusBefore,"Reading preserves Git status");
File.AppendAllText(Path.Combine(fixture,"hello world.txt"),"second line\n");File.WriteAllText(Path.Combine(fixture,"untracked.txt"),"not staged");
var unstagedRead=await GitReader.Read(fixture,false);
Check(unstagedRead.Patch.Contains("+second line")&&!unstagedRead.Message.Contains("untracked.txt"),"Unstaged diff excludes untracked files from commit draft");
Check((await GitReader.Read(fixture,true)).Patch==stagedRead.Patch,"Unstaged edits do not contaminate staged scope");
Check(GitReader.Compose("","")=="","Clean scope has no fake commit message");
Check(GitReader.Compose("R100\told.txt\tnew.txt\nD\tremoved.txt\n","").Contains("Rename old.txt → new.txt"),"Rename and delete summary parsing");
Console.WriteLine($"ALL CHECKS: {count} passed.");
