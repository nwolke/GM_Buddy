import { useState } from "react";
import { Relationship, RelationshipType } from "@/types/npc";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Label } from "@/app/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/app/components/ui/select";
import { Input } from "@/app/components/ui/input";
import { EntityItem } from "@/types/entity";

interface RelationshipAddModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sourceEntity: EntityItem;
  allEntities: EntityItem[];
  existingRelationships: Relationship[];
  onAdd: (relationship: Omit<Relationship, 'id'>) => Promise<void>;
}

const relationshipTypes: RelationshipType[] = [
  'acquaintance', 'ally', 'contact/informant', 'employer', 'enemy',
  'family', 'lover', 'mentor', 'patron', 'rival', 'stranger', 'vassal/follower',
];
const CUSTOM_RELATIONSHIP_OPTION = "__custom__";

export function RelationshipAddModal({
  open,
  onOpenChange,
  sourceEntity,
  allEntities,
  existingRelationships,
  onAdd,
}: RelationshipAddModalProps) {
  const [targetId, setTargetId] = useState<string>("");
  const [relationshipType, setRelationshipType] = useState<RelationshipType | typeof CUSTOM_RELATIONSHIP_OPTION>("ally");
  const [attitudeScore, setAttitudeScore] = useState(0);
  const [customType, setCustomType] = useState("");
  const [description, setDescription] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // IDs already related to source (in either direction)
  const relatedKeys = new Set(
    existingRelationships.map(rel => {
      const isSource = rel.npcId1 === sourceEntity.id && rel.entityType1 === sourceEntity.entityType;
      const otherId = isSource ? rel.npcId2 : rel.npcId1;
      const otherType = isSource ? rel.entityType2 : rel.entityType1;
      return `${otherType}-${otherId}`;
    })
  );

  const sourceKey = `${sourceEntity.entityType}-${sourceEntity.id}`;

  // Group available entities by type, excluding source and already-related
  const availableEntities = allEntities.filter(e => {
    const key = `${e.entityType}-${e.id}`;
    return key !== sourceKey && !relatedKeys.has(key);
  });

  const npcsAvailable = availableEntities.filter(e => e.entityType === 'npc');
  const pcsAvailable = availableEntities.filter(e => e.entityType === 'pc');

  const handleAdd = async () => {
    if (!targetId) return;
    const [targetType, targetIdStr] = targetId.split('-') as ['npc' | 'pc', string];
    const numericTargetId = Number(targetIdStr);
    if (Number.isNaN(numericTargetId)) return;

    setSaving(true);
    setError(null);
    try {
      await onAdd({
        npcId1: sourceEntity.id,
        npcId2: numericTargetId,
        entityType1: sourceEntity.entityType,
        entityType2: targetType,
        type: relationshipType === CUSTOM_RELATIONSHIP_OPTION ? 'stranger' : relationshipType,
        description: description || undefined,
        attitudeScore,
        customType: relationshipType === CUSTOM_RELATIONSHIP_OPTION ? customType.trim() || undefined : undefined,
      });
      // Only clear and close on success
      setTargetId("");
      setRelationshipType("ally");
      setAttitudeScore(0);
      setCustomType("");
      setDescription("");
      onOpenChange(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add relationship. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Add Relationship</DialogTitle>
          <DialogDescription>
            Create a new connection from <strong>{sourceEntity.name}</strong> to another entity.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div className="grid gap-2">
            <Label htmlFor="target-select">Connect to</Label>
            <Select value={targetId} onValueChange={setTargetId}>
              <SelectTrigger id="target-select">
                <SelectValue placeholder="Choose an entity" />
              </SelectTrigger>
              <SelectContent>
                {npcsAvailable.length > 0 && (
                  <>
                    <div className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                      NPCs
                    </div>
                    {npcsAvailable.map(e => (
                      <SelectItem key={`npc-${e.id}`} value={`npc-${e.id}`}>
                        {e.name}
                        {e.lineage && e.class && (
                          <span className="text-muted-foreground"> ({e.lineage} {e.class})</span>
                        )}
                      </SelectItem>
                    ))}
                  </>
                )}
                {pcsAvailable.length > 0 && (
                  <>
                    <div className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                      Player Characters
                    </div>
                    {pcsAvailable.map(e => (
                      <SelectItem key={`pc-${e.id}`} value={`pc-${e.id}`}>
                        {e.name}
                      </SelectItem>
                    ))}
                  </>
                )}
                {availableEntities.length === 0 && (
                  <div className="px-2 py-3 text-sm text-muted-foreground text-center">
                    All entities are already connected.
                  </div>
                )}
              </SelectContent>
            </Select>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="type-select">Relationship Type</Label>
            <Select
              value={relationshipType}
              onValueChange={(val) => setRelationshipType(val as RelationshipType | typeof CUSTOM_RELATIONSHIP_OPTION)}
            >
              <SelectTrigger id="type-select">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {relationshipTypes.map(type => (
                  <SelectItem key={type} value={type}>
                    {type.split('/').map(part => part.charAt(0).toUpperCase() + part.slice(1)).join('/')}
                  </SelectItem>
                ))}
                <SelectItem value={CUSTOM_RELATIONSHIP_OPTION}>Custom...</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {relationshipType === CUSTOM_RELATIONSHIP_OPTION && (
            <div className="grid gap-2">
              <Label htmlFor="custom-type">Custom Type Name <span className="text-destructive">*</span></Label>
              <Input
                id="custom-type"
                value={customType}
                onChange={(e) => setCustomType(e.target.value)}
                placeholder="e.g. Blood Oath, Sworn Shield"
                maxLength={100}
              />
              {relationshipType === CUSTOM_RELATIONSHIP_OPTION && !customType.trim() && (
                <p className="text-xs text-muted-foreground">A custom type name is required.</p>
              )}
            </div>
          )}

          <div className="grid gap-2">
            <Label htmlFor="attitude-score">
              Attitude Score: <span className={`font-semibold ${attitudeScore > 0 ? 'text-green-400' : attitudeScore < 0 ? 'text-red-400' : 'text-muted-foreground'}`}>
                {attitudeScore > 0 ? '+' : ''}{attitudeScore}
              </span>
              <span className="text-muted-foreground ml-1 font-normal text-xs">
                ({attitudeScore <= -4 ? 'Hostile' : attitudeScore <= -2 ? 'Unfriendly' : attitudeScore <= 1 ? 'Neutral' : attitudeScore <= 3 ? 'Friendly' : 'Devoted'})
              </span>
            </Label>
            <Input
              id="attitude-score"
              type="number"
              min={-5}
              max={5}
              step={1}
              value={attitudeScore}
              onChange={(e) => {
                const next = Number(e.target.value);
                if (Number.isNaN(next)) return;
                setAttitudeScore(Math.max(-5, Math.min(5, Math.trunc(next))));
              }}
            />
          </div>

          <div className="grid gap-2">
            <Label htmlFor="description">Description (Optional)</Label>
            <Input
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Additional details about this relationship"
            />
          </div>
        </div>

        {error && (
          <p className="text-sm text-destructive px-1">{error}</p>
        )}
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
            Cancel
          </Button>
          <Button
            onClick={handleAdd}
            disabled={!targetId || saving || (relationshipType === CUSTOM_RELATIONSHIP_OPTION && !customType.trim())}
            className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
          >
            {saving ? 'Adding...' : 'Add Relationship'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
