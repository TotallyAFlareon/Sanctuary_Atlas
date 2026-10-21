using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

public sealed class UpdateRelease {
 public int Version;
 public string Url, Hash;
 public string ContentUrl, ContentHash;
}
public static class AtlasUpdates {
 public const int CurrentVersion=24;
 public const string DefaultRepository="TotallyAFlareon/Sanctuary_Atlas";
 public static bool ValidRepository(string repository){return Regex.IsMatch(repository??"",@"^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})/[A-Za-z0-9_.-]+$");}
 public static UpdateRelease ParseRelease(string json,string repo){
  if(!ValidRepository(repo))throw new InvalidOperationException("Invalid repository name.");
  var root=new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string,object>;
  if(root==null || !root.ContainsKey("tag_name"))throw new InvalidOperationException("The release has no version tag.");
  var match=Regex.Match(Convert.ToString(root["tag_name"]),@"^v([0-9]+)$");int version;
  if(!match.Success || !Int32.TryParse(match.Groups[1].Value,out version))throw new InvalidOperationException("The release version must look like v12.");
  var release=new UpdateRelease{Version=version};
  object assetValue;
  if(!root.TryGetValue("assets",out assetValue))throw new InvalidOperationException("The release has no installer.");
  var assets=assetValue as object[];if(assets==null)throw new InvalidOperationException("The release has no installer.");
  foreach(var value in assets){
   var asset=value as Dictionary<string,object>;if(asset==null)continue;
   object name,url,digest,size;
   if(!asset.TryGetValue("name",out name))continue;
   string assetName=Convert.ToString(name);if(assetName!="SanctuaryAtlasSetup.exe"&&assetName!="SanctuaryAtlasContent.xml")continue;
   if(!asset.TryGetValue("browser_download_url",out url)||!asset.TryGetValue("digest",out digest)||!asset.TryGetValue("size",out size))throw new InvalidOperationException("The release installer is missing its download link or checksum.");
   string link=Convert.ToString(url),hash=Convert.ToString(digest);long bytes=Convert.ToInt64(size);
   string prefix="https://github.com/"+repo+"/releases/download/";
   if(!link.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)||!Regex.IsMatch(hash??"",@"^sha256:[a-fA-F0-9]{64}$")||bytes<1||bytes>(assetName=="SanctuaryAtlasContent.xml"?2000000:50000000))throw new InvalidOperationException("The release file details are invalid.");
   if(assetName=="SanctuaryAtlasSetup.exe"){release.Url=link;release.Hash=hash.Substring(7);}else{release.ContentUrl=link;release.ContentHash=hash.Substring(7);}
  }
  if(version>CurrentVersion&&release.Url==null)throw new InvalidOperationException("The release needs an asset named SanctuaryAtlasSetup.exe.");
  return release;
 }
 static WebClient Client(){var c=new WebClient();c.Headers.Add("User-Agent","Sanctuary-Atlas-Updater");c.Headers.Add("Accept","application/vnd.github+json");return c;}
 public static UpdateRelease Fetch(string repository){ServicePointManager.SecurityProtocol|=(SecurityProtocolType)3072;using(var c=Client())return ParseRelease(c.DownloadString("https://api.github.com/repos/"+repository+"/releases/latest"),repository);}
 public static string Download(UpdateRelease release){
  string file=Path.Combine(Path.GetTempPath(),"SanctuaryAtlas-Update-"+Guid.NewGuid().ToString("N")+".exe");
  try{using(var c=Client())c.DownloadFile(release.Url,file);using(var input=File.OpenRead(file))using(var sha=SHA256.Create()){
   string actual=BitConverter.ToString(sha.ComputeHash(input)).Replace("-","");
   if(!String.Equals(actual,release.Hash,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("The downloaded installer did not match its checksum.");
  }return file;}catch{if(File.Exists(file))File.Delete(file);throw;}
 }
 public static byte[] DownloadContent(UpdateRelease release){
  if(release.ContentUrl==null||release.ContentHash==null)throw new InvalidOperationException("No content update is available.");
  byte[] data;using(var c=Client())using(var input=c.OpenRead(release.ContentUrl))using(var output=new MemoryStream()){
   byte[] buffer=new byte[8192];int read;while((read=input.Read(buffer,0,buffer.Length))>0){if(output.Length+read>2000000)throw new InvalidOperationException("The content update is too large.");output.Write(buffer,0,read);}data=output.ToArray();
  }
  if(data.Length==0||data.Length>2000000)throw new InvalidOperationException("The content update has an invalid size.");
  using(var sha=SHA256.Create()){string actual=BitConverter.ToString(sha.ComputeHash(data)).Replace("-","");if(!String.Equals(actual,release.ContentHash,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("The content update did not match its checksum.");}
  return data;
 }
}

