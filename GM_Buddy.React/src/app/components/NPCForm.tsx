import { useState, useEffect } from "react";
import { NPC } from "@/types/npc";
import { gameSystemApi, ApiGameSystem } from "@/services/api";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import { Textarea } from "@/app/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/components/ui/select";

interface NPCFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (npc: Omit<NPC, 'id'> | NPC) => void;
  editingNPC?: NPC | null;
}

export function NPCForm({ open, onOpenChange, onSave, editingNPC }: NPCFormProps) {
const [formData, setFormData] = useState({
  name: "",
  race: "",
  class: "",
  description: "",
  system: "Dungeons & Dragons (5e)",
  faction: "",
  notes: ""
});

const [gameSystems, setGameSystems] = useState<ApiGameSystem[]>([]);
const [loadingGameSystems, setLoadingGameSystems] = useState(false);

// Load game systems when component mounts
useEffect(() => {
  const loadGameSystems = async () => {
    try {
      setLoadingGameSystems(true);
      const systems = await gameSystemApi.getGameSystems();
      setGameSystems(systems);
    } catch (error) {
      console.error('Failed to load game systems:', error);
    } finally {
      setLoadingGameSystems(false);
    }
  };

  if (open) {
    loadGameSystems();
  }
}, [open]);

useEffect(() => {
  if (editingNPC) {
    setFormData({
      name: editingNPC.name,
      race: editingNPC.race,
      class: editingNPC.class,
      description: editingNPC.description,
      system: editingNPC.system || "Dungeons & Dragons (5e)",
      faction: editingNPC.faction || "",
      notes: editingNPC.notes || ""
    });
  } else {
    setFormData({
      name: "",
      race: "",
      class: "",
      description: "",
      system: "Dungeons & Dragons (5e)",
      faction: "",
      notes: ""
    });
  }
}, [editingNPC, open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (editingNPC) {
      onSave({ ...editingNPC, ...formData });
    } else {
      onSave(formData);
    }
    
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{editingNPC ? "Edit NPC" : "Create New NPC"}</DialogTitle>
          <DialogDescription>
            {editingNPC ? "Update the details of your NPC." : "Add a new NPC to your campaign."}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Enter NPC name"
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="system">Game System *</Label>
              <Select
                value={formData.system}
                onValueChange={(value) => setFormData({ ...formData, system: value })}
                disabled={loadingGameSystems}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a game system" />
                </SelectTrigger>
                <SelectContent>
                  {gameSystems.map((system) => (
                    <SelectItem key={system.game_system_id} value={system.game_system_name}>
                      {system.game_system_name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="race">Race *</Label>
                <Input
                  id="race"
                  value={formData.race}
                  onChange={(e) => setFormData({ ...formData, race: e.target.value })}
                  placeholder="e.g., Human, Elf, Dwarf"
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="class">Class *</Label>
                <Input
                  id="class"
                  value={formData.class}
                  onChange={(e) => setFormData({ ...formData, class: e.target.value })}
                  placeholder="e.g., Warrior, Mage, Rogue"
                  required
                />
              </div>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="faction">Faction</Label>
              <Input
                id="faction"
                value={formData.faction}
                onChange={(e) => setFormData({ ...formData, faction: e.target.value })}
                placeholder="e.g., The Iron Guard, Merchants Guild"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe this NPC's appearance, personality, or role"
                rows={3}
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="notes">Notes</Label>
              <Textarea
                id="notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                placeholder="Additional notes, plot hooks, or important information"
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit">
              {editingNPC ? "Update" : "Create"} NPC
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
