import { useState, useEffect } from 'react';
import { PcStance } from '@/hooks/useNpcPcStances';
import { relationshipApi, ApiRelationshipType } from '@/services/api';
import { EntityItem } from '@/types/entity';
import { Relationship } from '@/types/npc';
import { Badge } from '@/app/components/ui/badge';
import { Button } from '@/app/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/app/components/ui/select';
import { DispositionBadge } from '@/app/components/DispositionBadge';
import { DispositionSlider } from '@/app/components/DispositionSlider';
import { RelationshipAddModal } from '@/app/components/RelationshipAddModal';
import { Pencil, Check, X, Plus, User } from 'lucide-react';

interface NpcPcStanceGridProps {
  npcEntity: EntityItem;
  stances: PcStance[];
  loading: boolean;
  allEntities: EntityItem[];
  existingRelationships: Relationship[];
  onStanceChanged: () => void;
  onAddRelationship: (relationship: Omit<Relationship, 'id'>) => Promise<void>;
}

interface EditState {
  pcId: number;
  relationshipTypeId: number | null;
  disposition: number | null;
}

export function NpcPcStanceGrid({
  npcEntity,
  stances,
  loading,
  allEntities,
  existingRelationships,
  onStanceChanged,
  onAddRelationship,
}: NpcPcStanceGridProps) {
  const [editState, setEditState] = useState<EditState | null>(null);
  const [saving, setSaving] = useState(false);
  const [relationshipTypes, setRelationshipTypes] = useState<ApiRelationshipType[]>([]);
  const [addModalOpen, setAddModalOpen] = useState(false);

  // Load relationship types for the dropdown
  useEffect(() => {
    relationshipApi.getRelationshipTypes().then(setRelationshipTypes).catch(console.error);
  }, []);

  const startEdit = (stance: PcStance) => {
    setEditState({
      pcId: stance.pcId,
      relationshipTypeId: stance.relationshipTypeId,
      disposition: stance.disposition,
    });
  };

  const cancelEdit = () => {
    setEditState(null);
  };

  const saveEdit = async (stance: PcStance) => {
    if (!editState || !stance.entityRelationshipId) return;

    setSaving(true);
    try {
      // We need to build the full ApiEntityRelationship for the PUT
      // The stance came from an NPC-PC relationship, so rebuild the shape
      await relationshipApi.updateRelationship(stance.entityRelationshipId, {
        entity_relationship_id: stance.entityRelationshipId,
        source_entity_type: 'npc',
        source_entity_id: npcEntity.id,
        target_entity_type: 'pc',
        target_entity_id: stance.pcId,
        relationship_type_id: editState.relationshipTypeId ?? 0,
        disposition: editState.disposition,
        campaign_id: npcEntity.campaignId,
      });
      setEditState(null);
      onStanceChanged();
    } catch (err) {
      console.error('[NpcPcStanceGrid] Failed to save stance:', err);
    } finally {
      setSaving(false);
    }
  };

  // Filter "Quick Add" to PCs that don't already have stances
  const pcsWithStances = new Set(
    stances.filter(s => s.entityRelationshipId !== null).map(s => s.pcId)
  );
  const hasUnconnectedPcs = stances.some(s => s.entityRelationshipId === null);

  if (loading) {
    return (
      <div className="py-2">
        <p className="text-xs text-muted-foreground italic">Loading PC stances...</p>
      </div>
    );
  }

  if (stances.length === 0) {
    return (
      <div className="py-2">
        <p className="text-xs text-muted-foreground italic">
          No PCs in this campaign yet.
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
          PC Stances ({stances.filter(s => s.entityRelationshipId !== null).length}/{stances.length})
        </p>
        {hasUnconnectedPcs && (
          <Button
            size="sm"
            variant="outline"
            onClick={() => setAddModalOpen(true)}
            className="h-6 text-xs border-green-500/30 hover:bg-green-500/10 text-green-400"
          >
            <Plus className="size-3 mr-1" />
            Add
          </Button>
        )}
      </div>

      <div className="space-y-1.5">
        {stances.map(stance => {
          const isEditing = editState?.pcId === stance.pcId;
          const hasStance = stance.entityRelationshipId !== null;

          return (
            <div
              key={stance.pcId}
              className={`rounded-lg border p-2 transition-colors group ${
                hasStance
                  ? 'border-border/50 bg-card/50'
                  : 'border-dashed border-border/30 bg-card/20'
              }`}
            >
              {/* PC name row */}
              <div className="flex items-center gap-2 mb-1">
                <User className="size-3 text-green-400/60 shrink-0" />
                <span className="text-sm font-medium truncate flex-1">
                  {stance.pcName}
                </span>

                {hasStance && !isEditing && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-5 w-5 shrink-0 opacity-0 group-hover:opacity-100 hover:bg-primary/10"
                    onClick={() => startEdit(stance)}
                    aria-label={`Edit stance for ${stance.pcName}`}
                  >
                    <Pencil className="size-3" />
                  </Button>
                )}
              </div>

              {/* Display mode */}
              {hasStance && !isEditing && (
                <div className="flex items-center gap-1.5 pl-5">
                  {stance.relationshipType && (
                    <Badge
                      variant="outline"
                      className="text-xs"
                    >
                      {stance.relationshipType}
                    </Badge>
                  )}
                  <DispositionBadge disposition={stance.disposition} showScore={true} />
                </div>
              )}

              {/* No stance yet */}
              {!hasStance && !isEditing && (
                <p className="text-xs text-muted-foreground italic pl-5">
                  No stance set
                </p>
              )}

              {/* Edit mode */}
              {isEditing && editState && (
                <div className="pl-5 space-y-2 mt-1">
                  <div className="grid gap-1">
                    <Select
                      value={editState.relationshipTypeId?.toString() ?? ''}
                      onValueChange={(val) =>
                        setEditState(prev => prev ? { ...prev, relationshipTypeId: Number(val) } : prev)
                      }
                    >
                      <SelectTrigger className="h-7 text-xs">
                        <SelectValue placeholder="Relationship type" />
                      </SelectTrigger>
                      <SelectContent>
                        {relationshipTypes.map(rt => (
                          <SelectItem
                            key={rt.relationship_type_id}
                            value={rt.relationship_type_id.toString()}
                          >
                            {rt.type_name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <DispositionSlider
                    value={editState.disposition}
                    onChange={(val) =>
                      setEditState(prev => prev ? { ...prev, disposition: val } : prev)
                    }
                    id={`stance-disp-${stance.pcId}`}
                  />

                  <div className="flex gap-1 justify-end">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={cancelEdit}
                      disabled={saving}
                      className="h-6 text-xs"
                    >
                      <X className="size-3 mr-1" />
                      Cancel
                    </Button>
                    <Button
                      size="sm"
                      onClick={() => saveEdit(stance)}
                      disabled={saving || !editState.relationshipTypeId}
                      className="h-6 text-xs bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
                    >
                      <Check className="size-3 mr-1" />
                      {saving ? 'Saving...' : 'Save'}
                    </Button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Quick Add modal — pre-filtered to PCs only */}
      {addModalOpen && (
        <RelationshipAddModal
          open={addModalOpen}
          onOpenChange={setAddModalOpen}
          sourceEntity={npcEntity}
          allEntities={allEntities.filter(e =>
            e.entityType === 'pc' && !pcsWithStances.has(e.id)
          )}
          existingRelationships={existingRelationships}
          onAdd={async (rel) => {
            await onAddRelationship(rel);
            onStanceChanged();
          }}
        />
      )}
    </div>
  );
}
