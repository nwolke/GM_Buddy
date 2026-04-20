import { useState } from "react";
import { NPC, Relationship, RelationshipType } from "@/types/npc";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Label } from "@/app/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/components/ui/select";
import { Input } from "@/app/components/ui/input";
import { Trash2 } from "lucide-react";
import { Badge } from "@/app/components/ui/badge";

interface RelationshipManagerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  currentNPC: NPC | null;
  allNPCs: NPC[];
  relationships: Relationship[];
  onAddRelationship: (relationship: Omit<Relationship, 'id'>) => void;
  onDeleteRelationship: (id: number) => void;
}

const relationshipTypes: RelationshipType[] = [
  'acquaintance', 'ally', 'contact/informant', 'employer', 'enemy',
  'family', 'lover', 'mentor', 'patron', 'rival', 'stranger', 'vassal/follower',
];
const CUSTOM_RELATIONSHIP_OPTION = "__custom__";

const relationshipColors: Record<string, string> = {
  acquaintance: 'bg-slate-500',
  ally: 'bg-green-500',
  'contact/informant': 'bg-teal-500',
  employer: 'bg-amber-600',
  enemy: 'bg-red-500',
  family: 'bg-purple-500',
  lover: 'bg-rose-500',
  mentor: 'bg-blue-500',
  patron: 'bg-sky-500',
  rival: 'bg-orange-500',
  stranger: 'bg-zinc-500',
  'vassal/follower': 'bg-stone-500',
  custom: 'bg-lime-500',
  neutral: 'bg-gray-500',
};

export function RelationshipManager({
  open,
  onOpenChange,
  currentNPC,
  allNPCs,
  relationships,
  onAddRelationship,
  onDeleteRelationship
}: RelationshipManagerProps) {
  const [selectedNPCId, setSelectedNPCId] = useState<string>("");
  const [relationshipType, setRelationshipType] = useState<RelationshipType | typeof CUSTOM_RELATIONSHIP_OPTION>("ally");
  const [attitudeScore, setAttitudeScore] = useState(0);
  const [customType, setCustomType] = useState("");
  const [description, setDescription] = useState("");

  if (!currentNPC) return null;

  // Get relationships involving the current NPC
  const currentRelationships = relationships.filter(
    rel => rel.npcId1 === currentNPC.id || rel.npcId2 === currentNPC.id
  );

  // Get NPCs that are already in a relationship with current NPC
  const relatedNPCIds = new Set(
    currentRelationships.map(rel => 
      rel.npcId1 === currentNPC.id ? rel.npcId2 : rel.npcId1
    )
  );

  // Filter available NPCs (exclude current NPC and those already related)
  const availableNPCs = allNPCs.filter(
    npc => npc.id !== currentNPC.id && !relatedNPCIds.has(npc.id)
  );

  const handleAddRelationship = () => {
    if (!selectedNPCId) return;

    const numericNPCId = Number(selectedNPCId);
    if (Number.isNaN(numericNPCId)) return;

    onAddRelationship({
      npcId1: currentNPC.id,
      npcId2: numericNPCId,
      entityType1: 'npc',
      entityType2: 'npc',
      type: relationshipType === CUSTOM_RELATIONSHIP_OPTION ? 'stranger' : relationshipType,
      description: description || undefined,
      attitudeScore,
      customType: relationshipType === CUSTOM_RELATIONSHIP_OPTION ? customType.trim() || undefined : undefined,
    });

    setSelectedNPCId("");
    setRelationshipType("ally");
    setAttitudeScore(0);
    setCustomType("");
    setDescription("");
  };

  const getNPCName = (id: number) => {
    return allNPCs.find(npc => npc.id === id)?.name || "Unknown";
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Manage Relationships for {currentNPC.name}</DialogTitle>
          <DialogDescription>
            Define how this NPC relates to others in your campaign.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Existing Relationships */}
          <div>
            <h3 className="font-medium mb-3">Current Relationships</h3>
            {currentRelationships.length === 0 ? (
              <p className="text-sm text-muted-foreground">No relationships yet.</p>
            ) : (
              <div className="space-y-2">
                {currentRelationships.map(rel => {
                  const otherNPCId = rel.npcId1 === currentNPC.id ? rel.npcId2 : rel.npcId1;
                  const otherNPCName = getNPCName(otherNPCId);
                  
                  return (
                    <div key={rel.id} className="flex items-center justify-between p-3 border rounded-lg">
                      <div className="flex items-center gap-3 flex-1">
                        <Badge className={relationshipColors[rel.type] ?? relationshipColors.neutral}>
                          {rel.customType || rel.type}
                        </Badge>
                        <span className={`text-xs font-semibold ${
                          rel.attitudeScore > 0 ? 'text-green-400' : rel.attitudeScore < 0 ? 'text-red-400' : 'text-muted-foreground'
                        }`}>
                          {rel.attitudeScore > 0 ? '+' : ''}{rel.attitudeScore}
                        </span>
                        <div>
                          <p className="font-medium">{otherNPCName}</p>
                          {rel.description && (
                            <p className="text-sm text-muted-foreground">{rel.description}</p>
                          )}
                        </div>
                      </div>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => onDeleteRelationship(rel.id)}
                      >
                        <Trash2 className="size-4 text-destructive" />
                      </Button>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* Add New Relationship */}
          {availableNPCs.length > 0 && (
            <div className="border-t pt-6">
              <h3 className="font-medium mb-3">Add New Relationship</h3>
              <div className="space-y-4">
                <div className="grid gap-2">
                  <Label htmlFor="npc-select">Select NPC</Label>
                  <Select value={selectedNPCId} onValueChange={setSelectedNPCId}>
                    <SelectTrigger id="npc-select">
                      <SelectValue placeholder="Choose an NPC" />
                    </SelectTrigger>
                    <SelectContent>
                      {availableNPCs.map(npc => (
                        <SelectItem key={npc.id} value={npc.id.toString()}>
                          {npc.name} ({npc.lineage} {npc.class})
                        </SelectItem>
                      ))}
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
                    <Label htmlFor="custom-type">Custom Type Name</Label>
                    <Input
                      id="custom-type"
                      value={customType}
                      onChange={(e) => setCustomType(e.target.value)}
                      placeholder="e.g. Blood Oath, Sworn Shield"
                      maxLength={100}
                    />
                  </div>
                )}

                <div className="grid gap-2">
                  <Label htmlFor="attitude-score">Attitude Score (-5 to +5)</Label>
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

                <Button onClick={handleAddRelationship} disabled={!selectedNPCId || (relationshipType === CUSTOM_RELATIONSHIP_OPTION && !customType.trim())}>
                  Add Relationship
                </Button>
              </div>
            </div>
          )}

          {availableNPCs.length === 0 && currentRelationships.length > 0 && (
            <div className="border-t pt-6">
              <p className="text-sm text-muted-foreground">
                All other NPCs are already related to {currentNPC.name}.
              </p>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
