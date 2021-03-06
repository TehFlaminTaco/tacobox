using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;
using Sandbox.Tools;
using Sandbox.UI;

public static class AdminCore{
    public static List<Admin> admins = new();
    public static List<Rank> ranks = new();
    public static bool Setup = false;

    public static void CheckDirectory(){
        Assert.True(Local.Pawn is null);

        if(!FileSystem.Data.DirectoryExists("admin"))
            FileSystem.Data.CreateDirectory("admin");
    }

    public static void LoadOrMakeAdmins(){
        Assert.True(Local.Pawn is null);
        admins = new();
        if(!FileSystem.Data.FileExists("admin/admins.json")){
            admins.Add(new Admin{
                steamid = 76561198041670444L,
                rankkey = "host"
            });
            FileSystem.Data.WriteJson("admin/admins.json", admins);
        }else{
            admins = FileSystem.Data.ReadJson<List<Admin>>("admin/admins.json");
        }
    }

    public static void LoadOrMakeRanks(){
        Assert.True(Local.Pawn is null);
        ranks = new();
        if(!FileSystem.Data.FileExists("admin/ranks.json")){
            // TODO: Extract rank defaults
            ranks.Add(new Rank{
                Name = "Guest",
                NameColor = Color.Gray.Hex
            });
            ranks.Add(new Rank{
                Name = "Host",
                Authority = 999.0f,
                NameColor = Color.Red.Hex,
                Flags = new(new[]{
                    new Rank.Permission{name = "allCommands"},
                    new Rank.Permission{name = "allAuthority"},
                    new Rank.Permission{name = "editRanks"},
                    new Rank.Permission{name = "seeSilent"}
                })
            });
            FileSystem.Data.WriteJson("admin/ranks.json", ranks);
        }else{
            ranks = FileSystem.Data.ReadJson<List<Rank>>("admin/ranks.json");
        }
    }

    public static Rank GetRank(this Client c){
        string rankName = admins.Where(x=>x.steamid == c.PlayerId).FirstOrDefault()?.rankkey??"Guest";
        return ranks.First(c=>c.Name.ToLower() == rankName.ToLower());
    }

    public static bool HasCommand(this Client c, string name){
        return c.GetRank().HasCommand(name);
    }

    public static bool HasFlag(this Client c, string name){
        return c.GetRank().HasFlag(name);
    }
    public static bool CanTouch(this Client c, string rank){
        return c.GetRank().CanTouch(rank);
    }
    public static bool CanTouch(this Client c, Client other){
        return c.GetRank().CanTouch(other.GetRank().Name);
    }
    public static bool CanTouch(this Client c, Entity other){
        return other.IsWorld || other.GetSpawner() is null ? c.GetRank().HasFlag("touchWorldspawn") : (other.GetSpawner()==c) || c.GetRank().CanTouch(other.GetSpawner().GetRank().Name);
    }
    public static bool HasTool(this Client c, string name){
        return c.GetRank().HasTool(name);
    }
    public static bool CanSpawn(this Client c, PropType t){
        return c.GetRank().SpawnLimit($"{t}")==-1||(c.EntsOfType(t).Count() < c.GetRank().SpawnLimit($"{t}"));
    }
    public static bool CanSpawnProp(this Client c, string model){
        return c.GetRank().CanSpawnProp(model);
    }
    public static bool CanSpawnEnt(this Client c, string name){
        return c.GetRank().CanSpawnEnt(name);
    }
    public static void HitLimit(this Client c, PropType t){
        if(c.GetRank().SpawnLimit($"{t}")==0){
            TacoChatBox.AddChatEntry(To.Single(c), "white", "", $"?????? You are not allowed to spawn {t}s!");
        }else{
            TacoChatBox.AddChatEntry(To.Single(c), "white", "", $"?????? You have hit the {t} limit!");
        }
    }

    public static void BannedProp(this Client c, string model){
        TacoChatBox.AddChatEntry(To.Single(c), "white", "", $"?????? You are not allowed to spawn {model}!");
    }
    public static void BannedEnt(this Client c, string ent){
        TacoChatBox.AddChatEntry(To.Single(c), "white", "", $"?????? You are not allowed to spawn {ent}!");
    }

    public static string ColorName(this Client c){
        return $"[color={c.GetRank().NameColor}]{c.Name}[/color]";
    }

    public static To SeeSilent(Client listener, bool isSilent){
        return isSilent ? To.Multiple(Client.All.Where(c=>c==listener||c.HasFlag("seeSilent"))) : To.Everyone;
    }
    public static To SeeSilent(Player listener, bool isSilent){
        return SeeSilent(listener.Client, isSilent);
    }

    public static bool CanTool(this BaseTool tool){
        return tool.Owner.Client.HasTool(tool.ClassInfo.Name);
    }

    public static string[] AllFlags(){
        return new[]{
            "allCommands",
            "allAuthority",
            "allTools",
            "allProps",
            "allEnts",
            "spawnSwerks",
            "unlimitedSpawns",
            "editRanks",
            "seeSilent",
            "showDeniedTools",
            "touchWorldspawn",
        };
    }

    [Event.Hotload]
    public static void OnReload(){
        if(Local.Pawn is not null)return;
        CheckDirectory();
        LoadOrMakeAdmins();
        LoadOrMakeRanks();

        ReinformClients();
        Setup = true;
    }

    public static void SaveData(){
        if(Local.Pawn is not null)return;
        FileSystem.Data.WriteJson("admin/admins.json", admins);
        FileSystem.Data.WriteJson("admin/ranks.json", ranks);
    }
    
    public static void ReinformClients(){
        if(Local.Pawn is not null)return;
        AdminCoreUpdate.UpdateAdmins(JsonSerializer.Serialize(admins));
        AdminCoreUpdate.UpdateRanks(JsonSerializer.Serialize(ranks));
    }
    public class Admin {
        public long steamid {get; set;}
        public string rankkey {get; set;}
    }
}

public partial class AdminCoreUpdate{
    [ClientRpc]
    public static void UpdateAdmins(string admins){
        AdminCore.admins = JsonSerializer.Deserialize<List<AdminCore.Admin>>(admins);
        SpawnMenu.Instance?.UpdateToolsVisible();
        AdminCore.Setup = AdminCore.ranks is not null;
    }

    [ClientRpc]
    public static void UpdateRanks(string ranks){
        AdminCore.ranks = JsonSerializer.Deserialize<List<Rank>>(ranks);
        SpawnMenu.Instance?.UpdateToolsVisible();
        AdminCore.Setup = AdminCore.admins is not null;
    }
}