using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
class Setup : Form {
 Label status; Button install;
 Setup(){Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);Text="Install Sanctuary Atlas";Size=new Size(510,300);StartPosition=FormStartPosition.CenterScreen;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;BackColor=Color.FromArgb(23,23,27);ForeColor=Color.FromArgb(230,225,214);Font=new Font("Segoe UI",10);
 var title=new Label{Text="SANCTUARY ATLAS",Location=new Point(30,25),Size=new Size(430,35),ForeColor=Color.FromArgb(210,172,100),Font=new Font("Segoe UI",19,FontStyle.Bold)}; Controls.Add(title);
 var desc=new Label{Text="Install the D2R build planner for your Windows account. A Start menu shortcut will be created.",Location=new Point(32,78),Size=new Size(425,58)};Controls.Add(desc);
 status=new Label{Text="Installs without administrator access.",Location=new Point(32,150),Size=new Size(425,50),ForeColor=Color.FromArgb(157,151,142)};Controls.Add(status);
 install=new Button{Text="Install",Location=new Point(310,204),Size=new Size(145,38),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(70,56,38),ForeColor=Color.White};install.Click+=Install;Controls.Add(install); }
 static string AppDirectory(){return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SanctuaryAtlas");}
 static string InstallFiles(string dir){
  Directory.CreateDirectory(dir);string exe=Path.Combine(dir,"Sanctuary Atlas.exe");string temp=Path.Combine(dir,"Sanctuary Atlas.new.exe");
  using(var input=Assembly.GetExecutingAssembly().GetManifestResourceStream("AtlasApp"))using(var output=File.Create(temp))input.CopyTo(output);
  try{if(File.Exists(exe))File.Replace(temp,exe,null);else File.Move(temp,exe);}finally{if(File.Exists(temp))File.Delete(temp);}
  using(var input=Assembly.GetExecutingAssembly().GetManifestResourceStream("RunewizardLicense"))using(var output=File.Create(Path.Combine(dir,"Runewizard License.txt")))input.CopyTo(output);
  return exe;
 }
 void Install(object s,EventArgs e){try{string dir=AppDirectory();string exe=InstallFiles(dir); string programs=Environment.GetFolderPath(Environment.SpecialFolder.Programs);string shortcut=Path.Combine(programs,"Sanctuary Atlas.lnk"); Type shellType=Type.GetTypeFromProgID("WScript.Shell"); object shell=Activator.CreateInstance(shellType); object link=shellType.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{shortcut});Type lt=link.GetType();lt.InvokeMember("TargetPath",BindingFlags.SetProperty,null,link,new object[]{exe});lt.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,link,new object[]{dir});lt.InvokeMember("IconLocation",BindingFlags.SetProperty,null,link,new object[]{exe+",0"});lt.InvokeMember("Description",BindingFlags.SetProperty,null,link,new object[]{"D2R build planner"});lt.InvokeMember("Save",BindingFlags.InvokeMethod,null,link,null);
 status.Text="Installed! Find Sanctuary Atlas in your Start menu.";install.Text="Open app";install.Click-=Install;install.Click+=(x,y)=>{System.Diagnostics.Process.Start(exe);Close();};}catch(Exception ex){MessageBox.Show("Installation could not be completed: "+ex.Message,"Sanctuary Atlas",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
 [STAThread] static void Main(string[] args){Application.EnableVisualStyles();if(args.Length==2 && args[0]=="--apply-update"){try{int pid=Int32.Parse(args[1]);try{var running=System.Diagnostics.Process.GetProcessById(pid);if(!running.WaitForExit(60000))throw new Exception("The app did not close in time. Please close it and run the installer again.");}catch(ArgumentException){}string updated=InstallFiles(AppDirectory());System.Diagnostics.Process.Start(updated);return;}catch(Exception ex){MessageBox.Show("Update could not be completed: "+ex.Message,"Sanctuary Atlas",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}}if(args.Length>0 && args[0]=="--self-test"){string dir=Path.Combine(Environment.CurrentDirectory,"work\\install-test");Directory.CreateDirectory(dir);string marker=Path.Combine(dir,"saved-data-test.txt");File.WriteAllText(marker,"keep");string exe=InstallFiles(dir);InstallFiles(dir);if(File.ReadAllText(marker)!="keep"||!File.Exists(Path.Combine(dir,"Runewizard License.txt")))Environment.Exit(2); Type t=Type.GetTypeFromProgID("WScript.Shell");object sh=Activator.CreateInstance(t);object l=t.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,sh,new object[]{Path.Combine(dir,"test.lnk")});Type lt=l.GetType();lt.InvokeMember("TargetPath",BindingFlags.SetProperty,null,l,new object[]{exe});lt.InvokeMember("Save",BindingFlags.InvokeMethod,null,l,null);if(!File.Exists(exe)||!File.Exists(Path.Combine(dir,"test.lnk")))Environment.Exit(1);return;}Application.Run(new Setup());}
}


