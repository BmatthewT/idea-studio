# Idea Studio

<img src="IdeaStudio/Assets/logo.png" alt="Idea Studio logo" width="144" />

**created by matthew**

A native Windows workspace for developing ideas into prompts, requirements documents, and mind maps, with a Git commit-message helper.

## Features

- Opening menu: choose **Idea Studio** or **Commit Helper**.
- Guided briefs for software projects, everyday tasks, homework, and other goals.
- Platform checks and appropriate design presets for Android, Windows, and websites.
- Optional local Ollama assistance to propose features, workflows, logic, and acceptance checks; proposals are reviewed before filling empty fields.
- Editable prompts and requirements, with Markdown and SVG mind-map export.
- Local project save/open, black interface, short opening animation respecting Windows animation preferences, and author attribution.
- Settings with 100 machine-translated action-menu languages and regional flag images. Help paragraphs and some controls remain English; this is not full-interface localization.
- Commit Helper verifies the active github.com login, reads staged or unstaged tracked changes, and drafts an editable commit message locally. It does not stage, commit, push, or upload the diff.

## Requirements

- Windows x64.
- .NET 10 SDK for building from source; self-contained published builds include their runtime.
- Git and [GitHub CLI](https://cli.github.com/) for Commit Helper. Sign in using its Login GitHub button or `gh auth login --hostname github.com --web`.
- Optional [Ollama](https://ollama.com/) running locally with an installed text model. Enter its exact model name in Settings. No model is bundled or downloaded automatically.

## Run and test

From the repository root:

```powershell
dotnet run --project IdeaStudio
dotnet run --project IdeaStudio.Checks -c Release
```

The check runner covers document generation, purpose separation, platform conflicts, attribution, project round trips, SVG escaping, and actual temporary Git repositories. It creates test fixtures under ignored `work/`.

## Build a portable Windows app

```powershell
dotnet publish IdeaStudio -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o publish
```

Keep the `Locales` folder alongside `publish/IdeaStudio.exe`.

To create the installer, install [Inno Setup](https://jrsoftware.org/isinfo.php), publish as above, then run:

```powershell
ISCC.exe installer.iss
```

The installer appears in `dist/`. It installs per-user, creates Start Menu shortcuts, and includes an uninstaller. Builds are unsigned.

## Use

Enter an idea, choose its purpose, and supply audience, desired outcome, and required content. If you need help developing those details, set an Ollama model and use **Develop my idea**. Review its suggestions, choose a design for coding projects, then generate and edit the documents. Offline mode produces structured templates; it is not generative AI.

Commit Helper requires GitHub login before reading a selected local repository. Staged changes are the default. Untracked paths are listed separately, and their content is excluded. Its local message generator describes file operations with a generic `chore` subject; edit that subject to explain the actual intent. It does not infer semantic changes using AI.

## Data and limitations

Projects are saved only on request. Settings are stored at `%LOCALAPPDATA%/IdeaStudio/settings.json`. Ollama requests go to `http://localhost:11434`; GitHub authentication/account checks use the GitHub CLI. No credentials belong in this repository.

The design library is curated, not trained on or automatically crawled from repositories. The mind map is an outline/SVG, not a freeform node editor. Live Ollama generation and a full interactive GitHub sign-in walkthrough were not validated in the current release; automated checks do not guarantee AI output quality. github.com is supported; enterprise hosts are not configured.

## References and assets

- Design references: [Fluent UI](https://github.com/microsoft/fluentui), [Radix Themes](https://github.com/radix-ui/themes), [Carbon](https://github.com/carbon-design-system/carbon), [Mantine](https://github.com/mantinedev/mantine), and [Android Compose samples](https://github.com/android/compose-samples). Referencing a project does not imply endorsement.
- Menu translations: machine-generated using Google Translate; not reviewed in all languages.
- Regional flags: [FlagCDN](https://flagcdn.com/). Flags are visual aids, not exclusive representations of languages.
- Logo supplied by the project owner. Icons are resized derivatives of that logo.

No open-source license has been assigned by the owner yet.
