namespace GM_Buddy.Contracts.DTOs
{
    public record NpcDto
    {
        public int Npc_Id { get; set; }
        public required string Account { get; set; }
        public required string Name { get; set; }
        public required string Lineage { get; set; }
        public required string Occupation { get; set; }
        public required string System { get; set; }

        public DnDStats? Stats { get; set; }
        public string? Description { get; set; }
        public string? Gender { get; set; }
    }
}
