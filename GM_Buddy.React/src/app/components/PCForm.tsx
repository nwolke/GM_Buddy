import { useState, useEffect } from "react";
import { PC } from "@/types/pc";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/app/components/ui/dialog";
import { Button } from "@/app/components/ui/button";
import { Input } from "@/app/components/ui/input";
import { Label } from "@/app/components/ui/label";
import { Textarea } from "@/app/components/ui/textarea";

interface PCFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (pc: Omit<PC, 'id'> | PC) => Promise<void>;
  editingPC?: PC | null;
  campaignId: number;
}

export function PCForm({ open, onOpenChange, onSave, editingPC, campaignId }: PCFormProps) {
  const [formData, setFormData] = useState({
    name: "",
    description: "",
  });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (editingPC) {
      setFormData({
        name: editingPC.name,
        description: editingPC.description || "",
      });
    } else {
      setFormData({ name: "", description: "" });
    }
  }, [editingPC, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedName = formData.name.trim();
    if (!trimmedName) {
      return;
    }

    if (!campaignId) {
      console.error('[PCForm] Cannot save PC: campaignId is required');
      return;
    }

    const pcData = {
      name: trimmedName,
      description: formData.description || undefined,
      campaignId,
    };

    setSaving(true);
    try {
      if (editingPC) {
        await onSave({ ...editingPC, ...pcData });
      } else {
        await onSave(pcData as Omit<PC, 'id'>);
      }
      onOpenChange(false);
    } catch (err) {
      console.error('[PCForm] Failed to save PC:', err);
      // Keep dialog open so user can retry
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{editingPC ? "Edit Player Character" : "Add Player Character"}</DialogTitle>
          <DialogDescription>
            {editingPC
              ? "Update the details of your player character."
              : "Add a new player character to your roster."}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="pc-name">Name *</Label>
              <Input
                id="pc-name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Enter character name"
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="pc-description">Description</Label>
              <Textarea
                id="pc-description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe this character's appearance, class, background, or role"
                rows={4}
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
              Cancel
            </Button>
            <Button type="submit" disabled={saving}>
              {saving ? "Saving..." : `${editingPC ? "Update" : "Add"} Character`}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
