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

interface EditableRelationshipRowProps {
  relationship: Relationship;
  relatedEntity: EntityItem | undefined;
  currentEntityId: number;
  onUpdateRelationship: (
    id: number,
    updates: Partial<Pick<Relationship, 'type' | 'description' | 'attitudeScore'>>
  ) => Promise<void>;
  onDeleteRelationship: (id: number) => void;
}

const clampAttitudeScore = (score: number) => Math.min(5, Math.max(-5, score));

function EditableRelationshipRow({
  relationship,
  relatedEntity,
  currentEntityId,
  onUpdateRelationship,
  onDeleteRelationship,
}: EditableRelationshipRowProps) {
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

  const relatedName = relatedEntity?.name
    ?? `Entity #${relationship.npcId1 === currentEntityId ? relationship.npcId2 : relationship.npcId1}`;
  const relatedLabel = relatedEntity?.entityType === 'npc' ? 'NPC' : 'Player Character';

  return (
    <div className="rounded-lg border border-border/50 bg-card/50 group p-2 space-y-2">
      {/* Row 1: type (editable) + name + delete */}
      <div className="flex items-start gap-2">
        <div className="shrink-0 pt-0.5">
          {isTypeEditing ? (
            <select
              className="h-7 rounded border border-border/60 bg-background px-1.5 text-xs"
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
              title="Click to change type"
              onClick={() => setIsTypeEditing(true)}
              className={`inline-flex h-auto items-center rounded-md border px-2 py-0.5 text-xs font-semibold transition-colors cursor-pointer hover:opacity-80 ${relationshipBadgeColors[draft.type] ?? relationshipBadgeColors.neutral}`}
            >
              {draft.type}
            </button>
          )}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">{relatedName}</p>
          <p className="text-xs text-muted-foreground">{relatedLabel}</p>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 shrink-0 opacity-0 group-hover:opacity-100 transition-opacity hover:bg-destructive/20 hover:text-destructive"
          onClick={() => onDeleteRelationship(relationship.id)}
          aria-label="Delete relationship"
        >
          <Trash2 className="size-3" />
        </Button>
      </div>

      {/* Row 2: description (click to edit) */}
      <div>
        {isDescriptionEditing ? (
          <input
            className="h-7 w-full rounded border border-border/60 bg-background px-2 text-xs"
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
            title="Click to edit description"
            className="w-full text-left text-xs text-muted-foreground italic hover:text-foreground/80 transition-colors"
            onClick={() => setIsDescriptionEditing(true)}
          >
            {draft.description || <span className="opacity-50">Add description…</span>}
          </button>
        )}
      </div>

      {/* Row 3: attitude +/- controls + save indicator */}
      <div className="flex items-center gap-3 flex-wrap">
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
                    <EditableRelationshipRow
                      key={rel.id}
                      relationship={rel}
                      relatedEntity={related}
                      currentEntityId={entity.id}
                      onUpdateRelationship={onUpdateRelationship}
                      onDeleteRelationship={onDeleteRelationship}
                    />
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
