import { KeyboardEvent, useEffect, useRef, useState } from "react";
import { Relationship } from "@/types/npc";
import { EntityItem } from "@/types/entity";
import { Badge } from "@/app/components/ui/badge";
import { Button } from "@/app/components/ui/button";
import { ScrollArea } from "@/app/components/ui/scroll-area";
import { Separator } from "@/app/components/ui/separator";
import { RelationshipAddModal } from "@/app/components/RelationshipAddModal";
import { AttitudeControl } from "@/app/components/AttitudeControl";
import { SaveIndicator } from "@/app/components/SaveIndicator";
import { useAutoSave } from "@/hooks/useAutoSave";
import { Trash2, Plus, Shield, User } from "lucide-react";

interface EntityDetailPanelProps {
  entity: EntityItem | null;
  relationships: Relationship[];
  allEntities: EntityItem[];
  onAddRelationship: (relationship: Omit<Relationship, 'id'>) => Promise<void>;
  onDeleteRelationship: (id: number) => Promise<void>;
  onUpdateRelationship: (
    id: number,
    updates: Partial<Pick<Relationship, 'type' | 'description' | 'attitudeScore'>>
  ) => Promise<void>;
}

const editableRelationshipTypes: Relationship['type'][] = [
  'acquaintance',
  'ally',
  'contact/informant',
  'employer',
  'enemy',
  'family',
  'lover',
  'mentor',
  'patron',
  'rival',
  'stranger',
  'vassal/follower',
  'neutral',
];

interface EditablePcStanceRowProps {
  relationship: Relationship;
  pcName: string;
  onUpdateRelationship: (
    id: number,
    updates: Partial<Pick<Relationship, 'type' | 'description' | 'attitudeScore'>>
  ) => Promise<void>;
}

const clampAttitudeScore = (score: number) => Math.min(5, Math.max(-5, score));

function EditablePcStanceRow({
  relationship,
  pcName,
  onUpdateRelationship,
}: EditablePcStanceRowProps) {
  const [draft, setDraft] = useState({
    type: relationship.type,
    description: relationship.description ?? '',
    attitudeScore: relationship.attitudeScore ?? 0,
  });
  const [isTypeEditing, setIsTypeEditing] = useState(false);
  const [isDescriptionEditing, setIsDescriptionEditing] = useState(false);
  const [scoreErrorFlash, setScoreErrorFlash] = useState(false);
  const draftRef = useRef(draft);

  useEffect(() => {
    const nextDraft = {
      type: relationship.type,
      description: relationship.description ?? '',
      attitudeScore: relationship.attitudeScore ?? 0,
    };
    setDraft(nextDraft);
    draftRef.current = nextDraft;
  }, [relationship.attitudeScore, relationship.description, relationship.type]);

  const autoSave = useAutoSave(async (nextDraft: typeof draft) => {
    const previousDraft = draftRef.current;
    try {
      await onUpdateRelationship(relationship.id, {
        type: nextDraft.type,
        description: nextDraft.description.trim() ? nextDraft.description : undefined,
        attitudeScore: nextDraft.attitudeScore,
      });
      draftRef.current = nextDraft;
    } catch (error) {
      setDraft(previousDraft);
      draftRef.current = previousDraft;

      if (nextDraft.attitudeScore !== previousDraft.attitudeScore) {
        setScoreErrorFlash(true);
        setTimeout(() => setScoreErrorFlash(false), 300);
      }

      throw error;
    }
  }, 300);

  const updateDraft = (nextDraft: typeof draft) => {
    setDraft(nextDraft);
    autoSave.scheduleSave(nextDraft);
  };

  const handleDescriptionKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      event.currentTarget.blur();
    }
  };

  return (
    <div className="rounded-lg border border-border/50 bg-card/30 p-2">
      <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">PC</p>
      <p className="text-sm font-medium">{pcName}</p>
      <div className="mt-2">
        <p className="text-[11px] uppercase tracking-wider text-muted-foreground">Type</p>
        {isTypeEditing ? (
          <select
            className="h-8 rounded border border-border/60 bg-background px-2 text-xs"
            value={draft.type}
            onChange={(event) => {
              const nextDraft = { ...draft, type: event.target.value as Relationship['type'] };
              updateDraft(nextDraft);
              setIsTypeEditing(false);
            }}
            onBlur={() => setIsTypeEditing(false)}
            autoFocus
          >
            {editableRelationshipTypes.map(type => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        ) : (
          <button
            type="button"
            className="text-xs underline decoration-dotted underline-offset-4"
            onClick={() => setIsTypeEditing(true)}
          >
            {draft.type}
          </button>
        )}
      </div>
      <div className="mt-2">
        <p className="text-[11px] uppercase tracking-wider text-muted-foreground">Description</p>
        {isDescriptionEditing ? (
          <input
            className="h-8 w-full rounded border border-border/60 bg-background px-2 text-xs"
            value={draft.description}
            onChange={(event) => updateDraft({ ...draft, description: event.target.value })}
            onBlur={() => {
              setIsDescriptionEditing(false);
              void autoSave.flush();
            }}
            onKeyDown={handleDescriptionKeyDown}
            autoFocus
          />
        ) : (
          <button
            type="button"
            className="max-w-[220px] truncate text-left text-xs text-muted-foreground underline decoration-dotted underline-offset-4"
            onClick={() => setIsDescriptionEditing(true)}
          >
            {draft.description || 'Add description'}
          </button>
        )}
      </div>
      <div className="mt-2">
        <p className="text-[11px] uppercase tracking-wider text-muted-foreground">Attitude</p>
        <div className="flex flex-col items-start gap-1">
          <AttitudeControl
            score={draft.attitudeScore}
            showErrorFlash={scoreErrorFlash}
            onIncrement={() => {
              const nextScore = clampAttitudeScore(draft.attitudeScore + 1);
              if (nextScore !== draft.attitudeScore) {
                updateDraft({ ...draft, attitudeScore: nextScore });
              }
            }}
            onDecrement={() => {
              const nextScore = clampAttitudeScore(draft.attitudeScore - 1);
              if (nextScore !== draft.attitudeScore) {
                updateDraft({ ...draft, attitudeScore: nextScore });
              }
            }}
          />
          <SaveIndicator status={autoSave.status} error={autoSave.error} onRetry={autoSave.retry} />
        </div>
      </div>
    </div>
  );
}

const relationshipBadgeColors: Record<string, string> = {
  acquaintance: 'bg-slate-500/20 text-slate-400 border-slate-500/30',
  ally: 'bg-green-500/20 text-green-400 border-green-500/30',
  'contact/informant': 'bg-teal-500/20 text-teal-400 border-teal-500/30',
  employer: 'bg-amber-600/20 text-amber-300 border-amber-600/30',
  enemy: 'bg-red-500/20 text-red-400 border-red-500/30',
  family: 'bg-purple-500/20 text-purple-400 border-purple-500/30',
  lover: 'bg-rose-500/20 text-rose-400 border-rose-500/30',
  mentor: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
  patron: 'bg-sky-500/20 text-sky-400 border-sky-500/30',
  rival: 'bg-orange-500/20 text-orange-400 border-orange-500/30',
  stranger: 'bg-zinc-500/20 text-zinc-400 border-zinc-500/30',
  'vassal/follower': 'bg-stone-500/20 text-stone-400 border-stone-500/30',
  neutral: 'bg-gray-500/20 text-gray-400 border-gray-500/30',
};

export function EntityDetailPanel({
  entity,
  relationships,
  allEntities,
  onAddRelationship,
  onDeleteRelationship,
  onUpdateRelationship,
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
  const pcStances = isNpc
    ? entityRelationships
        .filter(rel =>
          rel.entityType1 === 'pc' &&
          rel.entityType2 === 'npc' &&
          rel.npcId2 === entity.id
        )
        .map(rel => ({
          relationship: rel,
          pc: getRelatedEntity(rel),
        }))
    : [];

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
                          {rel.type}
                        </Badge>
                        <span className={`text-xs font-semibold ${
                          rel.attitudeScore > 0 ? 'text-green-400' : rel.attitudeScore < 0 ? 'text-red-400' : 'text-muted-foreground'
                        }`}>
                          {rel.attitudeScore > 0 ? '+' : ''}{rel.attitudeScore}
                        </span>
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

          {isNpc && (
            <div>
              <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">
                PC Stances ({pcStances.length})
              </p>
              {pcStances.length === 0 ? (
                <p className="text-sm text-muted-foreground italic">
                  No PC stances for this NPC yet.
                </p>
              ) : (
                <div className="space-y-2">
                  {pcStances.map(({ relationship, pc }) => (
                    <EditablePcStanceRow
                      key={relationship.id}
                      relationship={relationship}
                      pcName={pc?.name ?? "Unknown PC"}
                      onUpdateRelationship={onUpdateRelationship}
                    />
                  ))}
                </div>
              )}
            </div>
          )}
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
