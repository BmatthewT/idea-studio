using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace IdeaStudio;
public sealed class MenuLanguage { public string Code {get;set;}="en"; public string Name {get;set;}="English"; public string Country {get;set;}="gb"; public Dictionary<string,string>? Labels {get;set;} }
public partial class MainWindow
{
    string menuLanguage="en";
    static string SettingsPath=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"IdeaStudio","settings.json");
    List<MenuLanguage> languages=[];
    string Tr(string text)=>languages.FirstOrDefault(l=>l.Code==menuLanguage)?.Labels?.GetValueOrDefault(text)??text;
    void LoadSettings(){try{languages=JsonSerializer.Deserialize<List<MenuLanguage>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Locales","languages.json")))??[];if(File.Exists(SettingsPath)){var s=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(SettingsPath))!;menuLanguage=s.GetValueOrDefault("language","en");model=s.GetValueOrDefault("model","");}}catch{Status.Text="Settings could not be loaded; using defaults.";}}
    void SettingsClick(object sender,RoutedEventArgs e)
    {
        if(busy)return;Capture();var w=new Window{Title=Tr("Settings"),Owner=this,Width=550,Height=560,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=Background,Foreground=Foreground,Resources=Resources};
        var p=new StackPanel{Margin=new Thickness(24)};w.Content=p;p.Children.Add(Text(Tr("Language"),24));
        p.Children.Add(Text("Menu language only. Machine translations may need review.",12));
        var search=new TextBox{Margin=new Thickness(0,8,0,8)};System.Windows.Automation.AutomationProperties.SetName(search,Tr("Search"));p.Children.Add(search);
        var list=new ListBox{Height=230,Background=Ink("#101114"),Foreground=Foreground};p.Children.Add(list);
        void Populate(){list.Items.Clear();foreach(var l in languages.Where(l=>l.Name.Contains(search.Text,StringComparison.OrdinalIgnoreCase)||l.Code.Contains(search.Text,StringComparison.OrdinalIgnoreCase))){var row=new StackPanel{Orientation=Orientation.Horizontal};var path=Path.Combine(AppContext.BaseDirectory,"Locales",l.Country+".png");if(File.Exists(path))row.Children.Add(new Image{Source=new System.Windows.Media.Imaging.BitmapImage(new Uri(path)),Width=26,Height=18,Margin=new Thickness(0,0,10,0)});row.Children.Add(new TextBlock{Text=l.Name+" ("+l.Code+")"});var item=new ListBoxItem{Content=row,Tag=l,IsSelected=l.Code==menuLanguage};list.Items.Add(item);}}search.TextChanged+=(_,_)=>Populate();Populate();
        p.Children.Add(Text("Ollama model",14));var modelBox=new TextBox{Text=model};p.Children.Add(modelBox);
        var actions=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,16,0,0)};p.Children.Add(actions);actions.Children.Add(Button(Tr("Apply"),()=>{if(list.SelectedItem is not ListBoxItem selected)return;var lang=(MenuLanguage)selected.Tag;Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);File.WriteAllText(SettingsPath,JsonSerializer.Serialize(new Dictionary<string,string>{{"language",lang.Code},{"model",modelBox.Text}}));menuLanguage=lang.Code;model=modelBox.Text;RenderHome();w.Close();}));actions.Children.Add(Button(Tr("Cancel"),()=>w.Close()));w.ShowDialog();
    }
}

