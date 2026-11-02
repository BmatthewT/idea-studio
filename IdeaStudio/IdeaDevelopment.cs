using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
namespace IdeaStudio;
public partial class MainWindow
{
    async Task DevelopIdea()
    {
        if(busy)return;Capture();
        if(string.IsNullOrWhiteSpace(model)){MessageBox.Show(this,"Set an installed Ollama model in Settings first. Idea development needs a local AI model.","Develop idea");return;}
        busy=true;Page.IsEnabled=false;Next.IsEnabled=false;Back.IsEnabled=false;
        generation=new CancellationTokenSource(TimeSpan.FromMinutes(5));Status.Text="Developing proposed features, workflow, and logic with local AI…";
        try{
            using var client=new HttpClient{Timeout=TimeSpan.FromMinutes(5)};
            var request=new{model,stream=false,format="json",messages=new[]{
                new{role="system",content="Develop a rough idea into a concrete proposed brief. Return JSON string fields Audience, Outcome, Features (one per line, maximum 15), UserJourney, AcceptanceMeasures, TechnicalDecisions. Respect supplied details and target. Fill unknown details with explicitly labeled proposals, never claim user approval. For coding propose MVP versus optional features, a coherent user journey, core algorithm in numbered steps/pseudocode, data model, failure handling, measurable checks, and screen/design recommendations. Do not require the user to know algorithms or technology. For ordinary tasks propose useful content, format, steps and checks without software jargon. Do not invent repository links or claim browsing. Use the language of the idea."},
                new{role="user",content=JsonSerializer.Serialize(new{project.Idea,project.Purpose,project.Platform,project.Design,project.Audience,project.Outcome,project.Features,project.Constraints,project.UserJourney,project.AcceptanceMeasures,project.TechnicalDecisions})}}};
            using var response=await client.PostAsJsonAsync("http://localhost:11434/api/chat",request,generation.Token);response.EnsureSuccessStatusCode();
            using var envelope=JsonDocument.Parse(await response.Content.ReadAsStringAsync(generation.Token));
            var proposal=JsonSerializer.Deserialize<Project>(envelope.RootElement.GetProperty("message").GetProperty("content").GetString()??"",Composer.Json)??throw new InvalidOperationException("Empty proposal.");
            Composer.Validate(proposal);
            if(new[]{proposal.Audience,proposal.Outcome,proposal.Features,proposal.UserJourney,proposal.AcceptanceMeasures,proposal.TechnicalDecisions}.Any(string.IsNullOrWhiteSpace)||Composer.Features(proposal).Length>30)throw new InvalidOperationException("Incomplete proposal. Try another model or retry.");
            var preview=$"AI suggestions, not confirmed requirements. Apply only to currently empty fields? You can edit them before generation.\n\nAudience: {proposal.Audience}\n\nOutcome: {proposal.Outcome}\n\nFeatures:\n{proposal.Features}\n\nWorkflow:\n{proposal.UserJourney}\n\nChecks:\n{proposal.AcceptanceMeasures}\n\nLogic and decisions:\n{proposal.TechnicalDecisions}";
            if(MessageBox.Show(this,preview,"Review idea suggestions",MessageBoxButton.YesNo)==MessageBoxResult.Yes){
                if(string.IsNullOrWhiteSpace(project.Audience))project.Audience=proposal.Audience;
                if(string.IsNullOrWhiteSpace(project.Outcome))project.Outcome=proposal.Outcome;
                if(string.IsNullOrWhiteSpace(project.Features))project.Features=proposal.Features;
                if(string.IsNullOrWhiteSpace(project.UserJourney))project.UserJourney=proposal.UserJourney;
                if(string.IsNullOrWhiteSpace(project.AcceptanceMeasures))project.AcceptanceMeasures=proposal.AcceptanceMeasures;
                if(string.IsNullOrWhiteSpace(project.TechnicalDecisions))project.TechnicalDecisions=proposal.TechnicalDecisions;
                dirty=true;Status.Text="Suggestions applied to empty fields. Review and edit before continuing.";
            }else Status.Text="Suggestions discarded. Your brief is unchanged.";
        }catch(Exception ex){Status.Text="Idea development failed: "+ex.Message+" Your brief is unchanged.";}
        finally{busy=false;generation?.Dispose();generation=null;Page.IsEnabled=true;Render();}
    }
}
