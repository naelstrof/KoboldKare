using System.Text.RegularExpressions;

// Using information gathered from https://developer.valvesoftware.com/wiki/SteamID 
public static class SteamIDParser {
    public enum EAccountType : byte {
        Invalid = 0,
        Individual = 1,
        Multiseat = 2,
        GameServer = 3,
        AnonymousGameServer = 4,
        Pending = 5,
        ContentServer = 6,
        Clan = 7,
        Chat = 8,
        P2PSuperSeeder = 9,
        AnonymousUser = 10
    }
    private static bool TryLookUpAccountTypeOffset(EAccountType accountType, out ulong offset) {
        switch (accountType) {
            case EAccountType.Invalid:
                offset = 0;
                return false;
            case EAccountType.Individual:
                offset = 0x0110000100000000;
                return true;
            case EAccountType.Multiseat:
                offset = 0;
                return true;
            case EAccountType.GameServer:
                offset = 0;
                return true;
            case EAccountType.AnonymousGameServer:
                offset = 0;
                return true;
            case EAccountType.Pending:
                offset = 0;
                return false;
            case EAccountType.ContentServer:
                offset = 0;
                return false;
            case EAccountType.Clan:
                offset = 0x0170000000000000;
                return true;
            case EAccountType.Chat:
                offset = 0;
                return true;
            case EAccountType.P2PSuperSeeder:
                offset = 0;
                return true;
            case EAccountType.AnonymousUser:
                offset = 0;
                return true;
            default:
                offset = 0;
                return false;
        }
    }
    
    public static bool TryParseSteamId(string input, out ulong steamId64, EAccountType accountType = EAccountType.Individual) {
        steamId64 = 0;
        if (string.IsNullOrWhiteSpace(input)) {
            return false;
        }

        input = input.Trim();

        if (ulong.TryParse(input, out var num) && num > 0) {
            steamId64 = num;
            return true;
        }

        var m = Regex.Match(input, @"^STEAM_[0-9]:(\d):(\d+)$", RegexOptions.IgnoreCase);
        if (m.Success) {
            if (uint.TryParse(m.Groups[1].Value, out var y) && ulong.TryParse(m.Groups[2].Value, out var z)) {
                if (TryLookUpAccountTypeOffset(accountType, out var offset)) {
                    var accountId = z * 2 + y;
                    steamId64 = accountId + offset;
                    return true;
                }
            }
            return false;
        }

        m = Regex.Match(input, @"^\[?[^0-9]:1:(\d+)\]?$", RegexOptions.IgnoreCase);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, out var account)) {
            if (TryLookUpAccountTypeOffset(accountType, out var offset)) {
                steamId64 = account + offset;
                return true;
            }
            return false;
        }

        m = Regex.Match(input, @"profiles/(\d{17,20})", RegexOptions.IgnoreCase);
        if (m.Success && ulong.TryParse(m.Groups[1].Value, out var profileId)) {
            steamId64 = profileId;
            return true;
        }

        return false;
    } 
}
