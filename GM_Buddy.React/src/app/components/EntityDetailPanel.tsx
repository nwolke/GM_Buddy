import { useState } from "react";
import { Relationship } from "@/types/npc";
import { EntityItem } from "@/types/entity";
import { Badge } from "@/app/components/ui/badge";
import { Button } from "@/app/components/ui/button";
import { ScrollArea } from "@/app/components/ui/scroll-area";
import { Separator } from "@/app/components/ui/separator";
import { RelationshipAddModal } from "@/app/components/RelationshipAddModal";
import { Trash2, Plus, Shield, User } from "lucide-react";

interface EntityDetailPanelProps {
  entity: EntityItem | null;
  relationships: Relationship[];
  allEntities: EntityItem[];
  onAddRelationship: (relationship: Omit<Relationship, 'id'>) => Promise<void>;
  onDeleteRelationship: (id: number) => Promise<void>;
}

const relationshipBadgeColors: Record<string, string> = {
  acquaintance: 'bg-slate-500/20 text-slate-400 border-slate-500/30',
  ally: 'bg-green-500/20 text-green-400 border-green-500/30',
  child: 'bg-pink-500/20 text-pink-400 border-pink-500/30',
  contact: 'bg-teal-500/20 text-teal-400 border-teal-500/30',
  employee: 'bg-amber-500/20 text-amber-400 border-amber-500/30',
  employer: 'bg-amber-600/20 text-amber-300 border-amber-600/30',
  enemy: 'bg-red-500/20 text-red-400 border-red-500/30',
  family: 'bg-purple-500/20 text-purple-400 border-purple-500/30',
  follower: 'bg-indigo-500/20 text-indigo-400 border-indigo-500/30',
  friend: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
  informant: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
  leader: 'bg-indigo-600/20 text-indigo-300 border-indigo-600/30',
  lover: 'bg-rose-500/20 text-rose-400 border-rose-500/30',
  member: 'bg-violet-500/20 text-violet-400 border-violet-500/30',
  mentor: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
  parent: 'bg-pink-600/20 text-pink-300 border-pink-600/30',
  patron: 'bg-sky-500/20 text-sky-400 border-sky-500/30',
  protege: 'bg-sky-600/20 text-sky-300 border-sky-600/30',
  rival: 'bg-orange-500/20 text-orange-400 border-orange-500/30',
  sibling: 'bg-fuchsia-500/20 text-fuchsia-400 border-fuchsia-500/30',
  spouse: 'bg-rose-600/20 text-rose-300 border-rose-600/30',
  stranger: 'bg-zinc-500/20 text-zinc-400 border-zinc-500/30',
  student: 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30',
  vassal: 'bg-stone-500/20 text-stone-400 border-stone-500/30',
  custom: 'bg-lime-500/20 text-lime-400 border-lime-500/30',
  neutral: 'bg-gray-500/20 text-gray-400 border-gray-500/30',
};

export function EntityDetailPanel({
  entity,
  relationships,
  allEntities,
  onAddRelationship,
  onDeleteRelationship,
}: EntityDetailPanelProps) {
  const [addModalOpen, setAddModalOpen] = useState(false);

  if (!entity) {
    return (
      <div className="flex flex-col items-center justify-center h-full p-6 text-center">
        <div className="bg-gradient-to-br from-primary/10 to-accent/10 w-16 h-16 rounded-full flex items-center justify-center mb-4">
          <Shield className="size-8 text-primary/50" />
        </div>
        <p className="text-muted-foreground text-sm leading-relaxed">
          Click an entity on the graph or in the list to view its details and relationships.
        </p>
      </div>
    );
  }

  const entityRelationships = relationships.filter(rel => {
    const isSource = rel.npcId1 === entity.id && rel.entityType1 === entity.entityType;
    const isTarget = rel.npcId2 === entity.id && rel.entityType2 === entity.entityType;
    return isSource || isTarget;
  });

  const getRelatedEntity = (rel: Relationship): EntityItem | undefined => {
    const isSource = rel.npcId1 === entity.id && rel.entityType1 === entity.entityType;
    const otherId = isSource ? rel.npcId2 : rel.npcId1;
    const otherType = isSource ? rel.entityType2 : rel.entityType1;
    return allEntities.find(e => e.id === otherId && e.entityType === otherType);
  };

  const isNpc = entity.entityType === 'npc';

  return (
    <div className="flex flex-col h-full min-h-0">
      {/* Entity header */}
      <div className="p-4 border-b border-primary/20">
        <div className="flex items-start gap-3">
          <div className={`p-2 rounded-lg ${isNpc ? 'bg-primary/20' : 'bg-green-500/20'}`}>
            {isNpc
              ? <Shield className="size-5 text-primary" />
              : <User className="size-5 text-green-400" />
            }
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 flex-wrap">
              <h2 className="font-bold text-lg leading-tight truncate">{entity.name}</h2>
              <Badge
                variant="outline"
                className={isNpc
                  ? 'text-xs border-primary/40 text-primary'
                  : 'text-xs border-green-500/40 text-green-400'
                }
              >
                {isNpc ? 'NPC' : 'PC'}
              </Badge>
            </div>
            {isNpc && entity.lineage && entity.class && (
              <p className="text-sm text-muted-foreground mt-0.5">
                <span className="text-primary font-medium">{entity.lineage}</span>
                <span className="mx-1">•</span>
                <span className="text-accent font-medium">{entity.class}</span>
              </p>
            )}
          </div>
        </div>
      </div>

      <ScrollArea className="flex-1 min-h-0">
        <div className="p-4 space-y-4">
          {/* Entity details */}
          {entity.faction && (
            <div className="bg-secondary/50 rounded-lg p-3 border border-primary/20">
              <p className="text-xs font-semibold text-primary uppercase tracking-wider mb-1">Faction</p>
              <p className="text-sm">{entity.faction}</p>
            </div>
          )}

          {entity.description && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-1">Description</p>
              <p className="text-sm text-foreground/90 leading-relaxed">{entity.description}</p>
            </div>
          )}

          {entity.notes && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-1">Notes</p>
              <p className="text-sm text-foreground/80 italic leading-relaxed">{entity.notes}</p>
            </div>
          )}

          <Separator className="bg-primary/20" />

          {/* Relationships section */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                Relationships ({entityRelationships.length})
              </p>
              <Button
                size="sm"
                variant="outline"
                onClick={() => setAddModalOpen(true)}
                className="h-7 text-xs border-primary/30 hover:bg-primary/10 hover:text-primary"
              >
                <Plus className="size-3 mr-1" />
                Add
              </Button>
            </div>

            {entityRelationships.length === 0 ? (
              <p className="text-sm text-muted-foreground italic">
                No relationships yet. Add one to connect this entity to others.
              </p>
            ) : (
              <div className="space-y-2">
                {entityRelationships.map(rel => {
                  const related = getRelatedEntity(rel);
                  return (
                    <div
                      key={rel.id}
                      className="flex items-start gap-2 p-2 rounded-lg border border-border/50 bg-card/50 group"
                    >
                      <div className="flex items-center gap-1.5 shrink-0 mt-0.5">
                        <Badge
                          variant="outline"
                          className={`text-xs ${relationshipBadgeColors[rel.type] ?? relationshipBadgeColors.neutral}`}
                        >
                          {rel.customType || rel.type}
                        </Badge>
                        {rel.attitudeScore !== 0 && (
                          <span className={`text-xs font-semibold ${
                            rel.attitudeScore > 0 ? 'text-green-400' : 'text-red-400'
                          }`}>
                            {rel.attitudeScore > 0 ? '+' : ''}{rel.attitudeScore}
                          </span>
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">
                          {related?.name ?? `Entity #${rel.npcId1 === entity.id ? rel.npcId2 : rel.npcId1}`}
                        </p>
                        {related && (
                          <p className="text-xs text-muted-foreground">
                            {related.entityType === 'npc' ? 'NPC' : 'Player Character'}
                          </p>
                        )}
                        {rel.description && (
                          <p className="text-xs text-muted-foreground mt-0.5 italic">{rel.description}</p>
                        )}
                      </div>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-7 w-7 shrink-0 opacity-0 group-hover:opacity-100 transition-opacity hover:bg-destructive/20 hover:text-destructive"
                        onClick={() => onDeleteRelationship(rel.id)}
                        aria-label="Delete relationship"
                      >
                        <Trash2 className="size-3" />
                      </Button>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </ScrollArea>

      {addModalOpen && (
        <RelationshipAddModal
          open={addModalOpen}
          onOpenChange={setAddModalOpen}
          sourceEntity={entity}
          allEntities={allEntities}
          existingRelationships={entityRelationships}
          onAdd={onAddRelationship}
        />
      )}
    </div>
  );
}
