import { useState, useEffect } from "react";
import { NPC } from "@/types/npc";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import { Textarea } from "@/app/components/ui/textarea";

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
    faction: "",
    notes: ""
  });

  useEffect(() => {
    if (editingNPC) {
      setFormData({
        name: editingNPC.name,
        race: editingNPC.race,
        class: editingNPC.class,
        description: editingNPC.description,
        faction: editingNPC.faction || "",
        notes: editingNPC.notes || ""
      });
    } else {
      setFormData({
        name: "",
        race: "",
        class: "",
        description: "",
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
