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
  onDeleteRelationship: (id: string) => void;
}

const relationshipTypes: RelationshipType[] = ['ally', 'enemy', 'family', 'rival', 'mentor', 'student', 'neutral'];

const relationshipColors: Record<RelationshipType, string> = {
  ally: 'bg-green-500',
  enemy: 'bg-red-500',
  family: 'bg-purple-500',
  rival: 'bg-orange-500',
  mentor: 'bg-blue-500',
  student: 'bg-cyan-500',
  neutral: 'bg-gray-500'
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
  const [relationshipType, setRelationshipType] = useState<RelationshipType>("ally");
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

    onAddRelationship({
      npcId1: currentNPC.id,
      npcId2: selectedNPCId,
      type: relationshipType,
      description: description || undefined
    });

    setSelectedNPCId("");
    setRelationshipType("ally");
    setDescription("");
  };

  const getNPCName = (id: string) => {
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
                        <Badge className={relationshipColors[rel.type]}>
                          {rel.type}
                        </Badge>
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
                        <SelectItem key={npc.id} value={npc.id}>
                          {npc.name} ({npc.race} {npc.class})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="type-select">Relationship Type</Label>
                  <Select value={relationshipType} onValueChange={(val) => setRelationshipType(val as RelationshipType)}>
                    <SelectTrigger id="type-select">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {relationshipTypes.map(type => (
                        <SelectItem key={type} value={type}>
                          {type.charAt(0).toUpperCase() + type.slice(1)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
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

                <Button onClick={handleAddRelationship} disabled={!selectedNPCId}>
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
