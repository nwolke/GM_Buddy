import { describe, expect, it } from "vitest";
import { render, screen } from "@/test/utils";
import { CampaignAnalyticsPanel } from "@/app/components/CampaignAnalyticsPanel";
import type { NPC, Relationship } from "@/types/npc";
import type { PC } from "@/types/pc";

describe("CampaignAnalyticsPanel", () => {
  const npcs: NPC[] = [
    { id: 1, name: "Aria", lineage: "Human", class: "Rogue", description: "", faction: "Guild" },
    { id: 2, name: "Borin", lineage: "Dwarf", class: "Fighter", description: "", faction: "Guild" },
    { id: 3, name: "Cyra", lineage: "Elf", class: "Mage", description: "", faction: "Council" },
  ];

  const pcs: PC[] = [
    { id: 10, name: "Dain", description: "Paladin", campaignId: 1 },
    { id: 11, name: "Ely", description: "Ranger", campaignId: 1 },
  ];

  const relationships: Relationship[] = [
    {
      id: 101,
      npcId1: 1,
      npcId2: 10,
      entityType1: "npc",
      entityType2: "pc",
      type: "ally",
      attitudeScore: 2,
    },
    {
      id: 102,
      npcId1: 10,
      npcId2: 2,
      entityType1: "pc",
      entityType2: "npc",
      type: "friend",
      attitudeScore: 4,
    },
    {
      id: 103,
      npcId1: 1,
      npcId2: 11,
      entityType1: "npc",
      entityType2: "pc",
      type: "enemy",
      attitudeScore: -5,
      is_active: false,
    },
  ];

  it("renders entity counts and organization totals", () => {
    render(<CampaignAnalyticsPanel npcs={npcs} pcs={pcs} relationships={relationships} />);

    const npcCard = screen.getByText("NPCs").closest("[data-slot='card']");
    const pcCard = screen.getByText("PCs").closest("[data-slot='card']");
    const organizationCard = screen.getByText("Organizations").closest("[data-slot='card']");

    expect(npcCard).toHaveTextContent("3");
    expect(pcCard).toHaveTextContent("2");
    expect(organizationCard).toHaveTextContent("2");
  });

  it("computes average attitude from active relationships only", () => {
    render(<CampaignAnalyticsPanel npcs={npcs} pcs={pcs} relationships={relationships} />);

    expect(screen.getByText("3.00")).toBeInTheDocument();
  });

  it("shows the most-connected entity using inbound and outbound relationships", () => {
    render(<CampaignAnalyticsPanel npcs={npcs} pcs={pcs} relationships={relationships} />);

    expect(screen.getByText("Aria")).toBeInTheDocument();
    expect(screen.getByText("2 links")).toBeInTheDocument();
  });

  it("renders empty-state chart messaging when no relationships exist", () => {
    render(<CampaignAnalyticsPanel npcs={npcs} pcs={pcs} relationships={[]} />);

    expect(screen.getByText("No relationships to analyze yet.")).toBeInTheDocument();
  });
});
