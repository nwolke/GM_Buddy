using Dapper;
using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts;
using GM_Buddy.Contracts.DbModels;
using GM_Buddy.Contracts.DTOs;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace GM_Buddy.Business
{
    public class NpcLogic : INpcLogic
    {
        public required IDbConnector _dbConnector;
        private DbSettings _dbSettings;
        public NpcLogic(IDbConnector dbConnector, IOptions<DbSettings> dbSettings)
        {
            _dbConnector = dbConnector;
            _dbSettings = dbSettings.Value;
        }


        public async Task<IEnumerable<npc_type>> GetNpcList(int user_id)
        {
            var connectionString = $"Host={_dbSettings.Host};Port={_dbSettings.Port};Database={_dbSettings.Database};Username={_dbSettings.Username};Password={_dbSettings.Password};Timeout=300;CommandTimeout=300;Pooling=false";
            Console.WriteLine(connectionString);
            var builder = new NpgsqlDataSourceBuilder(connectionString);
            builder.MapComposite<npc_type>("npc_type");
            SqlMapper.AddTypeMap(typeof(npc_type), DbType.Object);
            await using var connection = builder.Build();
            var con = connection.CreateConnection();
            var allNpcs = await con.QueryAsync<npc_type>(sql: $"SELECT get_npcs({user_id})", commandType: CommandType.Text);

            return allNpcs;
        }

        public async Task<dynamic?> GetNpc(int npc_id)
        {
            try
            {
                using (var con = _dbConnector.CreateConnection())
                {
                    var singleNpc = await con.QueryFirstAsync($"select * from npc where npc_id = {npc_id}");
                    return singleNpc;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //public async Task<NpcDto?> GetNpc(int npc_id)
        //{
        //    var singleNpc = await (from npc in _gmBuddyDbContext.Npcs
        //                     where npc.npc_id == npc_id
        //                     select new NpcDto
        //                     {
        //                         npc_id = npc.npc_id,
        //                         account = npc.account.account_name,
        //                         name = npc.name,
        //                         stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
        //                         description = npc.description,
        //                         lineage = npc.lineage.lineage_name,
        //                         occupation = npc.occupation.occupation_name,
        //                         system = npc.game_system.game_system_name
        //                     }).FirstOrDefaultAsync();
        //    return singleNpc;
        //}

        //public async Task<bool> AddNewNpc(NpcDto newNpc)
        //{
        //    _gmBuddyDbContext.Npcs.Add(new Npc
        //    {
        //        account = _gmBuddyDbContext.Accounts.Where(x => x.account_name == newNpc.account).First(),
        //        name = newNpc.name,
        //        description = newNpc.description,
        //        stats = JsonSerializer.Serialize(newNpc.stats),
        //        lineage = _gmBuddyDbContext.Lineages.Where(x => x.lineage_name.Equals(newNpc.lineage)).First(),
        //        occupation = _gmBuddyDbContext.Occupations.Where(x => x.occupation_name.Equals(newNpc.occupation)).First(),
        //        game_system = _gmBuddyDbContext.GameSystems.Where(x => x.game_system_name.Equals(newNpc.system)).First(),
        //    });

        //    var result = await _gmBuddyDbContext.SaveChangesAsync();
        //    return result > 0;
        //}

        //public async Task<bool> UpdateNpc(NpcDto updatedNpc)
        //{
        //    var npcToUpdate = _gmBuddyDbContext.Npcs.FirstOrDefault(x => x.npc_id == updatedNpc.npc_id);
        //    if (npcToUpdate != null)
        //    {
        //        npcToUpdate.description = updatedNpc.description;
        //        npcToUpdate.name = updatedNpc.name;
        //        npcToUpdate.stats = JsonSerializer.Serialize(updatedNpc.stats);
        //        npcToUpdate.lineage = _gmBuddyDbContext.Lineages.First(x => x.lineage_name == updatedNpc.lineage);
        //        npcToUpdate.occupation = _gmBuddyDbContext.Occupations.First(x => x.occupation_name == updatedNpc.occupation);

        //        var result = await _gmBuddyDbContext.SaveChangesAsync();
        //        return result > 0;
        //    }
        //    return false;
        //}

        //public async Task<bool> DeleteNpc(int npc_id)
        //{
        //    var toRemove = await _gmBuddyDbContext.Npcs.Where(x => x.npc_id == npc_id).FirstOrDefaultAsync();
        //    if (toRemove != null)
        //    {
        //        _gmBuddyDbContext.Npcs.Remove(toRemove);
        //        await _gmBuddyDbContext.SaveChangesAsync();
        //        return true;
        //    }    
        //    return false;
        //}
    }
}
