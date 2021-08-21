using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;

public static class AdminCore{
    public static List<Admin> admins = new();
    public static List<Rank> ranks = new();

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
                steamid = 76561198041670444ul,
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
                    new Rank.Permission{flag_or_command = "allCommands"},
                    new Rank.Permission{flag_or_command = "allAuthority"},
                    new Rank.Permission{flag_or_command = "editRanks"}
                })
            });
            FileSystem.Data.WriteJson("admin/ranks.json", ranks);
        }else{
            ranks = FileSystem.Data.ReadJson<List<Rank>>("admin/ranks.json");
        }
    }

    public static Rank GetRank(this Client c){
        string rankName = admins.Where(x=>x.steamid == c.SteamId).FirstOrDefault()?.rankkey??"Guest";
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

    public static string[] AllFlags(){
        return new[]{
            "allCommands",
            "allAuthority",
            "editRanks",
        };
    }

    [Event.Hotload]
    public static void OnReload(){
        if(Local.Pawn is not null)return;
        CheckDirectory();
        LoadOrMakeAdmins();
        LoadOrMakeRanks();

        ReinformClients();
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
        public ulong steamid {get; set;}
        public string rankkey {get; set;}
    }
}

public partial class AdminCoreUpdate{
    [ClientRpc]
    public static void UpdateAdmins(string admins){
        AdminCore.admins = JsonSerializer.Deserialize<List<AdminCore.Admin>>(admins);
    }

    [ClientRpc]
    public static void UpdateRanks(string ranks){
        AdminCore.ranks = JsonSerializer.Deserialize<List<Rank>>(ranks);
    }
}