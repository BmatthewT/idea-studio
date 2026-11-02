using System.IO;
using System.Text;
using System.Text.Json;
using System.Security;

namespace IdeaStudio;

public sealed record Design(string Name, string Description, string Color, string Url, string Rules);
public static class Library
{
    public static readonly Design[] Designs = [
        new("Fluent workspace", "Familiar Windows tools with quiet surfaces and clear commands.", "#315D82", "https://github.com/microsoft/fluentui", "Use Segoe UI, layered neutral surfaces, clear command bars, keyboard navigation, and restrained selection color. Adapt to the platform; do not imitate web controls in a native app."),
        new("Radix essentials", "Compact, precise layouts for focused web products.", "#685D48", "https://github.com/radix-ui/themes", "Use a disciplined spacing scale, neutral panels, accessible focus rings, a single accent, and consistent interactive states. Build layout around the user's primary task."),
        new("Carbon console", "Dense information, strong type, and deliberate structure.", "#293E62", "https://github.com/carbon-design-system/carbon", "Use a clear grid, strong typographic hierarchy, compact data displays, explicit action labels, and complete loading, empty, and error states."),
        new("Mantine product", "Flexible forms and approachable product interfaces.", "#256E66", "https://github.com/mantinedev/mantine", "Use readable forms, contextual validation, deliberate spacing, accessible components, and a restrained palette. Prefer task-specific layouts over generic dashboards."),
        new("Editorial studio", "Warm paper, expressive headings, and space to think.", "#A34F32", "", "Use warm ivory surfaces, dark ink text, serif display headings paired with a readable system sans, generous document margins, and terracotta only for the primary action. Avoid decorative gradients, fake metrics, and repetitive cards."),
        new("Material Android", "Native Android patterns with Material 3 and touch-friendly controls.", "#A4C639", "https://github.com/android/compose-samples", "Use Kotlin and Jetpack Compose with Material 3 as a proposed native stack unless the user specifies another Android stack. Follow Android navigation and back behavior, touch targets of at least 48dp, accessible typography, adaptive layouts, and system insets. Do not substitute HTML pages or Windows controls for a native Android APK.")
    ];
}
public sealed class Project
{
    public int Version { get; set; } = 1;
    public string Idea { get; set; } = "";
    public string Purpose { get; set; } = "Coding";
    public string Platform { get; set; } = "Website";
    public string Audience { get; set; } = "";
    public string Outcome { get; set; } = "";
    public string Features { get; set; } = "";
    public string Constraints { get; set; } = "";
    public string Design { get; set; } = "Editorial studio";
    public string DesignNotes { get; set; } = "";
    public string Reference { get; set; } = "";
    public string UserJourney { get; set; } = "";
    public string AcceptanceMeasures { get; set; } = "";
    public string TechnicalDecisions { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string Prd { get; set; } = "";
    public List<Branch> Branches { get; set; } = [];
    public string Generation { get; set; } = "";
}
public sealed class Branch
{
    public string Title { get; set; } = "";
    public List<string> Items { get; set; } = [];
}
public static class Composer
{
    public static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public static string Required(string value, string question) => string.IsNullOrWhiteSpace(value) ? $"[To confirm: {question}]" : value.Trim();
    public static string[] Features(Project p) => p.Features.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    public static void Generate(Project p)
    {
        BriefRules.EnsureReady(p);
        if (p.Purpose != "Coding") { TaskComposer.Generate(p); Attribution.Apply(p); return; }
        var coding = p.Purpose == "Coding";
        var homework = p.Purpose == "Homework";
        var design = Library.Designs.FirstOrDefault(d => d.Name == p.Design) ?? Library.Designs[4];
        var audience = p.Audience.Trim();
        var outcome = p.Outcome.Trim();
        var constraints = string.IsNullOrWhiteSpace(p.Constraints) ? "No additional constraints were specified. Ask before making consequential budget, privacy, or deployment decisions." : p.Constraints.Trim();
        var features = Features(p);
        var scope = features.Length > 0 ? string.Join("\n", features.Select((f, i) => $"{i + 1}. {f}")) : "[To confirm: list the essential requirements before implementation.]";
        var designText = coding ? $"## Design direction\n{design.Name}: {design.Rules}\n" + (string.IsNullOrWhiteSpace(p.DesignNotes) ? "" : $"Personal direction: {p.DesignNotes.Trim()}\n") + $"Reference: {(string.IsNullOrEmpty(design.Url) ? "Original editorial direction" : design.Url)}\n" + (string.IsNullOrWhiteSpace(p.Reference) ? "" : $"Additional reference / notes: {p.Reference.Trim()}\n") + $"Treat references as inspiration, verify their licenses before reusing code or assets, and adapt patterns to {p.Platform}.\n" : "";
        var role = coding ? "a senior product engineer and thoughtful interface designer" : homework ? "a patient tutor who explains reasoning and checks understanding" : "a precise assistant who turns a goal into a usable result";
        p.Prompt = $"""
Act as {role}.

## Goal
{p.Idea.Trim()}

## Context
Purpose: {(coding ? "Software implementation for the end users below" : p.Purpose)}
{(coding ? "Target: " + p.Platform : "Deliverable: a response appropriate to the stated purpose")}
Audience: {audience}
Success: {outcome}

## Required scope
{scope}

## User journey and acceptance
{(string.IsNullOrWhiteSpace(p.UserJourney) ? "Clarify the starting state, user actions, required inputs, and visible final result before implementation." : p.UserJourney.Trim())}
{(string.IsNullOrWhiteSpace(p.AcceptanceMeasures) ? "For each requirement, propose: user action → observable result → failure or edge case. Label checks as proposals until confirmed. For time-based success, define timing start/stop, representative inputs, device conditions, and a repeatable measurement procedure. Do not invent step counts or measured results." : "Author-specified acceptance checks: " + p.AcceptanceMeasures.Trim())}
{(features.Length == 1 ? "Only one requirement is confirmed. Identify dependencies needed to achieve the success criterion. Put supporting feature suggestions in 'Proposed scope (not approved)'; do not implement these without confirmation." : "Keep checks traceable to confirmed requirements.")}

## Technical decisions
{(string.IsNullOrWhiteSpace(p.TechnicalDecisions) ? (coding ? "Clarify storage and retention, offline/network needs, accounts, permissions, and deployment before choosing dependencies. Consider notifications only if the workflow needs them. Do not infer sensitive-data collection from the audience label." : "Confirm sources, output format, and delivery constraints.") : p.TechnicalDecisions.Trim())}

## Constraints
{constraints}

## Logic and data specification
For the approved scope, define inputs, outputs, data entities and relationships, validation rules, state changes, and persistence needs. Write numbered algorithm steps and readable pseudocode for the core workflow. Explain important tradeoffs and relevant edge cases such as duplicate input, missing data, failed operations, and recovery. Keep algorithm choices consistent with the target platform and approved constraints. Do not add accounts, cloud services, tracking, or notifications unless required or explicitly approved.

## Design deliverable
Describe the primary screens, navigation, component states, typography, spacing, contrast, and responsive/adaptive behavior appropriate to this idea. Explain how the chosen reference supports the user's task; a repository link alone is not a design specification. Distinguish optional improvements from the MVP.

{designText}
## Working approach
1. Clarification phase: if material information is missing or contradictory, ask at most three prioritized questions and STOP. Do not implement in that response. If nothing material is missing, proceed.
2. Implementation phase: begin after answers arrive or the user explicitly authorizes stated assumptions. Separate confirmed requirements, proposed scope, and assumptions. Do not silently expand scope.
3. {(coding ? "Plan the smallest complete user journey, then implement it end to end. Reuse existing conventions and avoid speculative abstractions." : homework ? "Start with what the learner knows, explain one step at a time, and use hints before revealing a worked solution." : "Choose a clear structure suited to the audience, then complete each requirement with concrete details.")}
4. {(coding ? "Cover validation, keyboard use, accessible contrast, persistence where needed, and empty, loading, and failure states." : "Check facts, distinguish evidence from inference, and do not invent sources.")}
5. Write relevant checks. Run them only if an execution environment is available. Separate tests written, tests actually executed with observed results, and checks not performed. Never claim successful builds, tests, or measurements without execution evidence.

## Output contract
{(coding ? "Provide the implementation, setup and run instructions, relevant tests and their results, and known limitations. Include an acceptance checklist tied to the requirements." : homework ? "Provide a learning plan, explained steps, a practice exercise, and a short understanding check. Cite sources when external facts are needed." : "Provide the finished deliverable, supporting rationale where useful, and a concise checklist showing how it meets the goal.")}

## Completion check
The result serves {audience}, addresses every confirmed requirement, and demonstrates: {outcome}.
""";
        var acceptance = features.Length > 0 ? string.Join("\n", features.Select((f, i) => $"- AC-{i + 1:00}: Demonstrate \"{f}\" in the intended user journey. Confirm expected result and a relevant failure or edge case before implementation.")) : "- Define measurable acceptance criteria once the essential requirements are confirmed.";
        p.Prd = $"""
# {(coding ? "Product requirements" : "Project requirements")}

> Offline structured draft. Inputs are confirmed by the author; proposed workflow and checks still need review.

## 1. Problem and intent
{p.Idea.Trim()}

## 2. Audience and desired outcome
- Audience: {audience}
- Success criterion: {outcome}
- Purpose: {p.Purpose}
{(coding ? "- Platform: " + p.Platform : "")}

## 3. Required scope
{scope}

## 4. Proposed workflow
Author-provided journey: {(string.IsNullOrWhiteSpace(p.UserJourney) ? "Not specified; clarify before implementation." : p.UserJourney)}
Author-provided acceptance checks: {(string.IsNullOrWhiteSpace(p.AcceptanceMeasures) ? "Not specified; propose observable checks for confirmation." : p.AcceptanceMeasures)}
Technical decisions: {(string.IsNullOrWhiteSpace(p.TechnicalDecisions) ? "Clarify storage, connectivity, permissions, and delivery requirements." : p.TechnicalDecisions)}
{(coding ? "Discover the main action → enter required information → validate → complete the action → review the result → recover from errors. Refine this sequence to match the confirmed features." : homework ? "Establish current understanding → explain the concept → work through an example → practice independently → check understanding." : "Gather inputs → confirm expectations → produce the deliverable → review against requirements → revise.")}

## 5. Acceptance criteria to refine
{acceptance}
- The outcome is evaluated against: {outcome}

{designText}
## 6. Quality requirements
{(coding ? "- All visible actions work; feedback explains what happened and what to do next.\n- Keyboard access and readable contrast are verified.\n- Validate inputs and handle unavailable dependencies without losing user work.\n- Specify data retention and access requirements before collecting personal data.\n- Establish performance targets using representative devices and data." : "- The output suits the audience and the requested format.\n- Reasoning is clear and external claims have verifiable sources.\n- Uncertainty and missing information are explicit.")}

## 7. Constraints and exclusions
{constraints}

Proposed non-goals: features outside the confirmed scope, unrequested integrations, and unsupported claims of completeness.

## 8. Delivery plan
1. Resolve the open questions and agree on acceptance criteria.
2. Complete the primary workflow with representative inputs.
3. Verify requirements and relevant failure cases.
4. Review the result with the intended audience and document remaining gaps.

## 9. Risks and open questions
- Is the target audience sufficiently specific?
- Are the required inputs and permissions available?
- Are the success criteria measurable?
- Which constraints or dependencies could prevent delivery?
- Which requirements are assumptions rather than confirmed needs?
""";
        p.Branches = [new() { Title = "Purpose", Items = [p.Purpose, coding ? p.Platform : "Audience-appropriate output"] }, new() { Title = "People & outcome", Items = [audience, outcome] }, new() { Title = "Scope", Items = features.Length > 0 ? features.ToList() : ["Confirm essential requirements"] }, new() { Title = coding ? "Design" : "Approach", Items = coding ? [design.Name, design.Rules, p.DesignNotes] : [homework ? "Explain, practice, check understanding" : "Draft, review, revise"] }, new() { Title = "Constraints", Items = [constraints] }, new() { Title = "Validation", Items = ["Check each requirement", "Review assumptions", "Test relevant failure cases"] }];
        p.Generation = "Offline structured draft";
        Attribution.Apply(p);
    }
    public static void Validate(Project p)
    {
        if (p.Version != 1 || new[] { p.Idea, p.Purpose, p.Platform, p.Audience, p.Outcome, p.Features, p.Constraints, p.Design, p.DesignNotes, p.Reference, p.Prompt, p.Prd, p.Generation }.Any(s => s == null || s.Length > 200000) || p.Branches == null || p.Branches.Count > 12 || p.Branches.Any(b => b == null || b.Title == null || b.Title.Length > 1000 || b.Items == null || b.Items.Count > 30 || b.Items.Any(i => i == null || i.Length > 30000))) throw new InvalidDataException("This project contains unsupported or invalid data.");
    }
    public static string MindMarkdown(Project p) => "created by matthew\n\n# " + p.Idea.Replace('\n', ' ') + "\n\n" + string.Join("\n\n", p.Branches.Select(b => "## " + b.Title + "\n" + string.Join("\n", b.Items.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => "- " + i))));
    public static string Svg(Project p)
    {
        static string Esc(string s) => SecurityElement.Escape(s) ?? "";
        static IEnumerable<string> Wrap(string s, int max)
        {
            var line = "";
            foreach (var word in s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length + word.Length + 1 > max && line.Length > 0) { yield return line; line = ""; }
                for (var n = 0; n < word.Length; n += max) { var part = word.Substring(n, Math.Min(max, word.Length - n)); if (n > 0) { yield return line; line = ""; } line += (line.Length > 0 ? " " : "") + part; }
            }
            if (line.Length > 0) yield return line;
        }
        var title = Wrap(p.Idea, 90).ToArray();
        var y = 70 + title.Length * 25;
        var body = new StringBuilder();
        for (int i = 0; i < title.Length; i++) body.Append($"<text x='40' y='{45 + i * 25}' font-size='21' font-weight='600'>{Esc(title[i])}</text>");
        foreach (var branch in p.Branches)
        {
            var lines = branch.Items.Where(s => !string.IsNullOrWhiteSpace(s)).SelectMany(s => Wrap("• " + s, 100)).ToArray();
            var h = Math.Max(80, 52 + lines.Length * 23);
            body.Append($"<path d='M40  {y - 20} V{y + 25} H70' fill='none' stroke='#A34F32' stroke-width='2'/><rect x='70' y='{y}' width='880' height='{h}' rx='5' fill='white' stroke='#D5CCBF'/><text x='90' y='{y + 28}' font-size='18' font-weight='600'>{Esc(branch.Title)}</text>");
            for (int i = 0; i < lines.Length; i++) body.Append($"<text x='90' y='{y + 55 + i * 23}' font-size='15'>{Esc(lines[i])}</text>");
            y += h + 24;
        }
        body.Append($"<text x='40' y='{y + 5}' font-size='13'>created by matthew</text>"); return $"<svg xmlns='http://www.w3.org/2000/svg' width='990' height='{y + 20}' viewBox='0 0 990 {y + 20}'><rect width='100%' height='100%' fill='#F6F2EB'/><g font-family='Segoe UI, sans-serif' fill='#292B28'>{body}</g></svg>";
    }
}

