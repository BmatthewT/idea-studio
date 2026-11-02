using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
namespace IdeaStudio;
public partial class MainWindow
{
    void HomeClick(object sender,RoutedEventArgs e){if(busy)return;Capture();RenderHome();}
    void ModeControls(bool studio){NewMenu.Visibility=OpenMenu.Visibility=SaveMenu.Visibility=Steps.Visibility=Back.Visibility=Next.Visibility=studio?Visibility.Visible:Visibility.Collapsed;}
    void RenderHome()
    {
        fields.Clear();Page.Children.Clear();ModeControls(false);
        Heading("What would you like to do?", "Pilih alat yang ingin digunakan.");
        Page.Children.Insert(0,new Image { Source=new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/logo.png")), Width=96,Height=100,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,18) });
        Page.Children.Add(Text("Idea Studio",22));Page.Children.Add(Text("Kembangkan ide menjadi prompt, PRD, dan mind map."));
        Page.Children.Add(Button("Open Idea Studio",()=>{ModeControls(true);Render();}));
        Page.Children.Add(Text("\nCommit Helper",22));Page.Children.Add(Text("Baca perubahan Git dan buat draft commit message. Login GitHub diperlukan."));
        Page.Children.Add(Button("Open Commit Helper",RenderCommit));Status.Text="created by matthew";
    }
    void RenderCommit()
    {
        fields.Clear();Page.Children.Clear();ModeControls(false);
        Heading("Commit Helper", "GitHub login → pilih repository → baca perubahan → tinjau pesan commit.");
        Page.Children.Add(Text("Requires Git and GitHub CLI (gh). Diff stays on this computer. No commit, staging, or push is performed.",12));
        Page.Children.Add(Button("Login GitHub",()=>{
            var info=new ProcessStartInfo("gh"){UseShellExecute=true};foreach(var arg in new[]{"auth","login","--hostname","github.com","--web","--git-protocol","https"})info.ArgumentList.Add(arg);Process.Start(info);
            Status.Text="Complete GitHub sign-in in the opened terminal/browser, then check login below.";
        }));
        var account=Text("GitHub: not checked");Page.Children.Add(account);
        var check=new Button{Content="Check GitHub login",Margin=new Thickness(0,8,0,14)};Page.Children.Add(check);
        check.Click+=async(_,_)=>{try{account.Text="Checking…";var user=await GitReader.Run("gh",null,"api","--hostname","github.com","user","--jq",".login");account.Text="GitHub: "+user.Trim();}catch(Exception ex){account.Text=ex.Message;}};
        var folder=new TextBox{IsReadOnly=true,Margin=new Thickness(0,8,0,8)};Page.Children.Add(folder);
        Page.Children.Add(Button("Choose repository",()=>{var d=new OpenFolderDialog();if(d.ShowDialog(this)==true)folder.Text=d.FolderName;}));
        var staged=new CheckBox{Content="Read staged changes (recommended)",IsChecked=true,Margin=new Thickness(0,16,0,16)};Page.Children.Add(staged);
        var read=new Button{Content="Read changes & generate message"};Page.Children.Add(read);
        var summary=new TextBox{IsReadOnly=true,AcceptsReturn=true,Height=130,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Margin=new Thickness(0,12,0,12)};Page.Children.Add(summary);
        var patch=new TextBox{IsReadOnly=true,AcceptsReturn=true,Height=180,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Page.Children.Add(new Expander{Header="Inspect Git patch",Content=patch});
        Page.Children.Add(Text("Commit message · editable draft",17));
        var message=new TextBox{AcceptsReturn=true,Height=150,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Page.Children.Add(message);
        Page.Children.Add(Text("Local generation summarizes file operations, not semantic intent. Replace the generic subject with the reason for the change. Refresh after changing the repository or staging area.",12));
        Page.Children.Add(Button("Copy commit message",()=>{if(!string.IsNullOrWhiteSpace(message.Text)){Clipboard.SetText(message.Text);Status.Text="Commit message copied.";}}));
        read.Click+=async(_,_)=>{
            if(string.IsNullOrWhiteSpace(folder.Text)){Status.Text="Choose a Git repository first.";return;}
            busy=true;read.IsEnabled=false;message.Clear();patch.Clear();summary.Clear();Status.Text="Checking GitHub login and reading Git changes…";
            try{
                var user=await GitReader.Run("gh",null,"api","--hostname","github.com","user","--jq",".login");account.Text="GitHub: "+user.Trim();
                var result=await GitReader.Read(folder.Text,staged.IsChecked==true);summary.Text=result.Summary;patch.Text=result.Patch;message.Text=result.Message;
                Status.Text=result.Message.Length==0?"No changes in this scope. Stage files with Git, or select unstaged tracked changes.":"Draft generated locally. Review the patch and edit the message before copying.";
            }catch(Exception ex){Status.Text=ex.Message;}finally{busy=false;read.IsEnabled=true;}
        };
    }
}
