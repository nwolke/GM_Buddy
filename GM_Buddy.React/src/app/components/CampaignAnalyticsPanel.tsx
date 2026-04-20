import { useMemo } from "react";
import { NPC, Relationship } from "@/types/npc";
import { PC } from "@/types/pc";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/app/components/ui/card";
import { Badge } from "@/app/components/ui/badge";
import { ChartContainer, ChartTooltip, ChartTooltipContent, type ChartConfig } from "@/app/components/ui/chart";
import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";

interface CampaignAnalyticsPanelProps {
  npcs: NPC[];
  pcs: PC[];
  relationships: Relationship[];
}

const relationshipTypeColors: Record<string, string> = {
  acquaintance: "#64748b",
  ally: "#10b981",
  child: "#ec4899",
  contact: "#14b8a6",
  employee: "#f59e0b",
  employer: "#d97706",
  enemy: "#ef4444",
  family: "#a855f7",
  follower: "#6366f1",
  friend: "#34d399",
  informant: "#eab308",
  leader: "#4f46e5",
  lover: "#f43f5e",
  member: "#8b5cf6",
  mentor: "#3b82f6",
  parent: "#db2777",
  patron: "#0ea5e9",
  protege: "#0284c7",
  rival: "#f97316",
  sibling: "#d946ef",
  spouse: "#e11d48",
  stranger: "#71717a",
  student: "#06b6d4",
  vassal: "#78716c",
  custom: "#84cc16",
  neutral: "#6b7280",
};

const chartConfig = {
  count: {
    label: "Relationships",
    color: "hsl(var(--primary))",
  },
} satisfies ChartConfig;

const formatRelationshipTypeLabel = (type: string) =>
  type
    .replace(/_/g, " ")
    .replace(/\b\w/g, match => match.toUpperCase());

const isRelationshipActive = (relationship: Relationship) => {
  if (typeof relationship.isActive === "boolean") {
    return relationship.isActive;
  }

  if (typeof relationship.is_active === "boolean") {
    return relationship.is_active;
  }

  return true;
};

export function CampaignAnalyticsPanel({
  npcs,
  pcs,
  relationships,
}: CampaignAnalyticsPanelProps) {
  const totalOrganizations = useMemo(
    () =>
      new Set(
        npcs
          .map(npc => npc.faction?.trim())
          .filter((faction): faction is string => Boolean(faction)),
      ).size,
    [npcs],
  );

  const relationshipTypeData = useMemo(() => {
    const counts = new Map<string, number>();

    relationships.forEach(relationship => {
      const typeKey = relationship.type === "custom" && relationship.customType?.trim()
        ? relationship.customType.trim()
        : relationship.type;
      counts.set(typeKey, (counts.get(typeKey) ?? 0) + 1);
    });

    return Array.from(counts.entries())
      .map(([type, count]) => ({
        type,
        label: formatRelationshipTypeLabel(type),
        count,
        fill: relationshipTypeColors[type.toLowerCase()] ?? "#6b7280",
      }))
      .sort((a, b) => b.count - a.count || a.label.localeCompare(b.label));
  }, [relationships]);

  const averageAttitudeScore = useMemo(() => {
    const activeRelationships = relationships.filter(isRelationshipActive);
    if (activeRelationships.length === 0) {
      return null;
    }

    const total = activeRelationships.reduce(
      (sum, relationship) => sum + (relationship.attitudeScore ?? 0),
      0,
    );

    return total / activeRelationships.length;
  }, [relationships]);

  const mostConnectedEntity = useMemo<{ name: string; count: number } | null>(() => {
    const entityNameByKey = new Map<string, string>();
    npcs.forEach(npc => entityNameByKey.set(`npc-${npc.id}`, npc.name));
    pcs.forEach(pc => entityNameByKey.set(`pc-${pc.id}`, pc.name));

    const counts = new Map<string, number>();
    entityNameByKey.forEach((_, key) => counts.set(key, 0));

    relationships.forEach(relationship => {
      const sourceKey = `${relationship.entityType1}-${relationship.npcId1}`;
      const targetKey = `${relationship.entityType2}-${relationship.npcId2}`;
      counts.set(sourceKey, (counts.get(sourceKey) ?? 0) + 1);
      counts.set(targetKey, (counts.get(targetKey) ?? 0) + 1);
    });

    let topEntity: { name: string; count: number } | null = null;
    counts.forEach((count, key) => {
      const name = entityNameByKey.get(key);
      if (!name) return;

      if (!topEntity || count > topEntity.count || (count === topEntity.count && name.localeCompare(topEntity.name) < 0)) {
        topEntity = { name, count };
      }
    });

    return topEntity;
  }, [npcs, pcs, relationships]);

  return (
    <div className="h-full min-h-0 overflow-auto pr-1">
      <div className="space-y-4">
        <div className="grid gap-3 sm:grid-cols-3">
          <Card className="bg-card/50 border-primary/20">
            <CardHeader className="pb-2">
              <CardDescription>NPCs</CardDescription>
              <CardTitle className="text-2xl">{npcs.length}</CardTitle>
            </CardHeader>
          </Card>
          <Card className="bg-card/50 border-primary/20">
            <CardHeader className="pb-2">
              <CardDescription>PCs</CardDescription>
              <CardTitle className="text-2xl">{pcs.length}</CardTitle>
            </CardHeader>
          </Card>
          <Card className="bg-card/50 border-primary/20">
            <CardHeader className="pb-2">
              <CardDescription>Organizations</CardDescription>
              <CardTitle className="text-2xl">{totalOrganizations}</CardTitle>
            </CardHeader>
          </Card>
        </div>

        <div className="grid gap-4 xl:grid-cols-[2fr_1fr]">
          <Card className="bg-card/50 border-primary/20">
            <CardHeader className="pb-0">
              <CardTitle className="text-base">Relationship Type Breakdown</CardTitle>
              <CardDescription>Count by relationship type</CardDescription>
            </CardHeader>
            <CardContent className="pt-4">
              {relationshipTypeData.length === 0 ? (
                <p className="text-sm text-muted-foreground">No relationships to analyze yet.</p>
              ) : (
                <ChartContainer config={chartConfig} className="h-72 w-full" data-testid="relationship-type-chart">
                  <BarChart data={relationshipTypeData} margin={{ top: 8, right: 8, left: 0, bottom: 8 }}>
                    <CartesianGrid vertical={false} />
                    <XAxis dataKey="label" tickLine={false} axisLine={false} interval={0} angle={-25} textAnchor="end" height={60} />
                    <YAxis allowDecimals={false} />
                    <ChartTooltip content={<ChartTooltipContent />} />
                    <Bar dataKey="count" radius={4} fill="var(--color-count)" />
                  </BarChart>
                </ChartContainer>
              )}
            </CardContent>
          </Card>

          <Card className="bg-card/50 border-primary/20">
            <CardHeader className="pb-0">
              <CardTitle className="text-base">Campaign Health Snapshot</CardTitle>
              <CardDescription>Derived from active campaign relationships</CardDescription>
            </CardHeader>
            <CardContent className="space-y-5 pt-4">
              <div className="space-y-1">
                <p className="text-xs uppercase tracking-wide text-muted-foreground">Average attitude score</p>
                <p className="text-2xl font-semibold">
                  {averageAttitudeScore === null ? "N/A" : averageAttitudeScore.toFixed(2)}
                </p>
              </div>

              <div className="space-y-2">
                <p className="text-xs uppercase tracking-wide text-muted-foreground">Most-connected entity</p>
                {mostConnectedEntity ? (
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-medium">{mostConnectedEntity.name}</span>
                    <Badge variant="secondary">{mostConnectedEntity.count} links</Badge>
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No entities available.</p>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
