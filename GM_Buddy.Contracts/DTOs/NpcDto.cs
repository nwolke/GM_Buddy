using Contracts;

namespace GM_Buddy.Contracts.DTOs
{
    public record NpcDto
    {
        public int npc_id { get; set; }
        public required string account { get; set; }
        public required string name { get; set; }
        public required string lineage { get; set; }
        public required string occupation { get; set; }
        public required string system { get; set; }
        public DnDStats? stats { get; set; }
        public string? description { get; set; }
    }
}
